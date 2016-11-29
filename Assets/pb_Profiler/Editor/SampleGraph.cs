using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Parabox.Debug
{
	/**
	 *	Draw a graph from selected samples.
	 */
	public class SampleGraph : SampleView
	{
		const BindingFlags ALL_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		public override void SetProfiler(pb_Profiler profiler)
		{
			base.SetProfiler(profiler);
		}

		/**
		 *	Draw a visual representation of the profiler.
		 */
		public override void Draw()
		{
			Rect rect = EditorGUILayout.GetControlRect(false, 200);

			pb_Sample root = profiler.GetRootSample().children[0];

			if( Event.current.type == EventType.Repaint)
			{
				List<long> samples = root.sampleHistory;
				int count = samples.Count;
				float max = root.max;
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.AppendLine("max: " + max);
				sb.AppendLine(Event.current.type.ToString());

				MethodInfo mi = typeof(HandleUtility).GetMethod("ApplyWireMaterial", ALL_FLAGS);
				mi.Invoke(null, null);

				GL.Begin(GL.LINES);

				float x = rect.x;
				float y = (samples[0] / max) * rect.height;

				for(int i = 1; i < count; i++)
				{
					sb.AppendLine(string.Format("{0}, {1}", x, y));
					GL.Vertex3(rect.x + x, rect.y + (rect.height - y), 0f );
					x = (i / (float) (count - 1)) * rect.width;
					y = (samples[i] / max) * rect.height;
					GL.Vertex3(rect.x + x, rect.y + (rect.height - y), 0f );
				}

				GL.End();
			}

			GUILayout.Label("sample: " + root.name);
		}
	}
}
