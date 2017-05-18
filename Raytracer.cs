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
        public Surface displaySurf = new Surface(512,512);

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
        
        static float yWorldSize = 10;
        static float yHalfWorldSize = yWorldSize / 2;
        static float zWorldSize = 10;
        static float zHalfWorldSize = zWorldSize / 2;
        float xMultiplier;


        public void Render(Camera cam, Scene scene, Surface displaySurf)
        {
            screen.Clear(0);

            xMultiplier = xWorldSize / screen.width;//Deze wil je liever ergens anders, eenmalig een waarde geven, waar het niet elke frame hoeft.

            //Debugging view:

            //Text en scheiding
            screen.Print("Ray Tracer", 2, 2, 0xffffff);
            screen.Print("Debug view", screen.width / 2 + 2, 2, 0xffffff);
            for (int i = 0; i < screen.height; i++)
                screen.pixels[screen.width / 2 + i * screen.width] = 0xffffff;

            //Camera
            screen.pixels[screen.width / 2 + TX(cam.position.X) /*x <-- and z -->*/ + TZ(cam.position.Z) * screen.width] = 0x00ff00;
            Console.WriteLine(cam.position.X);
        }

        public int TX(float x)
        {
            x -= xHalfWorldSize;
            x *= xMultiplier;
            return (int)Math.Round(x);
        }

        public int TZ(float z)
        {
            return (int)z;
        }

    }
}
