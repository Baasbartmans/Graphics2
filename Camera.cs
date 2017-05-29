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

            fov = 10;
            screenDistance = 1 / fov;//dit moet verplicht aangepast worden, fov moet werken enzo

            screenDistance = 2;

            position = new Vector3(0,0,-4);

            screenZ = position.Z + screenDistance;

            xRotation = 0;
            yRotation = 0;

            

            updateScreen();

        }

        public void updateScreen()
        {
            direction = new Vector3(sinTable[xRotation], sinTable[yRotation], cosTable[xRotation] * cosTable[yRotation]);
            Vector3 dirPos = direction + position;

            right = new Vector3(cosTable[xRotation], 0, -sinTable[xRotation]);
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
