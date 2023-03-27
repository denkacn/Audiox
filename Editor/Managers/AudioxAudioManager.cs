using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Audiox.Editor.Managers
{
	[Serializable]
	public class AudioxAudioManager
	{
		public AudioSource Source;
		
		[SerializeField] private GameObject _hiddenGameObject;
		[SerializeField] private float _time;

		public void InitSource(AudioClip clip)
		{
			if (clip == null) return;
			
			if (_hiddenGameObject == null || Source == null)
			{
				Destroy();
				
				_hiddenGameObject = new GameObject("_hiddenGameObject")
				{
					hideFlags = HideFlags.HideAndDontSave
				};
				
				Source = _hiddenGameObject.AddComponent<AudioSource>();
				Source.clip = clip;
			}
		}

		public bool IsStartPlaying()
		{
			return Source != null && Source.clip != null && Source.time > 0f;
		}
		
		public void RestorePosition()
		{
			if (Source != null && Source.clip != null)
			{
				Source.time = _time;
				Source.Play();
				Source.Pause();
			}
		}

		public void SetPosition(float newTime)
		{
			if (Source != null && Source.clip != null)
			{
				_time = newTime;
				Source.time = newTime;
			}
		}

		public void Destroy()
		{
			if (_hiddenGameObject != null)
			{
				Object.DestroyImmediate(_hiddenGameObject);
			}
		}
	}
}