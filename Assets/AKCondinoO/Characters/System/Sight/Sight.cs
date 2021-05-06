using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Sight:MonoBehaviour{
public bool LOG=false;public int LOG_LEVEL=1;
[NonSerialized]int stopRaycastLayer;
void Awake(){
stopRaycastLayer=1<<LayerMask.NameToLayer("Default")|1<<LayerMask.NameToLayer("Char");
}
[SerializeField]protected float hearingRange=5f;[NonSerialized]public readonly Dictionary<int,(AI actor,Vector3 pos,bool soundHeard)>IsInHearingSight=new Dictionary<int,(AI,Vector3,bool)>();[SerializeField]protected float fieldOfViewAngle=180f;[NonSerialized]public readonly Dictionary<int,(AI actor,Vector3 pos,bool directSight)>IsInVisionSight=new Dictionary<int,(AI,Vector3,bool)>();
protected void OnTriggerStay(Collider other){
if(other.CompareTag("Player")){
AI actor=other.GetComponent<AI>();
if(actor!=null){
if(LOG&&LOG_LEVEL<=-10)Debug.Log(other.name+" is in sight...",this.transform.parent);
if(Vector3.Distance(other.transform.position,transform.parent.position)<=hearingRange){
if(LOG&&LOG_LEVEL<=-10)Debug.Log(other.name+" ...and in hearing range...",this.transform.parent);
if(!IsInHearingSight.ContainsKey(actor.Id)){
IsInHearingSight.Add(actor.Id,(actor,new Vector3(float.NaN,float.NaN,float.NaN),false));
}
}else{
if(LOG&&LOG_LEVEL<=-10)Debug.Log(other.name+" ...but not in hearing range",this.transform.parent);
if(IsInHearingSight.ContainsKey(actor.Id)){
IsInHearingSight.Remove(actor.Id);
}
}
Vector3 forward=transform.parent.forward,toPlayer=(other.transform.position-transform.parent.position).normalized;float angle=Vector3.Angle(toPlayer,forward);
if(angle<=fieldOfViewAngle*.5f){
if(LOG&&LOG_LEVEL<=-10)Debug.Log(other.name+" ...and in vision range...",this.transform.parent);
if(!IsInVisionSight.ContainsKey(actor.Id)){
IsInVisionSight.Add(actor.Id,(actor,new Vector3(float.NaN,float.NaN,float.NaN),false));
}
if(!Physics.Raycast(transform.position,(other.transform.position-transform.position).normalized,out RaycastHit hit,Vector3.Distance(other.transform.position,transform.position),stopRaycastLayer)||hit.collider==other){
if(LOG&&LOG_LEVEL<=-10)Debug.Log(other.name+" ...with direct sight",this.transform.parent);
var tuple=IsInVisionSight[actor.Id];
    tuple.pos=actor.transform.position;
    tuple.directSight=true;
IsInVisionSight[actor.Id]=tuple;
}else{
if(LOG&&LOG_LEVEL<=-10)Debug.Log(other.name+" ...but blocked by obstacle",this.transform.parent);
var tuple=IsInVisionSight[actor.Id];
    tuple.pos=new Vector3(float.NaN,float.NaN,float.NaN);
    tuple.directSight=false;
IsInVisionSight[actor.Id]=tuple;
}
}else{
if(LOG&&LOG_LEVEL<=-10)Debug.Log(other.name+" ...but not in vision range",this.transform.parent);
if(IsInVisionSight.ContainsKey(actor.Id)){
IsInVisionSight.Remove(actor.Id);
}
}
}
}
}
private void OnTriggerExit(Collider other){
if(other.CompareTag("Player")){
AI actor=other.GetComponent<AI>();
if(actor!=null){
if(IsInHearingSight.ContainsKey(actor.Id)){
IsInHearingSight.Remove(actor.Id);
}
if(IsInVisionSight.ContainsKey(actor.Id)){
IsInVisionSight.Remove(actor.Id);
}
}
}    
}
}