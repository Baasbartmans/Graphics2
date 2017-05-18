using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    class Camera
    {
        public Vector3 position;
        public Vector3 direction;
        public Vector3[] screen;
        public float distance;

        public Camera()
        {
            distance = 10;
            position = new Vector3(0,0,0);
            direction = new Vector3(0, 0, 1);
            screen = new Vector3[4] {new Vector3(-1,1,distance), new Vector3(1,1,distance), new Vector3(1,-1,distance), new Vector3(-1,-1,distance) };
        }
    }
}
