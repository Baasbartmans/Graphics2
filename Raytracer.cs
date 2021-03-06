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
        public int debugLines = 1;
        public float debugMod;
        public int sWidth, sHeight;
        public int sQuarterWidth, sHalfHeight;
        public int aa = 1;



        int recursionDepth = 100;

        int sphereCount;
        int planeCount;

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

            foreach (Primitive p in scene.primitives)
            {
                if (p is Sphere)
                {
                    sphereCount++;
                }
                if (p is Plane)
                {
                    planeCount++;
                }
            }
        }

        public int xHalfWorldSize;
        public int xWorldSize;
        public int xMultiplier;
        public int zHalfWorldSize;
        public int zWorldSize;
        public int zMultiplier;

        public void InitializeOutside()
        {
            sWidth = screen.width;
            sQuarterWidth = sWidth / 4;
            sHeight = screen.height;
            sHalfHeight = sHeight / 2;
            debugMod = sWidth / (debugLines * 2);
            pixelBuffer = new int[sHeight * sWidth];
            divX = 4f / sWidth;
            divY = 2f / sHeight;

            xHalfWorldSize = 5;
            xWorldSize = xHalfWorldSize * 2;
            xMultiplier = sWidth / (2 * xWorldSize);
            zHalfWorldSize = 5;
            zWorldSize = zHalfWorldSize * 2;
            zMultiplier = sHeight / zWorldSize;
        }

        public void Tick()
        {
            screen.Clear(0);

            Render(cam, scene, displaySurf);
        }


        Vector2 screenPosition;

        Vector3 AugustRay(int x, int y, Vector3 screenpoint, int limit)
        {
            bool black = true;
            Vector3 direction = Vector3.Normalize(screenpoint - cam.position);
            Vector2 screenCam = returnScreenCoordinates(cam.position);
            Vector3 pixelColor = Vector3.Zero;
            float distAtten = 0;
            Vector3 lightSum = Vector3.Zero;
            Vector3 baselightSum = Vector3.Zero;
            Vector3 baseSum = Vector3.Zero;
            Primitive basePrim = null;
            float baseDist = 0;
            Vector3 baseDir = Vector3.Zero;

            float shortestDistance = 1000;//1000 should be replaced with the length limit of a ray

            //voor deze ray de kortste distance zoeken(zodat dingen achter elkaar niet verschijnen )
            float distance = 0;
            Primitive currentPrim = null;
            ClosestPrim(cam.position, direction, 0, out currentPrim, out distance);
            basePrim = currentPrim;
            baseDist = distance;
            baseDir = direction;

           
            baselightSum = doehetnou(distance, currentPrim, direction);

            float recDist = 0;
            Vector3 recDir;
            Primitive recPrim = null;
            Vector3 origin = cam.position;
            Vector3 recOrg;

            if (currentPrim is Sphere && currentPrim.reflective)
            {

                //bounce and normal debug
                if (y == 0)
                {
                    if (x % (int)(debugMod) == 0)
                    {
                        Vector3 bounce = Bounce(direction, distance, currentPrim as Sphere, cam.position);
                        Vector2 begin = returnScreenCoordinates((distance * direction) + cam.position);
                        Vector2 eind = returnScreenCoordinates(bounce + ((distance * direction) + cam.position));

                        Vector2 kamera = returnScreenCoordinates(cam.position);

                        screen.Line((int)begin.X, (int)begin.Y, (int)eind.X, (int)eind.Y, 0xffff00);
                        screen.Line((int)kamera.X, (int)kamera.Y, (int)begin.X, (int)begin.Y, 0xffff00);
                    }

                }

                SecRay(x,y,(distance * direction) + cam.position, Bounce(direction, distance, currentPrim as Sphere, origin), recursionDepth, out recDist, out recPrim, out recDir, out recOrg);

                //if (recDist > 0)
                //    distance = recDist;

                //else
                //{
                //    lightSum += new Vector3(0.1f, 0.1f, 0.1f);
                //}
                currentPrim = recPrim;
                origin = recOrg;
                direction = recDir;
            }



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
                Vector3 basePoint = (baseDist * baseDir) - cam.position;

                if (y == 0)
                {
                    if (x % (int)(debugMod) == 0)
                    {
                        screenPosition = returnScreenCoordinates(cam.position + direction * shortestDistance);
                        screen.Line((int)screenCam.X, (int)screenCam.Y, (int)screenPosition.X, (int)screenPosition.Y, 0xff0000);
                    }

                }

                //texturing voor de reflectie
                if (currentPrim is Plane)
                {
                    int roundOff = (int)new Vector3((direction * distance) + origin).Z;
                    if (roundOff % 2 == 0) currentPrim.color = new Vector3(1, 1, 1);
                    else currentPrim.color = new Vector3(0.1f, 0.1f, 0.1f);
                }

                #region main light
                //light and shadows
                foreach (Light l in scene.lights)
                {
                    Vector3 shadowRay = new Vector3(point - l.position);
                    Vector3 baseRay = new Vector3(basePoint - l.position);

                    Vector3 lightDirection = Vector3.Normalize(shadowRay);
                    Vector3 baselightDirection = Vector3.Normalize(baseRay);

                    if (currentPrim is Sphere)
                    {
                        Vector3 sphereNormal = Vector3.Normalize(currentPrim.position - ((direction * distance) + cam.position));
                        float angle = Vector3.Dot(sphereNormal, lightDirection);
                        if (angle > epsilon)
                        {
                            float distanceAttenuation = 1 - (1 / (shadowRay.Length * shadowRay.Length));
                            // if (!currentPrim.reflective)
                            lightSum += LightSumCalc(l, direction, distance, angle, currentPrim, cam.position);
                            //Console.WriteLine(lightSum);
                            if (y == 0)
                            {
                                if (x % (int)(debugMod) == 0)
                                {
                                    screenPosition = returnScreenCoordinates(cam.position + direction * shortestDistance);
                                    Vector2 shadowRayScreenPosition = returnScreenCoordinates(point + new Vector3(-shadowRay.X, 0, -shadowRay.Z));
                                    //screen.Line((int)screenPosition.X, (int)screenPosition.Y, (int)shadowRayScreenPosition.X, (int)shadowRayScreenPosition.Y, 0x0000ff);
                                }
                            }

                        }
                    }
                    //}

                    //checkt voor plane hoe de schaduwen vallen
                    if (currentPrim is Plane)
                    {
                        float angle = Vector3.Dot((currentPrim as Plane).normal, lightDirection);
                        lightSum += LightSumCalc(l, direction, distance, angle, currentPrim, cam.position);
                    }

                    if (y == 0)
                    {
                        if (x % (int)(debugMod) == 0)
                        {
                            screenPosition = returnScreenCoordinates(cam.position + direction * shortestDistance);
                            Vector2 shadowRayScreenPosition = returnScreenCoordinates(point + new Vector3(-shadowRay.X, 0, -shadowRay.Z));
                            screen.Line((int)screenPosition.X, (int)screenPosition.Y, (int)shadowRayScreenPosition.X, (int)shadowRayScreenPosition.Y, 0x0000ff);
                        }
                    }
                }
                #endregion

                float red = 0, green = 0, blue = 0;

                red = 255 * (currentPrim.color.X * (lightSum.X * 0.5f + baselightSum.X * 0.5f))* (basePrim.color.X);
                green = 255 * (currentPrim.color.Y * (lightSum.Y * 0.5f + baselightSum.Y * 0.5f)) * (basePrim.color.Y);
                blue = 255 * (currentPrim.color.Z * (lightSum.Z * 0.5f + baselightSum.Z * 0.5f)) * (basePrim.color.Z);



                red = Clamp(red, 0, 255);
                green = Clamp(green, 0, 255);
                blue = Clamp(blue, 0, 255);


                pixelColor = new Vector3(red, green, blue);
            }


            if (y == 0)
            {
                if (x % (int)(debugMod) == 0)
                {
                    screenPosition = returnScreenCoordinates(cam.position + direction * shortestDistance);

                    Vector2 normalizedPosition = returnScreenCoordinates(cam.position + direction);
                    screen.Line((int)screenCam.X, (int)screenCam.Y, (int)screenPosition.X, (int)screenPosition.Y, 0xffff00);
                    screen.Line((int)screenCam.X, (int)screenCam.Y, (int)normalizedPosition.X, (int)normalizedPosition.Y, 0xff0000);
                }
            }



            for (int i = -2; i < 3; i++)
            {
                screen.Line((int)screenCam.X - 2, (int)screenCam.Y + i, (int)screenCam.X + 2, (int)screenCam.Y + i, 0x00ff00);
            }

            return pixelColor;
        }

        public Vector3 doehetnou(float baseDist, Primitive basePrim, Vector3 baseDir)
        {
            Vector3 baselightSum = Vector3.Zero;

            if (baseDist != 0 && basePrim != null)
            {
                //Vector3 point = (distance * direction) - cam.position;
                Vector3 basePoint = (baseDist * baseDir) - cam.position;

                #region base reflection
                //light and shadows
                if (basePrim.reflective)
                {
                    foreach (Light l in scene.lights)
                    {
                        // Vector3 shadowRay = new Vector3(point - l.position);
                        Vector3 baseRay = new Vector3(basePoint - l.position);

                        //Vector3 lightDirection = Vector3.Normalize(shadowRay);
                        Vector3 baselightDirection = Vector3.Normalize(baseRay);

                        if (basePrim is Sphere)
                        {
                            Vector3 sphereNormal = Vector3.Normalize(basePrim.position - ((baseDir * baseDist) + cam.position));
                            float angle = Vector3.Dot(sphereNormal, baselightDirection);
                            if (angle > epsilon)
                            {
                                float distanceAttenuation = 1 - (1 / (baseRay.Length * baseRay.Length));
                                // if (!currentPrim.reflective)
                                baselightSum += LightSumCalc(l, baseDir, baseDist, angle, basePrim, cam.position);
                            }
                        }
                        //}

                        //checkt voor plane hoe de schaduwen vallen
                        if (basePrim is Plane)
                        {
                            float angle = Vector3.Dot((basePrim as Plane).normal, baselightDirection);
                            baselightSum += LightSumCalc(l, baseDir, baseDist, angle, basePrim, cam.position);
                        }
                    }
                }
                #endregion

                
            }
            return baselightSum;
        }

        public void Render(Camera cam, Scene scene, Surface displaySurf)
        {
            if (aa == 2)
            {
                for (int x = -sQuarterWidth; x < sQuarterWidth; x++)
                {
                    for (int y = -sHalfHeight; y < sHalfHeight; y++)
                    {
                        Vector3 color = AugustRay(x, y, (x * divX * cam.right) + (y * divY * cam.up) + cam.position + cam.direction, 4);
                        int finalColor = (((int)color.X * 65536) + ((int)color.Y * 256) + (int)color.Z);
                        pixelBuffer[(y + sHalfHeight) * sWidth + (x + sQuarterWidth)] = finalColor;//screenpoint inserted here
                    }//for loop y
                }//(for loop x)
            }
            else if (aa == 1)
            {
                for (int x = -sQuarterWidth; x < sQuarterWidth; x += 2)
                {
                    for (int y = -sHalfHeight; y < sHalfHeight; y += 2)
                    {
                        Vector3 color = AugustRay(x, y, (x * divX * cam.right) + (y * divY * cam.up) + cam.position + cam.direction, 4);
                        int finalColor = (((int)color.X * 65536) + ((int)color.Y * 256) + (int)color.Z);

                        int bufferBuffer = finalColor;
                        pixelBuffer[(y + sHalfHeight) * sWidth + (x + sQuarterWidth)] = bufferBuffer;//
                        pixelBuffer[(y + sHalfHeight) * sWidth + (x + 1 + sQuarterWidth)] = bufferBuffer;//
                        pixelBuffer[(y + 1 + sHalfHeight) * sWidth + (x + sQuarterWidth)] = bufferBuffer;//
                        pixelBuffer[(y + 1 + sHalfHeight) * sWidth + (x + 1 + sQuarterWidth)] = bufferBuffer;//
                    }//for loop y
                }//(for loop x)
            }
            else if (aa == 3)
            {
                for (int x = -sQuarterWidth * 2; x < sQuarterWidth * 2; x += 2)
                {
                    for (int y = -sHalfHeight * 2; y < sHalfHeight * 2; y += 2)
                    {
                        float red = (AugustRay(x, y, (x * 0.5f * divX * cam.right) + (y * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).X + AugustRay(x + 1, y, ((x + 1) * 0.5f * divX * cam.right) + (y * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).X + AugustRay(x, y + 1, (x * 0.5f * divX * cam.right) + ((y + 1) * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).X + AugustRay(x + 1, y + 1, ((x + 1) * 0.5f * divX * cam.right) + ((y + 1) * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).X) * 0.25f;
                        float green = (AugustRay(x, y, (x * 0.5f * divX * cam.right) + (y * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).Y + AugustRay(x + 1, y, ((x + 1) * 0.5f * divX * cam.right) + (y * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).Y + AugustRay(x, y + 1, (x * 0.5f * divX * cam.right) + ((y + 1) * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).Y + AugustRay(x + 1, y + 1, ((x + 1) * 0.5f * divX * cam.right) + ((y + 1) * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).Y) * 0.25f;
                        float blue = (AugustRay(x, y, (x * 0.5f * divX * cam.right) + (y * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).Z + AugustRay(x + 1, y, ((x + 1) * 0.5f * divX * cam.right) + (y * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).Z + AugustRay(x, y + 1, (x * 0.5f * divX * cam.right) + ((y + 1) * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).Z + AugustRay(x + 1, y + 1, ((x + 1) * 0.5f * divX * cam.right) + ((y + 1) * 0.5f * divY * cam.up) + cam.position + cam.direction, 4).Z) * 0.25f;

                        int finalColor = (((int)red * 65536) + ((int)green * 256) + (int)blue);

                        pixelBuffer[(y / 2 + sHalfHeight) * sWidth + (x / 2 + sQuarterWidth)] = finalColor;

                        //if (x % 2 == 0 && y % 2 == 0)
                        //{
                        //    pixelBuffer[(y / 2 + sHalfHeight) * sWidth + (x / 2 + sQuarterWidth)] = 0;
                        //}

                        //int bufferBuffer = (AugustRay(x, y, (x * 0.5f * divX * cam.right) + (y * 0.5f * divY * cam.up) + cam.position + cam.direction, 4) >> 2);
                        //bufferBuffer &= 4144959;

                        //pixelBuffer[(y / 2 + sHalfHeight) * sWidth + (x / 2 + sQuarterWidth)] += bufferBuffer;
                    }//for loop y
                }//(for loop x)
            }

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

            for (int u = 0; u < sWidth * sHeight; u++)
            {
                if (u % sWidth < sHeight)
                    screen.pixels[u] = pixelBuffer[u];
            }

            screen.Print("WASD and Arrow keys to move, R to reset, F and G to change FOV", 5, 5, 0xffffff);
            screen.Print("Current FOV: " + cam.fov, sQuarterWidth * 2 + 5, 25, 0xffffff);
            screen.Print("1-2-3 to pick antialiasing.", sQuarterWidth * 2 + 5, 45, 0xffffff);
            screen.Print("x0.25, x1 and x4 respectively.", sQuarterWidth * 2 + 5, 65, 0xffffff);
            for (int i = 0; i < sHeight; i++)
                screen.pixels[sWidth / 2 + i * sWidth] = 0xffffff;

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

        Vector3 LightSumCalc(Light l, Vector3 direction, float distance, float angle, Primitive prim, Vector3 origin)
        {
            float lightSum = 0;

            foreach (Primitive p in scene.primitives)
            {
                if (p is Sphere)
                {
                    Vector3 lightPoint = l.position;
                    Vector3 intersectPoint = (direction * distance) + origin;
                    Vector3 lineDirection = Vector3.Normalize((lightPoint - intersectPoint) + intersectPoint);

                    float check2 = Intersect(intersectPoint, lineDirection, p as Sphere, 0);
                    if (check2 != 0 && check2 > 0 + epsilon)
                    {
                        return Vector3.Zero;
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
            return lightSum * l.color;
        }

        public void SecRay(int x, int y, Vector3 origin, Vector3 direction, int recDepth, out float distance, out Primitive prim, out Vector3 exitDir, out Vector3 exitOrg)
        {
            Primitive curPrim;
            float dist;
            ClosestPrim(origin, direction, recDepth, out curPrim, out dist);
            if (curPrim != null)
            {
                if (curPrim.reflective && recDepth > 0)
                {
                    SecRay(x,y,(dist * direction) + origin, Bounce(direction, dist, curPrim as Sphere, origin), recDepth - 1, out dist, out curPrim, out exitDir, out exitOrg);
                }
            }
            if (y == 0)
            {
                if (x % (int)(debugMod) == 0)
                {
                    Vector2 begin = returnScreenCoordinates((dist * direction) + cam.position);

                    Vector2 origin2 = returnScreenCoordinates(origin);
                    screen.Line((int)origin2.X, (int)origin2.Y, (int)begin.X, (int)begin.Y, 0xffff00);
                }

            }

            distance = dist;
            prim = curPrim;
            exitDir = direction;
            exitOrg = (dist * direction) + origin;
            //distance = dist;
            //prim = curPrim;
            //exitDir = direction;
            //exitOrg = (dist * direction) + origin;
        }


        public void ClosestPrim(Vector3 origin, Vector3 direction, int recDepth, out Primitive currentPrim, out float distance)
        {
            float thisdistance = 0;
            float currentDistance = 0;
            Primitive thiscurrentPrim = null;

            foreach (Primitive s in scene.primitives)
            {
                if (s is Sphere)
                {
                    currentDistance = Intersect(origin, direction, s as Sphere, recDepth);
                }
                else if (s is Plane)
                {
                    currentDistance = IntersectPlane(s as Plane, direction, origin, (s as Plane).point);
                }

                //of er is niks ingevuld, of er is een lagere waarde ingevuld
                if (currentDistance > 0)
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

        public float Intersect(Vector3 lineOrigin, Vector3 direction, Sphere sphere, int recDepth)
        {
            Vector3 difference = lineOrigin - sphere.position;

            float a = Vector3.Dot(direction, difference);

            float discriminant = (a * a - (difference).LengthSquared + (sphere.radiusSquared));

            if (discriminant >= 0)
            {
                float distance = -Vector3.Dot(direction, lineOrigin - sphere.position) - (float)Math.Sqrt(discriminant);

                return distance;
            }
            return 0;
        }

        public Vector3 Bounce(Vector3 direction, float distance, Sphere sphere, Vector3 origin)
        {
            Vector3 sphereNorm = Vector3.Normalize((direction * distance + origin) - sphere.position);
            return Vector3.Normalize(direction - (sphereNorm * (Vector3.Dot(direction, sphereNorm)) * 2));
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



        public Vector2 returnScreenCoordinates(Vector3 oldCoords)
        {
            Vector2 coords = new Vector2(oldCoords.X, oldCoords.Z);
            coords.X += xHalfWorldSize;
            coords.Y -= zHalfWorldSize;

            coords.X *= xMultiplier;
            coords.X += sWidth * 0.5f;
            coords.Y *= -zMultiplier;
            return coords;
        }
    }


}