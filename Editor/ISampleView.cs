using UnityEngine;
using System.Collections;

namespace Parabox.Debug
{

	/**
	 *	Interface for Editors of Profiler sample history.
	 */
	public abstract class SampleView
	{
		protected pb_Profiler profiler;

		public virtual void SetProfiler(pb_Profiler profiler)
		{
			this.profiler = profiler;
		}

		/**
		 *	Draw a visual representation of the profiler.
		 */
		public abstract void Draw();

		/**
		 *	Does this view need to be repainted?
		 */
		public bool wantsRepaint { get; set; }
	}
}
