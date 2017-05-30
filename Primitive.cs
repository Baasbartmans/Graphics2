using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    class Primitive
    {
        public Vector3 position;
        public Vector3 color;
        public bool reflective;
        public float percent;

        public Primitive( Vector3 color, bool reflective, float percent = 100)
        {
            this.color = color;
            this.reflective = reflective;
        }
    }
}
