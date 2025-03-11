using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace csgame
{
    class Block
    {

        private Vector3 position;
        private Vector3 size;

        public Block(Vector3 pos, Vector3 scale) {
            position = pos;
            size = scale;
        }

    }
}
