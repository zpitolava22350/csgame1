using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlockProperties {
    public partial class PropertiesWindow: Form {

        public float dx { get; set; }
        public float dy { get; set; }
        public float dz { get; set; }
        public string tex { get; set; }
        private Rectangle cropArea { get; set; }

        Bitmap TextureAtlas;

        public PropertiesWindow() {
            InitializeComponent();
        }

        private void PropertiesWindow_Load(object sender, EventArgs e) {
            switch (tex) {
                case "grass":
                    cropArea = new Rectangle(0, 0, 16, 16);
                    break;
                case "dirt":
                    cropArea = new Rectangle(16, 0, 32, 16);
                    break;
                case "stone":
                    cropArea = new Rectangle(32, 0, 48, 16);
                    break;
            }

            TextureAtlas = new Bitmap("stuff/texsheet.png");
            TextureAtlas = TextureAtlas.Clone(cropArea, TextureAtlas.PixelFormat);
            pbx_Texture.Image = TextureAtlas;
        }

        private void SelectedTextureChanged(object sender, EventArgs e) {

        }
    }
}
