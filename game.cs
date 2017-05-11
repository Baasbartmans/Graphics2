using System;
using System.IO;

namespace Template {

    class Game
    {
	    // member variables
	    public Surface screen;
        Raytracer raytracer = new Raytracer();
	    // initialize
	    public void Init()
	    {
            raytracer.screen = screen;
	    }
	    // tick: renders one frame
	    public void Tick()
	    {
            raytracer.Tick();		    
	    }
    }

} // namespace Template