using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
public class MainCamera:SimActor{
public static Dictionary<string,object[]>AllCommands=new Dictionary<string,object[]>();public static Dictionary<string,bool[]>AllStates=new Dictionary<string,bool[]>();
protected override void Awake(){
                   base.Awake();
foreach(FieldInfo field in typeof(Commands).GetFields(BindingFlags.Public|BindingFlags.Static)){
if(field.GetValue(null)is object[]command){
if(LOG&&LOG_LEVEL<=1)Debug.Log("add command:"+field.Name);
AllCommands.Add(field.Name,command);
}    
}
foreach(FieldInfo field in typeof(Enabled).GetFields(BindingFlags.Public|BindingFlags.Static)){
if(field.GetValue(null)is bool[]state){
if(LOG&&LOG_LEVEL<=1)Debug.Log("add command state status fields:"+field.Name);
AllStates.Add(field.Name,state);
}
}
}
[NonSerialized]public bool Focus=true;
private void OnApplicationFocus(bool focus){Focus=focus;}
[NonSerialized]public bool Escape;
protected override void Update(){
Escape=Input.GetKey(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyUp(KeyCode.Escape);
foreach(var command in AllCommands){UpdateCommandState(command);}
Enabled.PAUSE[0]=Enabled.PAUSE[0]||Escape||!Focus;
if(Enabled.PAUSE[0]!=Enabled.PAUSE[1]){
    if(Enabled.PAUSE[0]){
Cursor.visible=true;
Cursor.lockState=CursorLockMode.None;
    }else{
Cursor.visible=false;
Cursor.lockState=CursorLockMode.Locked;
    }
}
Enabled.MOUSE_ROTATION_DELTA_X[1]=Enabled.MOUSE_ROTATION_DELTA_X[0];Enabled.MOUSE_ROTATION_DELTA_X[0]=Commands.ROTATION_SENSITIVITY_X*Input.GetAxis("Mouse X");
Enabled.MOUSE_ROTATION_DELTA_Y[1]=Enabled.MOUSE_ROTATION_DELTA_Y[0];Enabled.MOUSE_ROTATION_DELTA_Y[0]=Commands.ROTATION_SENSITIVITY_Y*Input.GetAxis("Mouse Y");
if(!Enabled.PAUSE[0]){
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
#endregion
}
                   base.Update();
}
[NonSerialized]string _name;[NonSerialized]object[]_command;[NonSerialized]bool[]_state;
void UpdateCommandState(KeyValuePair<string,object[]>command){_name=command.Key;_command=command.Value;_state=AllStates[_name];
_state[1]=_state[0];
if(_command.Length==2&&_command[1]is string){
_state[0]=Get(_command[0],mode:(string)_command[1],_state[0]);
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
public static readonly bool[]PAUSE={true,true};
public static readonly bool[]FORWARD ={false,false};
public static readonly bool[]BACKWARD={false,false};
public static readonly bool[]RIGHT   ={false,false};
public static readonly bool[]LEFT    ={false,false};
public static readonly float[]MOUSE_ROTATION_DELTA_X={0,0};
public static readonly float[]MOUSE_ROTATION_DELTA_Y={0,0};
}
public static class Commands{
public static object[]PAUSE={KeyCode.Tab,"alternateDown"};
public static object[]FORWARD ={KeyCode.W,"activeHeld"};
public static object[]BACKWARD={KeyCode.S,"activeHeld"};
public static object[]RIGHT   ={KeyCode.D,"activeHeld"};
public static object[]LEFT    ={KeyCode.A,"activeHeld"};
public static float ROTATION_SENSITIVITY_X=360.0f;
public static float ROTATION_SENSITIVITY_Y=360.0f;
}