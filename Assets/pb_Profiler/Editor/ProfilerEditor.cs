using UnityEngine;
using UnityEditor;
using Parabox.Debug;
using System.Collections.Generic;

namespace Parabox.Debug
{
	public class ProfilerEditor : EditorWindow
	{
		/**
		 * Determines how the gui displays stopwatch values.
		 */
		enum Resolution
		{
			Tick = 0,
			Nanosecond = 1,
			Millisecond = 2
		}

		// The resolution (ticks, nanoseconds, milliseconds) to display information.
		static Resolution resolution = Resolution.Millisecond;

		SampleView sampleView = null;

		List<pb_Profiler> profiles
		{
			get
			{
				return pb_Profiler.activeProfilers.FindAll(x => x.GetRootSample().children.Count > 0);
			}
		}

		[MenuItem("Window/pb_Profiler")]
		public static void MenuInitProfilerWindow()
		{
			EditorWindow.GetWindow<ProfilerEditor>(false, "pb_Profiler", false).Show();
		}

		void OnEnable()
		{
			this.wantsMouseMove = true;
			resolution = (Resolution) EditorPrefs.GetInt("pb_Profiler.resolution", 2);
			EditorApplication.update -= Update;
			EditorApplication.update += Update;

			sampleView = new SampleTree();
		}

		const int UDPATE_FREQ = 1;	// 1 per frame
		int updateFreqCounter = 0;

		void Update()
		{
			if(updateFreqCounter++ > UDPATE_FREQ * 100)
			{
				updateFreqCounter = 0;
				Repaint();
			}
		}

		int view = 0;

		void OnGUI()
		{
			pb_Profiler profiler = profiles != null && profiles.Count > 0 ? profiles[view] : null;

			string[] display = new string[profiles.Count];
			int[] values = new int[display.Length];

			for(int i = 0; i < values.Length; i++)
			{
				display[i] = profiles[i].name;
				values[i] = i;
			}

			GUILayout.BeginHorizontal();

				EditorGUI.BeginChangeCheck();
					view = EditorGUILayout.IntPopup("Profiler", view, display, values);
				if(EditorGUI.EndChangeCheck())
				{
					profiler = profiles != null && profiles.Count > 0 ? profiles[view] : null;
					sampleView.SetProfiler(profiler);
				}

				resolution = (Resolution) EditorGUILayout.EnumPopup("Resolution", resolution);

			GUILayout.EndHorizontal();

			sampleView.Draw();

			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
				if(GUILayout.Button("Print"))
					UnityEngine.Debug.Log(profiler.ToString());

				if( GUILayout.Button("Clear", GUILayout.MaxWidth(120)) )
					profiler.Reset();
			GUILayout.EndHorizontal();


			if(sampleView.wantsRepaint)
			{
				sampleView.wantsRepaint = false;
				Repaint();
			}
		}

		/**
		 *	Convert sample tick to a string respecting the user-set resolution.
		 */
		public static string TickToString(long tick)
		{
			switch(resolution)
			{
				case Resolution.Nanosecond:
					return string.Format("{0} n", pb_Profiler.TicksToNanosecond(tick));

				case Resolution.Millisecond:
					return string.Format("{0} ms", pb_Profiler.TicksToMillisecond(tick));

				default:
					return tick.ToString();
			}
		}
	}
}
