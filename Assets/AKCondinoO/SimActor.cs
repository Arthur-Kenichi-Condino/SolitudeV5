﻿using AKCondinoO.Voxels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimActor:SimObject{
protected override void Awake(){
tgtPos=tgtPos_Pre=transform.position;
                   base.Awake();
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
#endregion
protected float rotationLerpVal,headRotationLerpVal;
protected Quaternion rotationLerpA,headRotationLerpA;
protected Quaternion rotationLerpB,headRotationLerpB;
public float InputViewRotationIncreaseSpeed;
protected Vector3 inputViewRotationEuler;
#region Rotation
protected Vector3 tgtRot    ,headTgtRot;
protected Vector3 tgtRot_Pre,headTgtRot_Pre;
protected float goToTgtRotTimer,headGoToTgtRotTimer;
#endregion
protected virtual void ProcessMovementInput(){
    if(rigidbody==null){
#region ROTATION LERP
        if(inputViewRotationEuler!=Vector3.zero){
            inputViewRotationEuler=Vector3.zero;
            tgtRot+=inputViewRotationEuler;
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
    }
}
#endregion
}