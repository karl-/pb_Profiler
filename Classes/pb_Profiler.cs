using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

/**
 * An incredibly simplistic Profiler for use with Unity editor code.
 */
namespace Parabox.Debug {

public class pb_Profiler
{
	static List<pb_Profiler> _activeProfilers = new List<pb_Profiler>();
	public static List<pb_Profiler> activeProfilers { get { return _activeProfilers; } }

	/**
	 * Constructor...
	 */
	public pb_Profiler()
	{
		if(!activeProfilers.Contains(this))
			activeProfilers.Add(this);
	}

	~pb_Profiler()
	{
		if(activeProfilers.Contains(this))
			activeProfilers.Remove(this);
	}

	pb_Sample sample = new pb_Sample("Parent", null);						///< The current sample tree.

	/**
	 * Begin a profile sample.
	 */
	// [System.Diagnostics.Conditional("PB_DEBUG")]
	public void BeginSample(string methodName)
	{
		sample = sample.Add(methodName);
	}

	/**
	 * Complete the sample.
	 */
	// [System.Diagnostics.Conditional("PB_DEBUG")]
	public void EndSample()
	{
		sample = sample.Stop();
	}

	/**
	 * Clear all the internals and start with fresh slate.
	 */
	// [System.Diagnostics.Conditional("PB_DEBUG")]
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
		pb_Sample root = GetRootSample();

		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for(int i = 0; i < root.children.Count; i++)
			sb.AppendLine(root.children[i].ToStringRecursive());

		return sb.ToString();
	}	

	/**
	 * Returns the parent sample of this profiler tree.
	 */
	public pb_Sample GetRootSample()
	{
		pb_Sample root = this.sample;

		while(root.parent != null)
		{
			root = root.parent;
		}

		return root;
	}
}
}