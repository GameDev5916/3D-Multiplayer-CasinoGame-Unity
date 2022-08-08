using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

///-----------------------------------------------------------------------------------------
///   Namespace:      BE
///   Class:          BEAudioManager
///   Description:    implement audio source pool & provide easy method to play sound
///   Usage :		  BEAudioManager.SoundPlay("ring");
///                   BEAudioManager.SoundPlay(1);
///                   BEAudioManager.MusicPlay();
///                   BEAudioManager.MusicStop();
///   Author:         BraveElephant inc.
///   Version: 		  v1.0 (2015-09-25)
///-----------------------------------------------------------------------------------------
namespace BE {
	public class BEAudioManager : MonoBehaviour {
		public	static BEAudioManager instance;

		public 	AudioClip[]   		audioClip;
		private List<AudioSource>	AudioSourcePool;
		public 	AudioSource  		AudioSourceBGM;

		void Awake() {
			instance=this;

			AudioSourcePool = new List<AudioSource>();
			AudioSourceAlloc();
		}

		void Start() {
			// when start, if music volume is not 0, play music
			//if(BESetting.MusicVolume != 0)
			//	BEAudioManager.MusicPlay();
		}

		public void AudioSourceAlloc() {
			AudioSourcePool.Clear();

			// we create 100 instance of audio source for pooling
			for(int i=0 ; i < 100 ; ++i) {
				AudioSource aS = (AudioSource)gameObject.AddComponent<AudioSource>();
				aS.loop = false;
				AudioSourcePool.Add(aS);
			}
		}

		public AudioSource AudioSourcePop() {
			if(AudioSourcePool.Count <= 0)	return null;

			// get front item & push back to the list
			AudioSource aS = AudioSourcePool[0];
			AudioSourcePool.RemoveAt(0);
			AudioSourcePool.Add(aS);

			return aS;
		}

		// Sound
		public static void SoundPlay(int iType) {
			//if(BESetting.SoundVolume == 0) return;

			if (Constants.SOUND == 1) return;
			AudioSource aS = instance.AudioSourcePop();
			aS.PlayOneShot(instance.audioClip[iType]);
		}

		public static void SoundPlay(AudioClip clip) {
			//if(BESetting.SoundVolume == 0) return;
			if (Constants.SOUND == 1) return;

			if (clip == null) return;

			AudioSource aS = instance.AudioSourcePop();
			aS.PlayOneShot(clip);
		}

		public void SoundPlayCoroutine(int iType, float fDelay) {
			StartCoroutine(SoundPlayIn(iType, fDelay));
		}

		public IEnumerator SoundPlayIn(int iType, float fDelay) {
			if(fDelay > 0.0001f) yield return new WaitForSeconds(fDelay);

			SoundPlay(iType);
		}

		// Music
		public static void MusicPlay() {
            if (BESetting.MusicVolume == 0) return;
            //if(!AudioSourceBGM.isPlaying) AudioSourceBGM.Stop();
            //AudioSourceBGM.clip = audioClipBGM[iType];
            instance.AudioSourceBGM.Play();
            //AudioSourceBGM.volume = 0.4f;
        }

        public static bool MusicIsPlaying() {
			return instance.AudioSourceBGM.isPlaying;
		}

		public static void MusicStop() {
			instance.AudioSourceBGM.Stop();
		}
	}
}