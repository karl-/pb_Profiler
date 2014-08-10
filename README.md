pb_Profiler
===========

### Brief

A simple profiler class for use in Unity3d, editor or runtime.

pb_Profiler was originally developed as a debugging tool for [ProBuilder](http://www.protoolsforunity3d.com/probuilder/).

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
