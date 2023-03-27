using System;
using System.Collections.Generic;
using Audiox.Runtime.Models;
using Audiox.Runtime.Tools;
using UnityEditor;
using UnityEngine;

namespace Audiox.Editor.Models
{
	[Serializable]
	public class Samples
	{
		[SerializeField] private List<Sample> _selectedSamples = new List<Sample>();
		
		private Sample _dragSideSample;
		private Vector2 _mousePositionDown;
		private float _dragStartPosition;
		private float _startShiftDownOffset;
		private bool _shouldDragSide;
		private bool _isDragLeft;
		private bool _isKeyAltDown;
		private bool _isDragStarted;

		private readonly Color _sampleLineColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 127f / 255f);
		private readonly Color _sampleColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 41 / 255f);
		private readonly Color _selectedColor = new Color(118 / 255f, 176 / 255f, 244 / 255f, 127 / 255f);

		public void OnMouseUp(int controlId)
		{
			if (GUIUtility.hotControl == controlId)
			{
				GUIUtility.hotControl = 0;
				if (CanSelectSamples())
				{
					OnSelectionFinished(GetRectFromSelection());
				}
				_isDragStarted = false;
				_shouldDragSide = false;
				Event.current.Use();
			}
		}

		public void OnMouseDown(float height)
		{
			_mousePositionDown = Event.current.mousePosition;
			var samples = AudioxEditorWindow.Instance.Data.Samples;
			if (!Event.current.alt && !Event.current.shift)
			{
				_selectedSamples.Clear();
			}

			foreach (var sample in samples)
			{
				var rectControlLeft = AudioxEditorWindow.Instance.GetPixelRect(sample.StartPosition);
				var rectControlRight = AudioxEditorWindow.Instance.GetPixelRect(sample.EndPosition);
				var cursorRect = new Rect(_mousePositionDown.x, _mousePositionDown.y, 2f, height);
				var overlapsLeft = rectControlLeft.Overlaps(cursorRect);
				var overlapsRight = rectControlRight.Overlaps(cursorRect);
				_shouldDragSide = overlapsLeft || overlapsRight;
				_isDragLeft = overlapsLeft;
				_dragStartPosition = Event.current.mousePosition.x;
				if (overlapsLeft || overlapsRight)
				{
					if (!_selectedSamples.Contains(sample))
					{
						_selectedSamples.Add(sample);
					}
					_dragSideSample = sample;
					break;
				}
				var width = Mathf.Abs(rectControlRight.xMin - rectControlLeft.xMin);
				var sampleHeight = rectControlRight.yMax - rectControlLeft.yMin;
				var sampleRect = new Rect(rectControlLeft.xMin, rectControlLeft.yMin, width, sampleHeight);
				if (sampleRect.Contains(Event.current.mousePosition))
				{
					if (!_selectedSamples.Contains(sample))
					{
						DeselectLayout();
						_selectedSamples.Add(sample);
					}
				}
			}
		}
		
		public void OnRepaint(int controlId)
		{
			if (controlId <= 0) 
				throw new ArgumentOutOfRangeException("controlId");
			if (GUIUtility.hotControl == controlId)
			{
				if (CanSelectSamples())
				{
					var rectSelection = GetRectFromSelection();
					if (rectSelection.size.magnitude > 0f)
					{
						GUI.skin.box.Draw(rectSelection, GUIContent.none, controlId);
					}
				}
			}
		}

		public void AddSample(float start, float end)
		{
			var data = AudioxEditorWindow.Instance.Data;
			var sampleName = "Sample " + (data.Samples.Count + 1);
			Undo.RecordObject(data, "Add Sample");
			data.Samples.Add(new Sample(start, end, sampleName));
		}

		public void SelectedLastSampleAdded()
		{
			var samples = AudioxEditorWindow.Instance.Data.Samples;
			if (samples != null && samples.Count > 0)
			{
				_dragStartPosition = Event.current.mousePosition.x;
				var sample = samples[samples.Count - 1];
				if (!_selectedSamples.Contains(sample))
				{
					DeselectLayout();
					_selectedSamples.Add(sample);
				}
			}
		}

		public List<Sample> GetSelectedSamples()
		{
			return _selectedSamples;
		}

		public int GetSelectedSamplesCount()
		{
			return _selectedSamples.Count;
		}

		public void Deselect()
		{
			_selectedSamples.Clear();
		}

		public Sample GetFirstSelectedSample()
		{
			if (_selectedSamples.Count > 0)
			{
				return _selectedSamples[0];
			}

			return null;
		}
		
		public void OnDragSample(float width, float paddingLeft)
		{
			if (_selectedSamples.Count > 0)
			{
				var data = AudioxEditorWindow.Instance.Data;
				var shouldDragCurrentSample = Event.current.shift;
				var shouldDragCurrentSampleSlowly = Event.current.alt;
				var mousePosX = Event.current.mousePosition.x;
				
				if ((!_isDragStarted || !_isKeyAltDown) && shouldDragCurrentSampleSlowly)
				{
					_isDragStarted = true;
					_startShiftDownOffset = mousePosX;
				}
				_isKeyAltDown = shouldDragCurrentSampleSlowly;
				
				var dragSpeed = shouldDragCurrentSampleSlowly ? 0.9f : 0f;
				var dragVector = mousePosX - _startShiftDownOffset;
				var offsetDrag = dragVector * dragSpeed;
				var positionX = mousePosX - paddingLeft;
				var moveSpeed = shouldDragCurrentSampleSlowly ? 0.1f : 1f;
				var moveVector = mousePosX - _dragStartPosition;
				var ratio = data.Clip.length / width;
				var dragPositionInSeconds = (positionX - offsetDrag) * ratio;
				var movePositionInSeconds = moveVector * moveSpeed * ratio;

				foreach (var sample in _selectedSamples)
				{
					if (dragPositionInSeconds < 0f)
					{
						dragPositionInSeconds = 0f;
					}

					if (dragPositionInSeconds > data.Clip.length)
					{
						dragPositionInSeconds = data.Clip.length;
					}

					if (_shouldDragSide && _dragSideSample == sample)
					{
						if (_isDragLeft)
						{
							sample.StartPosition = dragPositionInSeconds;
							
							if (sample.StartPosition > sample.EndPosition)
							{
								ExtraMethods.Swap(ref sample.StartPosition, ref sample.EndPosition);
								_isDragLeft = !_isDragLeft;
							}
						}
						else
						{
							sample.EndPosition = dragPositionInSeconds;
							
							if (sample.EndPosition < sample.StartPosition)
							{
								ExtraMethods.Swap(ref sample.StartPosition, ref sample.EndPosition);
								_isDragLeft = !_isDragLeft;
							}
						}
					}
					else if (shouldDragCurrentSample || shouldDragCurrentSampleSlowly)
					{
						var isLeftBorderReached = sample.StartPosition + movePositionInSeconds < 0f;
						var isRightBorderReached = sample.EndPosition + movePositionInSeconds >= data.Clip.length;
						if (isLeftBorderReached || isRightBorderReached)
						{
							movePositionInSeconds = isLeftBorderReached ? -sample.StartPosition : data.Clip.length - sample.EndPosition;
							_dragStartPosition = _startShiftDownOffset = Event.current.mousePosition.x;
							break;
						}
					}
				}

				if (!_shouldDragSide && (shouldDragCurrentSample || shouldDragCurrentSampleSlowly))
				{
					foreach (var sample in _selectedSamples)
					{
						sample.StartPosition += movePositionInSeconds;
						sample.EndPosition += movePositionInSeconds;
					}
					
					EditorUtility.SetDirty(AudioxEditorWindow.Instance.Data);
				}
				_dragStartPosition = Event.current.mousePosition.x;
			}
		}

		public void Draw()
		{
			var samples = AudioxEditorWindow.Instance.Data.Samples;
			var audioCurveRect = AudioxEditorWindow.Instance.GetCurveRect();
			var audioCurveWidth = audioCurveRect.width;
			var audioPanelHeight = AudioxEditorWindow.Instance.GetPanelHeight();
			var audioCurvePaddingLeft = audioCurveRect.xMin;
			
			Handles.BeginGUI();
			
			foreach (var sample in samples)
			{
				var startValue = GetPixelPosition(sample.StartPosition, audioCurveWidth, audioCurvePaddingLeft);
				var endValue = GetPixelPosition(sample.EndPosition, audioCurveWidth, audioCurvePaddingLeft);
				var leftBorder = GetPixelRect(startValue, audioPanelHeight);
				var rightBorder = GetPixelRect(endValue, audioPanelHeight);
				var sampleWidth = rightBorder.xMin - leftBorder.xMin;
				var sampleHeight = rightBorder.yMax - leftBorder.yMin;
				var rect = new Rect(leftBorder.xMin, leftBorder.yMin, sampleWidth, sampleHeight);
				
				sample.Rectangle = rect;
				
				EditorGUI.DrawRect(sample.Rectangle, _selectedSamples.Contains(sample) ? _selectedColor : _sampleColor);
				
				DrawSampleLine(leftBorder);
				DrawSampleLine(rightBorder);
				
				var textRect = rect;
				textRect.height -= AudioxEditorWindow.ScrollHeight;
				
				var testStyle = EditorStyles.whiteMiniLabel;
				testStyle.alignment = TextAnchor.MiddleCenter;
				
				EditorGUI.LabelField(textRect, sample.Name, testStyle);
			}
			
			Handles.EndGUI();
		}

		public float SecondsToTimeline(float time, float width)
		{
			var timelinePosition = time / AudioxEditorWindow.Instance.Data.Clip.length * width * width;
			return timelinePosition;
		}

		public void DrawInfo()
		{
			var selectedSample = GetFirstSelectedSample();
			if (selectedSample == null || GetSelectedSamplesCount() > 1) 
				return;

			if (AudioxEditorWindow.Instance != null && AudioxEditorWindow.Instance.Data != null)
			{
				Undo.RecordObject(AudioxEditorWindow.Instance.Data, "Timing Data Changed");
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Sample name: ", EditorStyles.label, GUILayout.Width(AudioxEditorWindow.LabelWidth));
			selectedSample.Name = EditorGUILayout.TextField(selectedSample.Name, EditorStyles.textField);
			EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Step: ", EditorStyles.label, GUILayout.Width(AudioxEditorWindow.LabelWidth));
            selectedSample.Step = EditorGUILayout.IntField(selectedSample.Step, EditorStyles.textField);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("IsLastInStep: ", EditorStyles.label, GUILayout.Width(AudioxEditorWindow.LabelWidth));
            selectedSample.IsLastInStep = EditorGUILayout.Toggle("", selectedSample.IsLastInStep);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
			var leftTime = ExtraMethods.FormatTimeSpan(TimeSpan.FromSeconds(selectedSample.StartPosition), true);
			EditorGUILayout.LabelField("Start: " + leftTime, EditorStyles.label);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			var rightTime = ExtraMethods.FormatTimeSpan(TimeSpan.FromSeconds(selectedSample.EndPosition), true);
			EditorGUILayout.LabelField("End: " + rightTime, EditorStyles.label);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			var durationValue = ExtraMethods.FormatTimeSpan(TimeSpan.FromSeconds(selectedSample.Size), true);
			EditorGUILayout.LabelField("Duration: " + durationValue, EditorStyles.label);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Sample Description: ", EditorStyles.label, GUILayout.Width(AudioxEditorWindow.LabelWidth));
			selectedSample.Description = EditorGUILayout.TextArea(selectedSample.Description, EditorStyles.textArea);
			EditorGUILayout.EndHorizontal();
		}
		
		private bool CanSelectSamples()
		{
			return !Event.current.alt && !Event.current.shift && !_shouldDragSide;
		}
		
		private void DrawSampleLine(Rect rect)
		{
			EditorGUI.DrawRect(rect, _sampleLineColor);
			EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
		}
		
		private float GetPixelPosition(float pos, float width, float padding)
		{
			return pos / AudioxEditorWindow.Instance.Data.Clip.length * (width) + padding;
		}

		private Rect GetPixelRect(float pos, float height)
		{
			return new Rect(pos, EditorStyles.toolbar.fixedHeight, 2f, height);
		}
		
		private void DeselectLayout()
		{
			GUI.SetNextControlName(string.Empty);
			GUI.FocusControl(string.Empty);
		}

		private void OnSelectionFinished(Rect selectedArea)
		{
			if (selectedArea.size == Vector2.zero) return;
			
			var samples = AudioxEditorWindow.Instance.Data.Samples;
			
			foreach (var sample in samples)
			{
				var overlaps = selectedArea.Overlaps(sample.Rectangle);
				if (overlaps && !_selectedSamples.Contains(sample))
				{
					_selectedSamples.Add(sample);
				}
				else if (!overlaps)
				{
					_selectedSamples.Remove(sample);
				}
			}
		}

		private Rect GetRectFromSelection()
		{
			var rect = new Rect(_mousePositionDown.x, _mousePositionDown.y,
				Event.current.mousePosition.x - _mousePositionDown.x,
				Event.current.mousePosition.y - _mousePositionDown.y);
			
			var panelHeight = AudioxEditorWindow.Instance.GetPanelHeight();
			
			if (_mousePositionDown.y + rect.height < EditorStyles.toolbar.fixedHeight)
			{
				rect.height = -_mousePositionDown.y + EditorStyles.toolbar.fixedHeight;
			}
			
			if (rect.height > panelHeight - _mousePositionDown.y)
			{
				rect.height = panelHeight - _mousePositionDown.y;
			}
			
			if (rect.width < 0)
			{
				rect.x += rect.width;
				rect.width = -rect.width;
			}
			
			if (rect.height < 0)
			{
				rect.y += rect.height;
				rect.height = -rect.height;
			}
			
			return rect;
		}

	}
}