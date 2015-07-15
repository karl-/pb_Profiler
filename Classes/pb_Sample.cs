using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

// #if UNITY_EDITOR
// using UnityEditor;
// #endif

/**
 * A Sample contains information on how many times a chunk of code
 * has been called, and how long each cycle took.  Generally
 * you'd want to use the bindings in pb_Profiler instead of calling
 * anything in here directly.
 */
namespace Parabox.Debug {

public class pb_Sample
{
	/// The name provied by pb_Profiler.BeginSample(string name);
	public string name;

	/**
	 * How many concurrent instances of this sample may be active.
	 * Really only useful if threading, and I doubt any of this is 
	 * thread-safe to begin with.
	 * @todo Consider removing this?
	 */
	const int MAX_STACKED_SAMPLES = 1;

	internal int timeIndex = 0;
	internal int concurrentSamples = -1;
	Stopwatch[] times = new Stopwatch[MAX_STACKED_SAMPLES];

	public pb_Sample parent;
	public List<pb_Sample> children = new List<pb_Sample>();
	public int sampleCount { get; private set; }
	/// Total ticks elapsed in this node
	public long sum { get; private set; }
	/// Average duration of ticks per-sample invocation
	public long average { get; private set; }
	/// Max duration in ticks out of all samples
	public long max { get; private set; }
	/// Min duration in ticks out of all samples
	public long min { get; private set; }
	/// The last tallied sample elapsed ticks
    public long lastSample { get; private set; }

	public pb_Sample(string name, pb_Sample parent)
	{
		// initialize stopwatches
		for(int i = 0; i < MAX_STACKED_SAMPLES; i++)
			times[i] = new System.Diagnostics.Stopwatch();

		this.name = name;
		this.parent = parent;

		timeIndex = 0;
		concurrentSamples = 0;	// 0 because one sample is automatically started

		this.times[timeIndex].Stop();
		this.times[timeIndex].Reset();	// Stopwatch.Restart() is .NET 4 +
		this.times[timeIndex].Start();
		
		sampleCount = 0;
		sum = 0;
		average = 0;
		min = 0;
		max = 0;
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

				children[ind].times[t_timeIndex].Stop();
				children[ind].times[t_timeIndex].Reset();
				children[ind].times[t_timeIndex].Start();

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

        times[c].Stop();

		sampleCount++;

		lastSample = times[c].ElapsedTicks;

		sum += lastSample;
		
		average = sum / sampleCount;

		if (lastSample < min || sampleCount < 2)
			min = lastSample;

		if (lastSample > max)
			max = lastSample;

		return concurrentSamples < 0 ? (this.parent ?? this) : this;
	}

	/**
	 * Zero out all values and samples associated with this sample, and it's children.
	 */
	public void Clear()
	{	
		sum = 0;
		sampleCount = 0;
		average = 0;
        min = 0;
        max = 0;

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
						tabs + "Average: " + pb_Profiler.TicksToNanosecond(average) + "n\n" + 
						tabs + "Percentage: " + (first ? 100f : Percentage() ).ToString("F2") + "%\n" +
						tabs + "Sum: " + pb_Profiler.TicksToNanosecond(sum) + "n\n" +
						tabs + "Min/Max: [" + pb_Profiler.TicksToNanosecond(min) + ", " + pb_Profiler.TicksToNanosecond(max) + "]n");

		for(int i = 0; i < children.Count; i++)
		{
			sb.Append(tabs + "|__\n");
			sb.Append( children[i].ToStringRecursive(tabs + "     ", false) );
		}

		return sb.ToString();
	}

#region Math

	public float Percentage()
	{
		if(parent == null || parent.sampleCount < 1)
			return 100f;
		else
			return (sum / (parent.sum == 0 ? 1f : (float)parent.sum)) * 100f;
	}

	// http://stackoverflow.com/questions/707370/clean-efficient-algorithm-for-wrapping-integers-in-c
	int Wrap(int value, int kLowerBound, int kUpperBound)
	{
		int range_size = kUpperBound - kLowerBound + 1;

		if (value < kLowerBound)
			value += range_size * ((kLowerBound - value) / range_size + 1);

		return kLowerBound + (value - kLowerBound) % range_size;
	}
#endregion
}
}