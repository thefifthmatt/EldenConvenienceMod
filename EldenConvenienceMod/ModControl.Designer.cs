namespace EldenConvenienceMod
{
    partial class ModControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.name = new System.Windows.Forms.Label();
            this.installedL = new System.Windows.Forms.Label();
            this.desc = new System.Windows.Forms.Label();
            this.install = new System.Windows.Forms.RadioButton();
            this.ignore = new System.Windows.Forms.RadioButton();
            this.uninstall = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // name
            // 
            this.name.AutoSize = true;
            this.name.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.name.Location = new System.Drawing.Point(3, 9);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(48, 16);
            this.name.TabIndex = 0;
            this.name.Text = "Name";
            // 
            // installedL
            // 
            this.installedL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.installedL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.installedL.ForeColor = System.Drawing.Color.LimeGreen;
            this.installedL.Location = new System.Drawing.Point(436, 9);
            this.installedL.Name = "installedL";
            this.installedL.Size = new System.Drawing.Size(143, 16);
            this.installedL.TabIndex = 2;
            this.installedL.Text = "Installed";
            this.installedL.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // desc
            // 
            this.desc.AutoSize = true;
            this.desc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.desc.Location = new System.Drawing.Point(3, 30);
            this.desc.Name = "desc";
            this.desc.Size = new System.Drawing.Size(75, 16);
            this.desc.TabIndex = 1;
            this.desc.Text = "Description";
            // 
            // install
            // 
            this.install.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.install.AutoSize = true;
            this.install.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.install.Location = new System.Drawing.Point(120, 53);
            this.install.Name = "install";
            this.install.Size = new System.Drawing.Size(59, 20);
            this.install.TabIndex = 4;
            this.install.TabStop = true;
            this.install.Text = "Install";
            this.install.UseVisualStyleBackColor = true;
            // 
            // ignore
            // 
            this.ignore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ignore.AutoSize = true;
            this.ignore.Checked = true;
            this.ignore.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ignore.Location = new System.Drawing.Point(6, 54);
            this.ignore.Name = "ignore";
            this.ignore.Size = new System.Drawing.Size(82, 20);
            this.ignore.TabIndex = 3;
            this.ignore.TabStop = true;
            this.ignore.Text = "No action";
            this.ignore.UseVisualStyleBackColor = true;
            // 
            // uninstall
            // 
            this.uninstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.uninstall.AutoSize = true;
            this.uninstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uninstall.Location = new System.Drawing.Point(216, 53);
            this.uninstall.Name = "uninstall";
            this.uninstall.Size = new System.Drawing.Size(76, 20);
            this.uninstall.TabIndex = 5;
            this.uninstall.TabStop = true;
            this.uninstall.Text = "Uninstall";
            this.uninstall.UseVisualStyleBackColor = true;
            // 
            // ModControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uninstall);
            this.Controls.Add(this.ignore);
            this.Controls.Add(this.install);
            this.Controls.Add(this.desc);
            this.Controls.Add(this.installedL);
            this.Controls.Add(this.name);
            this.Name = "ModControl";
            this.Size = new System.Drawing.Size(600, 77);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label name;
        private System.Windows.Forms.Label installedL;
        private System.Windows.Forms.Label desc;
        private System.Windows.Forms.RadioButton install;
        private System.Windows.Forms.RadioButton ignore;
        private System.Windows.Forms.RadioButton uninstall;
    }
}
