﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
public class MainCamera:MonoBehaviour{
public static Dictionary<string,object[]>AllCommands=new Dictionary<string,object[]>();public static Dictionary<string,bool[]>AllStates=new Dictionary<string,bool[]>();
void Awake(){
foreach(FieldInfo field in typeof(Commands).GetFields(BindingFlags.Public|BindingFlags.Static)){
if(field.GetValue(null)is object[]command){


    AllCommands.Add(field.Name,command);
    //Debug.LogWarning(field.FieldType);


}    
}
foreach(FieldInfo field in typeof(Enabled).GetFields(BindingFlags.Public|BindingFlags.Static)){

    
    Debug.LogWarning(field.FieldType);


}
}
void Update(){
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