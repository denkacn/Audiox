using System;
using UnityEngine;

namespace Audiox.Runtime.Models
{
	[Serializable]
	public class Sample
	{
		public float StartPosition;
		public float EndPosition;
		public string Name;
		[Multiline] public string Description;
		[HideInInspector] public Rect Rectangle;

		public int Step;
	    public bool IsLastInStep;
	    
	    public Sample(float start, float end, string name = "", string description = "", int step = 0, bool isLast = false)
	    {
		    StartPosition = start;
		    EndPosition = end;
		    Name = name;
		    Description = description;
		    Step = step;
		    IsLastInStep = isLast;
	    }
		
		public float Size
		{
			get { return Mathf.Abs(StartPosition - EndPosition); }
		}

		public override bool Equals(object obj)
		{
			var sample = (Sample)obj;
			return sample != null && Name == sample.Name && Rectangle == sample.Rectangle &&
			       Description == sample.Description && EndPosition == sample.EndPosition && StartPosition == sample.StartPosition;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}