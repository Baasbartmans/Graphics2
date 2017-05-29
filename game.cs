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

            Plane plane1 = new Plane(1, new Vector3(0, 1, 0), new Vector3(1, 1, 1), false);
            scene.primitives.Add(plane1);

            Sphere sphere1 = new Sphere(new Vector3(-1.5f, 0, 4), 1, new Vector3(0.5f, 1, 1), true);
           scene.primitives.Add(sphere1);

            Sphere sphere3 = new Sphere(new Vector3(1.5f, 0, 4), 1, new Vector3(1, 0.5f, 1), true);
            scene.primitives.Add(sphere3);

            Sphere sphere2 = new Sphere(new Vector3(0, 0, 2), 1, new Vector3(1, 1, 0.5f), true);
            scene.primitives.Add(sphere2);




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
                cam.position.X -= 0.1f;
                cam.updateScreen();
            }
            if (keyState.IsKeyDown(Key.Right))
            {
                cam.position.X += 0.1f;
                cam.updateScreen();
            }
        }
    }

} // namespace Template