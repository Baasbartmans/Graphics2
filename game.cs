using OpenTK;
using System;
using System.IO;
using template;
using OpenTK.Input;

namespace Template {

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
            displaySurf = new Surface(0,0);

            Light light = new Light(new Vector3(-15f, -3, 0));
            scene.lights.Add(light);

            Light light2 = new Light(new Vector3(0, -1, 30));
            scene.lights.Add(light2);

            Plane plane1 = new Plane(1, new Vector3(0, 1, 0), new Vector3(1, 1, 1), false);
            scene.primitives.Add(plane1);

            Plane plane2 = new Plane(3, new Vector3(-1, 0, 0), new Vector3(1, 1, 1), false);
            scene.primitives.Add(plane2);

            Plane plane3 = new Plane(3, new Vector3(1, 0, 0), new Vector3(1, 1, 1), false);
            scene.primitives.Add(plane3);

            Plane plane4 = new Plane(50, new Vector3(0, 0, 1), new Vector3(1, 1, 1), false);
            scene.primitives.Add(plane4);

            Sphere sphere2 = new Sphere(new Vector3(0, 0, 1), 1, new Vector3(1, 1, 0.5f), false);
            scene.primitives.Add(sphere2);

            Sphere sphere1 = new Sphere(new Vector3(-1.5f, 0, 5), 1, new Vector3(0.5f, 1, 1), true);
            scene.primitives.Add(sphere1);

            Sphere sphere3 = new Sphere(new Vector3(1.5f, 0, 5), 1, new Vector3(1, 0.5f, 1), true);
            scene.primitives.Add(sphere3);






            raytracer = new Raytracer(cam, scene, displaySurf);
            raytracer.screen = screen;
        }
        // tick: renders one frame
        public void Tick()
        {
            raytracer.Tick();

            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Key.Left))
            {
                cam.xRotation -= 8;
                if (cam.xRotation < 0)
                    cam.xRotation = 359;
                cam.updateScreen();
            }
            if (keyState.IsKeyDown(Key.Right))
            {
                cam.xRotation += 8;
                if (cam.xRotation > 359)
                    cam.xRotation = 0;
                cam.updateScreen();
            }
            if (keyState.IsKeyDown(Key.Up))
            {
                cam.yRotation -= 8;
                if (cam.yRotation < 0)
                    cam.yRotation = 359;
                cam.updateScreen();
            }
            if (keyState.IsKeyDown(Key.Down))
            {
                cam.yRotation += 8;
                if (cam.yRotation > 359)
                    cam.yRotation = 0;
                cam.updateScreen();
            }
            if (keyState.IsKeyDown(Key.Space))
            {
                Console.WriteLine("Direction: " + cam.direction);
                Console.WriteLine("Screen vars: " + cam.screen[0]);
                Console.WriteLine(cam.screen[1]);
                Console.WriteLine(cam.screen[2]);
                Console.WriteLine(cam.screen[3]);
            }
            if (keyState.IsKeyDown(Key.R))
            {
                Console.WriteLine(cam.right);
            }
        }
    }

} // namespace Template