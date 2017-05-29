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

        const int circleAccuracy = 110;

        float[] cosTable = new float[circleAccuracy];
        float[] sinTable = new float[circleAccuracy];

        public int[] pixelBuffer;
        
        Vector2 shadowPosition = new Vector2();//2dimensional vector that shows where to draw the shadow ray towards


        public Raytracer(Camera cam, Scene scene, Surface displaySurf)
        {

            this.cam = cam;
            this.scene = scene;
            this.displaySurf = displaySurf;

            pixelBuffer = new int[1024 * 512];

            const float oneOverCircleAccuracy = 1 / circleAccuracy;
            const float tableMultiplier = (float)Math.PI * 2 / (circleAccuracy - 1);
            for (int i = 0; i < circleAccuracy; i++)
            {
                cosTable[i] = (float)Math.Cos(i * tableMultiplier - oneOverCircleAccuracy);
                sinTable[i] = (float)Math.Sin(i * tableMultiplier - oneOverCircleAccuracy);
            }

        }

        public void Tick()
        {
            screen.Clear(0);

            Render(cam, scene, displaySurf);
        }






        public void Render(Camera cam, Scene scene, Surface displaySurf)
        {
            int maxDist = 30;

            for (int x = -256; x < 256; x++)
            {
                for (int y = -256; y < 256; y++)
                {
                    Vector3 screenpoint = new Vector3(x / 256f, y / 256f, cam.screenZ);//the point on the screen you're tracing towards
                    Vector3 direction = Vector3.Normalize(screenpoint - cam.position);

                    float shortestDistance = 1000;//1000 should be replaced with the length limit of a ray

                    //voor deze ray de kortste distance zoeken(zodat dingen achter elkaar niet verschijnen )
                    float distance = 0;
                    Primitive currentPrim = null;
                    ClosestPrim(direction, out currentPrim, out distance);

                    if (distance != 0 && currentPrim != null)
                    {
                        if (currentPrim is Sphere)
                        {
                            int g = 0;
                        }

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

                        float lightSum = 0;

                        Vector3 point = ((float)distance * direction) - cam.position;

                        //light and shadows
                        foreach (Light l in scene.lights)
                        {
                            Vector3 shadowRay = new Vector3(point - l.position);

                            Vector3 lightDirection = Vector3.Normalize(shadowRay);

                            if (currentPrim is Sphere)
                            {
                                Vector3 sphereNormal = currentPrim.position - ((direction * (float)distance) + cam.position);
                                float angle = Vector3.Dot(sphereNormal, lightDirection);

                                if (ShadowIntersect(scene, currentPrim as Sphere, point, shadowRay, l))//als hij niets raakt returnt shadowintersect true
                                {
                                    if (angle > epsilon)
                                    {
                                        float distanceAttenuation = 1 - (1 / (shadowRay.Length * shadowRay.Length));
                                        lightSum = angle * distanceAttenuation;

                                        shadowPosition = returnScreenCoordinates(point - shadowRay);
                                    }
                                }
                            }

                            if (currentPrim is Plane)
                            {

                                float angle = Vector3.Dot((currentPrim as Plane).normal, lightDirection);

                                if (ShadowIntersect(scene, currentPrim as Plane, point, shadowRay, l))
                                {
                                    if (angle > epsilon)
                                    {
                                        float distanceAttenuation = 1 - (1 / (shadowRay.Length * shadowRay.Length));
                                        lightSum = angle * distanceAttenuation;

                                    }
                                }
                            }
                        }


                        double red = 255 * currentPrim.color.X * (distAtten * 0.5 + lightSum * 0.5)
                            , green = 255 * currentPrim.color.Y * (distAtten * 0.5 + lightSum * 0.5)
                            , blue = 255 * currentPrim.color.Z * (distAtten * 0.5 + lightSum * 0.5);
                        pixelColor = ((int)red * 65536) + ((int)green * 256) + ((int)blue);

                        pixelBuffer[(y + 256) * screen.width + (x + 256)] = pixelColor;

                    }
                //}


                Vector2 screenCam = returnScreenCoordinates(cam.position);

                if (y == 0)
                {
                    Vector2 screenPosition = returnScreenCoordinates(cam.position + direction * (float)shortestDistance);

                    if (x % 64 == 0)
                    {
                        screen.Line((int)screenCam.X, (int)screenCam.Y, (int)screenPosition.X, (int)screenPosition.Y, 0xff0000);
                        screen.Line((int)screenPosition.X, (int)screenPosition.Y, (int)shadowPosition.X, (int)shadowPosition.Y, 0xffff00);
                    }


                }

                for (int i = -2; i < 3; i++)
                    screen.Line((int)screenCam.X - 2, (int)screenCam.Y + i, (int)screenCam.X + 2, (int)screenCam.Y + i, 0x0000ff);
            }//for loop y
            }//(for loop x)

           


            foreach (Primitive s in scene.primitives)
            {
                if (s is Sphere)
                {
                    Vector2 sphereScreenPosition = returnScreenCoordinates(s.position);

                    double red = 255 * s.color.X
                                , green = 255 * s.color.Y
                                , blue = 255 * s.color.Z;
                    int pixelColor = ((int)red * 65536) + ((int)green * 256) + ((int)blue);
                    
                    drawCircle(sphereScreenPosition, (s as Sphere).radius * zMultiplier, pixelColor);
                }
            }


            for(int u = 0; u < screen.width * screen.height; u++) {
                if(u % 1024 < 512)
                    screen.pixels[u] = pixelBuffer[u];
            }

            //Debugging view:
            screen.Print("Ray Tracer", 2, 2, 0xffffff);
            screen.Print("Debug view", screen.width / 2 + 2, 2, 0xffffff);
            for (int i = 0; i < screen.height; i++)
                screen.pixels[screen.width / 2 + i * screen.width] = 0xffffff;
            Vector2 screenFirst = returnScreenCoordinates(cam.screen[0]);
            Vector2 screenSecond = returnScreenCoordinates(cam.screen[1]);
            screen.Line((int)screenFirst.X, (int)screenFirst.Y, (int)screenSecond.X, (int)screenSecond.Y, 0xffffff);
        }



        public void drawCircle(Vector2 position, float radius, int color)
        {
            for (int i = 0; i < circleAccuracy - 1; i ++)
            {
                screen.Line((int)(radius * cosTable[i] + position.X), (int)(radius * sinTable[i] + position.Y),(int)(radius * cosTable[i + 1] + position.X), (int)(radius * sinTable[i + 1] + position.Y), color);
            }

        }

        public void ClosestPrim(Vector3 direction, out Primitive currentPrim, out float distance)
        {
            float thisdistance = 0;
            float currentDistance = 0;
            Primitive thiscurrentPrim = null;

            foreach (Primitive s in scene.primitives)
            {
                if (s is Sphere)
                {
                    currentDistance = Intersect(cam.position, direction, s as Sphere);
                }
                else if (s is Plane)
                {
                    currentDistance = IntersectPlane(s as Plane, direction, cam.position, (s as Plane).point);
                }

                //of er is niks ingevuld, of er is een lagere waarde ingevuld
                if (currentDistance != 0)
                {
                    if (thisdistance != 0)
                    {
                        if (currentDistance < thisdistance)
                        {
                            thisdistance = currentDistance;
                            thiscurrentPrim = s;
                        }
                    }
                    if (thisdistance == 0)
                    {
                        thisdistance = currentDistance;
                        thiscurrentPrim = s;
                    }
                }
            }
            if (thiscurrentPrim != null)
            {
                currentPrim = thiscurrentPrim;
            }
            else currentPrim = null;
            if (thisdistance != 0)
            {
                distance = thisdistance;
            }
            else distance = 0;

        }

        public float Clamp(float a, int min, int max)
        {
            if (a > max)
            { a = max; }
            if (a < min)
            { a = min; }
            return a;
        }

        public bool ShadowIntersect(Scene scene, Primitive s, Vector3 point, Vector3 shadowRay, Light l)
        {
            foreach (Primitive snew in scene.primitives)
            {
                if (snew != s)
                {
                    if (snew is Sphere)
                    {
                        if (Intersect(point,shadowRay, snew as Sphere) == 0)
                        {
                            return false;
                        }
                    }

                    //if (snew is Plane)
                    //{
                    //    if (IntersectPlane(snew as Plane, shadowRay, point, (snew as Plane).point) == 0)
                    //    {
                    //        return false;
                    //    }
                    //}
                }
            }
            return true;
        }


        public float Intersect(Vector3 lineOrigin, Vector3 direction, Sphere sphere)
        {
            float returnValue;

            float discriminant = (Vector3.Dot(direction, lineOrigin - sphere.position) * Vector3.Dot(direction, lineOrigin - sphere.position)) - (lineOrigin - sphere.position).LengthSquared + (sphere.radius * sphere.radius);
            if (discriminant >= 0)
            {
                


                float x1 = (-1 * Vector3.Dot(direction, lineOrigin - sphere.position)) + (float)Math.Sqrt(discriminant);
                float x2 = (-1 * Vector3.Dot(direction, lineOrigin - sphere.position)) - (float)Math.Sqrt(discriminant);

                if (x1 < x2) returnValue = x1;
                else returnValue = x2;

                //if (sphere.reflective)
                //{

                //    Vector3 newOrigin = (direction * returnValue) + cam.position;
                //    Vector3 sphereNormal = Vector3.Normalize(sphere.position - ((direction * (float)returnValue) + cam.position));
                //    // van https://stackoverflow.com/questions/573084/how-to-calculate-bounce-angle
                //    Vector3 u = (Vector3.Dot(direction, sphereNormal) / Vector3.Dot(sphereNormal, sphereNormal)) * sphereNormal;
                //    Vector3 w = direction - u;
                //    Vector3 newDirection = Vector3.Normalize(w - u);
                //    foreach (Primitive p in scene.primitives)
                //    {
                //        if (p is Sphere) return Intersect(newOrigin, newDirection, p as Sphere);
                //        if (p is Plane) return IntersectPlane(p as Plane, newDirection, newOrigin, (p as Plane).point);
                //    }

                //}

                return returnValue;
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
        static public int zHalfWorldSize = 5;
        public static int zWorldSize = zHalfWorldSize * 2;
        public int zMultiplier = 512 / zWorldSize;

        public Vector2 returnScreenCoordinates(Vector3 oldCoords)
        {
            Vector2 coords = new Vector2(oldCoords.X, oldCoords.Z);
            coords.X += xHalfWorldSize;
            coords.Y -= zHalfWorldSize;

            coords.X *= xMultiplier;
            coords.X += 512;
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
