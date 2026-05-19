using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct KeySoundPair
{
    public string key;
    public RandomSound sound;
}

[Serializable]
public struct TrackBPMPair
{
    public AudioClip track;
    public float bpm;
}

public class DJ : MonoBehaviour
{
    private static DJ instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private KeySoundPair[] soundsArr;
    [SerializeField] private TrackBPMPair[] traks;
    [SerializeField] private Material[] pulsedMaterials;

    private Dictionary<string, Queue<RandomSound>> sounds = new Dictionary<string, Queue<RandomSound>>();
    private int trackIndex = -1;

    public static void Play(string key)
    {
        if (instance.sounds.ContainsKey(key))
		{
            RandomSound sound = instance.sounds[key].Dequeue();
            sound.PlayRandom();
            instance.sounds[key].Enqueue(sound);
        }
        else
            Debug.LogWarning($"DJ have not a {key} sound");
    }

    private void PlayRandomTrack()
	{
        if(traks.Length <= 1)
		{
            Debug.LogError("DG traks.Length too small");
            return;
		}

        int newTrackIndex = trackIndex;
        while(newTrackIndex == trackIndex)
            newTrackIndex = UnityEngine.Random.Range(0, traks.Length);

        trackIndex = newTrackIndex;
        UpdateBPMView(traks[trackIndex].bpm);
        musicSource.clip = traks[trackIndex].track;
        musicSource.Play();
    }

    private void UpdateBPMView(float bpm)
	{
        float beatSpeed = bpm * (2f * Mathf.PI) / 60f;

        foreach (Material mat in pulsedMaterials)
            mat.SetFloat("_WaweSpeed", beatSpeed);
    }

	void Start()
    {
        instance = this;

        foreach (KeySoundPair pair in soundsArr)
		{
            sounds.Add(pair.key, new Queue<RandomSound>());
            sounds[pair.key].Enqueue(pair.sound);

            for(int i = 0; i < 5; i++)
                sounds[pair.key].Enqueue(Instantiate(pair.sound, transform));
        }

        PlayRandomTrack();
    }
}
