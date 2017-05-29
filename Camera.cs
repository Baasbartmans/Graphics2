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
        public float fov;
        public float screenDistance;
        public float screenZ;
        public int xRotation;
        public int yRotation;
        public int zRotation;

        public Camera()
        {
            fov = 10;
            screenDistance = 1 / fov;//dit moet verplicht aangepast worden, fov moet werken enzo

            screenDistance = 2;

            position = new Vector3(0,0,-4);

            screenZ = position.Z + screenDistance;

            xRotation = 0;
            yRotation = 0;

            direction = new Vector3(0, 0, 1);
            //scherm moet misschien ook nog kunnen draaien? Dat weet ik niet zeker

            updateScreen();
            
        }

        public void updateScreen()
        {
                screen = new Vector3[4] {
                new Vector3(position.X - 1, position.Y + 1, screenZ),
                new Vector3(position.X + 1, position.Y + 1, screenZ),
                new Vector3(position.X + 1, position.Y - 1, screenZ),
                new Vector3(position.X - 1, position.Y - 1, screenZ)
            };
        }
    }
}
