using OpenTK;
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
        public Surface screen;// = new Surface(1024, 512);
        public Scene scene;
        public Camera cam;
        public Surface displaySurf;

        public float maxDist = (1f / 1000f);

        public float div = (1f / 256f);

        public Raytracer(Camera cam, Scene scene, Surface displaySurf)
        {

            this.cam = cam;
            this.scene = scene;
            this.displaySurf = displaySurf;

        }

        public void Tick()
        {
            screen.Clear(0);

            Render(cam, scene, displaySurf);
        }

        public void Render(Camera cam, Scene scene, Surface displaySurf)
        {
            //render loop
            for (int x = 0; x < 512; x++)
            {
                for (int y = 0; y < 512; y++)
                {
                    //trek lijn vanaf camera door punt, bekijk of die intersect met sphere
                    float col = Intersect(cam.position, new Vector3((x - 256) * div, (y - 256) * div, cam.distance));
                    if (col != -1)
                    {
                        screen.pixels[y * screen.width + x] = (int)col;
                    }
                    else
                        Console.WriteLine(col);
                }
            }


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

        public float Intersect(Vector3 origin, Vector3 screenPoint)
        {
            foreach (Sphere s in scene.primitives)
            {
                if (Formula(s, cam.position, screenPoint) != -1)
                {
                    float scalar = Formula(s, cam.position, screenPoint);
                    float length = (new Vector3(cam.position + screenPoint * scalar)).Length;
                    float color = s.color;
                    if (scalar != 0)
                    {
                        int a = 1;
                    }
                    return 0xffffff * (1 - (length * maxDist));
                }
                

                //if ( != -1)
                //{
                //    //temp value, moet de cordZ nog vinden..
                //    float cordZ = 1;
                //    List<float> shadowColors = new List<float>();
                //    foreach (Light light in scene.lights)
                //    {
                //        shadowColors.Add(ShadowRay(new Vector3(cordX, cordY, cordZ), light));
                //    }
                //    //temp shadowColors 1 is wit, 0 is zwart, daartussen komen de kleuren terug.
                //    return s.color * shadowColors.Average();
                //}

                //dit doet hij niet
                return -1;
            }
            return -1;


        }

        public float ShadowRay(Vector3 origin, Light light)
        {
            Vector3 sum = light.position - origin;
            float vecLength = sum.Length;
            //als ie niks anders raakt.
            return vecLength;
        }

        public float Formula(Sphere s, Vector3 origin, Vector3 direction)
        {
            float part1 = 0 - Vector3.Dot(direction, new Vector3(origin - s.position));
            float discrim = (float)Math.Sqrt(Math.Pow(Vector3.Dot(direction, new Vector3(origin - s.position)), 2) - Math.Pow(new Vector3(origin - s.position).Length, 2) + Math.Pow(s.radius, 2));
            if (discrim < 0) return -1;
            if (discrim == 0) return part1 + discrim;
            else
            {
                float a = part1 + discrim;
                float b = part1 - discrim;
                if (a < b) return a;
                else return b;
            }
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
