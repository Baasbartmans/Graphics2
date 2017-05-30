using OpenTK;
using System;
using System.IO;
using template;
using OpenTK.Input;

namespace Template
{

    class Game
    {
        // member variables
        public Surface screen;
        public Scene scene;
        public Camera cam = new Camera();
        public Surface displaySurf;


        Raytracer raytracer;
        // initialize
        public void Init()
        {
            scene = new Scene();
            displaySurf = new Surface(0, 0);

            Light light2 = new Light(new Vector3(-3, -10, 0), new Vector3(1, 1f, 1f));
            scene.lights.Add(light2);

           //   Light light = new Light(new Vector3(-15f, -5, 0), new Vector3(1f,1, 1));
           // scene.lights.Add(light);





            Plane plane1 = new Plane(1, new Vector3(0, 1, 0), new Vector3(1, 1, 1), false);
            scene.primitives.Add(plane1);

            Plane plane2 = new Plane(5, new Vector3(-1, 0, 0), new Vector3(1, 1, 1), false);
            scene.primitives.Add(plane2);

            Plane plane3 = new Plane(5, new Vector3(1, 0, 0), new Vector3(1, 1, 1), false);
            scene.primitives.Add(plane3);

            Plane plane4 = new Plane(10, new Vector3(0, 0, 1), new Vector3(1, 1, 1), false);
            scene.primitives.Add(plane4);

            Plane plane5 = new Plane(10, new Vector3(0, 0, -1), new Vector3(1, 1, 1), false);
            scene.primitives.Add(plane5);

            Sphere sphere2 = new Sphere(new Vector3(-1.5f, 0, -2), 1, new Vector3(1, 1, 0.5f), true);
               scene.primitives.Add(sphere2);
            
             Sphere sphere1 = new Sphere(new Vector3(0, 0, 0), 1, new Vector3(0.5f, 1, 1), true, 100);
             scene.primitives.Add(sphere1);

            Sphere sphere3 = new Sphere(new Vector3(1.5f, 0, -2), 1, new Vector3(1, 0.5f, 1), true);
            scene.primitives.Add(sphere3);






            raytracer = new Raytracer(cam, scene, displaySurf);
            raytracer.screen = screen;
            raytracer.InitializeOutside();
        }
        // tick: renders one frame
        public void Tick()
        {
           // scene.lights[0].position += new Vector3(1f,0,0);

            raytracer.Tick();

            KeyboardState keystate = Keyboard.GetState();

            if (keystate.IsKeyDown(Key.Number1))
            {
                raytracer.aa = 1;
            }
            if (keystate.IsKeyDown(Key.Number2))
            {
                raytracer.aa = 2;
            }
            if (keystate.IsKeyDown(Key.Number3))
            {
                raytracer.aa = 3;
            }
            if (keystate.IsKeyDown(Key.Left))
            {
                cam.xRotation -= 8;
                if (cam.xRotation < 0)
                    cam.xRotation += 360;
                cam.UpdateScreen();
            }
            if (keystate.IsKeyDown(Key.Right))
            {
                cam.xRotation += 8;
                if (cam.xRotation > 359)
                    cam.xRotation -= 360;
                cam.UpdateScreen();
            }
            if (keystate.IsKeyDown(Key.Up))
            {
                cam.yRotation -= 8;
                if (cam.yRotation < 0)
                    cam.yRotation += 360;
                if (cam.yRotation > 90 && cam.yRotation < 270)
                    cam.yRotation = 270;
                cam.UpdateScreen();
            }
            if (keystate.IsKeyDown(Key.Down))
            {
                cam.yRotation += 8;
                if (cam.yRotation > 359)
                    cam.yRotation -= 360;
                if (cam.yRotation > 90 && cam.yRotation < 270)
                    cam.yRotation = 90;
                cam.UpdateScreen();
            }
            if (keystate.IsKeyDown(Key.W))
            {
                cam.position += 0.1f * Vector3.Normalize(new Vector3(cam.direction.X, 0, cam.direction.Z));
            }
            if (keystate.IsKeyDown(Key.D))
            {
                cam.position += 0.1f * Vector3.Normalize(new Vector3(cam.right.X, 0, cam.right.Z));
            }
            if (keystate.IsKeyDown(Key.A))
            {
                cam.position += 0.1f * Vector3.Normalize(new Vector3(cam.left.X, 0, cam.left.Z));
            }
            if (keystate.IsKeyDown(Key.S))
            {
                cam.position -= 0.1f * Vector3.Normalize(new Vector3(cam.direction.X, 0, cam.direction.Z));
            }
            if (keystate.IsKeyDown(Key.Space))
            {
                cam.position.Y -= 0.15f;
            }
            if (keystate.IsKeyDown(Key.ShiftLeft))
            {
                cam.position.Y += 0.15f;
            }
            if (keystate.IsKeyDown(Key.R))
            {
                cam.Initialize();
            }
            if (keystate.IsKeyDown(Key.F))
            {
                if (cam.fov < 179)
                {
                    cam.fov++;
                    cam.UpdateScreen();
                }
            }
            if (keystate.IsKeyDown(Key.G))
            {
                if (cam.fov > 0)
                {
                    cam.fov--;
                    cam.UpdateScreen();
                }
            }
            if (keystate.IsKeyDown(Key.T))
            {
                Console.WriteLine("TestOutput");
            }
        }
    }

} // namespace Template