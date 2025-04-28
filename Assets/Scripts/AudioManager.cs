using UnityEngine;

public class AudioManager : MonoBehaviour
{
	static AudioManager _instance; 
	public static AudioManager instance => _instance ?? (_instance = FindAnyObjectByType<AudioManager>());

	public AudioSource soundSource;
	public AudioSource[] musicSources;
	public AudioClip[] clips;

	void _Play(SoundType sound)
	{
		if(Time.timeSinceLevelLoad < 0.1f)
			return;

		soundSource.Stop();
		soundSource.clip = clips[(int)sound];
		soundSource.Play();
	}

    void Update()
    {
		foreach(var musicSource in musicSources)
			musicSource.volume = Persistence.settings.musicEnabled ? Persistence.settings.musicVolume : 0;
			
		soundSource.volume = Persistence.settings.soundEnabled ? Persistence.settings.soundVolume : 0;
    }

    public static void Play(SoundType sound) => instance._Play(sound);
}

public enum SoundType
{
	Pickup,
	Success,
	Unsuccess,
	LowSuccess,
	LowUnsuccess,
	CBA,
}