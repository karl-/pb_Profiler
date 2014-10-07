pb_Profiler
===========

### Brief

A simple profiler class for use in Unity3d, editor or runtime.

pb_Profiler was originally developed as a debugging tool for [ProBuilder](http://www.protoolsforunity3d.com/probuilder/).

![](interface.PNG?raw=true)

Here's what a simple profile stack looks like:

	RefreshUVCoordinates: 5
	Average: 0.01987572
	Percentage: 100.00%
	Sum: 0.09937859
	|__
	     RefreshUVHighlights: 5
	     Average: 0.01875515
	     Percentage: 94.36%
	     Sum: 0.09377575
	     |__
	          GeneratePolygonCrosshatch: 8
	          Average: 0.01140726
	          Percentage: 97.32%
	          Sum: 0.09125805
	          |__
	               Allocate Texture: 8
	               Average: 3.33786E-06
	               Percentage: 0.03%
	               Sum: 2.670288E-05
	          |__
	               Fill Clear: 8
	               Average: 0.002407789
	               Percentage: 21.11%
	               Sum: 0.01926231
	          |__
	               Find Intersections: 2048
	               Average: 1.284154E-05
	               Percentage: 28.82%
	               Sum: 0.02629948
	          |__
	               Fill Color: 2048
	               Average: 5.225185E-06
	               Percentage: 11.73%
	               Sum: 0.01070118
	          |__
	               SetPixels: 8
	               Average: 0.003358722
	               Percentage: 29.44%
	               Sum: 0.02686977


### Instructions

Wrap code you wish to profile in `BeginSample("Name")` and `EndSample()` calls.  Call `ToString()` to view a tree formatted stack with timing information.  Ex:

	public void SomeMethod()
	{
		pb_Profiler profiler = new pb_Profiler();
	
		profiler.BeginSample("SomeMethod");
		
		profiler.BeginSample("Allocate Vector3");
	
			Vector3 v0 = new Vector3(	rand[i+0],
										rand[i+1],
										rand[i+2] );
			Vector3 v1 = new Vector3( 	rand[i+1],
										rand[i+2],
										rand[i+0] );
		profiler.EndSample();
	
		// Run three times for profiling's sake.
		for(int i = 0; i < 3; i++)
		{
			profiler.BeginSample("Cross Product");
			Vector3 temp = Vector3.Cross(v0, v1);
			profiler.EndSample();
		}
	
		profiler.EndSample();
	
		Debug.Log(profiler.ToString());
	}
