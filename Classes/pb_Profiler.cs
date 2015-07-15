using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

/**
 * A simple Profiler for use with Unity editor code.  Resolution is in ticks, with methods to convert to nanosecond and millisecond.
 */
namespace Parabox.Debug {

public class pb_Profiler
{
	/// 1 billion nanoseconds per-second
	const long NSEC_TO_SEC = 1000000000;

	/// 1 million nanoseconds per-millisecond
	const long MS_TO_NSEC = 1000000;

	public static long TicksToNanosecond(long ticks)
	{
		return (NSEC_TO_SEC / Stopwatch.Frequency) * ticks;
	}

	public static long TicksToMillisecond(long ticks)
	{
		return ((NSEC_TO_SEC / Stopwatch.Frequency) * ticks) / MS_TO_NSEC;
	}

	static List<pb_Profiler> _activeProfilers = new List<pb_Profiler>();
	public static List<pb_Profiler> activeProfilers { get { return _activeProfilers; } }

	public string name { get; private set; }

	/**
	 * Constructor...
	 */
	public pb_Profiler(string name)
	{
		this.name = name;

		if(!activeProfilers.Contains(this))
			activeProfilers.Add(this);
	}
	
	/**
	 * Dee-structor.
	 */
	~pb_Profiler()
	{
		if(activeProfilers.Contains(this))
			activeProfilers.Remove(this);
	}

	pb_Sample sample = new pb_Sample("Parent", null);						///< The current sample tree.

	/**
	 * Begin a profile sample.
	 */
	public void BeginSample(string methodName)
	{
		sample = sample.Add(methodName);
	}

	/**
	 * Complete the sample.
	 */
	public void EndSample()
	{
		sample = sample.Stop();
	}

	/**
	 * Clear all the internals and start with fresh slate.
	 */
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