﻿using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SimObject:MonoBehaviour{[NonSerialized]protected System.Random mathrandom=new System.Random();
public bool LOG=false;public int LOG_LEVEL=1;public int DRAW_LEVEL=1;
[NonSerialized]public new Collider collider=null;[NonSerialized]public new Rigidbody rigidbody=null;
protected virtual void Awake(){
collider=GetComponent<Collider>();rigidbody=GetComponent<Rigidbody>();


    Debug.LogWarning(this.gameObject+" collider:"+collider);


if(collider!=null){
colliderDefaultSize=collider.bounds.size;
GetColliderData();
}
pos=pos_Pre=transform.position;
}
[NonSerialized]protected Vector3 colliderDefaultSize;
[NonSerialized]protected Vector3 colliderHalfExtents_v;protected Vector3 colliderHalfExtents{get{return colliderHalfExtents_v*RangeMultiplier;}set{colliderHalfExtents_v=value;}}
[NonSerialized]protected float colliderShortestExtent;
[NonSerialized]protected float BodyRadius;
[NonSerialized]protected float BodyRange_v;public float BodyRange{get{return BodyRange_v*RangeMultiplier;}set{BodyRange_v=value;}}
[NonSerialized]protected float RangeMultiplier=2f;
protected void GetColliderData(){


    Debug.LogWarning("this is _3DSprite:"+(this is _3DSprite));
if(!(this is _3DSprite)){RangeMultiplier=1f;}


colliderHalfExtents=collider.bounds.extents/2;
colliderShortestExtent=Mathf.Min(collider.bounds.extents.x,collider.bounds.extents.y,collider.bounds.extents.z);
BodyRange=BodyRadius=Mathf.Max(Mathf.Sqrt(Mathf.Pow(collider.bounds.extents.x,2)*2),
                               Mathf.Sqrt(Mathf.Pow(collider.bounds.extents.z,2)*2))+.1f;
if(LOG&&LOG_LEVEL<=1)Debug.Log("BodyRadius:"+BodyRange+", name:"+name);
}
protected virtual void OnEnable(){}
protected virtual void OnDisable(){}
protected virtual void OnDestroy(){}
protected bool IsGrounded;protected bool HittingWall;
protected virtual void FixedUpdate(){
if(rigidbody!=null){
IsGrounded=false;HittingWall=false;
if(LOG&&LOG_LEVEL<=0)Debug.Log("collisions.Count:"+collisions.Count+";dirtyCollisions.Count:"+dirtyCollisions.Count);
foreach(var collision in collisions){
for(int i=0;i<collision.Value.Count;i++){var contact=collision.Value[i];if(contact.normal==Vector3.zero)break;


IsGrounded=IsGrounded||(Vector3.Angle(contact.normal,Vector3.up)<=60&&contact.point.y<=transform.position.y-collider.bounds.extents.y+.1f);HittingWall=HittingWall||Vector3.Angle(contact.normal,Vector3.up)>60;
//Debug.LogWarning(Vector3.Angle(contact.normal,Vector3.up));


}
dirtyCollisions[collision.Key]=true;}
}
}
[NonSerialized]protected readonly Dictionary<Collider,List<ContactPoint>>collisions=new Dictionary<Collider,List<ContactPoint>>();[NonSerialized]readonly Dictionary<Collider,bool>dirtyCollisions=new Dictionary<Collider,bool>();[NonSerialized]readonly List<ContactPoint>contacts=new List<ContactPoint>();
void OnCollisionStay(Collision collision){
if(LOG&&LOG_LEVEL<=0)Debug.Log("collision stay:"+collision.collider.name);


if(!collisions.ContainsKey(collision.collider)){collisions.Add(collision.collider,new List<ContactPoint>());dirtyCollisions.Add(collision.collider,false);}else if(dirtyCollisions[collision.collider]){collisions[collision.collider].Clear();dirtyCollisions[collision.collider]=false;}collision.GetContacts(contacts);collisions[collision.collider].AddRange(contacts);
//if(!collisions.ContainsKey(collision.collider)){collisions.Add(collision.collider,new List<ContactPoint>());}else if(){}collision.GetContacts(contacts);collisions[collision.collider].AddRange();
if(DRAW_LEVEL<=-100){
for(int i=0;i<collision.contactCount;i++){var contact=collision.GetContact(i);
Debug.DrawRay(contact.point,contact.normal,Color.white,.5f);
}
}


//if(!collisions.ContainsKey(collision.collider)){collisions.Add(collision.collider,new List<ContactPoint>());}else if(){}collision.GetContacts(contacts);collisions[collision.collider].AddRange();
//if(DRAW_LEVEL<=-100){
//for(int i=0;i<collision.contactCount;i++){var contact=collision.GetContact(i);
//Debug.DrawRay(contact.point,contact.normal,Color.white,.5f);
//}
//}


//var tuple=(collision.gameObject,collision.collider,collision);
//if(!collisions.ContainsKey(tuple)){collisions.Add(tuple,new List<ContactPoint>());}collision.GetContacts(collisions[tuple]);
//if(DRAW_LEVEL<=-100){
//for(int i=0;i<collision.contactCount;i++){var contact=collision.GetContact(i);
//Debug.DrawRay(contact.point,contact.normal,Color.white,.5f);
//}
//}
}
void OnCollisionExit(Collision collision){
if(LOG&&LOG_LEVEL<=0)Debug.Log("collision exit:"+collision.collider.name);
//var tuple=(collision.gameObject,collision.collider,collision);
collisions.Remove(collision.collider);dirtyCollisions.Remove(collision.collider);
}
[NonSerialized]Vector3 pos;
[NonSerialized]Vector3 pos_Pre;
[NonSerialized]Vector2Int coord;[NonSerialized]int idx;[NonSerialized]Chunk chunk;[NonSerialized]protected bool OutOfSight_v=true;public virtual bool OutOfSight{get{return OutOfSight_v;}set{OutOfSight_v=value;}}[NonSerialized]protected bool OutOfSight_disable;//  Objects need to be disabled and enabled by managers
protected virtual void Update(){
if(rigidbody!=null){
pos=transform.position;
if(pos!=pos_Pre){
OutOfSight=false;
coord=ChunkManager.PosToCoord(pos);idx=ChunkManager.GetIdx(coord.x,coord.y);
    if(pos.y<-128||!ChunkManager.main.Chunks.TryGetValue(idx,out chunk)||!chunk.Built){
setOutOfSight();
    }
    pos_Pre=pos;
}
}
}
protected void setOutOfSight(){
        rigidbody.velocity=Vector3.zero;
        rigidbody.angularVelocity=Vector3.zero;
        pos=transform.position=pos_Pre;
OutOfSight=true;
}
protected virtual void LateUpdate(){}
#if UNITY_EDITOR
protected virtual void OnDrawGizmos(){
}
#endif
}