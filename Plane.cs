using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    class Plane : Primitive
    {
        private float distance;
        public Vector3 normal;
        public Vector3 point;

        public Plane(float distance, Vector3 normal, Vector3 color, bool reflective): base(color, reflective)
        {
            this.normal = normal;
            this.distance = distance;
            point = distance * normal;
            
        }
    }
}
