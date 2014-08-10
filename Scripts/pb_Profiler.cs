using UnityEngine;
using System.Diagnostics;
using System.Collections;
using System.Text;

/**
 * An incredibly simplistic Profiler for use with Unity editor code.
 */
public class pb_Profiler
{
	/**
	 * Constructor...
	 */
	public pb_Profiler()
	{
	}

	pb_Sample sample = new pb_Sample("Parent", null);	///< The current sample tree.

	/**
	 * Begin a profile sample.
	 */
	// [System.Diagnostics.Conditional("PROFILE_TIMES")]
	public void BeginSample(string methodName)
	{
		sample = sample.Add(methodName);
	}

	/**
	 * Complete the sample.
	 */
	// [System.Diagnostics.Conditional("PROFILE_TIMES")]
	public void EndSample()
	{
		sample = sample.Stop();
	}

	/**
	 * Clear all the internals and start with fresh slate.
	 */
	// [System.Diagnostics.Conditional("PROFILE_TIMES")]
	public void Reset()
	{
		sample = new pb_Sample("Parent", null);
	}

	/**
	 * Presents a pretty tiered string.
	 * @todo Write a nice editor interface instead o' just relying on 
	 * the ToString representation.
	 */
	public override string ToString()
	{
		pb_Sample root = sample;

		while(root.parent != null)
		{
			root = root.parent;
		}

		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for(int i = 0; i < root.children.Count; i++)
			sb.AppendLine(root.children[i].ToStringRecursive());

		return sb.ToString();
	}	
}