using System;
using System.IO;
using template;

namespace Template {

    class Game
    {
	    // member variables
	    public Surface screen;
        public Scene scene;
        public Camera cam;
        public Surface displaySurf;


        Raytracer raytracer = new Raytracer();
	    // initialize
	    public void Init()
	    {
            scene = new Scene();
            cam = new Camera();
            displaySurf = new Surface(0,0);

            raytracer.screen = screen;
            raytracer.scene = scene;
            raytracer.cam = cam;
            raytracer.displaySurf = displaySurf;

	    }
	    // tick: renders one frame
	    public void Tick()
	    {
            raytracer.Tick();		    
	    }
    }

} // namespace Template