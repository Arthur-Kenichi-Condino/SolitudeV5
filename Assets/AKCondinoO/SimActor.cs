using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimActor:SimObject{
protected override void Awake(){
                   base.Awake();
tgtPos=tgtPos_Pre=transform.position;
tgtRot=tgtRot_Pre=transform.eulerAngles;
}
[NonSerialized]Vector3 eulerAngles,headEulerAngles;[NonSerialized]float angleBetweenBodyAndHead;[NonSerialized]Vector3 stopMovement,moveSpeedRotated,moveSpeedToApplyToBody;
protected override void FixedUpdate(){
                   base.FixedUpdate();
if(rigidbody!=null){
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
moveSpeedToApplyToBody.x=moveSpeedRotated.x==0?rigidbody.velocity.x:moveSpeedRotated.x>0?(moveSpeedRotated.x>rigidbody.velocity.x?moveSpeedRotated.x:rigidbody.velocity.x):(moveSpeedRotated.x<rigidbody.velocity.x?moveSpeedRotated.x:rigidbody.velocity.x);
moveSpeedToApplyToBody.z=moveSpeedRotated.z==0?rigidbody.velocity.z:moveSpeedRotated.z>0?(moveSpeedRotated.z>rigidbody.velocity.z?moveSpeedRotated.z:rigidbody.velocity.z):(moveSpeedRotated.z<rigidbody.velocity.z?moveSpeedRotated.z:rigidbody.velocity.z);
moveSpeedToApplyToBody.y=moveSpeedRotated.y>0&&moveSpeedRotated.y>rigidbody.velocity.y?moveSpeedRotated.y:rigidbody.velocity.y;
        rigidbody.velocity=moveSpeedToApplyToBody;
}
}
protected override void Update(){
ProcessMovementInput();
                   base.Update();
}
#region Movement
public bool Lerp=true;
public float MoveLerpSpeed=.125f;
protected float moveLerpVal;
protected Vector3 moveLerpA;
protected Vector3 moveLerpB;
public Vector3 InputMoveAcceleration;
public Vector3 InputMaxMoveSpeed;
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
#endregion
#region POSITION LERP
        if(inputMoveSpeed!=Vector3.zero){
            tgtPos+=transform.rotation*inputMoveSpeed;
        }
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
    tgtPos=transform.position;
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