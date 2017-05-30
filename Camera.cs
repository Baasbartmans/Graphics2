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
        public int fov;
        public float screenDistance;
        public int xRotation;
        public int yRotation;

        public Vector3 right;
        public Vector3 left;
        public Vector3 up;
        public Vector3 down;

        public float[] cosTable;
        public float[] sinTable;

        const float toRads = (float)Math.PI * 2 / 360;

        public Camera()
        {
            cosTable = new float[360];
            sinTable = new float[360];
            for(int i = 0; i < 360; i++)
            {
                cosTable[i] = (float)Math.Cos(i * toRads);
                sinTable[i] = (float)Math.Sin(i * toRads);
            }

            Initialize();

        }

        public void Initialize()
        {
            fov = 110;

            //bij collision met plane, de plane evt. tekenen voor debug view

            position = new Vector3(0, 0, -4);

            xRotation = 0;
            yRotation = 0;



            UpdateScreen();
        }

        public void UpdateScreen()
        {

            direction = new Vector3(sinTable[xRotation], sinTable[yRotation], cosTable[xRotation] * cosTable[yRotation]);
            Vector3 dirPos = direction + position;

            //imaginary x = sinTable[fov / 2];
            //imaginary z = cosTable[fov / 2];
            //x en diepte van linker uiteinde van het scherm
            //als je ze beide  door de diepte deelt heb je x / diepte en 1
            screenDistance = sinTable[fov / 2] / cosTable[fov / 2];

            right = screenDistance * new Vector3(cosTable[xRotation], 0, -sinTable[xRotation]);
            left = -right;
            up = Vector3.Cross(direction, right);
            down = -up;

            screen = new Vector3[4] {
                dirPos + left + up,
                dirPos + right + up,
                dirPos + right + down,
                dirPos + left + down
            };
        }
    }
}
