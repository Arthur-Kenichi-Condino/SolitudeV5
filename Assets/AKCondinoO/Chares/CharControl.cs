//using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using static ActorManagementMentana;
public class CharControl:AI,iCamFollowable{
[NonSerialized]public static string[]saveSubfolder=new string[1];
public string ObjName{get{return gameObject.name;}}
public LinkedListNode<iCamFollowable>CamFollowableNode{get;set;}
public bool BeingCamFollowed{get;set;}
public Vector3 CamLookAtUp{get;set;}
public Vector3 CamLookAtForward{get;set;}
public Vector3 CamPosition{get;set;}
public Vector3 CamOffset;
public int uniqueId=-1;public string prefabName;
[NonSerialized]public DynamicCharacterAvatar avatar;[NonSerialized]public Dictionary<string,DnaSetter>dna;
protected override void Awake(){
                   base.Awake();
CamFollowableNode=MainCamera.CamFollowables.AddLast(this);


if(avatar==null){
avatar=GetComponentInChildren<DynamicCharacterAvatar>();
if(avatar!=null){
dna=avatar.GetDNA();
}}


//Debug.LogWarning(saveFolder+this.name.Replace("(Clone)","").ToString());


//if(avatar==null){
//avatar=GetComponentInChildren<DynamicCharacterAvatar>();
//if(avatar!=null){
//Debug.LogWarning("avatar:"+avatar);
//avatar.savePathType=DynamicCharacterAvatar.savePathTypes.FileSystem;
//Directory.CreateDirectory(avatar.savePath=saveSubfolder[0]=saveFolder+this.name.Replace("(Clone)","").ToString()+"/");saveSubfolder[0]+=(avatar.saveFilename="recipe")+".txt";
//Debug.LogWarning("saveSubfolder[0]:"+saveSubfolder[0]);


////file exists


//if(File.Exists(saveSubfolder[0])){avatar.SetLoadFilename(saveSubfolder[0],DynamicCharacterAvatar.loadPathTypes.FileSystem);}

    

////DoSave on disable
////;
            



////avatar.SetLoadFilename(saveSubfolder[0],DynamicCharacterAvatar.loadPathTypes.FileSystem);


////SetLoadFilename string replace (Clone)
////avatar.loadPathType=DynamicCharacterAvatar.loadPathTypes.FileSystem;
//}}


}
protected override void Update(){
if(!loaded){




loaded=true;}


//if(!initialized){initialized=true;
//if(avatar!=null&&!File.Exists(saveSubfolder[0])){
//avatar.DoSave();
//avatar.SetLoadFilename(saveSubfolder[0],DynamicCharacterAvatar.loadPathTypes.FileSystem);
//}
//}
                   base.Update();
}[NonSerialized]protected bool loaded=false;//[NonSerialized]bool initialized=false;
[NonSerialized]Vector3 _camPos=new Vector3();[NonSerialized]Vector3 _camRotatedOffset=new Vector3(1,0,1);
protected override void ProcessMovementInput(){
if(!(bool)Enabled.PAUSE[0]){
if(BeingCamFollowed){
if(IsGrounded||!HittingWall){
#region FORWARD BACKWARD
    if((bool)Enabled.FORWARD [0]){inputMoveSpeed.z+=InputMoveAcceleration.z;Autonomous=AutonomyDelayAfterControl;} 
    if((bool)Enabled.BACKWARD[0]){inputMoveSpeed.z-=InputMoveAcceleration.z;Autonomous=AutonomyDelayAfterControl;}
        if(!(bool)Enabled.FORWARD[0]&&!(bool)Enabled.BACKWARD[0]){inputMoveSpeed.z=0;}
            if( inputMoveSpeed.z>InputMaxMoveSpeed.z                               ){inputMoveSpeed.z= InputMaxMoveSpeed.z                               ;}
            if(-inputMoveSpeed.z>InputMaxMoveSpeed.z*MaxMoveSpeedBackwardMultiplier){inputMoveSpeed.z=-InputMaxMoveSpeed.z*MaxMoveSpeedBackwardMultiplier;}
#endregion
#region RIGHT LEFT
    if((bool)Enabled.RIGHT   [0]){inputMoveSpeed.x+=InputMoveAcceleration.x;Autonomous=AutonomyDelayAfterControl;} 
    if((bool)Enabled.LEFT    [0]){inputMoveSpeed.x-=InputMoveAcceleration.x;Autonomous=AutonomyDelayAfterControl;}
        if(!(bool)Enabled.RIGHT[0]&&!(bool)Enabled.LEFT[0]){inputMoveSpeed.x=0;}
            if( inputMoveSpeed.x>InputMaxMoveSpeed.x){inputMoveSpeed.x= InputMaxMoveSpeed.x;}
            if(-inputMoveSpeed.x>InputMaxMoveSpeed.x){inputMoveSpeed.x=-InputMaxMoveSpeed.x;}
#endregion
}else{
inputMoveSpeed.x=0;
inputMoveSpeed.z=0;
}


Debug.LogWarning((bool)Enabled.JUMP[0]+" "+IsGrounded);
if(IsGrounded&&(Jump||(Jump=(bool)Enabled.JUMP[0]!=(bool)Enabled.JUMP[1]))){
inputMoveSpeed.y=InputMaxMoveSpeed.y;Autonomous=AutonomyDelayAfterControl;


//Debug.LogWarning(inputMoveSpeed.y);


}else{
inputMoveSpeed.y=0;
}


#region ROTATE
inputViewRotationEuler.x+=-Enabled.MOUSE_ROTATION_DELTA_Y[0]*InputViewRotationIncreaseSpeed;
inputViewRotationEuler.y+= Enabled.MOUSE_ROTATION_DELTA_X[0]*InputViewRotationIncreaseSpeed;
inputViewRotationEuler.x=inputViewRotationEuler.x%360;
inputViewRotationEuler.y=inputViewRotationEuler.y%360;
if(Enabled.MOUSE_ROTATION_DELTA_Y[0]!=0||Enabled.MOUSE_ROTATION_DELTA_X[0]!=0)Autonomous=AutonomyDelayAfterControl;
#endregion
}
}
                   base.ProcessMovementInput();
}
protected override void LateUpdate(){
                   base.LateUpdate();
if(BeingCamFollowed){
_camPos=renderer!=null?renderer.transform.position:drawPos;
_camPos.y+=CamOffset.y;
_camPos+=headDrawRotation*Vector3.Scale(_camRotatedOffset,CamOffset);
    //CamPosition=drawPos+(headDrawRotation*CamOffset);
    CamPosition=_camPos;
    CamLookAtForward=((headDrawRotation*(Vector3.forward*1000))-CamPosition).normalized;
    //CamLookAtUp=transform.up;
    CamLookAtUp=Vector3.Cross(CamLookAtForward,headDrawRotation*Vector3.right).normalized;
}
}
#if UNITY_EDITOR
protected override void OnDrawGizmos(){
                   base.OnDrawGizmos();
}
#endif
}