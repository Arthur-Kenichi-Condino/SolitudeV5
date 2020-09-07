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
protected Motions MyMotion=Motions.MOTION_STAND; 
public float motionRhythm=0.0249f;[NonSerialized]protected float curAnimTime=-1;[NonSerialized]float curAnimTime_normalized;[NonSerialized]protected int attackStance=-1;
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


if(attackStance!=-1){
    MyMotion=attackStance==0?Motions.MOTION_ATTACK:Motions.MOTION_ATTACK2;
if(curAnimTime_normalized>=1){attackStance=-1;curAnimTime=-1;curAnimTime_normalized=0;}
}
if(attackStance==-1){
    MyMotion=(rigidbody!=null&&(Mathf.Abs(rigidbody.velocity.x)>.05f||Mathf.Abs(rigidbody.velocity.z)>.05f))?Motions.MOTION_MOVE:Motions.MOTION_STAND;
}


animator.SetBool("MOTION_STAND"  ,MyMotion==Motions.MOTION_STAND  );
animator.SetBool("MOTION_MOVE"   ,MyMotion==Motions.MOTION_MOVE   );
animator.SetBool("MOTION_ATTACK" ,MyMotion==Motions.MOTION_ATTACK );
animator.SetBool("MOTION_ATTACK2",MyMotion==Motions.MOTION_ATTACK2);


if(curAnimTime!=-1){curAnimTime+=motionRhythm*animator.GetCurrentAnimatorStateInfo(0).speed*animator.GetCurrentAnimatorStateInfo(0).length;curAnimTime_normalized=Mathf.Clamp01(curAnimTime/animator.GetCurrentAnimatorStateInfo(0).length);
animator.SetFloat("time",curAnimTime_normalized);
}


}
public enum Motions:int{
MOTION_STAND  =0,
MOTION_MOVE   =1,
MOTION_HIT    =2,
MOTION_ATTACK =3,
MOTION_ATTACK2=4,
MOTION_DEAD   =5,
}
}