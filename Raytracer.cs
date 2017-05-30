﻿using OpenTK;
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

        float maxDist = 1 / 5f;

        public float divX;
        public float divY;

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


            const float oneOverCircleAccuracy = 1 / circleAccuracy;
            const float tableMultiplier = (float)Math.PI * 2 / (circleAccuracy - 1);
            for (int i = 0; i < circleAccuracy; i++)
            {
                cosTable[i] = (float)Math.Cos(i * tableMultiplier - oneOverCircleAccuracy);
                sinTable[i] = (float)Math.Sin(i * tableMultiplier - oneOverCircleAccuracy);
            }

        }

        public void InitializeOutside()
        {
            pixelBuffer = new int[screen.height * screen.width];
            divX = 4f / screen.width;
            divY = 2f / screen.height;
        }

        public void Tick()
        {
            screen.Clear(0);

            Render(cam, scene, displaySurf);
        }




        int AugustRay(int x, int y, Vector3 screenpoint, int limit)
        {
            Vector3 direction = Vector3.Normalize(screenpoint - cam.position);
            Vector2 screenCam = returnScreenCoordinates(cam.position);
            int pixelColor = 0;
            float distAtten = 0;
            float lightSum = 0;

            float shortestDistance = 1000;//1000 should be replaced with the length limit of a ray

            //voor deze ray de kortste distance zoeken(zodat dingen achter elkaar niet verschijnen )
            float distance = 0;
            Primitive currentPrim = null;
            ClosestPrim(direction, out currentPrim, out distance);

            if (distance != 0 && currentPrim != null)
            {

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                }


                if (distance * maxDist <= 1)
                {
                    distAtten = 1 - (distance * maxDist);
                }


                Vector3 point = (distance * direction) - cam.position;

                //light and shadows
                foreach (Light l in scene.lights)
                {
                    Vector3 shadowRay = new Vector3(point - l.position);

                    Vector3 lightDirection = Vector3.Normalize(shadowRay);

                    if (currentPrim is Sphere)
                    {
                        Vector3 sphereNormal = currentPrim.position - ((direction * distance) + cam.position);
                        float angle = Vector3.Dot(sphereNormal, lightDirection);
                        if (angle > epsilon)
                        {
                            float distanceAttenuation = 1 - (1 / (shadowRay.Length * shadowRay.Length));
                            lightSum = angle * distanceAttenuation;

                        }
                    }

                    //checkt voor plane hoe de schaduwen vallen
                    if (currentPrim is Plane)
                    {
                        float angle = Vector3.Dot((currentPrim as Plane).normal, lightDirection);
                        lightSum += LightSumCalc(l, direction, distance, angle, currentPrim);
                    }
                }
                

                if (lightSum > 1)
                {
                    lightSum = 1;
                }

                if (y == 0)
                {
                    if (x % 10 == 0)
                    {
                        Vector2 screenPosition = returnScreenCoordinates(cam.position + direction * shortestDistance);
                        screen.Line((int)screenCam.X, (int)screenCam.Y, (int)screenPosition.X, (int)screenPosition.Y, 0xff0000);
                    }

                }

                double red = 255 * currentPrim.color.X * (distAtten * 0.5 + lightSum * 0.5)
                    , green = 255 * currentPrim.color.Y *(distAtten * 0.5 + lightSum * 0.5)
                    , blue = 255 * currentPrim.color.Z * (distAtten * 0.5 + lightSum * 0.5);
                pixelColor = ((int)red * 65536) + ((int)green * 256) + ((int)blue);
            }



            for (int i = -2; i < 3; i++)
            {
                screen.Line((int)screenCam.X - 2, (int)screenCam.Y + i, (int)screenCam.X + 2, (int)screenCam.Y + i, 0x0000ff);
            }

            return pixelColor;
        }

        public void Render(Camera cam, Scene scene, Surface displaySurf)
        {
            for (int x = -256; x < 256; x++)
            {
                for (int y = -256; y < 256; y++)
                {
                    pixelBuffer[(y + 256) * screen.width + (x + 256)] = AugustRay(x, y,(x * divX * cam.right) + (y * divY * cam.up) + cam.position + cam.direction, 4);//screenpoint inserted here
                }//for loop y
            }//(for loop x)


            //Debugging view:

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

            for (int u = 0; u < screen.width * screen.height; u++)
            {
                if (u % 1024 < 512)
                    screen.pixels[u] = pixelBuffer[u];
            }

            screen.Print("WASD and Arrow keys to move, R to reset, F and G to change FOV", 2, 2, 0xffffff);
            for (int i = 0; i < screen.height; i++)
                screen.pixels[screen.width / 2 + i * screen.width] = 0xffffff;

            Vector2 screenFirst = returnScreenCoordinates(cam.position + cam.left + cam.direction);
            Vector2 screenSecond = returnScreenCoordinates(cam.position + cam.right + cam.direction);

            screen.Line((int)screenFirst.X, (int)screenFirst.Y, (int)screenSecond.X, (int)screenSecond.Y, 0xffffff);//deze moet iets anders tekenen op het moment dat je omhoog kijkt ....
        }



        public void drawCircle(Vector2 position, float radius, int color)
        {
            for (int i = 0; i < circleAccuracy - 1; i++)
            {
                screen.Line((int)(radius * cosTable[i] + position.X), (int)(radius * sinTable[i] + position.Y), (int)(radius * cosTable[i + 1] + position.X), (int)(radius * sinTable[i + 1] + position.Y), color);
            }

        }

        float LightSumCalc(Light l, Vector3 direction, float distance, float angle, Primitive prim)
        {
            float lightSum = 0;

            foreach (Primitive p in scene.primitives)
            {
                if (p is Sphere && p != prim)
                {
                    Vector3 lightPoint = l.position;
                    Vector3 intersectPoint = (direction * distance) + cam.position;
                    Vector3 lineDirection = Vector3.Normalize((lightPoint - intersectPoint) + intersectPoint);

                    float check2 = Intersect(intersectPoint, lineDirection, p as Sphere);
                    if (check2 != 0)
                    {
                        return 0;
                    }
                    else
                    {
                        if (angle > epsilon)
                        {
                            float distanceAttenuation = 1 - (1 / ((lightPoint - intersectPoint).Length * (lightPoint - intersectPoint).Length));
                            lightSum = angle * distanceAttenuation;
                        }
                    }
                }
            }
            return lightSum;
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

        public bool NoShadowIntersect(Scene scene, Primitive s, Vector3 point, Vector3 shadowRay, Light l, out Primitive p)
        {
            Vector3 direction = Vector3.Normalize(l.position - point);

            foreach (Primitive snew in scene.primitives)
            {
                if (snew is Sphere)
                {
                    float length = Intersect(point, direction, snew as Sphere);
                    if (length < (l.position - point).Length && length > 0)
                    {
                        p = snew;
                        return false;
                    }
                }



                //if (snew != s)
                //{
                //    if (snew is Sphere)
                //    {
                //        if (Intersect(point,shadowRay, snew as Sphere) ==  0)
                //        {
                //            return false;
                //        }
                //    }

                //    //if (snew is Plane)
                //    //{
                //    //    if (IntersectPlane(snew as Plane, shadowRay, point, (snew as Plane).point) == 0)
                //    //    {
                //    //        return false;
                //    //    }
                //    //}
                //}
            }
            p = null;
            return true;
        }


        public float Intersect(Vector3 lineOrigin, Vector3 direction, Sphere sphere)
        {
            Vector3 difference = lineOrigin - sphere.position;

            float a = Vector3.Dot(direction, difference);

            float discriminant = (a * a - (difference).LengthSquared + (sphere.radiusSquared));

            if (discriminant >= 0)
            {
                return -Vector3.Dot(direction, lineOrigin - sphere.position) - (float)Math.Sqrt(discriminant);

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
            }
            return 0;
        }

        public float IntersectPlane(Plane p, Vector3 line, Vector3 origin, Vector3 point)
        {
            float distance = Vector3.Dot((point - origin), p.normal) / Vector3.Dot(p.normal, line) + epsilon;
            if (distance > 0)
                return distance;
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
    }


}