using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour {

	public static SoundPlayer instance;

	private static AudioSource[] sources;
	private static Dictionary<string, AudioClip[]> audioClips;


	private void Awake () {
		if (instance == null || instance == this) {
			instance = this;
		}
		else {
			Destroy(gameObject);
		}

		LoadSounds();

		AudioSource sourcePrefab = Resources.Load<AudioSource>("Prefabs/AudioSource");
		sources = new AudioSource[8];

		for (int i = 0; i < 8; i++) {
			sources[i] = Instantiate<AudioSource>(sourcePrefab);
			sources[i].transform.parent = instance.transform;
		}
	}


	public static void PlaySound(Sound sound, float volume, bool varyPitch) {
		if (instance == null) {
			return;
		}
		if (!audioClips.ContainsKey($"{sound}")) {
			return;
		}

		AudioClip[] clips = audioClips[$"{sound}"];
		int index = Random.Range(0, clips.Length);
		AudioSource source = null;

		for (int i = 0; i < sources.Length; i++) {
			if (!sources[i].isPlaying) {
				source = sources[i];

				break;
			}
		}

		if (source == null) {
			source = sources[Random.Range(0, sources.Length)];
		}

		source.clip = clips[index];
		source.volume = volume;
		
		if (varyPitch) {
			source.pitch = Random.Range(0.75f, 1.25f);
		}
		else {
			source.pitch = 1;
		}

		source.Play();
	}

	private void LoadSounds () {
		audioClips = new Dictionary<string, AudioClip[]>();

		foreach (string soundName in System.Enum.GetNames(typeof(Sound))) {
			AudioClip[] clips = Resources.LoadAll<AudioClip>($"Sounds/{soundName}/");
			audioClips.Add(soundName, clips);
		}
	}
}


public enum Sound {
	ArrowDamage = 0,
	ArrowFling = 1,
	AxeStrike = 2,
	Boat = 3,
	Death = 4,
	GameStart = 5,
	GameEnd = 6,
	Ladder = 7,
	MenuMove = 8,
	MenuClick = 9,
	PikeStrike = 10,
	SquadWipe = 11,
	SwordStrike = 12,
}













