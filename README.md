﻿This mod is an installer for these minor convenience mods. It can automatically merge itself into any other mod.

- Unlock all maps
- Unlock all icons (only while mod is active)
- Remove invasion requirement for Varré quest
- Unlock Sites of Grace while riding Torrent
- Warp from dungeons without defeating the boss
- Don't show tutorials (regardless of game options)
- Don't award achievements
- Additional weapon upgrade menus
- Additional purchase menus
- Additional sell menus
- Skip Siofra and Nokron flame pillar puzzles

These mods can be uninstalled, too. They will no longer take effect once uninstalled, with the exception of map unlocks, which are permanent for a playthrough.

This installer is intended to work on top of any other ModEngine-based mod, and for any version of Elden Ring. This is the main reason why it requires running an installer, so it can dynamically use the current Elden Ring version. Remember to back up your save file before using ModEngine, and restore your pre-mod save file before going back online.

**You may include these edits in other mods if you credit this mod.**

**However, do not create mods which consist only of these edits and nothing else. Please just direct people towards this mod in that case.**

## Installation

### 1. Download

Select a version of EldenConvenienceMod to download based on which mods you want to use.

If you want to select mods individually, edit an existing mod folder, or presets have not yet been updated for the latest version of Elden Ring, use the custom version. Otherwise, use a preset.

Extract the zip file using 7zip or a similar tool.

Make sure the .NET Desktop Runtime >=6.0 is installed, if you don't have it. Use this specific installer link if you run into issues with missing runtimes. https://aka.ms/dotnet/6.0/windowsdesktop-runtime-win-x64.exe

### 2. Set up Mod Engine

Download the latest version of [Mod Engine 2](https://github.com/soulsmods/ModEngine2/releases). Alternatively, use a mod which already has Mod Engine built in, like Item and Enemy Randomizer.

### 3. Customize

Run EldenConvenienceMod.exe and select the Elden Ring game exe, then select the directory where you wish to install the mods. In the default Mod Engine setup, this means selecting the directory called `mod`.

You can then select individual mods to install and click "Install selected" to install them.

### 4. Run the game

When using Mod Engine, use `launchmod_eldenring.bat` to launch the game. Do **not** use `modengine2_launcher.exe` directly, and do not launch the game from Steam.

If the launcher immediately closes, it is because the game location cannot be determined from Steam. Make sure Steam is running, and if necessary, change your Elden Ring install location to match the drive Steam is installed on.

### 5. Uninstall

If you used the mod standalone, edit `config_eldenring.toml` or delete the diectory to stop using the mod.

Otherwise, use EldenConvenienceMod.exe to uninstall mods. This will only work if they were originally installed using the installer.

Warning: if you loaded into a save file while using this mod, do not go back online without deleting the save file or restoring a pre-mod save file backup.

## Notes

This mod was created using SoulsFormats and Paramdex by TKGP and others. [Source on GitHub](https://github.com/thefifthmatt/EldenConvenienceMod). See the [modding wiki](http://soulsmodding.wikidot.com/) and join ?ServerName? to learn more.

If you have ideas for convenience mods to implement, please suggest them. Keep in mind these mods are meant to skip busywork, not allow the player to make more progress than would otherwise be easily possible.
