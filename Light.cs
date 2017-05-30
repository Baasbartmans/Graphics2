using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    class Light
    {
        public Vector3 position;
        public Vector3 intensity;
        private float red, green, blue;
        public Vector3 color;

        public Light(Vector3 position, Vector3 color)
        {
            this.position = position;
            intensity = new Vector3(red, green, blue);
            this.color = color;
        }
    }
}
