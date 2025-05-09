namespace BlockProperties {
    partial class PropertiesWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.pbx_Texture = new System.Windows.Forms.PictureBox();
            this.cbx_SelectedTexture = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbx_Texture)).BeginInit();
            this.SuspendLayout();
            // 
            // pbx_Texture
            // 
            this.pbx_Texture.Location = new System.Drawing.Point(12, 12);
            this.pbx_Texture.Name = "pbx_Texture";
            this.pbx_Texture.Size = new System.Drawing.Size(192, 192);
            this.pbx_Texture.TabIndex = 0;
            this.pbx_Texture.TabStop = false;
            // 
            // cbx_SelectedTexture
            // 
            this.cbx_SelectedTexture.FormattingEnabled = true;
            this.cbx_SelectedTexture.Location = new System.Drawing.Point(12, 210);
            this.cbx_SelectedTexture.Name = "cbx_SelectedTexture";
            this.cbx_SelectedTexture.Size = new System.Drawing.Size(192, 21);
            this.cbx_SelectedTexture.TabIndex = 1;
            this.cbx_SelectedTexture.SelectedIndexChanged += new System.EventHandler(this.SelectedTextureChanged);
            // 
            // PropertiesWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 383);
            this.Controls.Add(this.cbx_SelectedTexture);
            this.Controls.Add(this.pbx_Texture);
            this.Name = "PropertiesWindow";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pbx_Texture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbx_Texture;
        private System.Windows.Forms.ComboBox cbx_SelectedTexture;
    }
}

