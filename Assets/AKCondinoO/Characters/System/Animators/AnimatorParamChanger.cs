using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnimatorParamChanger:MonoBehaviour{
public bool LOG=false;public int LOG_LEVEL=1;public int DRAW_LEVEL=1;
[NonSerialized]public AI actor;[NonSerialized]public CharControl character;[NonSerialized]public Animator animator;
void OnEnable(){
actor=transform.root.GetComponent<AI>();character=actor as CharControl;
attackStance=-1;hitStance=-1;deadStance=-1;
}
[NonSerialized]int attackStance;[SerializeField]protected float attackStanceRhythmMultiplier=1f;[SerializeField]protected float attackStanceDamageTime=.15f;[SerializeField]protected float attackStanceDamageTimeEnd=.85f;[NonSerialized]protected bool attackStanceDamageStarted;[NonSerialized]protected bool attackStanceDamageStopped;[NonSerialized]protected int hitStance=-1;[SerializeField]protected float hitStanceRhythmMultiplier=1f;[NonSerialized]protected int deadStance=-1;[SerializeField]protected float deadStanceRhythmMultiplier=1f;
public float motionRhythm=0.0245f;[NonSerialized]protected float curAnimTime=-1;[NonSerialized]float curAnimTime_normalized;
[NonSerialized]Vector3 _horizontalMoveSpeed;[NonSerialized]Vector3 _forward;[NonSerialized]Vector3 _move;[NonSerialized]float _angle;[NonSerialized]float _turn;[NonSerialized]public float horizontalMoveSensibility=1f/3f;[NonSerialized]public bool backwardAvailable=true;
void Update(){
if(actor.IsUMA&&animator==null){
animator=actor.GetComponentInChildren<Animator>();

    Debug.LogWarning(actor+";animator:"+animator);

}
if(animator!=null){
}
}
[NonSerialized]bool animationChanged;[NonSerialized]int animationHash;[NonSerialized]int animationHashPreceding;[NonSerialized]bool ignoreNextAnimationChange;
void LateUpdate(){
if(animator!=null){
animationHashPreceding=animationHash;animationChanged=(animationHash=animator.GetCurrentAnimatorStateInfo(0).fullPathHash)!=animationHashPreceding;
if(attackStance!=-1){
if(animationChanged){
if(ignoreNextAnimationChange){
    ignoreNextAnimationChange=false;
}else{
curAnimTime=0;
}
}
curAnimTime_normalized=Mathf.Clamp01(curAnimTime/animator.GetCurrentAnimatorStateInfo(0).length);if(curAnimTime_normalized>=attackStanceDamageTime&&!attackStanceDamageStarted){
    actor.OnAttackAnimationStartDoDamage();attackStanceDamageStarted=true;}if(curAnimTime_normalized>=attackStanceDamageTimeEnd&&!attackStanceDamageStopped){actor.OnAttackAnimationStopDoDamage();attackStanceDamageStopped=true;}if(curAnimTime_normalized>=1){
                Debug.LogWarning("attackStance end");
    attackStance=-1;curAnimTime=-1;ignoreNextAnimationChange=true;actor.OnAttackAnimationEnd();attackStanceDamageStarted=false;attackStanceDamageStopped=false;}
}
animator.SetBool("MOTION_ATTACK_L1",attackStance==0);
animator.SetBool("MOTION_ATTACK_L2",attackStance==1);
animator.SetBool("MOTION_ATTACK_L3",attackStance==2);
animator.SetBool("MOTION_ATTACK_R1",attackStance==3);
animator.SetBool("MOTION_ATTACK_R2",attackStance==4);
animator.SetBool("MOTION_ATTACK_R3",attackStance==5);


_horizontalMoveSpeed=actor.rigidbody.velocity;
_horizontalMoveSpeed.x=0;
_horizontalMoveSpeed.y=0;
animator.SetFloat("Forward",(_horizontalMoveSpeed.magnitude*(backwardAvailable&&Vector3.Angle(actor.rigidbody.transform.forward,actor.rigidbody.velocity.normalized)>90?-1:1)*horizontalMoveSensibility)*(actor.Crouching?4f:1f),0.1f,Time.deltaTime);
animator.SetBool("OnGround",actor.OnGround);

            
_turn=Mathf.LerpUnclamped(_turn,Vector3.SignedAngle(_forward,actor.transform.forward,Vector3.up)*Mathf.Deg2Rad*100f,50f*Time.deltaTime);
_forward=actor.transform.forward;

            
//_move=_horizontalMoveSpeed;_angle=0;
//			// convert the world relative moveInput vector into a local-relative
//			// turn amount and forward amount required to head in the desired
//			// direction.
//			if (_move.magnitude > 1f){ _move.Normalize();
//			_move = actor.transform.InverseTransformDirection(_move);
//			_move = Vector3.ProjectOnPlane(_move, Vector3.up);
//			_angle = Mathf.Atan2(_move.x, _move.z);
//if(_angle>0&&_turn>=0&&_angle>_turn){_turn=_angle;
//}else 
//if(_angle<0&&_turn<=0&&_angle<_turn){_turn=_angle;
//}
//			}


Debug.LogWarning("_angle:"+_angle+";_turn:"+_turn);
animator.SetFloat("Turn",_turn,0.1f,Time.deltaTime);
            

if(LOG&&LOG_LEVEL<=-110)Debug.Log("actor.OnGround:"+actor.OnGround);

animator.SetFloat("Jump",actor.rigidbody.velocity.y);
animator.SetBool("Crouch",actor.CanCrouch&&actor.Crouching);


Debug.LogWarning(animationHash);


if(curAnimTime!=-1){curAnimTime+=(motionRhythm*(attackStance!=-1?((actor.Attributes.Aspd+1f)*attackStanceRhythmMultiplier):hitStance!=-1?hitStanceRhythmMultiplier:deadStanceRhythmMultiplier)*animator.GetCurrentAnimatorStateInfo(0).speed*animator.GetCurrentAnimatorStateInfo(0).length)*(Time.deltaTime/MainCamera._60FPSdeltaTime);
if(actor.Attributes.Aspd==0){
if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("Attributes.Aspd==0;some animations may bug");
}
animator.SetFloat("time",curAnimTime_normalized);
}
}
}
void InterruptCurrentAnimation(){
}
public void OnAttack(int attackStance){
    Debug.LogWarning("OnAttack(int attackStance):"+attackStance);
     InterruptCurrentAnimation();this.attackStance=attackStance;
}
public void FootR(string s){
    Debug.LogWarning("FootR");
}
public void FootL(string s){
    Debug.LogWarning("FootL");
}
public void Hit(string s){
    Debug.LogWarning("Hit");
}
}