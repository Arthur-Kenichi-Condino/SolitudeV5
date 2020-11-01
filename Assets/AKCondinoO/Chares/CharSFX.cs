using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CharSFX:MonoBehaviour{
[NonSerialized]public AI actor;[NonSerialized]public AudioSource audioSource;
void Awake(){
actor=GetComponentInChildren<AI>();audioSource=GetComponent<AudioSource>();
}
void OnDisable(){    
    Debug.LogWarning("reset lastSound");
lastSound=null;lastSoundPriority=-1;
changingSound=null;
}
public AudioClip[]sounds;public bool[]loop;public float[]pitch;public int[]priority;[NonSerialized]AudioClip lastSound=null;[NonSerialized]int lastSoundPriority=-1;
public void Play(int sound,bool restart=false){
if(sound>=sounds.Length){Stop();return;}
Play(sounds[sound],restart);
}
public void Play(AudioClip sound,bool restart=false){
if(sound==null){Stop();return;}
if(sound==lastSound&&!restart)return;
int priorityidx,pitchidx,loopidx=pitchidx=priorityidx=Array.IndexOf(sounds,sound);int importance=priorityidx==-1||priorityidx>=priority.Length?-1:priority[priorityidx];
if(importance<lastSoundPriority&&((audioSource.clip!=null&&audioSource.isPlaying)||changingSound!=null))return;
Debug.LogWarning("play "+sound.name+"; lastSound=="+lastSound);
lastSound=sound;lastSoundPriority=importance;
changingSound=StartCoroutine(CR_FadeToSound(sound,loopidx==-1||loopidx>=loop.Length?false:loop[loopidx],pitchidx==-1||pitchidx>=pitch.Length?1f:pitch[pitchidx]));
}
readonly WaitForSeconds CR_FadeToSound_waitForSeconds=new WaitForSeconds(0.05f);[NonSerialized]Coroutine changingSound=null;
public IEnumerator CR_FadeToSound(AudioClip sound,bool loop=false,float pitch=1f){
yield return StartCoroutine(CR_FadeToStop());
audioSource.clip=sound;
audioSource.volume=1;
audioSource.loop=loop;
audioSource.pitch=pitch;
audioSource.Play();
changingSound=null;
}
public void Stop(){
lastSound=null;lastSoundPriority=-1;
StartCoroutine(CR_FadeToStop());
}
public IEnumerator CR_FadeToStop(){
if(audioSource.isPlaying){
for(float volumeNext;audioSource.volume>0;volumeNext=audioSource.volume-0.1f,audioSource.volume=(volumeNext<=0)?(0):(volumeNext)){
yield return CR_FadeToSound_waitForSeconds;
}
audioSource.Stop();
}
}
}
public enum ActorSounds{
_ATTACK=0,
_DEAD=1,
_HIT=2,
_IDLE=3,
_MOVE=4,
}