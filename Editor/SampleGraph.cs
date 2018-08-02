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
	public static class SampleGraph
	{
		const BindingFlags ALL_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		static Color lineColor = new Color(90/255f, 190/255f, 255/255f, .8f);
		static Color guideColor = new Color(.5f, .5f, .5f, .3f);

		/**
		 *	Draw a visual representation of the profiler.
		 */
		public static void Draw(pb_Sample sample)
		{
			Rect rect = EditorGUILayout.GetControlRect(false, 150);
			RectOffset margin = new RectOffset(2, 2, 4, 4);
			rect.x += margin.left;
			rect.y += margin.top;
			rect.width -= margin.left + margin.right;
			rect.height -= margin.top + margin.bottom;
			Color prev_color = GUI.color;
			GUI.color = Color.gray;
			GUI.Box(rect, "", ProfilerStyles.chartBackgroundStyle);
			GUI.color = prev_color;

			if( Event.current.type == EventType.Repaint )
			{
				if(sample == null)
				{
					GUI.Label(rect, "Sample History Graph", ProfilerStyles.centeredGrayLabel);
					return;
				}

				List<long> samples = sample.sampleHistory;
				int count = samples.Count;

				if(count < 3)
				{
					GUI.Label(rect, "Too Few Samples to Graph", ProfilerStyles.centeredGrayLabel);
					return;
				}

				long min = samples[0], max = samples[0];

				for(int i = 1; i < count; i++)
				{
					if(samples[i] < min) min = samples[i];
					if(samples[i] > max) max = samples[i];
				}

				MethodInfo mi = typeof(HandleUtility).GetMethod("ApplyWireMaterial", ALL_FLAGS);
				mi.Invoke(null, null);

				GL.Begin(GL.LINES);

				GL.Color(guideColor);

				// draw guides
				GL.Vertex3(rect.x, rect.y + (rect.height * .25f), 0f);
				GL.Vertex3(rect.x + rect.width, rect.y + (rect.height * .25f), 0f);

				GL.Vertex3(rect.x, rect.y + (rect.height * .5f), 0f);
				GL.Vertex3(rect.x + rect.width, rect.y + (rect.height * .5f), 0f);

				GL.Vertex3(rect.x, rect.y + (rect.height * .75f), 0f);
				GL.Vertex3(rect.x + rect.width, rect.y + (rect.height * .75f), 0f);

				GL.Color(lineColor);

				float x = rect.x;
				float y = (samples[0] / (float) max) * rect.height;

				for(int i = 1; i < count; i++)
				{
					GL.Vertex3(rect.x + x, rect.y + (rect.height - y), 0f );
					x = (i / (float) (count - 1)) * rect.width;
					y = ((samples[i] - min) / (float) (max - min)) * rect.height;
					GL.Vertex3(rect.x + x, rect.y + (rect.height - y), 0f );
				}

				GL.End();

				GUIContent label = new GUIContent(ProfilerEditor.TickToString(min));
				float height = ProfilerStyles.chartAxisLabel.CalcHeight(label, EditorGUIUtility.currentViewWidth);
				Rect r = new Rect(rect.x + 2, ((rect.y + rect.height) - height) - 2, 200, height);
				GUI.Label(r, label, ProfilerStyles.chartAxisLabel);
				r.y = rect.y + 2;
				GUI.Label(r, ProfilerEditor.TickToString(max), ProfilerStyles.chartAxisLabel);

				label.text = sample.name;
				r.x = (rect.x + rect.width) - (ProfilerStyles.chartAxisLabel.CalcSize(label).x + 12);
				GUI.Label(r, label);
			}
		}
	}
}
