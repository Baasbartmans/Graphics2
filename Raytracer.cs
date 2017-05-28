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

        const float epsilon = 0.001f;

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
            double distance;
            int maxDist = 20;

            for (int x = -256; x < 256; x++)
            {
                for (int y = -256; y < 256; y++)
                {
                    Vector3 screenpoint = new Vector3(x / 256f, y / 256f, cam.fov);
                    Vector3 direction = Vector3.Normalize(screenpoint - cam.position);

                    foreach (Sphere s in scene.primitives)
                    {
                        distance = Intersect(cam.position, direction, s);

                        if (distance != 0)
                        {
                            int pixelColor;
                            if (distance == 9)
                            {
                                int g = 0;
                            }
                            double distAtten = 0;
                            if (distance / maxDist <= 1)
                            {
                                distAtten = 1 - distance / maxDist;
                            }
                            
                                
                            
                            //double distAtten = (DistanceAt(maxDist, (float)distance) / maxDist); // deptmap

                            float lightSum = 0;

                            Vector3 point = cam.position + ((float)distance * direction);

                            foreach (Light l in scene.lights)
                            {
                                Vector3 shadowRay = new Vector3(point - l.position);
                                Vector3 lightDirection = Vector3.Normalize(shadowRay);
                                Vector3 sphereNormal = s.position - ((direction * (float)distance) + cam.position);
                                float angle = Vector3.Dot(sphereNormal, lightDirection);

                                if (ShadowIntersect(scene, s, point, shadowRay))
                                {
                                    if (angle > epsilon)
                                    {
                                        float distanceAttenuation = 1 - (1 / (shadowRay.Length * shadowRay.Length));
                                        lightSum = angle * distanceAttenuation;
                                    }
                                }

                                

                                

                                




                            //    float distLight = Math.Abs(Intersect(point, lightDirection, s));

                            //    if (scene.primitives.Count > 1)
                            //    {
                            //        foreach (Sphere ss in scene.primitives)
                            //        {
                            //            float testDist = Math.Abs(Intersect(point, lightDirection, ss));

                            //            //checken of hij intersect met andere dingen dichterbij
                            //            if (testDist == 0 || testDist > epsilon && testDist > distLight)
                            //            {
                            //                lightSum += (1 / (distLight * distLight));
                            //            }
                            //        }
                            //    }
                            //    //dan is er uberhaubt geen intersection
                            //    else
                            //    {
                            //        lightSum += (1 / (distLight * distLight));
                            //    }
                            }
                            //if (lightSum > 1)
                            //    lightSum = 1;

                            double red = 255 * s.color.X * lightSum//(distAtten * 0.5 + lightSum * 0.5)
                                , green = 255 * s.color.Y * lightSum// (distAtten * 0.5 + lightSum * 0.5)
                                , blue = 255 * s.color.Z * lightSum;//(distAtten * 0.5 + lightSum * 0.5);
                            pixelColor = ((int)red * 65536) + ((int)green * 256) + ((int)blue);

                            screen.pixels[(y + 256) * screen.width + (x + 256)] = pixelColor;


                            //debug lines
                            if (y == 0 && x % 64 == 0)
                            {
                                for (int i = 0; i > -1000; i--)
                                {
                                    Vector3 position = (direction * i) + new Vector3(768,0,384) + cam.position;// + new Vector3(((int)cam.position.X * -1 + 384), 0, ((int)cam.position.Z + 512 + 256));
                                    if((int)(position.Z) * screen.width + (int)(position.X) < screen.pixels.Length && (int)(position.Z) * screen.width + (int)(position.X) >= 0 && (direction * i).Length < distance)
                                    screen.pixels[(int)(position.Z) * screen.width + (int)(position.X)] = 0xffffff;
                                }  
                            }

                            if (y == 0)
                            {
                                Vector3 position = (direction * (float)distance) + new Vector3(768, 0, 384);
                                screen.pixels[(int)(position.Z) * screen.width + (int)(position.X)] = 0xffffff;
                            }

                            //if (y == 0)// % 64 == 0)
                            //{
                            //    screen.pixels[(int)(((int)(distance * 100) * 0.1f * -1 + 384) * screen.width) + (x + 512 + 256)] = 0xffffff;
                            //}
                        }

                    }

                }
            }

            //draw cam
            screen.pixels[((int)cam.position.X * -1 + 384) * screen.width + ((int)cam.position.Z + 512 + 256)] = 0xffffff;
            screen.pixels[((int)cam.position.X * -1 + 384) * screen.width + ((int)cam.position.Z + 512 + 256 + 1)] = 0xffffff;
            screen.pixels[((int)cam.position.X * -1 + 384 + 1) * screen.width + ((int)cam.position.Z + 512 + 256)] = 0xffffff;
            screen.pixels[((int)cam.position.X * -1 + 384 + 1) * screen.width + ((int)cam.position.Z + 512 + 256 + 1)] = 0xffffff;

            //draw screen
            for (int i = -5; i < 5; i++)
            {
                screen.pixels[((int)cam.position.X * -1 - (int)cam.fov + 384) * screen.width + ((int)cam.position.Z + 512 + 256 + i)] = 0xffffff;
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

        //public float DistanceAt(float max, float current)
        //{
        //    if (max - current > 0) return max - current;
        //    return 0;
        //}

        public float Clamp(float a, int min, int max)
        {
            if (a > max)
            { a = max; }
            if (a < min)
            { a = min; }
            return a;
        }

        public bool ShadowIntersect(Scene scene, Sphere s, Vector3 point, Vector3 shadowRay)
        {
            foreach (Primitive snew in scene.primitives)
            {
                if (snew != s)
                {
                    if (snew is Sphere)
                    {
                        if (Intersect(point, shadowRay, snew as Sphere) == 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        public float Intersect(Vector3 lineOrigin, Vector3 direction, Sphere sphere)
        {
            float discriminant = (Vector3.Dot(direction, lineOrigin - sphere.position) * Vector3.Dot(direction, lineOrigin - sphere.position)) - (lineOrigin - sphere.position).LengthSquared + (sphere.radius * sphere.radius);
            if (discriminant >= 0)
            {

                float x1 = (-1 * Vector3.Dot(direction, lineOrigin - sphere.position)) + (float)Math.Sqrt(discriminant);
                float x2 = (-1 * Vector3.Dot(direction, lineOrigin - sphere.position)) - (float)Math.Sqrt(discriminant);

                if (x1 < x2) return x1;
                return x2;
            }
            return 0;
        }

        public float IntersectPlane(Plane p)
        {


            return 0;
        }



        //public float Intersect(Vector3 origin, Vector3 screenPoint)
        //{
        //    foreach (Sphere s in scene.primitives)
        //    {
        //        if (Formula(s, cam.position, screenPoint) != -1)
        //        {
        //            float scalar = Formula(s, cam.position, screenPoint);
        //            float length = (new Vector3(cam.position + screenPoint * scalar)).Length;
        //            float color = s.color;
        //            return 0xffffff * (1 - (length * maxDist));
        //        }


        //        //if ( != -1)
        //        //{
        //        //    //temp value, moet de cordZ nog vinden..
        //        //    float cordZ = 1;
        //        //    List<float> shadowColors = new List<float>();
        //        //    foreach (Light light in scene.lights)
        //        //    {
        //        //        shadowColors.Add(ShadowRay(new Vector3(cordX, cordY, cordZ), light));
        //        //    }
        //        //    //temp shadowColors 1 is wit, 0 is zwart, daartussen komen de kleuren terug.
        //        //    return s.color * shadowColors.Average();
        //        //}

        //        //dit doet hij niet
        //        return -1;
        //    }
        //    return -1;


        //}

        public float ShadowRay(Vector3 origin, Light light)
        {
            Vector3 sum = light.position - origin;
            float vecLength = sum.Length;
            //als ie niks anders raakt.
            return vecLength;
        }

        public float Formula(Sphere s, Vector3 origin, Vector3 direction)
        {

            Vector3 ogPS = new Vector3(origin - s.position);
            //float part1 = 0 - Vector3.Dot(direction, ogPS);
            //float discrim = (float)(Math.Pow(Vector3.Dot(direction, ogPS), 2) - Math.Pow(ogPS.Length, 2) + Math.Pow(s.radius, 2));

            float b = 2 * Vector3.Dot(direction, s.position);
            float c = -2 * Vector3.Dot(origin, s.position) + s.position.LengthSquared + origin.LengthSquared - (s.radius * s.radius);

            float discrim = (b * b - 4 * c) * 0.5f;


            if (discrim < 0)
                return -1;
            if (discrim == 0)
                return -b + (float)Math.Sqrt(discrim);
            else
            {
                float first = -b + (float)Math.Sqrt(discrim);
                float second = -b - (float)Math.Sqrt(discrim);
                if (first < second) return first;
                else return second;
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
