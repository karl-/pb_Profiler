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
namespace Parabox.Debug {

public class pb_Sample
{
	public string name;

	const int MAX_STACKED_SAMPLES = 32;

	internal int timeIndex = 0;
	internal int concurrentSamples = -1;
	float[] times = new float[MAX_STACKED_SAMPLES];

	public pb_Sample parent;
	public List<pb_Sample> children = new List<pb_Sample>();
	public int sampleCount { get; private set; }
	public float sum { get; private set; }
	public float average { get; private set; }

	public pb_Sample(string name, pb_Sample parent)
	{
		this.name = name;
		this.parent = parent;

		timeIndex = 0;
		concurrentSamples = 0;	// 0 because one sample is automatically started

		#if UNITY_EDITOR
		this.times[timeIndex] = (float)EditorApplication.timeSinceStartup;		///< @todo Use System.Diagnostics.Stopwatch instead.
		#else
		this.times[timeIndex] = (float)Time.realtimeSinceStartup;
		#endif

		sampleCount = 0;
		sum = 0f;
		average = 0f;
	}

	/**
	 * Has the appropriate EndSample() call been made for this instance?
	 */
	public bool Complete()
	{
		return concurrentSamples < 0;
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

			// A sample with this name is already running - add this time to the concurrent
			// time array
			if(ind > -1)
			{
				int t_timeIndex = children[ind].timeIndex + 1;
				if (t_timeIndex > MAX_STACKED_SAMPLES-1)
					t_timeIndex = 0;

				#if UNITY_EDITOR
				children[ind].times[t_timeIndex] = (float)EditorApplication.timeSinceStartup;
				#else
				children[ind].times[t_timeIndex] = (float)Time.realtimeSinceStartup;
				#endif

				children[ind].timeIndex = t_timeIndex;
				children[ind].concurrentSamples++;

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
		int c = Wrap(timeIndex - concurrentSamples, 0, MAX_STACKED_SAMPLES);

		concurrentSamples--;

		#if UNITY_EDITOR
		times[c] = (float)EditorApplication.timeSinceStartup - times[c];
		#else
		times[c] = (float)Time.realtimeSinceStartup - times[c];
		#endif

		sampleCount++;
		sum += times[c];
		average = sum / sampleCount;

		return concurrentSamples < 0 ? (this.parent ?? this) : this;
	}

	// Deceptively difficult problem!
	// http://stackoverflow.com/questions/707370/clean-efficient-algorithm-for-wrapping-integers-in-c
	int Wrap(int value, int kLowerBound, int kUpperBound)
	{
		int range_size = kUpperBound - kLowerBound + 1;

		if (value < kLowerBound)
			value += range_size * ((kLowerBound - value) / range_size + 1);

		return kLowerBound + (value - kLowerBound) % range_size;
	}

	public void Clear()
	{	
		timeIndex = 0;
		concurrentSamples = -1;

		for(int i = 0; i < this.children.Count; i++)
			this.children[i].Clear();

		this.children.Clear();
	}

	/**
	 * Returns just this sample's name, average time, and total sample count.
	 */
	public override string ToString()
	{
		return this.name + ": " + average + "\nSamples: " + sampleCount;
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
		StringBuilder sb = new StringBuilder();
		sb.AppendLine(	tabs + this.name + ": " + sampleCount + "\n" +
						tabs + "Average: " + average + "\n" + 
						tabs + "Percentage: " + (first ? 100f : (sum / parent.sum) * 100f).ToString("F2") + "%\n" + 
						tabs + "Sum: " + sum );

		for(int i = 0; i < children.Count; i++)
		{
			sb.Append(tabs + "|__\n");
			sb.Append( children[i].ToStringRecursive(tabs + "     ", false) );
		}

		return sb.ToString();
	}

#region Math	///< Could just use Linq for this, but maybe performance would be an issue?

	public float Percentage()
	{
		if(parent == null || parent.sampleCount < 1)
			return 100f;
		else
			return (sum / parent.sum) * 100f;
	}
#endregion
}
}