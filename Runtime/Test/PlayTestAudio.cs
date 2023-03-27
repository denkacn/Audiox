using System.Collections;
using UnityEngine;

namespace Audiox.Runtime.Test
{
    public class PlayTestAudio : MonoBehaviour
    {
        [SerializeField] private AudioxPlayer _player;
        [SerializeField] private string _clipName;
        
        IEnumerator Start()
        {
            yield return new WaitForSeconds(3);
            _player.Play(_clipName);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
