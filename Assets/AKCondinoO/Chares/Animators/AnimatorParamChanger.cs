using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnimatorParamChanger:MonoBehaviour{
[NonSerialized]public AI actor;[NonSerialized]public Animator animator;
void OnEnable(){
actor=GetComponent<AI>();
}
void Update(){
if(actor.IsUMA&&animator==null){
animator=GetComponentInChildren<Animator>();

    Debug.LogWarning(actor+";animator:"+animator);

}
}
}