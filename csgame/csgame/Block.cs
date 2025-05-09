using Microsoft.Xna.Framework;

namespace csgame {
    class Block {

        public Vector3 position { get; set; }
        public Vector3 size { get; private set; }

        public string tex { get; private set; }

        public float cLX { get; private set; }
        public float cHX { get; private set; }
        public float cLY { get; private set; }
        public float cHY { get; private set; }

        public Block(Vector3 pos, Vector3 scale, string tex) {
            position = pos;
            size = scale;
            this.tex = tex;
            switch (tex) {
                case "grass":
                    cLX = 0.0f;
                    cLY = 0.0f;
                    cHX = 0.1f;
                    cHY = 0.1f;
                    break;
                case "dirt":
                    cLX = 0.1f;
                    cLY = 0.0f;
                    cHX = 0.2f;
                    cHY = 0.1f;
                    break;
                case "stone":
                    cLX = 0.2f;
                    cLY = 0.0f;
                    cHX = 0.3f;
                    cHY = 0.1f;
                    break;
            }
        }

    }
}
