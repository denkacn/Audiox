using System;
using UnityEngine;

namespace Audiox.Editor.Models
{
	public class Input
	{
		public Action OnMouseWheel;
		public Action<int> OnMouseDown;
		public Action<int> OnMouseDrag;
		public Action<int> OnMouseUp;
		public Action<int> OnRepaint;

		public void Update()
		{
			var controlId = GUIUtility.GetControlID(FocusType.Passive);
			switch (Event.current.GetTypeForControl(controlId))
			{
				case EventType.ScrollWheel:
					if (OnMouseWheel != null)
					{
						OnMouseWheel();
					}
					break;
				case EventType.MouseDown:
					if (OnMouseDown != null)
					{
						OnMouseDown(controlId);
					}
					break;
				case EventType.MouseDrag:
					if (OnMouseDrag != null)
					{
						OnMouseDrag(controlId);
					}
					break;
				case EventType.MouseUp:
					if (OnMouseUp != null)
					{
						OnMouseUp(controlId);
					}
					break;
				case EventType.Repaint:
					if (OnRepaint != null)
					{
						OnRepaint(controlId);
					}
					break;
			}
		}
	}
}