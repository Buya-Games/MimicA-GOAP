using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;//class is defined at bottom of this script
    
    void Awake()
    {
        foreach (Sound s in sounds){
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }

    public void PlaySound(string name, float delay, float volume, float pitch = 1){
        Sound s = Array.Find(sounds, sound => sound.name  == name);
        if (s == null){
            Debug.Log("sound " + name + " not found");
            return;
        }
        s.source.pitch = pitch;
        s.source.volume = volume;
        if (delay > 0){
            s.source.PlayDelayed(delay);
        } else {
            s.source.Play();
        }
    }
}

[System.Serializable] public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0,1f)] public float volume;
    [Range(0.1f,3)] public float pitch;
    [HideInInspector] public AudioSource source;

}
