using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    class Sphere : Primitive
    {
        public float radius;

        public Sphere(Vector3 position, float radius, Vector3 color): base(position, color)
        {
            base.position = position;
            this.radius = radius;
        }
    }
}
