namespace EldenConvenienceMod
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.modpanel = new System.Windows.Forms.Panel();
            this.moddirL = new System.Windows.Forms.TextBox();
            this.moddirB = new System.Windows.Forms.Button();
            this.gameexeB = new System.Windows.Forms.Button();
            this.gameexeL = new System.Windows.Forms.TextBox();
            this.runB = new System.Windows.Forms.Button();
            this.explainL = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // modpanel
            // 
            this.modpanel.AutoScroll = true;
            this.modpanel.AutoSize = true;
            this.modpanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.modpanel.Location = new System.Drawing.Point(0, 0);
            this.modpanel.MaximumSize = new System.Drawing.Size(0, 600);
            this.modpanel.MinimumSize = new System.Drawing.Size(10, 10);
            this.modpanel.Name = "modpanel";
            this.modpanel.Size = new System.Drawing.Size(644, 10);
            this.modpanel.TabIndex = 0;
            // 
            // moddirL
            // 
            this.moddirL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.moddirL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.moddirL.Location = new System.Drawing.Point(12, 41);
            this.moddirL.Name = "moddirL";
            this.moddirL.Size = new System.Drawing.Size(426, 22);
            this.moddirL.TabIndex = 3;
            this.moddirL.TextChanged += new System.EventHandler(this.moddirL_TextChanged);
            // 
            // moddirB
            // 
            this.moddirB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.moddirB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.moddirB.Location = new System.Drawing.Point(444, 39);
            this.moddirB.Name = "moddirB";
            this.moddirB.Size = new System.Drawing.Size(192, 25);
            this.moddirB.TabIndex = 4;
            this.moddirB.Text = "Select mod directory...";
            this.moddirB.UseVisualStyleBackColor = true;
            this.moddirB.Click += new System.EventHandler(this.moddirB_Click);
            // 
            // gameexeB
            // 
            this.gameexeB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.gameexeB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gameexeB.Location = new System.Drawing.Point(444, 11);
            this.gameexeB.Name = "gameexeB";
            this.gameexeB.Size = new System.Drawing.Size(192, 25);
            this.gameexeB.TabIndex = 2;
            this.gameexeB.Text = "Select game exe...";
            this.gameexeB.UseVisualStyleBackColor = true;
            this.gameexeB.Click += new System.EventHandler(this.gameexeB_Click);
            // 
            // gameexeL
            // 
            this.gameexeL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameexeL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gameexeL.Location = new System.Drawing.Point(12, 13);
            this.gameexeL.Name = "gameexeL";
            this.gameexeL.Size = new System.Drawing.Size(426, 22);
            this.gameexeL.TabIndex = 1;
            this.gameexeL.TextChanged += new System.EventHandler(this.gameexeL_TextChanged);
            // 
            // runB
            // 
            this.runB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.runB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.runB.Location = new System.Drawing.Point(444, 68);
            this.runB.Name = "runB";
            this.runB.Size = new System.Drawing.Size(192, 25);
            this.runB.TabIndex = 5;
            this.runB.Text = "Install selected";
            this.runB.UseVisualStyleBackColor = true;
            this.runB.Click += new System.EventHandler(this.runB_Click);
            // 
            // explainL
            // 
            this.explainL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.explainL.AutoSize = true;
            this.explainL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.explainL.Location = new System.Drawing.Point(12, 99);
            this.explainL.Name = "explainL";
            this.explainL.Size = new System.Drawing.Size(520, 16);
            this.explainL.TabIndex = 6;
            this.explainL.Text = "Created by thefifthmatt. You may include these edits in other mods if you credit " +
    "this mod.";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 121);
            this.Controls.Add(this.explainL);
            this.Controls.Add(this.runB);
            this.Controls.Add(this.gameexeB);
            this.Controls.Add(this.gameexeL);
            this.Controls.Add(this.moddirB);
            this.Controls.Add(this.moddirL);
            this.Controls.Add(this.modpanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Elden Ring Convenience Mod Installer v0.2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel modpanel;
        private System.Windows.Forms.TextBox moddirL;
        private System.Windows.Forms.Button moddirB;
        private System.Windows.Forms.Button gameexeB;
        private System.Windows.Forms.TextBox gameexeL;
        private System.Windows.Forms.Button runB;
        private System.Windows.Forms.Label explainL;
    }
}
