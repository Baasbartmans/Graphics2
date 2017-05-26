using OpenTK;
using System;
using System.IO;
using template;

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
            //cam = new Camera();
            displaySurf = new Surface(0,0);

            //raytracer.scene = scene;
            //raytracer.cam = cam;
            //raytracer.displaySurf = displaySurf;
            Light light = new Light(new Vector3(-10, 10, 5));
            scene.lights.Add(light);

            Sphere sphere1 = new Sphere(new Vector3(-1.5f, 0, 30), 1, new Vector3(0.5f, 1, 1));
            scene.primitives.Add(sphere1);

            Sphere sphere3 = new Sphere(new Vector3(1.5f, 0, 30), 1, new Vector3(1, 0.5f, 1));
            scene.primitives.Add(sphere3);

            Sphere sphere2 = new Sphere(new Vector3(0, 0, 20), 1, new Vector3(1,1,0.5f));
            scene.primitives.Add(sphere2);

            

            raytracer = new Raytracer(cam, scene, displaySurf);
            raytracer.screen = screen;
        }
	    // tick: renders one frame
	    public void Tick()
	    {
            raytracer.Tick();		    
	    }
    }

} // namespace Template