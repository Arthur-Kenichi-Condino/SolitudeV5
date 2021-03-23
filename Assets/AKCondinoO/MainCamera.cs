using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
public class MainCamera:SimActor{
public static Dictionary<string,object[]>AllCommands=new Dictionary<string,object[]>();public static Dictionary<string,object[]>AllStates=new Dictionary<string,object[]>();
public static readonly LinkedList<iCamFollowable>CamFollowables=new LinkedList<iCamFollowable>();public iCamFollowable CamFollowing=null;public SimActor CamControlled=null;
protected override void Awake(){
                   base.Awake();
foreach(FieldInfo field in typeof(Commands).GetFields(BindingFlags.Public|BindingFlags.Static)){
if(field.GetValue(null)is object[]command){
if(LOG&&LOG_LEVEL<=1)Debug.Log("add command:"+field.Name);
AllCommands.Add(field.Name,command);
}    
}
foreach(FieldInfo field in typeof(Enabled).GetFields(BindingFlags.Public|BindingFlags.Static)){
if(field.GetValue(null)is object[]state){
if(LOG&&LOG_LEVEL<=1)Debug.Log("add command state status fields:"+field.Name);
AllStates.Add(field.Name,state);
}
}
}
[NonSerialized]public bool Focus=true;
private void OnApplicationFocus(bool focus){Focus=focus;}
[NonSerialized]public bool Escape;
protected override void Update(){


ProcessGameCommands();


Escape=Input.GetKey(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyUp(KeyCode.Escape);
foreach(var command in AllCommands){UpdateCommandState(command);}
Enabled.PAUSE[0]=(bool)Enabled.PAUSE[0]||Escape||!Focus;
if((bool)Enabled.PAUSE[0]!=(bool)Enabled.PAUSE[1]){
    if((bool)Enabled.PAUSE[0]){
Cursor.visible=true;
Cursor.lockState=CursorLockMode.None;
    }else{
Cursor.visible=false;
Cursor.lockState=CursorLockMode.Locked;
    }
}
Enabled.MOUSE_ROTATION_DELTA_X[1]=Enabled.MOUSE_ROTATION_DELTA_X[0];Enabled.MOUSE_ROTATION_DELTA_X[0]=Commands.ROTATION_SENSITIVITY_X*Input.GetAxis("Mouse X");
Enabled.MOUSE_ROTATION_DELTA_Y[1]=Enabled.MOUSE_ROTATION_DELTA_Y[0];Enabled.MOUSE_ROTATION_DELTA_Y[0]=Commands.ROTATION_SENSITIVITY_Y*Input.GetAxis("Mouse Y");


//if(CamControlled==null){
//}else if(CamFollowing is SimActor followed&&followed!=CamControlled){
//}
//if(CamControlled==null){
//if(CamFollowing!=null&&CamFollowing is SimActor followed){
//}else if(CamFollowables.Count>0){
//}
//}


if(!(bool)Enabled.PAUSE[0]){
if((bool)Enabled.SWITCH_CAMERA_MODE[0]!=(bool)Enabled.SWITCH_CAMERA_MODE[1]){
if((bool)Enabled.SWITCH_CAMERA_MODE[0]){
if(CamFollowables.Count>0){
CamFollowing=CamFollowables.First.Value;CamFollowables.RemoveFirst();CamFollowables.AddLast(CamFollowing.CamFollowableNode);
CamFollowing.BeingCamFollowed=true;
Lerp=false;
if(LOG&&LOG_LEVEL<=1)Debug.Log("camera is now following "+CamFollowing.ObjName+"; CamFollowables.Count:"+CamFollowables.Count);
}
}else{
if(CamFollowing!=null)CamFollowing.BeingCamFollowed=false;
CamFollowing=null;
Lerp=true;
}


if(!(CamFollowing is SimActor followed)){
                    Debug.LogWarning("followed is invalid to be cam controlled character");
CamControlled=null;
}else if(followed!=CamControlled){
                    Debug.LogWarning("set followed["+followed+"] as new cam controlled character");
CamControlled=followed;
}


}


if(CamControlled==null&&!(CamFollowing is SimActor)){
foreach(var followable in CamFollowables){
if(followable is SimActor actor){

                        
                    Debug.LogWarning("validate cam controlled:set actor["+actor+"] as new cam controlled character");
CamControlled=actor;


break;}
}
}
//if(CamControlled==null&&CamFollowing==null){
//                    Debug.LogWarning("get new cam controlled character");
//if(CamFollowables.Count>0){
//foreach(var followable in CamFollowables){
//if(followable is SimActor actor){





//}
//}
//}
//}


if(CamFollowing==null){
#region FORWARD BACKWARD
    if((bool)Enabled.FORWARD [0]){inputMoveSpeed.z+=InputMoveAcceleration.z;} 
    if((bool)Enabled.BACKWARD[0]){inputMoveSpeed.z-=InputMoveAcceleration.z;}
        if(!(bool)Enabled.FORWARD[0]&&!(bool)Enabled.BACKWARD[0]){inputMoveSpeed.z=0;}
            if( inputMoveSpeed.z>InputMaxMoveSpeed.z){inputMoveSpeed.z= InputMaxMoveSpeed.z;}
            if(-inputMoveSpeed.z>InputMaxMoveSpeed.z){inputMoveSpeed.z=-InputMaxMoveSpeed.z;}
#endregion
#region RIGHT LEFT
    if((bool)Enabled.RIGHT   [0]){inputMoveSpeed.x+=InputMoveAcceleration.x;} 
    if((bool)Enabled.LEFT    [0]){inputMoveSpeed.x-=InputMoveAcceleration.x;}
        if(!(bool)Enabled.RIGHT[0]&&!(bool)Enabled.LEFT[0]){inputMoveSpeed.x=0;}
            if( inputMoveSpeed.x>InputMaxMoveSpeed.x){inputMoveSpeed.x= InputMaxMoveSpeed.x;}
            if(-inputMoveSpeed.x>InputMaxMoveSpeed.x){inputMoveSpeed.x=-InputMaxMoveSpeed.x;}
#endregion
#region ROTATE
inputViewRotationEuler.x+=-Enabled.MOUSE_ROTATION_DELTA_Y[0]*InputViewRotationIncreaseSpeed;
inputViewRotationEuler.y+= Enabled.MOUSE_ROTATION_DELTA_X[0]*InputViewRotationIncreaseSpeed;
inputViewRotationEuler.x=inputViewRotationEuler.x%360;
inputViewRotationEuler.y=inputViewRotationEuler.y%360;
#endregion
}else{
inputMoveSpeed=Vector3.zero;
    tgtRot=Quaternion.LookRotation(CamFollowing.CamLookAtForward,CamFollowing.CamLookAtUp).eulerAngles;tgtPos=CamFollowing.CamPosition;
}
}else{
inputMoveSpeed=Vector3.zero;
}
                   base.Update();
}
#region UI-to-Command translating
Vector3 _coordToSignedCoord{get;}=new Vector3(-Chunk.Width/2f,-Chunk.Height/2f,-Chunk.Depth/2f);Vector3 _adjToCenter{get;}=new Vector3(-.5f,-.5f,-.5f);
public enum SelectedGameModeTool:int{
None=-1,
SimInteractionWheel=0,
TerrainCarveCube=1,
TerrainCarveSphere=2,
Hand=3,
Water=4,
}[NonSerialized]public SelectedGameModeTool CurrentTool=SelectedGameModeTool.SimInteractionWheel;[NonSerialized]SelectedGameModeTool LastTool=SelectedGameModeTool.SimInteractionWheel;


void ProcessGameCommands(){


UICore.DetectMouseOnUI();


    //Debug.LogWarning("UICore.UIReceivedInput:"+UICore.UIReceivedInput);


if(CurrentTool!=LastTool){
SwitchTool(LastTool,CurrentTool);
LastTool=CurrentTool;
}
Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);


Vector3 hitPoint;bool hitTerrain=false;
if(Physics.Raycast(ray,out RaycastHit hit,1000f)){
hitPoint=hit.point;hitTerrain=hit.collider.GetComponent<TerrainChunk>()!=null;
}else{
hitPoint=ray.origin+ray.direction*1000f;
}


Debug.DrawLine(ray.origin,hitPoint);
switch(CurrentTool){
case(SelectedGameModeTool.SimInteractionWheel):{

        
if(hit.normal!=Vector3.zero){

                        
if(!UICore.UIReceivedInput&&!(bool)Enabled.ACTION_1[0]&&(bool)Enabled.ACTION_1[0]!=(bool)Enabled.ACTION_1[1]){
    Debug.LogWarning("action SimInteractionWheel");


if(CamControlled is AI controllable){
controllable.MoveTo(tgt:ray);
}


}


}


    break;
}
case(SelectedGameModeTool.TerrainCarveCube):{


Vector2Int cnkRgn=ChunkManager.PosToRgn(hitPoint);
Vector3Int vCoord=Chunk.PosToCoord(hitPoint);
Vector3 previewPos=vCoord+_coordToSignedCoord;
        previewPos.x+=cnkRgn.x;
        previewPos.z+=cnkRgn.y;
    //Debug.LogWarning(previewPos);


if(TerrainCarveCubeIndicators[0]!=null){TerrainCarveCubeIndicators[0].transform.position=previewPos+Vector3.Scale(_adjToCenter,TerrainCarveCubeIndicators[0].transform.lossyScale);TerrainCarveCubeIndicators[0].transform.rotation=Quaternion.identity;}
if(hitTerrain){
if(!UICore.UIReceivedInput&&!(bool)Enabled.ACTION_1[0]&&(bool)Enabled.ACTION_1[0]!=(bool)Enabled.ACTION_1[1]){

    Debug.LogWarning("action Edit at "+previewPos+"; ..."+Enabled.ACTION_1[0]+"!="+Enabled.ACTION_1[1]);
//ChunkManager.main.Edit(previewPos,new Vector3Int(1,1,1),51,MaterialId.Dirt,ChunkManager.EditMode.Cube);

}
}
    break;
}
}

   
if((bool)Enabled.ACTION_1[0]&&(bool)Enabled.ACTION_1[0]!=(bool)Enabled.ACTION_1[1]){
    Debug.LogWarning("ResetMouseCheck");
    

UICore.ResetMouseCheck();


}
}
public GameObject[]TerrainCarveCubeIndicators;
void SwitchTool(SelectedGameModeTool LastTool,SelectedGameModeTool CurrentTool){
switch(LastTool){
case(SelectedGameModeTool.TerrainCarveCube):{
foreach(var indicator in TerrainCarveCubeIndicators){
indicator.SetActive(false);
}
    break;
}
}
switch(CurrentTool){
case(SelectedGameModeTool.TerrainCarveCube):{
foreach(var indicator in TerrainCarveCubeIndicators){
indicator.SetActive(true);
}
    break;
}
}
}


#endregion
[NonSerialized]string _name;[NonSerialized]object[]_command;[NonSerialized]object[]_state;
void UpdateCommandState(KeyValuePair<string,object[]>command){_name=command.Key;_command=command.Value;_state=AllStates[_name];
_state[1]=_state[0];
if(_command.Length==2&&_command[1]is string){
_state[0]=Get(_command[0],mode:(string)_command[1],(bool)_state[0]);
}
}
bool Get(object command,string mode,bool currState){
if(mode.Contains("alternate")){
    if((mode=="alternateUp"  &&(command is KeyCode?Input.GetKeyUp  ((KeyCode)command):command is int?Input.GetMouseButtonUp  ((int)command):Input.GetButtonUp  ((string)command)))||
       (mode=="alternateDown"&&(command is KeyCode?Input.GetKeyDown((KeyCode)command):command is int?Input.GetMouseButtonDown((int)command):Input.GetButtonDown((string)command)))){
    return!currState;
    }
    return currState;
}
return mode=="activeHeld"?(command is KeyCode?Input.GetKey    ((KeyCode)command):command is int?Input.GetMouseButton    ((int)command):Input.GetButton    ((string)command)):
      (mode==    "whenUp"?(command is KeyCode?Input.GetKeyUp  ((KeyCode)command):command is int?Input.GetMouseButtonUp  ((int)command):Input.GetButtonUp  ((string)command)):
                          (command is KeyCode?Input.GetKeyDown((KeyCode)command):command is int?Input.GetMouseButtonDown((int)command):Input.GetButtonDown((string)command)));
}
}
public static class Enabled{
public static readonly object[]PAUSE={true,true};
public static readonly object[]FORWARD ={false,false};
public static readonly object[]BACKWARD={false,false};
public static readonly object[]RIGHT   ={false,false};
public static readonly object[]LEFT    ={false,false};
public static readonly object[]JUMP    ={false,false};
public static readonly float[]MOUSE_ROTATION_DELTA_X={0,0};
public static readonly float[]MOUSE_ROTATION_DELTA_Y={0,0};
public static readonly object[]SWITCH_CAMERA_MODE={false,false};//  Free camera or follow and command a player
public static readonly object[]ACTION_1={false,false};
public static readonly object[]ACTION_2={false,false};
public static readonly object[]INTERACT={false,false,0f};
}
public static class Commands{
public static object[]PAUSE={KeyCode.Tab,"alternateDown"};
public static object[]FORWARD ={KeyCode.W,"activeHeld"};
public static object[]BACKWARD={KeyCode.S,"activeHeld"};
public static object[]RIGHT   ={KeyCode.D,"activeHeld"};
public static object[]LEFT    ={KeyCode.A,"activeHeld"};
public static object[]JUMP    ={KeyCode.E,"whenUp"};
public static float ROTATION_SENSITIVITY_X=360.0f;
public static float ROTATION_SENSITIVITY_Y=360.0f;
public static object[]SWITCH_CAMERA_MODE={KeyCode.RightAlt,"alternateDown"};  //  Free or following the player
public static object[]ACTION_1={(int)0,"activeHeld"};
public static object[]ACTION_2={(int)1,"activeHeld"};
public static object[]INTERACT={KeyCode.G,"holdDelay",1f};//  So you can't, for example, "steal an item" instantly
}