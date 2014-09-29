using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class pb_Profiler_Interface : EditorWindow
{
	Color odd_column_color = new Color(.86f, .86f, .86f, 1f);

	List<pb_Profiler> profiles { get { return pb_Profiler.activeProfilers; } }

	[MenuItem("Tools/pb_Profiler Window")]
	public static void MenuInitProfilerWindow()
	{
		EditorWindow.GetWindow<pb_Profiler_Interface>(true, "pb_Profiler", true).Show();
	}

	void OnEnable()
	{
		EditorApplication.update += Update;
	}

	const int UDPATE_FREQ = 1;	// 1 per frame
	int updateFreqCounter = 0;
	void Update()
	{
		if(updateFreqCounter++ > UDPATE_FREQ * 100)
		{
			updateFreqCounter=0;
			Repaint();
		}
	}

	// int n = 0;
	int view = 0;
	Vector2 scroll = Vector2.zero;
	void OnGUI()
	{
		// odd_column_color = EditorGUILayout.ColorField("col", odd_column_color);
		// GUILayout.Label(odd_column_color.r + ", " + odd_column_color.g+ ", " + odd_column_color.b + ", " + odd_column_color.a);
		// n = EditorGUILayout.IntField("n", n);

		string[] display = new string[profiles.Count];
		int[] values = new int[display.Length];
		for(int i = 0; i < values.Length; i++)
		{
			display[i] = "Profiler: " + i;
			values[i] = i;
		}
		view = EditorGUILayout.IntPopup("Profiler", view, display, values);

		// DRAW

		if(view < 0 || view >= profiles.Count)
			return;

		pb_Sample root = profiles[view].GetRootSample();
		if(root.children.Count != 1) return;
		root = root.children[0];

		Color bg = GUI.backgroundColor;
		GUILayout.BeginHorizontal(EditorStyles.toolbar);
			EditorGUILayout.Space();
			GUILayout.Label("Sample", EditorStyles.toolbarButton, GUILayout.MinWidth(name_width-4), GUILayout.MaxWidth(name_width-4));
			GUI.backgroundColor = odd_column_color;
			GUILayout.Label("Calls", EditorStyles.toolbarButton, GUILayout.MinWidth(sample_width), GUILayout.MaxWidth(sample_width));
			GUI.backgroundColor = bg;
			GUILayout.Label("%", EditorStyles.toolbarButton, GUILayout.MinWidth(percent_width), GUILayout.MaxWidth(percent_width));
			GUI.backgroundColor = odd_column_color;
			GUILayout.Label("Avg", EditorStyles.toolbarButton, GUILayout.MinWidth(avg_width), GUILayout.MaxWidth(avg_width));
			GUI.backgroundColor = bg;
			GUILayout.Label("Sum", EditorStyles.toolbarButton, GUILayout.MinWidth(sum_width), GUILayout.MaxWidth(sum_width));

			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		scroll = EditorGUILayout.BeginScrollView(scroll);

		DrawSampleTree(root);

		EditorGUILayout.EndScrollView();
	}

	int name_width = 300;
	int sample_width = 60;
	int percent_width = 60;
	int sum_width = 60;
	int avg_width = 60;

	void DrawSampleTree(pb_Sample sample) { DrawSampleTree(sample, 0); }
	void DrawSampleTree(pb_Sample sample, int indent)
	{
		string ind = "";
		for(int i = 0; i < indent; i++)	
			ind += "\t";

		GUILayout.BeginHorizontal();
			GUILayout.Label(ind + sample.name, GUILayout.MinWidth(name_width), GUILayout.MaxWidth(name_width));

			GUILayout.Label(sample.sampleCount.ToString(), GUILayout.MinWidth(sample_width), GUILayout.MaxWidth(sample_width));
			GUILayout.Label(sample.Percentage().ToString("F4"), GUILayout.MinWidth(percent_width), GUILayout.MaxWidth(percent_width));
			GUILayout.Label(sample.Average().ToString("F4"), GUILayout.MinWidth(avg_width), GUILayout.MaxWidth(avg_width));
			GUILayout.Label(sample.Sum().ToString("F4"), GUILayout.MinWidth(sum_width), GUILayout.MaxWidth(sum_width));
		GUILayout.EndHorizontal();
	
		indent++;
		foreach(pb_Sample child in sample.children)
		{
			DrawSampleTree(child, indent);
		}
	}
}
