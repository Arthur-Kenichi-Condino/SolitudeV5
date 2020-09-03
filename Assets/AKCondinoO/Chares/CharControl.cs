using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CharControl:AI,iCamFollowable{
public string ObjName{get{return gameObject.name;}}
public LinkedListNode<iCamFollowable>CamFollowableNode{get;set;}
public bool BeingCamFollowed{get;set;}
public Vector3 CamLookAtUp{get;set;}
public Vector3 CamLookAtForward{get;set;}
public Vector3 CamPosition{get;set;}
public Vector3 CamOffset;
protected override void Awake(){
                   base.Awake();
CamFollowableNode=MainCamera.CamFollowables.AddLast(this);
}
protected override void ProcessMovementInput(){
if(!Enabled.PAUSE[0]){
if(BeingCamFollowed){
#region FORWARD BACKWARD
    if(Enabled.FORWARD [0]){inputMoveSpeed.z+=InputMoveAcceleration.z;} 
    if(Enabled.BACKWARD[0]){inputMoveSpeed.z-=InputMoveAcceleration.z;}
        if(!Enabled.FORWARD[0]&&!Enabled.BACKWARD[0]){inputMoveSpeed.z=0;}
            if( inputMoveSpeed.z>InputMaxMoveSpeed.z){inputMoveSpeed.z= InputMaxMoveSpeed.z;}
            if(-inputMoveSpeed.z>InputMaxMoveSpeed.z){inputMoveSpeed.z=-InputMaxMoveSpeed.z;}
#endregion
#region RIGHT LEFT
    if(Enabled.RIGHT   [0]){inputMoveSpeed.x+=InputMoveAcceleration.x;} 
    if(Enabled.LEFT    [0]){inputMoveSpeed.x-=InputMoveAcceleration.x;}
        if(!Enabled.RIGHT[0]&&!Enabled.LEFT[0]){inputMoveSpeed.x=0;}
            if( inputMoveSpeed.x>InputMaxMoveSpeed.x){inputMoveSpeed.x= InputMaxMoveSpeed.x;}
            if(-inputMoveSpeed.x>InputMaxMoveSpeed.x){inputMoveSpeed.x=-InputMaxMoveSpeed.x;}
#endregion
#region ROTATE
inputViewRotationEuler.x+=-Enabled.MOUSE_ROTATION_DELTA_Y[0]*InputViewRotationIncreaseSpeed;
inputViewRotationEuler.y+= Enabled.MOUSE_ROTATION_DELTA_X[0]*InputViewRotationIncreaseSpeed;
inputViewRotationEuler.x=inputViewRotationEuler.x%360;
inputViewRotationEuler.y=inputViewRotationEuler.y%360;
#endregion
}
}
                   base.ProcessMovementInput();
}
#if UNITY_EDITOR
protected override void OnDrawGizmos(){
                   base.OnDrawGizmos();
}
#endif
}