pb_Profiler
===========

### Quick Start

The profiler is distributed as a package. To install in your project, copy `com.karl.profiler` into your Packages directory.

Ex,

```
MyProject
	Assets
		...
	Packages
		com.karl.profiler
		manifest.json
```

If you are using a version of Unity prior to 2018.1, install by copying the `com.karl.profiler` folder directly into the assets directory.

### Brief

A simple profiler class for use in Unity3d, editor or runtime.

pb_Profiler was originally developed as a debugging tool for [ProBuilder](http://www.protoolsforunity3d.com/probuilder/).

![](pb_profiler.png?raw=true)

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

Wrap code you want to profile in `BeginSample("Name")` and `EndSample()` calls.  Call `ToString()` to view a tree formatted stack with timing information.  All active profiler instances will also be automatically viewable in the `Window / pb_Profiler Window`.  Ex:

	[MenuItem("Window/pb_Profiler Test Quick &d")]
	static void MenuTestProfiler()
	{
		pb_Profiler profiler = new pb_Profiler("Test");

		profiler.BeginSample("test a");

		profiler.BeginSample("sleep 100ms");
		Thread.Sleep(100);
		profiler.EndSample();

		profiler.BeginSample("test b");

		profiler.BeginSample("sleep 20ms");
		Thread.Sleep(20);
		profiler.EndSample();

		profiler.BeginSample("sleep 10ms");
		Thread.Sleep(10);
		profiler.EndSample();

		profiler.BeginSample("sleep 40ms");
		Thread.Sleep(40);
		profiler.EndSample();

		profiler.BeginSample("sleep 10ms");
		Thread.Sleep(10);
		profiler.EndSample();

		profiler.EndSample();

		profiler.EndSample();
	}
