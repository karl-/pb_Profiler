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
		private pb_Profiler profiler = null;

		void OnGUI()
		{
			int availableProfilerCount = profiles.Count;
			string[] display = new string[availableProfilerCount];
			int[] values = new int[availableProfilerCount];

			for(int i = 0; i < availableProfilerCount; i++)
			{
				display[i] = profiles[i].name;
				values[i] = i;
			}

			if(profiler == null && availableProfilerCount > 0)
			{
				profiler = profiles[0];
				sampleView.SetProfiler(profiler);
			}

			GUILayout.BeginHorizontal(EditorStyles.toolbar);

				EditorGUI.BeginChangeCheck();
					view = EditorGUILayout.IntPopup("", view, display, values, EditorStyles.toolbarDropDown);
				if(EditorGUI.EndChangeCheck())
				{
					profiler = view > -1 && view < availableProfilerCount ? profiles[view] : null;
					sampleView.SetProfiler(profiler);
				}

				GUILayout.FlexibleSpace();

				resolution = (Resolution) EditorGUILayout.EnumPopup("", resolution, EditorStyles.toolbarDropDown);

			GUILayout.EndHorizontal();


			if(profiler == null)
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label("No Profiler Loaded", ProfilerStyles.centeredGrayLabel);
			}
			else
			{
				sampleView.Draw();
			}

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
