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
            double distance = 0;
            int maxDist = 20;

            for (int x = -256; x < 256; x++)
            {
                for (int y = -256; y < 256; y++)
                {
                    Vector3 screenpoint = new Vector3(x / 256f, y / 256f, cam.fov);
                    Vector3 direction = Vector3.Normalize(screenpoint - cam.position);
                    
                    float shortestDistance = 1000;//100 should be replaced with the length limit of a ray


                    foreach (Sphere s in scene.primitives)

                    foreach (Primitive s in scene.primitives)
                    {
                        if (s is Sphere)
                        {
                            distance = Intersect(cam.position, direction, s as Sphere);
                        }
                        if (s is Plane)
                        {
                            distance = IntersectPlane(s as Plane, direction, cam.position, new Vector3(0, -1, 0));
                        }

                        if (distance != 0)
                        {
                            if (distance < shortestDistance)
                            {
                                shortestDistance = (float)distance;
                            }

                            int pixelColor;
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
                                if (s is Sphere)
                                {
                                    Vector3 shadowRay = new Vector3(point - l.position);
                                    Vector3 lightDirection = Vector3.Normalize(shadowRay);
                                    Vector3 sphereNormal = s.position - ((direction * (float)distance) + cam.position);
                                    float angle = Vector3.Dot(sphereNormal, lightDirection);

                                    if (ShadowIntersect(scene, s as Sphere, point, shadowRay))
                                    {
                                        if (angle > epsilon)
                                        {
                                            float distanceAttenuation = 1 - (1 / (shadowRay.Length * shadowRay.Length));
                                            lightSum = angle * distanceAttenuation;
                                        }
                                    }
                                }
                            }

                            double red = 255 * s.color.X * (distAtten * 0.5 + lightSum * 0.5)
                                , green = 255 * s.color.Y * (distAtten * 0.5 + lightSum * 0.5)
                                , blue = 255 * s.color.Z * (distAtten * 0.5 + lightSum * 0.5);
                            pixelColor = ((int)red * 65536) + ((int)green * 256) + ((int)blue);

                            screen.pixels[(y + 256) * screen.width + (x + 256)] = pixelColor;

                            


                            //debug lines

                            //if (y == 0)// % 64 == 0)
                            //{
                            //    screen.pixels[(int)(((int)(distance * 100) * 0.1f * -1 + 384) * screen.width) + (x + 512 + 256)] = 0xffffff;
                            //}

                        }

                        
                    }

                    Vector2 screenCam = returnScreenCoordinates(cam.position);

                    if (y == 0)
                    {
                        Vector2 screenPosition = returnScreenCoordinates(cam.position + direction * (float)shortestDistance);
                        if (screenPosition.X > (screen.width / 2) && screenPosition.X < screen.width && screenPosition.Y > 0 && screenPosition.Y < screen.height)
                        {
                            screen.pixels[(int)screenPosition.X + (int)screenPosition.Y * screen.width] = 0xffffff;
                        }

                        if (x % 64 == 0)
                        {
                            screen.Line((int)screenCam.X, (int)screenCam.Y, (int)screenPosition.X, (int)screenPosition.Y, 0xff0000);
                        }


                    }

                    for (int i = -2; i < 3; i++)
                        screen.Line((int)screenCam.X - 2, (int)screenCam.Y + i, (int)screenCam.X + 2, (int)screenCam.Y + i, 0x0000ff);
                }
            }

            //Debugging view:
            screen.Print("Ray Tracer", 2, 2, 0xffffff);
            screen.Print("Debug view", screen.width / 2 + 2, 2, 0xffffff);
            for (int i = 0; i < screen.height; i++)
                screen.pixels[screen.width / 2 + i * screen.width] = 0xffffff;
        }

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

        public float IntersectPlane(Plane p, Vector3 line, Vector3 origin, Vector3 point)
        {
            //check of ze parallel zijn
            if (Vector3.Dot(p.normal, line) != 0)
            {
                float distance = Vector3.Dot((point - origin), p.normal) / Vector3.Dot(line, p.normal);
                if (distance > 0)
                    return distance;
            }

            return 0;
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

        static public int xHalfWorldSize = 5;
        public static int xWorldSize = xHalfWorldSize * 2;
        public int xMultiplier = 512 / xWorldSize;
        static public int zHalfWorldSize = 35;
        public static int zWorldSize = zHalfWorldSize * 2;
        public int zMultiplier = 512 / zWorldSize;

        public Vector2 returnScreenCoordinates(Vector3 oldCoords)
        {
            Vector2 coords = new Vector2(oldCoords.X, oldCoords.Z);
            coords.X += xHalfWorldSize;
            coords.X *= xMultiplier;
            coords.X += 512;
            coords.Y -= zHalfWorldSize;
            coords.Y *= -zMultiplier;
            return coords;
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
