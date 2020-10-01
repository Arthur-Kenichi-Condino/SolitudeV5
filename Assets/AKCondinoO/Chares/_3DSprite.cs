using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class _3DSprite:AI{
[NonSerialized]protected Animator animator;[NonSerialized]protected new SpriteRenderer renderer;
protected override void Awake(){
                   base.Awake();
animator=GetComponentInChildren<Animator>();renderer=GetComponentInChildren<SpriteRenderer>();
}
public float motionRhythm=0.0245f;[NonSerialized]protected float curAnimTime=-1;[NonSerialized]float curAnimTime_normalized;
[NonSerialized]Vector3 _forward,_cameraForward,_forwardFromCameraToSprite;[NonSerialized]bool _back;[NonSerialized]bool _flipX;
protected override void LateUpdate(){
                   base.LateUpdate();
animator.transform.position=drawPos;
animator.transform.rotation=Quaternion.LookRotation((Camera.main.transform.position-animator.transform.position).normalized,Vector3.up);
_forward=Vector3.Scale(drawRotation.eulerAngles,Vector3.up);
_forward=Quaternion.Euler(_forward)*Vector3.forward;
_cameraForward=Vector3.Scale(Camera.main.transform.eulerAngles,Vector3.up);
_cameraForward=Quaternion.Euler(_cameraForward)*Vector3.forward;
_forwardFromCameraToSprite=(drawPos-Camera.main.transform.position).normalized;
_flipX=Vector3.SignedAngle(_forward,_forwardFromCameraToSprite,Vector3.up)>=0;renderer.flipX=_flipX;
_back=Vector3.Angle(_forward,_forwardFromCameraToSprite)<=90;animator.SetBool("back",_back);

    
if(deadStance!=-1){
BlockMovement=true;
    MyMotion=Motions.MOTION_DEAD;
curAnimTime_normalized=Mathf.Clamp01(curAnimTime/animator.GetCurrentAnimatorStateInfo(0).length);if(curAnimTime_normalized>=1){
    curAnimTime=-1;animator.SetFloat("time",curAnimTime_normalized);}
}
if(hitStance!=-1){
BlockMovement=true;
    MyMotion=Motions.MOTION_HIT;
curAnimTime_normalized=Mathf.Clamp01(curAnimTime/animator.GetCurrentAnimatorStateInfo(0).length);if(curAnimTime_normalized>=1){
    hitStance=-1;curAnimTime=-1;}
}
if(attackStance!=-1){
BlockMovement=true;
    MyMotion=attackStance==0?Motions.MOTION_ATTACK:Motions.MOTION_ATTACK2;
curAnimTime_normalized=Mathf.Clamp01(curAnimTime/animator.GetCurrentAnimatorStateInfo(0).length);if(curAnimTime_normalized>=attackStanceDamageTime){
    DoDamageHitbox();}if(curAnimTime_normalized>=1){
    attackStance=-1;curAnimTime=-1;}
}
if(deadStance==-1&&hitStance==-1&&attackStance==-1){
BlockMovement=false;
    MyMotion=(rigidbody!=null&&(Mathf.Abs(rigidbody.velocity.x)>.05f||Mathf.Abs(rigidbody.velocity.z)>.05f))?Motions.MOTION_MOVE:Motions.MOTION_STAND;
}


animator.SetBool("MOTION_STAND"  ,MyMotion==Motions.MOTION_STAND  );
animator.SetBool("MOTION_MOVE"   ,MyMotion==Motions.MOTION_MOVE   );
animator.SetBool("MOTION_HIT"    ,MyMotion==Motions.MOTION_HIT    );
animator.SetBool("MOTION_ATTACK" ,MyMotion==Motions.MOTION_ATTACK );
animator.SetBool("MOTION_ATTACK2",MyMotion==Motions.MOTION_ATTACK2);
animator.SetBool("MOTION_DEAD"   ,MyMotion==Motions.MOTION_DEAD   );


if(curAnimTime!=-1){curAnimTime+=motionRhythm*(attackStance!=-1?(Attributes.Aspd*attackStanceRhythmMultiplier):hitStance!=-1?hitStanceRhythmMultiplier:deadStanceRhythmMultiplier)*animator.GetCurrentAnimatorStateInfo(0).speed*animator.GetCurrentAnimatorStateInfo(0).length;
if(Attributes.Aspd==0){
if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("Attributes.Aspd==0;some animations may bug");
}
animator.SetFloat("time",curAnimTime_normalized);
}


}
}