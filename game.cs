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

            Sphere sphere1 = new Sphere(new Vector3(0, 0, 40), 0.1f);
            scene.primitives.Add(sphere1);

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