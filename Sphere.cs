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
        public float visRadius;
        public float radiusSquared;

        public Sphere(Vector3 position, float radius, Vector3 color, bool reflective, float percent = 100): base( color, reflective, percent)
        {
            base.position = position;
            this.radius = radius;
            this.radiusSquared = radius * radius;
            base.percent =percent / 100f;
        }
    }
}
