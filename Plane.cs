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
        private Vector3 origin;
        private float distance;
        private Vector3 normal;

        public Plane(Vector3 position, Vector3 normal, float distance, Vector3 color): base(position, color)
        {
            base.position = position;
            this.normal = normal;
            this.distance = distance;
            
        }
    }
}
