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

        public Sphere(Vector3 position, float radius): base(position)
        {
            base.position = position;
            this.radius = radius;
        }
    }
}
