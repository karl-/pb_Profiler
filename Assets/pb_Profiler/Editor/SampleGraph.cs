using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Parabox.Debug
{
	public class SampleGraph : SampleView
	{
		public override void SetProfiler(pb_Profiler profiler)
		{
			base.SetProfiler(profiler);
		}

		/**
		 *	Draw a visual representation of the profiler.
		 */
		public override void Draw()
		{

		}
	}
}
