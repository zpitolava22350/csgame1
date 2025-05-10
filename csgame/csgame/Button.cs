using Microsoft.Xna.Framework;

namespace csgame {
    internal class Button {

        public Vector2 Position { get; private set; }
        public Vector2 Size { get; private set; }
        public Color Color { get; private set; }
        
        public delegate void onclick();
        public onclick OnClick { get; set; }

        public Button(Vector2 pos, Vector2 size, Color color) {
            Position = pos;
            Size = size;
            Color = color;
        }

        public Button(Vector2 pos, Vector2 size, Color color, onclick clk) : this(pos, size, color) {
            OnClick = clk;
        }

        public void CheckClick(int mouseX, int mouseY) {
            if (ClickedOn(mouseX, mouseY)) {
                OnClick.Invoke();
            }
        }

        private bool ClickedOn(int mouseX, int mouseY) {
            if (mouseX >= Position.X && mouseX <= Position.X + Size.X) {
                if (mouseY >= Position.Y && mouseY <= Position.Y + Size.Y) {
                    return true;
                }
            }
            return false;
        }
    }
}