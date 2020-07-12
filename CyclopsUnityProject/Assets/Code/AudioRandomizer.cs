using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioRandomizer : MonoBehaviour
{
  private void Start()
  {
    AudioSource rAudioSource = GetComponent<AudioSource>();
    if (rAudioSource && !rAudioSource.isPlaying)
    {
      int rand = Random.Range(0, 4);
      rAudioSource.pitch = .8f + (rand * .1f);
      rAudioSource.Play();
    }
  }
}
