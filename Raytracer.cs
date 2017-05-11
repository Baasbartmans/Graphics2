using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using template;

namespace Template
{
    class Raytracer
    {
        public Surface screen;

        public Raytracer()
        {
            //(in raytracer moet dit)

            
        }

        public void Tick()
        {
            screen.Clear(0);

            Render();
        }

        public void Render(Camera cam, Scene scene, Surface displaySurf)
        {


            //Variables that are supposed to come from the actual camera and other classes:
            int xWorldSize = 10;
            int yWorldSize = 10;
            int zWorldSize = 10;
            int cameraX = 0;
            int cameraY = 0;
            int cameraZ = 3;


            //Debugging view:
            screen.Print("Ray Tracer", 2, 2, 0xffffff);
            screen.Print("Debug view", screen.width / 2 + 2, 2, 0xffffff);
            for (int i = 0; i < screen.height; i++)
                screen.pixels[screen.width / 2 + i * screen.width] = 0xffffff;
            screen.pixels[screen.width / 2 + ((cameraX / xWorldSize) * (screen.width / 2)) /*x <-- and z -->*/ + (cameraZ / zWorldSize) * (screen.width * screen.width)] = 0x00ff00;
        }

        public int TX(float x)
        {
            return (int)x;
        }

        public int TZ(float z)
        {
            return (int)z;
        }

    }
}
