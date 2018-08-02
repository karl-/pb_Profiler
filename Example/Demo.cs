#define PROFILE_TIMES

using UnityEngine;
using UnityEditor;
using System.Collections;
using Parabox.Debug;

public class Demo : Editor
{
	const int SAMPLE_COUNT = 300;

	[MenuItem("Tools/pb_Profiler Demo")]
	public static void Run()
	{
		// Run some intensive calculations an time them.

		pb_Profiler profiler = new pb_Profiler("Demo");
		float[] rand = new float[SAMPLE_COUNT];
		bool superBreak = false;

		profiler.BeginSample("Run CPU Intensive Tasks");

		profiler.BeginSample("Generate Random Numbers");
		{
			for(int i = 0; i < SAMPLE_COUNT; i++)
			{
				profiler.BeginSample("Show Progress Bar");
				if( EditorUtility.DisplayCancelableProgressBar("Doing some CPU intensive tasks...", "Generating lots of random numbers.", (i/(float)SAMPLE_COUNT) / 2f) )
				{
					superBreak = true;
					break;
				}
				profiler.EndSample();

				profiler.BeginSample("Random.Range");
				rand[i] = Random.Range(0f, 1000f);
				profiler.EndSample();
			}
		}
		profiler.EndSample();


		profiler.BeginSample("Do Math with Random Numbers");
		{
			for(int i = 0; i < SAMPLE_COUNT-3; i++)
			{
				profiler.BeginSample("Show Progress Bar");
				if( EditorUtility.DisplayCancelableProgressBar("Doing some CPU intensive tasks...", "Generating lots of random numbers.", ((i/(float)SAMPLE_COUNT) / 2f) + .5f) || superBreak)
					break;
				profiler.EndSample();

				profiler.BeginSample("Allocate Vector3");
				Vector3 v0 = new Vector3(	rand[i+0],
											rand[i+1],
											rand[i+2] );
				Vector3 v1 = new Vector3( 	rand[i+1],
											rand[i+2],
											rand[i+0] );
				profiler.EndSample();

				profiler.BeginSample("Cross -> Dot Product");

					profiler.BeginSample("Cross Product");
						Vector3 f = Vector3.Cross(v0, v1);
					profiler.EndSample();

					profiler.BeginSample("Dot Product");
						f.x = Vector3.Dot(f, v0);
					profiler.EndSample();

				profiler.EndSample();

				profiler.BeginSample("Normalize");
					f = Vector3.Normalize(v0);
				profiler.EndSample();
			}
		}
		EditorUtility.ClearProgressBar();

		profiler.EndSample();	// </Do Math>

		profiler.EndSample();	// </Run CPU>

		Debug.Log(profiler.ToString());
	}
}
