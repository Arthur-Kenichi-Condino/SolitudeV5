﻿using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActorManagementMentana;
public class SimActor:SimObject{
[NonSerialized]protected bool canFly_v;protected bool canFly{get{return canFly_v;}set{
if(value){
if(constantForce==null){constantForce=this.gameObject.AddComponent<ConstantForce>();}
if(rigidbody!=null)constantForce.force=new Vector3(0,-(rigidbody.mass*Physics.gravity.y),0);
}
canFly_v=value;
}}[NonSerialized]protected new ConstantForce constantForce=null;
[SerializeField]public RolePlayingAttributes Attributes=new RolePlayingAttributes();
[Serializable]public class RolePlayingAttributes{
[SerializeField]public int STR;
[SerializeField]public int VIT;
[SerializeField]public int INT;
[SerializeField]public int AGI;
[SerializeField]public int DEX;
[SerializeField]public int LUK;
[SerializeField]public float BaseMaxStamina;[SerializeField]public float CurStamina;
[SerializeField]public float BaseMaxFocus;[SerializeField]public float CurFocus;
[SerializeField]public float BaseAspd;public float Aspd{get{return BaseAspd;}}
[SerializeField]public float BaseHit;public float Hit{get{return BaseHit;}}
[SerializeField]public float BaseFlee;public float Flee{get{return BaseFlee;}}
[SerializeField]public float BaseDEF;public float DEF{get{return BaseDEF;}}
[SerializeField]public float BaseMDEF;public float MDEF{get{return BaseMDEF;}}
[SerializeField]public float BaseATK;public float ATK{get{return BaseATK;}}
[SerializeField]public float BaseMATK;public float MATK{get{return BaseMATK;}}
}
public virtual void InitAttributes(bool random=true){
if(LOG&&LOG_LEVEL<=100)Debug.LogWarning(GetType()+":attributes were not set!");
ValidateAttributesSet(0);
}
public virtual float GetBaseMaxStamina(){
return(Attributes.VIT*100+Attributes.STR*50+Attributes.AGI*20);
}
public virtual float GetBaseMaxFocus(){
return(Attributes.VIT*.5f+Attributes.INT*100);
}
public virtual float GetBaseAspd(){
return Mathf.Clamp((Attributes.AGI/100f+Attributes.DEX*.5f/100f)/2f*1.5f,.5f,1f);
}
public virtual float GetBaseHit(){
return(Attributes.DEX/100f+Attributes.LUK*.5f/100f)/1.5f;
}
public virtual float GetBaseFlee(){
return(Attributes.AGI/100f+Attributes.LUK*.5f/100f)/1.5f;
}
public virtual float GetBaseDEF(){
return(Attributes.VIT*1.5f+Attributes.AGI*.5f);
}
public virtual float GetBaseMDEF(){
return(Attributes.INT*1.5f+Attributes.LUK*.5f);
}
public virtual float GetBaseATK(){
return(Attributes.STR*2.5f+Attributes.DEX*.5f+Attributes.AGI*.45f+Attributes.LUK*.05f);
}
public virtual float GetBaseMATK(){
return(Attributes.INT*3.5f);
}
protected void ValidateAttributesSet(int version){
if(version<=0){
if(LOG&&LOG_LEVEL<=100)Debug.LogWarning(GetType()+":attributes are invalid! [0]");
Attributes.STR=mathrandom.Next(1,100);
Attributes.VIT=mathrandom.Next(1,100);
Attributes.INT=mathrandom.Next(1,100);
Attributes.AGI=mathrandom.Next(1,100);
Attributes.DEX=mathrandom.Next(1,100);
Attributes.LUK=mathrandom.Next(1,100);
    Attributes.BaseMaxStamina=Attributes.CurStamina=GetBaseMaxStamina();
       Attributes.BaseMaxFocus=Attributes.CurFocus=GetBaseMaxFocus();
                    Attributes.BaseAspd=GetBaseAspd();
}
if(version<=1){
Attributes.BaseDEF=GetBaseDEF();Attributes.BaseMDEF=GetBaseMDEF();
Attributes.BaseATK=GetBaseATK();Attributes.BaseMATK=GetBaseMATK();
}
if(version<=2){
Attributes.BaseHit=GetBaseHit();Attributes.BaseFlee=GetBaseFlee();
}
}
[SerializeField]public SkillIds[]onWillTakeDamageSkills;public readonly Dictionary<SkillIds,Skill>Skills=new Dictionary<SkillIds,Skill>();
protected override void Awake(){
    //Debug.LogWarning("here");
                   base.Awake();
tgtPos=tgtPos_Pre=transform.position;
tgtRot=tgtRot_Pre=transform.eulerAngles;headTgtRot=headTgtRot_Pre=tgtRot;
drawPos=transform.position;drawRotation=headDrawRotation=transform.rotation;


        foreach(var skillId in onWillTakeDamageSkills){
            switch(skillId){
                case(SkillIds._EVADE):{if(!Skills.ContainsKey(SkillIds._EVADE)){Skills.Add(SkillIds._EVADE,new _EVADE());}
                break;}
            }
        }


}
[NonSerialized]protected bool Jump;
[NonSerialized]Vector3 eulerAngles,headEulerAngles;[NonSerialized]float angleBetweenBodyAndHead;[NonSerialized]Vector3 stopMovement,moveSpeedRotated,moveSpeedToApplyToBody;
protected override void FixedUpdate(){
                   base.FixedUpdate();
if(rigidbody!=null){
if(!canFly){
        if(inputMoveSpeed.y>0&&!IsGrounded){
            inputMoveSpeed.y=0;
        }
}
        if(inputViewRotationEuler!=Vector3.zero){
headEulerAngles+=inputViewRotationEuler;
            inputViewRotationEuler=Vector3.zero;
        }
        if(inputMoveSpeed.x==0&&inputMoveSpeed.z==0){
eulerAngles.y+=(angleBetweenBodyAndHead=headEulerAngles.y-eulerAngles.y)>=90?angleBetweenBodyAndHead-90:angleBetweenBodyAndHead<=-90?angleBetweenBodyAndHead+90:0;
        }else{
eulerAngles.y=headEulerAngles.y;
        }
        rigidbody.rotation=Quaternion.Euler(eulerAngles);
headEulerAngles.x=Mathf.Clamp(headEulerAngles.x,rigidbody.rotation.eulerAngles.x-90,rigidbody.rotation.eulerAngles.x+90);
moveSpeedRotated=rigidbody.rotation*inputMoveSpeed;
        if(inputMoveSpeed.x==0&&inputMoveSpeed.z==0){
stopHorizontalMovement();
        }
moveSpeedToApplyToBody.x=moveSpeedRotated.x==0?rigidbody.velocity.x:moveSpeedRotated.x>0?(moveSpeedRotated.x>rigidbody.velocity.x?moveSpeedRotated.x:rigidbody.velocity.x):(moveSpeedRotated.x<rigidbody.velocity.x?moveSpeedRotated.x:rigidbody.velocity.x);
moveSpeedToApplyToBody.z=moveSpeedRotated.z==0?rigidbody.velocity.z:moveSpeedRotated.z>0?(moveSpeedRotated.z>rigidbody.velocity.z?moveSpeedRotated.z:rigidbody.velocity.z):(moveSpeedRotated.z<rigidbody.velocity.z?moveSpeedRotated.z:rigidbody.velocity.z);
if(!canFly){
moveSpeedToApplyToBody.y=moveSpeedRotated.y>0&&moveSpeedRotated.y>rigidbody.velocity.y?moveSpeedRotated.y:rigidbody.velocity.y;
}else{
    //Debug.LogWarning("inputMoveSpeed.y:"+inputMoveSpeed.y+"; moveSpeedRotated.y:"+moveSpeedRotated.y);
        if(inputMoveSpeed.y==0){
stopVerticalMovement();
        }
moveSpeedToApplyToBody.y=moveSpeedRotated.y==0?rigidbody.velocity.y:moveSpeedRotated.y>0?(moveSpeedRotated.y>rigidbody.velocity.y?moveSpeedRotated.y:rigidbody.velocity.y):(moveSpeedRotated.y<rigidbody.velocity.y?moveSpeedRotated.y:rigidbody.velocity.y);
}


//Debug.LogWarning(inputMoveSpeed.y+" "+moveSpeedRotated.y);


Jump=false;
        rigidbody.velocity=moveSpeedToApplyToBody;
void stopHorizontalMovement(){
stopMovement=rigidbody.velocity;
stopMovement.x=0;
stopMovement.z=0;
    rigidbody.velocity=stopMovement;
}
void stopVerticalMovement(){
stopMovement=rigidbody.velocity;
stopMovement.y=0;
    rigidbody.velocity=stopMovement;
}
}
}
protected override void Update(){
ProcessMovementInput();
                   base.Update();
if(!OutOfSight_v&&manager!=null&&manager.RemoveFarAwayActors&&
   (Mathf.Abs(Center.x-transform.position.x)>HalfSize.x||
    Mathf.Abs(Center.z-transform.position.z)>HalfSize.z)){
setOutOfSight();
}
}
#region Movement
public bool Lerp=true;
public float MoveLerpSpeed=.125f;
protected float moveLerpVal;
protected Vector3 moveLerpA;
protected Vector3 moveLerpB;
public Vector3 InputMoveAcceleration;
public Vector3 InputMaxMoveSpeed;public float MaxMoveSpeedBackwardMultiplier=1f;
protected Vector3 inputMoveSpeed;
#region Position
public float GetNewTgtPosCdTime=.05f;
protected Vector3 tgtPos;
protected Vector3 tgtPos_Pre;
protected Vector3Int tgtCoord;protected Vector3Int tgtRgn;protected int tgtIdx;protected Chunk tgtChunk;
protected float goToTgtPosTimer;
protected Vector3 drawPos;
#endregion
public float RotationLerpSpeed=.125f,HeadRotationLerpSpeed=.125f;
protected float rotationLerpVal,headRotationLerpVal;
protected Quaternion rotationLerpA,headRotationLerpA;
protected Quaternion rotationLerpB,headRotationLerpB;
public float InputViewRotationIncreaseSpeed;
[NonSerialized]protected Vector3 inputViewRotationEuler;
#region Rotation
public float GetNewTgtRotCdTime=.05f;
protected Vector3 tgtRot    ,headTgtRot;
protected Vector3 tgtRot_Pre,headTgtRot_Pre;
protected float goToTgtRotTimer,headGoToTgtRotTimer;
protected Quaternion drawRotation,headDrawRotation;
#endregion
protected virtual void ProcessMovementInput(){
if(rigidbody==null){
#region ROTATION LERP
        if(inputViewRotationEuler!=Vector3.zero){
            tgtRot+=inputViewRotationEuler;
            inputViewRotationEuler=Vector3.zero;
        }
if(Lerp){
        if(goToTgtRotTimer==0){
            if(tgtRot!=tgtRot_Pre){
if(LOG&&LOG_LEVEL<=-20)Debug.Log("input rotation detected:start rotating to tgtRot:"+tgtRot);
                rotationLerpVal=0;
                rotationLerpA=transform.rotation;
                rotationLerpB=Quaternion.Euler(tgtRot);
                tgtRot_Pre=tgtRot;
                goToTgtRotTimer+=Time.deltaTime;
            }
        }else{
            goToTgtRotTimer+=Time.deltaTime;
        }
        if(goToTgtRotTimer!=0){
            rotationLerpVal+=RotationLerpSpeed;
            if(rotationLerpVal>=1){
                rotationLerpVal=1;
                goToTgtRotTimer=0;
if(LOG&&LOG_LEVEL<=-20)Debug.Log("tgtRot:"+tgtRot+" reached");
            }
if(LOG&&LOG_LEVEL<=-30)Debug.Log("rA:"+rotationLerpA+";rB:"+rotationLerpB);
            transform.rotation=Quaternion.Lerp(rotationLerpA,rotationLerpB,rotationLerpVal);
            if(goToTgtRotTimer>GetNewTgtRotCdTime){
                if(tgtRot!=tgtRot_Pre){
if(LOG&&LOG_LEVEL<=-20)Debug.Log("get new tgtRot:"+tgtRot+";don't need to lerp all the way to old target before going to a new one");
                    goToTgtRotTimer=0;
                }
            }
        }
}else{
            transform.rotation=Quaternion.Euler(tgtRot);
}
#endregion
#region POSITION LERP
        if(inputMoveSpeed!=Vector3.zero){
            tgtPos+=(transform.rotation*inputMoveSpeed)/**(spfControl-Time.deltaTime)*//**(Time.deltaTime)*/*(Time.deltaTime/MainCamera._30FPSdeltaTime);
        }
if(Lerp){
        if(goToTgtPosTimer==0){
            if(tgtPos!=tgtPos_Pre){
if(LOG&&LOG_LEVEL<=-20)Debug.Log("input movement detected:start going to tgtPos:"+tgtPos);
                moveLerpVal=0;
                moveLerpA=transform.position;
                moveLerpB=tgtPos;
                tgtPos_Pre=tgtPos;
                goToTgtPosTimer+=Time.deltaTime;
            }
        }else{
            goToTgtPosTimer+=Time.deltaTime;
        }
        if(goToTgtPosTimer!=0){
            moveLerpVal+=MoveLerpSpeed;
            if(moveLerpVal>=1){
                moveLerpVal=1;
                goToTgtPosTimer=0;
if(LOG&&LOG_LEVEL<=-20)Debug.Log("tgtPos:"+tgtPos+" reached");
            }
            transform.position=Vector3.Lerp(moveLerpA,moveLerpB,moveLerpVal);
            if(goToTgtPosTimer>GetNewTgtPosCdTime){
                if(tgtPos!=tgtPos_Pre){
if(LOG&&LOG_LEVEL<=-20)Debug.Log("get new tgtPos:"+tgtPos+";don't need to lerp all the way to old target before going to a new one");
                    goToTgtPosTimer=0;
                }
            }
        }
}else{
    //Debug.LogWarning(this);
            transform.position=tgtPos;
}
#endregion
}else{
    headTgtRot=headEulerAngles;
    if(headGoToTgtRotTimer==0){
        if(headTgtRot!=headTgtRot_Pre){
            headRotationLerpVal=0;
            headRotationLerpA=headDrawRotation;
            headRotationLerpB=Quaternion.Euler(headTgtRot);
            headTgtRot_Pre=headTgtRot;
            headGoToTgtRotTimer+=Time.deltaTime;
        }
    }else{
        headGoToTgtRotTimer+=Time.deltaTime;
    }
    if(headGoToTgtRotTimer!=0){
        headRotationLerpVal+=HeadRotationLerpSpeed;
        if(headRotationLerpVal>=1){
            headRotationLerpVal=1;
            headGoToTgtRotTimer=0;
        }
        headDrawRotation=Quaternion.Lerp(headRotationLerpA,headRotationLerpB,headRotationLerpVal);
        if(headGoToTgtRotTimer>GetNewTgtRotCdTime){
            if(headTgtRot!=headTgtRot_Pre){
                headGoToTgtRotTimer=0;
            }
        }
    }
    tgtRot=transform.eulerAngles;
    if(goToTgtRotTimer==0){
        if(tgtRot!=tgtRot_Pre){
            rotationLerpVal=0;
            rotationLerpA=drawRotation;
            rotationLerpB=Quaternion.Euler(tgtRot);
            tgtRot_Pre=tgtRot;
            goToTgtRotTimer+=Time.deltaTime;
        }
    }else{
        goToTgtRotTimer+=Time.deltaTime;
    }
    if(goToTgtRotTimer!=0){
        rotationLerpVal+=RotationLerpSpeed;
        if(rotationLerpVal>=1){
            rotationLerpVal=1;
            goToTgtRotTimer=0;
        }
        drawRotation=Quaternion.Lerp(rotationLerpA,rotationLerpB,rotationLerpVal);
        if(goToTgtRotTimer>GetNewTgtRotCdTime){
            if(tgtRot!=tgtRot_Pre){
                goToTgtRotTimer=0;
            }
        }
    }
    tgtPos=transform.position;


    //Debug.LogWarning("tgtPos:"+tgtPos);


    if(goToTgtPosTimer==0){
        if(tgtPos!=tgtPos_Pre){
            moveLerpVal=0;
            moveLerpA=drawPos;
            moveLerpB=tgtPos;
            tgtPos_Pre=tgtPos;
            goToTgtPosTimer+=Time.deltaTime;
        }
    }else{
        goToTgtPosTimer+=Time.deltaTime;
    }
    if(goToTgtPosTimer!=0){
        moveLerpVal+=MoveLerpSpeed;
        if(moveLerpVal>=1){
            moveLerpVal=1;
            goToTgtPosTimer=0;
        }
        drawPos=Vector3.Lerp(moveLerpA,moveLerpB,moveLerpVal);
        if(goToTgtPosTimer>GetNewTgtPosCdTime){
            if(tgtPos!=tgtPos_Pre){
                goToTgtPosTimer=0;
            }
        }
    }
}
}
#endregion
public override void Teleport(Quaternion rotation,Vector3 position,bool goThroughWalls=false){
        //Debug.LogWarning(rotation*transform.forward);
                base.Teleport(rotation,position,goThroughWalls);
        headEulerAngles=rotation.eulerAngles;
        //tgtRot=tgtRot_Pre=rotation.eulerAngles;
}
#region
protected override void LateUpdate(){
                   base.LateUpdate();
//if(renderer!=null){
if(IsUMA){
//    Debug.LogWarning("renderer:"+renderer+" renderer.transform.parent:"+renderer.transform.parent.gameObject,renderer);
//renderer.transform.parent.parent.rotation=drawRotation;
//renderer.transform.parent.parent.localPosition=drawPos;
umaData.transform.rotation=drawRotation;
umaData.transform.position=drawPos;
    //Debug.LogWarning("umaData.transform.position:"+umaData.transform.position);
}
//}
}
#endregion
#if UNITY_EDITOR
protected override void OnDrawGizmos(){
                   base.OnDrawGizmos();
}
#endif
}
public interface iCamFollowable{
string ObjName{get;}
LinkedListNode<iCamFollowable>CamFollowableNode{get;set;}
bool BeingCamFollowed{get;set;}
Vector3 CamLookAtUp{get;set;}
Vector3 CamLookAtForward{get;set;}
Vector3 CamPosition{get;set;}
}