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
protected override void Update(){
foreach(var command in AllCommands){UpdateCommandState(command);}
    if(Enabled.FORWARD [0]){inputMoveSpeed.z+=InputMoveAcceleration.z;} 
    if(Enabled.BACKWARD[0]){inputMoveSpeed.z-=InputMoveAcceleration.z;}
        if(!Enabled.FORWARD[0]&&!Enabled.BACKWARD[0]){inputMoveSpeed.z=0;}
            if( inputMoveSpeed.z>InputMaxMoveSpeed.z){inputMoveSpeed.z= InputMaxMoveSpeed.z;}
            if(-inputMoveSpeed.z>InputMaxMoveSpeed.z){inputMoveSpeed.z=-InputMaxMoveSpeed.z;}
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
public static readonly bool[]FORWARD ={false,false};
public static readonly bool[]BACKWARD={false,false};
public static readonly bool[]RIGHT   ={false,false};
public static readonly bool[]LEFT    ={false,false};
}
public static class Commands{
public static object[]FORWARD ={KeyCode.W,"activeHeld"};
public static object[]BACKWARD={KeyCode.S,"activeHeld"};
public static object[]RIGHT   ={KeyCode.D,"activeHeld"};
public static object[]LEFT    ={KeyCode.A,"activeHeld"};
}