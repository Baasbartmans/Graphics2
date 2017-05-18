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
        public Camera cam = new Camera();
        public Scene scene = new Scene();
        public Surface displaySurf = new Surface(512, 512);

        public Raytracer()
        {


        }

        public void Tick()
        {


            Render(cam, scene, displaySurf);
        }

        //Variables that are supposed to come from the actual camera and other classes:
        static float xWorldSize = 10;
        static float xHalfWorldSize = xWorldSize / 2;
        static float xHalfScreenSize;
        static float yWorldSize = 10;
        static float yHalfWorldSize = yWorldSize / 2;
        static float zWorldSize = 10;
        static float zHalfWorldSize = zWorldSize / 2;
        static float zHalfScreenSize;
        float xMultiplier;
        float zMultiplier;


        public void Render(Camera cam, Scene scene, Surface displaySurf)
        {
            screen.Clear(0);


            xHalfScreenSize = screen.width / 2;
            xMultiplier = xHalfScreenSize / xWorldSize;//Deze wil je liever ergens anders, eenmalig een waarde geven, waar het niet elke frame hoeft.
            zHalfScreenSize = screen.height / 2;
            zMultiplier = zHalfScreenSize / zWorldSize;

            //Debugging view:

            //Text en scheiding
            screen.Print("Ray Tracer", 2, 2, 0xffffff);
            screen.Print("Debug view", screen.width / 2 + 2, 2, 0xffffff);
            for (int i = 0; i < screen.height; i++)
                screen.pixels[screen.width / 2 + i * screen.width] = 0xffffff;

            //Camera
            for(int i = 0; i < 2; i++)
                for(int j = 0; j < 2; j++)
                    screen.pixels[TXDebug(cam.position.X) + i /*x <-- and z -->*/ + (TZ(cam.position.Z) + j) * screen.width] = 0x00ff00;

            screen.Print("C", TXDebug(cam.position.X) - 5, TZ(cam.position.Z) + 7, 0x00ff00);

            //Screen... 

            //this is curretly solely based off camera location
            screen.Line(TXDebug(cam.position.X - 1), TZ(cam.position.Z - 2), TXDebug(cam.position.X + 1), TZ(cam.position.Z - 2), 0xffffff);

            //Objects are to be implemented

            //Actual rays are to be implemented
        }

        public int TXDebug(float x)
        {
            x += xHalfWorldSize;
            x *= xMultiplier;
            x += xHalfScreenSize;
            return (int)Math.Round(x);
        }

        public int TZ(float z)
        {
            z += zHalfWorldSize;
            z *= zMultiplier;
            z += zHalfScreenSize;
            return (int)z;
        }

    }
}
