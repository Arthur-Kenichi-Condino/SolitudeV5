using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnimatorParamChanger:MonoBehaviour{
[NonSerialized]public AI actor;[NonSerialized]public Animator animator;
void OnEnable(){
actor=transform.root.GetComponent<AI>();
}
[NonSerialized]Vector3 _horizontalMoveSpeed;[NonSerialized]public float horizontalMoveSensibility=1f/3f;[NonSerialized]public bool backwardAvailable=true;
void Update(){
if(actor.IsUMA&&animator==null){
animator=actor.GetComponentInChildren<Animator>();

    Debug.LogWarning(actor+";animator:"+animator);

}
if(animator!=null){
_horizontalMoveSpeed=actor.rigidbody.velocity;
_horizontalMoveSpeed.y=0;
animator.SetFloat("Forward",_horizontalMoveSpeed.magnitude*(backwardAvailable&&Vector3.Angle(actor.rigidbody.transform.forward,actor.rigidbody.velocity.normalized)>90?-1:1)*horizontalMoveSensibility,0.1f,Time.deltaTime);
animator.SetFloat("Jump",actor.rigidbody.velocity.y);
}
}
public void FootR(string s){
    Debug.LogWarning("FootR");
}
public void FootL(string s){
    Debug.LogWarning("FootL");
}
}