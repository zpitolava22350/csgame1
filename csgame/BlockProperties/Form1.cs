using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        Bitmap TextureAtlas;

        public PropertiesWindow() {
            InitializeComponent();

            TextureAtlas = new Bitmap("stuff/texsheet.png");
            pbx_Texture.Image = TextureAtlas;
        }

        private void SelectedTextureChanged(object sender, EventArgs e) {

        }
    }
}
