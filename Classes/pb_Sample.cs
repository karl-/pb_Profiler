using UnityEngine;
using System.Collections.Generic;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

/**
 * A Sample contains information on how many times a chunk of code
 * has been called, and how long each cycle took.  Generally
 * you'd want to use the bindings in pb_Profiler instead of calling
 * anything in here directly.
 */
public class pb_Sample
{
	public string name;

	internal int timeIndex = -1;
	List<float> times = new List<float>();

	public pb_Sample parent;
	public List<pb_Sample> children = new List<pb_Sample>();
	public int sampleCount  { get { return  (times.Count-1) - timeIndex; } }

	public pb_Sample(string name, pb_Sample parent)
	{
		this.name = name;
		this.parent = parent;

		#if UNITY_EDITOR
		this.times.Add( (float)EditorApplication.timeSinceStartup );		///< @todo Use System.Diagnostics.Stopwatch instead.
		#else
		this.times.Add( (float)Time.realtimeSinceStartup );
		#endif

		timeIndex++;
	}

	/**
	 * Has the appropriate EndSample() call been made for this instance?
	 */
	public bool Complete()
	{
		return timeIndex < 0;
	}

	/**
	 * Add a sample as a child of this sample.  Automatically searches for an existing sample of the same type in
	 * children so that duplicates are now added.
	 */
	public pb_Sample Add(string name)
	{
		if(children.Count > 0)
		{
			int ind = children.FindIndex(x => x.name.Equals(name));

			if(ind > -1)
			{
				#if UNITY_EDITOR
				children[ind].times.Add( (float)EditorApplication.timeSinceStartup );
				#else
				children[ind].times.Add( (float)Time.realtimeSinceStartup );
				#endif

				children[ind].timeIndex++;

				return children[ind];
			}
		}

		children.Add( new pb_Sample(name, this) );

		return children[children.Count-1];
	}

	/**
	 * Stops the pb_Sample's timer and returns the next active sample (can be parent or same sample).
	 */
	public pb_Sample Stop()
	{
		int c = times.Count-1;

		#if UNITY_EDITOR
		times[c - timeIndex] = (float)EditorApplication.timeSinceStartup - times[c - timeIndex];
		#else
		times[c - timeIndex] = (float)Time.realtimeSinceStartup - times[c - timeIndex];
		#endif

		timeIndex--;

		return timeIndex < 0 ? (this.parent ?? this) : this;
	}

	public void Clear()
	{	
		times.Clear();
		timeIndex = -1;

		for(int i = 0; i < this.children.Count; i++)
			this.children[i].Clear();

		this.children.Clear();
	}

	/**
	 * Returns just this sample's name, average time, and total sample count.
	 */
	public override string ToString()
	{
		return this.name + ": " + Average() + "\nSamples: " + times.Count;
	}

	/**
	 * Return all children of this Sample in a Tree-esque format.
	 */
	public string ToStringRecursive()
	{
		return ToStringRecursive("", true);
	}

	private string ToStringRecursive(string tabs, bool first)
	{
		float sum = Sum();

		StringBuilder sb = new StringBuilder();
		sb.AppendLine(	tabs + this.name + ": " + times.Count + "\n" +
						tabs + "Average: " + Average() + "\n" + 
						tabs + "Percentage: " + (first ? 100f : (sum / parent.Sum()) * 100f).ToString("F2") + "%\n" + 
						tabs + "Sum: " + sum );

		for(int i = 0; i < children.Count; i++)
		{
			sb.Append(tabs + "|__\n");
			sb.Append( children[i].ToStringRecursive(tabs + "     ", false) );
		}

		return sb.ToString();
	}

#region Math	///< Could just use Linq for this, but maybe performance would be an issue?

	public float Average()
	{
		float avg = 0f;
		int completed = (times.Count-1) - timeIndex;

		for(int i = 0; i < completed; i++)
			avg += times[i];

		return avg /= (float)completed;
	}

	public float Sum()
	{
		float sum = 0f;

		for(int i = 0; i < sampleCount; i++)
			sum += times[i];

		return sum;
	}

	public float Percentage()
	{
		if(parent == null || parent.sampleCount < 1)
			return 100f;
		else
			return (Sum() / parent.Sum()) * 100f;
	}
#endregion
}