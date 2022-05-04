using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SoulsIds;
using SoulsFormats;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using static SoulsIds.GameSpec;
using static EldenConvenienceMod.Mods;

namespace EldenConvenienceMod
{
    internal class Installer
    {
        private readonly string modDir;
        private readonly string gameExe;

        private static readonly Dictionary<string, string> fileDirArchives = new Dictionary<string, string>
        {
            ["/event"] = "Data0",
            ["/script/talk"] = "Data0",
        };

        public Installer(string modDir, string gameExe = null)
        {
            this.modDir = modDir;
            this.gameExe = gameExe;
        }

        private Dictionary<string, string> GetMergeFiles(List<string> paths)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (string path in paths)
            {
                string key = path;
                string checkName;
                if (path.StartsWith("/"))
                {
                    checkName = path.TrimStart('/');
                }
                else if (path.EndsWith(".param"))
                {
                    key = "regulation.bin";
                    checkName = "regulation.bin";
                }
                else throw new Exception($"Internal error: unknown mod file {path}");
                FileInfo checkFile = new FileInfo(Path.Combine(modDir, checkName));
                if (checkFile.Exists)
                {
                    ret[key] = checkFile.FullName;
                }
            }
            return ret;
        }

        public SortedSet<Mod> GetInstalled()
        {
            List<string> paths = AllMods.SelectMany(x => modFiles[x]).Distinct().ToList();
            Dictionary<string, string> mergePaths = GetMergeFiles(paths);

            Dictionary<string, EMEVD> emevds = new Dictionary<string, EMEVD>();
            Dictionary<string, BND4> esds = new Dictionary<string, BND4>();
            Dictionary<string, PARAM> allParams = new Dictionary<string, PARAM>();

            foreach (KeyValuePair<string, string> entry in mergePaths)
            {
                string path = entry.Value;
                string name = GameEditor.BaseName(path);
                try
                {
                    if (path.EndsWith(".emevd.dcx"))
                    {
                        emevds[name] = EMEVD.Read(path);
                    }
                    else if (path.EndsWith(".talkesdbnd.dcx"))
                    {
                        esds[name] = BND4.Read(path);
                    }
                    else if (path.EndsWith("regulation.bin"))
                    {
                        BND4 paramBnd = SFUtil.DecryptERRegulation(path);
                        List<string> paramNames = paths.Where(p => p.EndsWith(".param")).Select(GameEditor.BaseName).ToList();
                        allParams = LoadParams(paramBnd, paramNames);
                    }
                }
                // More detailed error message during installation
                catch (Exception) { }
            }

            SortedSet<Mod> check = new SortedSet<Mod>();
            // Console.WriteLine($"Emevds {emevds.Count} ESDs {esds.Count} Params {allParams.Count}");
            if (emevds.Count > 0 || esds.Count > 0 || allParams.Count > 0)
            {
                new Mods().Run(AllMods, NoMods, emevds, esds, allParams, check);
            }
            return check;
        }

        public void Run(SortedSet<Mod> install, SortedSet<Mod> uninstall, List<string> opt = null)
        {
            opt = opt ?? new List<string>();
            if (gameExe == null) throw new Exception("Internal error: missing path");
            string gameDir = Path.GetDirectoryName(gameExe);
            string regPath = Path.Combine(gameDir, "regulation.bin");
            if (!File.Exists(regPath)) throw new Exception($"Game data file not found: {regPath}");
            SortedSet<Mod> mods = new SortedSet<Mod>(install.Concat(uninstall));
            List<string> paths = mods.SelectMany(x => modFiles[x]).Distinct().ToList();
            Dictionary<string, string> mergePaths = GetMergeFiles(paths);
            if (opt.Contains("fresh")) mergePaths.Clear();

            Dictionary<string, string> pathArchives = new Dictionary<string, string>();
            List<string> paramNames = new List<string>();
            foreach (string path in paths)
            {
                if (path.EndsWith(".param"))
                {
                    paramNames.Add(GameEditor.BaseName(path));
                    continue;
                }
                // Windows is sick
                string fileDir = Path.GetDirectoryName(path).Replace("\\", "/");
                fileDirArchives.TryGetValue(fileDir, out string archive);
                pathArchives[path] = archive;
            }

            Dictionary<string, byte[]> contents = ReadBdtFiles(gameDir, pathArchives);

            Dictionary<string, EMEVD> emevds = new Dictionary<string, EMEVD>();
            Dictionary<string, BND4> esds = new Dictionary<string, BND4>();
            Dictionary<string, PARAM> allParams = new Dictionary<string, PARAM>();
            BND4 paramBnd = null;

            foreach (KeyValuePair<string, byte[]> file in contents)
            {
                string path = file.Key;
                string name = GameEditor.BaseName(path);
                byte[] data = file.Value;
                if (mergePaths.TryGetValue(path, out string mergePath))
                {
                    data = File.ReadAllBytes(mergePath);
                }
                if (path.EndsWith(".emevd.dcx"))
                {
                    emevds[name] = EMEVD.Read(data);
                }
                else if (path.EndsWith(".talkesdbnd.dcx"))
                {
                    esds[name] = BND4.Read(data);
                }
                else
                {
                    throw new Exception($"Internal error: unknown game file {path}");
                }
            }

            if (paramNames.Count > 0)
            {
                mergePaths.TryGetValue("regulation.bin", out string baseRegPath);
                baseRegPath = baseRegPath ?? regPath;
                paramBnd = SFUtil.DecryptERRegulation(baseRegPath);
                allParams = LoadParams(paramBnd, paramNames);
            }

            // Install mods and stuff
            new Mods().Run(install, uninstall, emevds, esds, allParams, null);

            // Write files. Some duplication with path logic here
            if (opt.Contains("dryrun")) return;
            bool verbose = opt.Contains("verbose");
            foreach (KeyValuePair<string, EMEVD> entry in emevds)
            {
                string path = Path.Combine(modDir, $@"event\{entry.Key}.emevd.dcx");
                if (verbose) Console.WriteLine($"Writing {path}");
                EMEVD emevd = entry.Value;
                if (emevd.StringData != null)
                {
                    string data = Encoding.Unicode.GetString(emevd.StringData);
                    string text = "EldenConvenienceMod";
                    if (!data.Contains(text))
                    {
                        data += "\0" + text;
                        emevd.StringData = Encoding.Unicode.GetBytes(data);
                    }
                    // Console.WriteLine(string.Join("", data.Select(x => x < 32 ? $"\\{(int)x:d2}" : $"{x}")));
                }
                emevd.Write(path);
            }
            foreach (KeyValuePair<string, BND4> entry in esds)
            {
                string path = Path.Combine(modDir, $@"script\talk\{entry.Key}.talkesdbnd.dcx");
                if (verbose) Console.WriteLine($"Writing {path}");
                entry.Value.Write(path);
            }
            if (paramBnd != null)
            {
                foreach (BinderFile bndFile in paramBnd.Files)
                {
                    string name = GameEditor.BaseName(bndFile.Name);
                    if (allParams.TryGetValue(name, out PARAM param))
                    {
                        bndFile.Bytes = param.Write();
                    }
                }
                string path = Path.Combine(modDir, "regulation.bin");
                if (verbose) Console.WriteLine($"Writing {path}");
                SFUtil.EncryptERRegulation(path, paramBnd);
            }
        }


        private static Dictionary<string, PARAM> LoadParams(IBinder paramBnd, List<string> paramNames)
        {
            Dictionary<string, PARAM> allParams = new Dictionary<string, PARAM>();
            List<PARAMDEF> defs = LoadDefs();
            foreach (BinderFile bndFile in paramBnd.Files)
            {
                // N:\GR\data\Param\param\GameParam\SpEffectParam.param
                string name = GameEditor.BaseName(bndFile.Name);
                if (paramNames.Contains(name))
                {
                    PARAM param = PARAM.Read(bndFile.Bytes);
                    if (!param.ApplyParamdefCarefully(defs))
                    {
                        throw new Exception($"Could not read param {name}");
                    }
                    allParams[name] = param;
                }
            }
            return allParams;
        }

        // Global static state is pretty terrible, but internal to this class and synchronized
        private static readonly List<PARAMDEF> lazyDefs = new List<PARAMDEF>();
        private static List<PARAMDEF> LoadDefs()
        {
            lock (lazyDefs)
            {
                if (lazyDefs.Count == 0)
                {
                    foreach (string resource in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                    {
                        if (resource.Contains("Defs") && resource.EndsWith(".xml"))
                        {
                            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                            {
                                using (StreamReader reader = new StreamReader(stream))
                                {
                                    PARAMDEF def = PARAMDEF.XmlDeserializeText(reader.ReadToEnd());
                                    lazyDefs.Add(def);
                                }
                            }
                        }
                    }
                }
                return lazyDefs;
            }
        }

#if DEBUG
        internal static void FindCommandEmevds()
        {
            GameEditor editor = new GameEditor(FromGame.ER);
            HashSet<(int, int)> deleteCommands = new HashSet<(int, int)>
            {
                (2003, 28),  // Achievement
                (2007, 15),  // Tutorial popup
            };
            Dictionary<(int, int), SortedSet<string>> locs = deleteCommands.ToDictionary(c => c, c => new SortedSet<string>());
            foreach (string path in Directory.GetFiles(Path.Combine(editor.Spec.GameDir, "event"), "*.emevd.dcx"))
            {
                string name = GameEditor.BaseName(path);
                EMEVD emevd = EMEVD.Read(path);
                foreach (EMEVD.Event ev in emevd.Events)
                {
                    foreach (EMEVD.Instruction ins in ev.Instructions)
                    {
                        (int, int) key = (ins.Bank, ins.ID);
                        if (locs.TryGetValue(key, out SortedSet<string> loc))
                        {
                            loc.Add(name);
                        }
                    }
                }
            }
            foreach (KeyValuePair<(int, int), SortedSet<string>> entry in locs)
            {
                Console.WriteLine($"{entry.Key.Item1}[{entry.Key.Item2:d2}] {string.Join(" ", entry.Value)}");
            }
        }
#endif

        private static Dictionary<string, byte[]> ReadBdtFiles(string gameDir, Dictionary<string, string> pathArchives)
        {
            Dictionary<string, byte[]> ret = new Dictionary<string, byte[]>();
            List<string> archives = pathArchives.Select(e => e.Value).Distinct().ToList();
            bool debugOrigin = false;
            if (archives.Any(p => p == null))
            {
#if DEBUG
                archives = EldenKeys.Keys.ToList();
                debugOrigin = true;
#endif
                if (!debugOrigin)
                {
                    throw new Exception($"Internal error: No archive info for {string.Join(", ", pathArchives.Where(e => e.Value == null).Select(e => e.Key))}");
                }
            }
            Dictionary<ulong, string> tryFind = pathArchives.Keys.ToDictionary(t => ComputeHash(t), t => t);
            foreach (string archive in archives)
            {
                string bhdPath = Path.Combine(gameDir, $"{archive}.bhd");
                string bdtPath = Path.Combine(gameDir, $"{archive}.bdt");
                if (!File.Exists(bhdPath)) throw new Exception($"Game data file not found: {bhdPath}");
                if (!File.Exists(bdtPath)) throw new Exception($"Game data file not found: {bdtPath}");

                BHD5 bhd;
                using (MemoryStream bhdStream = DecryptRsa(bhdPath, EldenKeys[archive]))
                {
                    bhd = BHD5.Read(bhdStream, BHD5.Game.EldenRing);
                }
                using (FileStream bdtStream = File.OpenRead(bdtPath))
                {
                    foreach (BHD5.Bucket bucket in bhd.Buckets)
                    {
                        foreach (BHD5.FileHeader file in bucket)
                        {
                            if (tryFind.TryGetValue(file.FileNameHash, out string name))
                            {
                                byte[] bytes = file.ReadFile(bdtStream);
                                ret[name] = bytes;
                                if (debugOrigin)
                                {
                                    Console.WriteLine($"{archive}: {name}");
                                }
                            }
                        }
                    }
                }
            }
            List<string> missing = pathArchives.Keys.Except(ret.Keys).ToList();
            if (missing.Count > 0)
            {
                throw new Exception($"Game data file not found: {string.Join(", ", missing)}");
            }
            return ret;
        }

        private static Dictionary<string, string> EldenKeys = new Dictionary<string, string>
        {
            ["Data0"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEA9Rju2whruXDVQZpfylVEPeNxm7XgMHcDyaaRUIpXQE0qEo+6Y36L
P0xpFvL0H0kKxHwpuISsdgrnMHJ/yj4S61MWzhO8y4BQbw/zJehhDSRCecFJmFBz
3I2JC5FCjoK+82xd9xM5XXdfsdBzRiSghuIHL4qk2WZ/0f/nK5VygeWXn/oLeYBL
jX1S8wSSASza64JXjt0bP/i6mpV2SLZqKRxo7x2bIQrR1yHNekSF2jBhZIgcbtMB
xjCywn+7p954wjcfjxB5VWaZ4hGbKhi1bhYPccht4XnGhcUTWO3NmJWslwccjQ4k
sutLq3uRjLMM0IeTkQO6Pv8/R7UNFtdCWwIERzH8IQ==
-----END RSA PUBLIC KEY-----",
            ["Data1"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAxaBCHQJrtLJiJNdG9nq3deA9sY4YCZ4dbTOHO+v+YgWRMcE6iK6o
ZIJq+nBMUNBbGPmbRrEjkkH9M7LAypAFOPKC6wMHzqIMBsUMuYffulBuOqtEBD11
CAwfx37rjwJ+/1tnEqtJjYkrK9yyrIN6Y+jy4ftymQtjk83+L89pvMMmkNeZaPON
4O9q5M9PnFoKvK8eY45ZV/Jyk+Pe+xc6+e4h4cx8ML5U2kMM3VDAJush4z/05hS3
/bC4B6K9+7dPwgqZgKx1J7DBtLdHSAgwRPpijPeOjKcAa2BDaNp9Cfon70oC+ZCB
+HkQ7FjJcF7KaHsH5oHvuI7EZAl2XTsLEQIENa/2JQ==
-----END RSA PUBLIC KEY-----",
            ["Data2"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBDAKCAQEA0iDVVQ230RgrkIHJNDgxE7I/2AaH6Li1Eu9mtpfrrfhfoK2e7y4O
WU+lj7AGI4GIgkWpPw8JHaV970Cr6+sTG4Tr5eMQPxrCIH7BJAPCloypxcs2BNfT
GXzm6veUfrGzLIDp7wy24lIA8r9ZwUvpKlN28kxBDGeCbGCkYeSVNuF+R9rN4OAM
RYh0r1Q950xc2qSNloNsjpDoSKoYN0T7u5rnMn/4mtclnWPVRWU940zr1rymv4Jc
3umNf6cT1XqrS1gSaK1JWZfsSeD6Dwk3uvquvfY6YlGRygIlVEMAvKrDRMHylsLt
qqhYkZNXMdy0NXopf1rEHKy9poaHEmJldwIFAP////8=
-----END RSA PUBLIC KEY-----",
            ["Data3"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAvRRNBnVq3WknCNHrJRelcEA2v/OzKlQkxZw1yKll0Y2Kn6G9ts94
SfgZYbdFCnIXy5NEuyHRKrxXz5vurjhrcuoYAI2ZUhXPXZJdgHywac/i3S/IY0V/
eDbqepyJWHpP6I565ySqlol1p/BScVjbEsVyvZGtWIXLPDbx4EYFKA5B52uK6Gdz
4qcyVFtVEhNoMvg+EoWnyLD7EUzuB2Khl46CuNictyWrLlIHgpKJr1QD8a0ld0PD
PHDZn03q6QDvZd23UW2d9J+/HeBt52j08+qoBXPwhndZsmPMWngQDaik6FM7EVRQ
etKPi6h5uprVmMAS5wR/jQIVTMpTj/zJdwIEXszeQw==
-----END RSA PUBLIC KEY-----",
        };

        // Thank you UXM
        private const ulong PRIME = 0x85; // 0x25 previous, now 0x85
        private static ulong ComputeHash(string path)
        {
            string hashable = path.Trim().Replace('\\', '/').ToLowerInvariant();
            if (!hashable.StartsWith("/"))
                hashable = '/' + hashable;
            return hashable.Aggregate(0ul, (i, c) => i * PRIME + c);
        }

        private static MemoryStream DecryptRsa(string filePath, string key)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            AsymmetricKeyParameter keyParameter = GetKeyOrDefault(key);
            RsaEngine engine = new RsaEngine();
            engine.Init(false, keyParameter);

            MemoryStream outputStream = new MemoryStream();
            using (FileStream inputStream = File.OpenRead(filePath))
            {

                int inputBlockSize = engine.GetInputBlockSize();
                int outputBlockSize = engine.GetOutputBlockSize();
                byte[] inputBlock = new byte[inputBlockSize];
                while (inputStream.Read(inputBlock, 0, inputBlock.Length) > 0)
                {
                    byte[] outputBlock = engine.ProcessBlock(inputBlock, 0, inputBlockSize);

                    int requiredPadding = outputBlockSize - outputBlock.Length;
                    if (requiredPadding > 0)
                    {
                        byte[] paddedOutputBlock = new byte[outputBlockSize];
                        outputBlock.CopyTo(paddedOutputBlock, requiredPadding);
                        outputBlock = paddedOutputBlock;
                    }

                    outputStream.Write(outputBlock, 0, outputBlock.Length);
                }
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }

        public static AsymmetricKeyParameter GetKeyOrDefault(string key)
        {
            try
            {
                PemReader pemReader = new PemReader(new StringReader(key));
                return (AsymmetricKeyParameter)pemReader.ReadObject();
            }
            catch
            {
                return null;
            }
        }
    }
}
