using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Audiox.Editor.Models
{
	[Serializable]
	public class Curve
	{
		private const float ScaleY = 0.5f;

		[SerializeField] private float[] _audioCurveData;
		
		private readonly Color _proColor = new Color(255 / 255f, 168 / 255f, 7 / 255f);
		private readonly Color _defaultColor = new Color(215 / 255f, 149 / 255f, 20 / 255f);

		public void LoadData(AudioClip clip)
		{
			if (clip == null) return;
			
			var clipPath = AssetDatabase.GetAssetPath(clip);
			var importer = AssetImporter.GetAtPath(clipPath);
			var assembly = Assembly.GetAssembly(typeof(AssetImporter));
			var type = assembly.GetType("UnityEditor.AudioUtil");
			var audioUtilGetMinMaxData = type.GetMethod("GetMinMaxData");
			
			_audioCurveData = audioUtilGetMinMaxData.Invoke(null, new object[] {importer}) as float[];
		}

		public void Render()
		{
			if (Event.current.type == EventType.Repaint && _audioCurveData != null)
			{
				var curveRect = AudioxEditorWindow.Instance.GetCurveRect();
				
				GL.PushMatrix();
				GL.LoadPixelMatrix();
				GL.Begin(GL.QUADS);
				GL.Color(EditorGUIUtility.isProSkin ? _proColor : _defaultColor);
				
				for (var i = 0; i < _audioCurveData.Length; i += 4)
				{
					var y1 = curveRect.yMin + curveRect.height / 2f + _audioCurveData[i + 0] * -curveRect.height * ScaleY;
					var y2 = curveRect.yMin + curveRect.height / 2f + _audioCurveData[i + 1] * -curveRect.height * ScaleY;
					var y3 = curveRect.yMin + curveRect.height / 2f + _audioCurveData[i + 2] * -curveRect.height * ScaleY;
					var y4 = curveRect.yMin + curveRect.height / 2f + _audioCurveData[i + 3] * -curveRect.height * ScaleY;
					var x1 = curveRect.xMin + (i / 4 + 0) / (_audioCurveData.Length / curveRect.width) * 4f;
					var x2 = curveRect.xMin + (i / 4 + 1) / (_audioCurveData.Length / curveRect.width) * 4f;

					var v0 = new Vector3(x1, y2, 0);
					var v1 = new Vector3(x1, y1, 0);
					var v2 = new Vector3(x2, y3, 0);
					var v3 = new Vector3(x2, y4, 0);

					GL.Vertex(v0);
					GL.Vertex(v1);
					GL.Vertex(v2);
					GL.Vertex(v3);
				}
				
				GL.End();
				GL.PopMatrix();
			}
		}
	}
}