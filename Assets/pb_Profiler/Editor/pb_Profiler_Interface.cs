using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Parabox.Debug;

public class pb_Profiler_Interface : EditorWindow
{
	/// Every other row in the times display will be drawn with this color
	Color odd_column_color = new Color(.86f, .86f, .86f, 1f);

	Color[] column_colors = new Color[]
	{
		new Color(.22f, .22f, .22f, 1f),
		new Color(.25f, .25f, .25f, 1f)
	};

	Color highlight = new Color(90/255f, 190/255f, 255/255f, 1f);

	/**
	 * Determines how the gui displays stopwatch values.
	 */
	enum Resolution
	{
		Tick = 0,
		Nanosecond = 1,
		Millisecond = 2
	}

	/// The resolution (ticks, nanoseconds, milliseconds) to display information.
	Resolution resolution = Resolution.Millisecond;

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
		EditorWindow.GetWindow<pb_Profiler_Interface>(false, "pb_Profiler", false).Show();
	}

	void OnEnable()
	{
		this.wantsMouseMove = true;
		resolution = (Resolution) EditorPrefs.GetInt("pb_Profiler.resolution", 2);
		EditorApplication.update -= Update;
		EditorApplication.update += Update;

		if(!EditorGUIUtility.isProSkin)
		{
			highlight = new Color(.3f, .3f, .3f, 1f);
			column_colors[0] = new Color(0.7607f, 0.7607f, 0.7607f, 1f);
			column_colors[1] = new Color(0.73f, 0.73f, 0.73f, 1f);
		}
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
	Vector2 scroll = Vector2.zero;
	pb_Sample hoveringSample = null;
	Rect hoveringRect = new Rect(0,0,0,0);
	bool wantsRepaint = false;
	GUIStyle chartStyle = null;
	GUIStyle entryStyle = null;

	Dictionary<string, bool> row_visibility = new Dictionary<string, bool>();

	static Event currentEvent;

	void OnGUI()
	{
		currentEvent = Event.current;

		if(chartStyle == null)
		{
			chartStyle = new GUIStyle(EditorStyles.toolbar);
			entryStyle = new GUIStyle(EditorStyles.toolbarButton);
			chartStyle.normal.background = null;
			entryStyle.normal.background = EditorGUIUtility.whiteTexture;
			chartStyle.onNormal.background = null;
			entryStyle.onNormal.background = EditorGUIUtility.whiteTexture;

			if(!EditorGUIUtility.isProSkin)
				entryStyle.normal.textColor = Color.black;
		}

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
				row_visibility.Clear();

			resolution = (Resolution) EditorGUILayout.EnumPopup("Resolution", resolution);

		GUILayout.EndHorizontal();

		// DRAW

		if(view < 0 || view >= profiles.Count)
			return;

		pb_Sample root = profiles[view].GetRootSample();

		if(root.children.Count < 1)
			return;

		DrawChart(root);

		GUILayout.BeginHorizontal();
			if(GUILayout.Button("Print"))
				UnityEngine.Debug.Log(profiles[view].ToString());

			if( GUILayout.Button("Clear", GUILayout.MaxWidth(120)) )
				profiles[view].Reset();
		GUILayout.EndHorizontal();
	}

	string TickToString(long tick)
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
			GUI.Box(hoveringRect, "", borderStyle);
		}

		GUI.backgroundColor = original;

		EditorGUILayout.EndScrollView();

		if(wantsRepaint)
		{
			wantsRepaint = false;
			Repaint();
		}
	}

	int FIELD_WIDTH = 90;

	Color color = new Color(0,0,0,1);
	const float COLOR_BLOCK_SIZE = 16f;
	const int COLOR_BLOCK_PAD = 6;

	void DrawSampleTree(pb_Sample sample) { DrawSampleTree(sample, 0, ""); }
	void DrawSampleTree(pb_Sample sample, int indent, string key_prefix)
	{
		string key = key_prefix + sample.name;
		int childCount = sample.children.Count;

		if(!row_visibility.ContainsKey(key))
			row_visibility.Add(key, true);

		GUILayout.BeginHorizontal(chartStyle);

			int n = 0;

			GUI.backgroundColor = column_colors[n++ % 2];

			GUILayout.BeginHorizontal(entryStyle);

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
			GUILayout.Label(sample.sampleCount.ToString(), 			entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
			GUI.backgroundColor = column_colors[n++ % 2];
			GUILayout.Label(sample.Percentage().ToString("F2"), 	entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
			GUI.backgroundColor = column_colors[n++ % 2];
			GUILayout.Label(TickToString(sample.average),			entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
			GUI.backgroundColor = column_colors[n++ % 2];
			GUILayout.Label(TickToString(sample.sum), 				entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
			GUI.backgroundColor = column_colors[n++ % 2];
			GUILayout.Label(TickToString(sample.min),				entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
			GUI.backgroundColor = column_colors[n++ % 2];
			GUILayout.Label(TickToString(sample.max),				entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
			GUI.backgroundColor = column_colors[n++ % 2];
			GUILayout.Label(TickToString(sample.lastSample), 		entryStyle, GUILayout.MinWidth(FIELD_WIDTH), GUILayout.MaxWidth(FIELD_WIDTH));
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

	private static GUIStyle _borderStyle;
	private static GUIStyle borderStyle
	{
		get
		{
			if(_borderStyle == null)
			{
				_borderStyle = new GUIStyle();
				_borderStyle.normal.background = Resources.Load<Texture2D>("Border");
				_borderStyle.border = new RectOffset(1,1,1,1);
			}
			return _borderStyle;
		}
	}

	private static GUIStyle _splitStyle;
	private static GUIStyle SplitStyle
	{
		get
		{
			if(_splitStyle == null)
			{
				_splitStyle = new GUIStyle();
				_splitStyle.normal.background = EditorGUIUtility.whiteTexture;
				_splitStyle.margin = new RectOffset(6,6,0,0);
			}
			return _splitStyle;
		}
	}

	/**
	 * Draw a solid color block at rect.
	 */
	public static void DrawSolidColor(Rect rect, Color col)
	{
		Color old = UnityEngine.GUI.backgroundColor;
		UnityEngine.GUI.backgroundColor = col;
		UnityEngine.GUI.Box(rect, "", SplitStyle);
		UnityEngine.GUI.backgroundColor = old;
	}
}
