using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;

namespace Parabox.Debug
{
	public class SampleTree : SampleView
	{
		// Every other row in the times display will be drawn with this color
		Color odd_column_color = new Color(.86f, .86f, .86f, 1f);

		Color[] column_colors = new Color[]
		{
			new Color(.22f, .22f, .22f, 1f),
			new Color(.25f, .25f, .25f, 1f)
		};

		Color highlight = new Color(90/255f, 190/255f, 255/255f, 1f);
		Vector2 scroll = Vector2.zero;
		pb_Sample hoveringSample = null;
		Rect hoveringRect = new Rect(0,0,0,0);
		Color color = new Color(0,0,0,1);
		Dictionary<string, bool> row_visibility = new Dictionary<string, bool>();

		const int FIELD_WIDTH = 90;
		const float COLOR_BLOCK_SIZE = 16f;
		const int COLOR_BLOCK_PAD = 6;

		private Event currentEvent;

		public SampleTree()
		{
			if(!EditorGUIUtility.isProSkin)
			{
				highlight = new Color(.3f, .3f, .3f, 1f);
				column_colors[0] = new Color(0.7607f, 0.7607f, 0.7607f, 1f);
				column_colors[1] = new Color(0.73f, 0.73f, 0.73f, 1f);
			}
		}

		public override void SetProfiler(pb_Profiler profiler)
		{
			base.SetProfiler(profiler);
			row_visibility.Clear();
		}

		public override void Draw()
		{
			currentEvent = Event.current;

			if(profiler == null)
			{
				// GUILayout.FlexibleSpace();
				// GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("No Profiler Loaded", EditorStyles.centeredGreyMiniLabel);
				// GUILayout.FlexibleSpace();
				// GUILayout.EndHorizontal();
				return;
			}

			pb_Sample root = profiler.GetRootSample();

			if(root.children.Count < 1)
				return;

			DrawChart(root);
		}

		void DrawChart(pb_Sample root)
		{
			Color bg = GUI.backgroundColor;

			GUILayout.BeginHorizontal(EditorStyles.toolbar);

				GUILayout.Label("Sample", EditorStyles.toolbarButton);
				GUI.backgroundColor = odd_column_color;
				GUILayout.Label("Calls", EditorStyles.toolbarButton, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = bg;
				GUILayout.Label("%", EditorStyles.toolbarButton, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = odd_column_color;
				GUILayout.Label("Avg", EditorStyles.toolbarButton, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = bg;
				GUILayout.Label("Sum", EditorStyles.toolbarButton, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = odd_column_color;
				GUILayout.Label("Min", EditorStyles.toolbarButton, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = bg;
				GUILayout.Label("Max", EditorStyles.toolbarButton, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = odd_column_color;
				GUILayout.Label("Current", EditorStyles.toolbarButton, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));

			GUILayout.EndHorizontal();

			hoveringSample = null;

			scroll = EditorGUILayout.BeginScrollView(scroll);

			Color original = GUI.backgroundColor;

			foreach(pb_Sample child in root.children)
				DrawSampleTree(child);

			if(hoveringSample != null)
			{
				GUI.backgroundColor = highlight;
				GUI.Box(hoveringRect, "", ProfilerStyles.borderStyle);
			}

			GUI.backgroundColor = original;

			EditorGUILayout.EndScrollView();
		}

		void DrawSampleTree(pb_Sample sample) { DrawSampleTree(sample, 0, ""); }
		void DrawSampleTree(pb_Sample sample, int indent, string key_prefix)
		{
			string key = key_prefix + sample.name;
			int childCount = sample.children.Count;

			if(!row_visibility.ContainsKey(key))
				row_visibility.Add(key, true);

			GUILayout.BeginHorizontal(ProfilerStyles.chartStyle);

				int n = 0;

				GUI.backgroundColor = column_colors[n++ % 2];

				GUILayout.BeginHorizontal(ProfilerStyles.entryStyle);

					GUILayout.Space(indent * (childCount > 0 ? 10 : 14));

					// don't use a Foldout control because it always eats the current event, which breaks clicking to follow stack trace
					if(childCount > 0)
						row_visibility[key] = EditorGUILayout.Toggle(row_visibility[key], EditorStyles.foldout, GUILayout.MaxWidth(14));

					GUILayout.Label(sample.name);

				GUILayout.EndHorizontal();

				Rect r = GUILayoutUtility.GetLastRect();

				color.r = sample.Percentage() / 100f;
				color.b = 1f - color.r;

				r.x = (r.width + r.x) - COLOR_BLOCK_SIZE - COLOR_BLOCK_PAD;
				r.width = COLOR_BLOCK_SIZE;
				r.y += (r.height-COLOR_BLOCK_SIZE)/2f;
				r.height = COLOR_BLOCK_SIZE;

				DrawSolidColor(r, color);

				GUI.backgroundColor = column_colors[n++ % 2];
				GUILayout.Label(sample.sampleCount.ToString(), 					ProfilerStyles.entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = column_colors[n++ % 2];
				GUILayout.Label(sample.Percentage().ToString("F2"), 			ProfilerStyles.entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = column_colors[n++ % 2];
				GUILayout.Label(ProfilerEditor.TickToString(sample.average),	ProfilerStyles.entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = column_colors[n++ % 2];
				GUILayout.Label(ProfilerEditor.TickToString(sample.sum), 		ProfilerStyles.entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = column_colors[n++ % 2];
				GUILayout.Label(ProfilerEditor.TickToString(sample.min),		ProfilerStyles.entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = column_colors[n++ % 2];
				GUILayout.Label(ProfilerEditor.TickToString(sample.max),		ProfilerStyles.entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = column_colors[n++ % 2];
				GUILayout.Label(ProfilerEditor.TickToString(sample.lastSample), ProfilerStyles.entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
				GUI.backgroundColor = column_colors[n++ % 2];

			GUILayout.EndHorizontal();

			Rect lastRect = GUILayoutUtility.GetLastRect();

			if(	(currentEvent.type == EventType.MouseDown && currentEvent.clickCount > 1) &&
				lastRect.Contains(currentEvent.mousePosition) )
			{
				StackFrame frame = sample.stackTrace.GetFrame(0);

				string filePathRel;

				if( RelativeFilePath(frame.GetFileName(), out filePathRel) )
				{
					UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(filePathRel, typeof(TextAsset));
					int lineNumber = frame.GetFileLineNumber();
					AssetDatabase.OpenAsset(obj, lineNumber);
				}
			}

			if( lastRect.Contains(currentEvent.mousePosition) )
			{
				if(hoveringSample != sample)
				{
					hoveringSample = sample;
					hoveringRect = new Rect(
						lastRect.x + 6,
						lastRect.y,
						lastRect.width - 12,
						lastRect.height + 2);
					wantsRepaint = true;
				}
			}

			if(row_visibility[key])
			{
				indent++;
				foreach(pb_Sample child in sample.children)
				{
					DrawSampleTree(child, indent, key);
				}
			}
		}

		private static bool RelativeFilePath(string filePath, out string relPath)
		{
			relPath = "";

			if(filePath == null)
				return false;

			int pathLen = filePath.Length;
			int ind = filePath.IndexOf("Assets");

			if(ind < 0)
				return false;

			relPath = filePath.Substring(ind, pathLen-ind);

			return true;
		}

		/**
		 * Draw a solid color block at rect.
		 */
		public static void DrawSolidColor(Rect rect, Color col)
		{
			Color old = UnityEngine.GUI.backgroundColor;
			UnityEngine.GUI.backgroundColor = col;
			UnityEngine.GUI.Box(rect, "", ProfilerStyles.splitStyle);
			UnityEngine.GUI.backgroundColor = old;
		}
	}
}
