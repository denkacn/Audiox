﻿using System;
using Audiox.Runtime.Tools;
using UnityEditor;
using UnityEngine;

namespace Audiox.Editor.Models
{
	[Serializable]
	public class Timeline
	{
		private const float MinSegmentWidth = 50f;
		private const float LabelWidth = 48f;
		
		private readonly Color _segmentsColor = Color.gray;
		
		public void Draw(float clipLength)
		{
			var curveRect = AudioxEditorWindow.Instance.GetCurveRect();
			var padding = curveRect.xMin;
			var fixedToolbarHeight = EditorStyles.toolbar.fixedHeight;
			var segmentOffsetX = padding;
			var halfOfHeight = fixedToolbarHeight / 2f;
			var quartOfHeight = fixedToolbarHeight / 8f;
			var bigSegmentY = fixedToolbarHeight - halfOfHeight;
			var miniSegmentY = fixedToolbarHeight - quartOfHeight;
			var time = TimeSpan.FromSeconds(clipLength);
			var totalSecondsInPixelsMin = (float) time.TotalSeconds * MinSegmentWidth;
			var segmentRatio = totalSecondsInPixelsMin / curveRect.width;
			var timeStep = GetTimeStep(segmentRatio);
			var totalSegments = (float) (time.TotalSeconds / timeStep);
			var segmentsWidth = curveRect.width / totalSegments;

			for (var i = 0; i <= totalSegments; i++)
			{
				var newTime = TimeSpan.FromSeconds(i * timeStep);
				var formattedTime = ExtraMethods.FormatTimeSpan(newTime);
				EditorGUI.LabelField(new Rect(segmentsWidth * i + segmentOffsetX, 0, LabelWidth, fixedToolbarHeight), formattedTime, EditorStyles.miniLabel);
				EditorGUI.DrawRect(new Rect(segmentsWidth * i + segmentOffsetX, bigSegmentY, 1, halfOfHeight), Color.gray);
				for (var j = 0.2f; j < 1f; j += 0.2f)
				{
					EditorGUI.DrawRect(new Rect(segmentsWidth * i + (segmentsWidth * j) + segmentOffsetX, miniSegmentY, 1, quartOfHeight), _segmentsColor);
				}
			}
		}

		private float GetTimeStep(float timeStep)
		{
			if (timeStep < 1f)
			{
				timeStep = 1f;
			}
			else if (timeStep < 5f)
			{
				timeStep = 5f;
			}
			else if (timeStep < 10f)
			{
				timeStep = 10f;
			}
			else if (timeStep < 30f)
			{
				timeStep = 30f;
			}
			else if (timeStep < 60f)
			{
				timeStep = 60f;
			}
			else if (timeStep < 120f)
			{
				timeStep = 120f;
			}
			else if (timeStep < 300f)
			{
				timeStep = 300f;
			}
			else if (timeStep < 600f)
			{
				timeStep = 600f;
			}
			else if (timeStep < 1200f)
			{
				timeStep = 1200f;
			}
			else
			{
				timeStep = 1800f;
			}
			return timeStep;
		}
	}
}