using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimObject:MonoBehaviour{
public bool LOG=false;public int LOG_LEVEL=1;public int DRAW_LEVEL=1;
[NonSerialized]public new Collider collider=null;[NonSerialized]public new Rigidbody rigidbody=null;
protected virtual void Awake(){
collider=GetComponent<Collider>();rigidbody=GetComponent<Rigidbody>();
if(collider!=null){
colliderDefaultSize=collider.bounds.size;
GetColliderData();
}
pos=pos_Pre=transform.position;
}
[NonSerialized]protected Vector3 colliderDefaultSize;
[NonSerialized]protected Vector3 colliderHalfExtents;
[NonSerialized]protected float colliderShortestExtent;
[NonSerialized]public float BodyRadius;
protected void GetColliderData(){
colliderHalfExtents=collider.bounds.extents/2;
colliderShortestExtent=Mathf.Min(collider.bounds.extents.x,collider.bounds.extents.y,collider.bounds.extents.z);
BodyRadius=Mathf.Max(Mathf.Sqrt(Mathf.Pow(collider.bounds.extents.x,2)*2),
                     Mathf.Sqrt(Mathf.Pow(collider.bounds.extents.z,2)*2))+.1f;
if(LOG&&LOG_LEVEL<=1)Debug.Log("BodyRadius:"+BodyRadius+", name:"+name);
}
protected virtual void OnEnable(){}
protected virtual void OnDisable(){}
protected virtual void OnDestroy(){}
protected virtual void FixedUpdate(){}
[NonSerialized]Vector3 pos;
[NonSerialized]Vector3 pos_Pre;
[NonSerialized]Vector2Int coord;[NonSerialized]int idx;[NonSerialized]Chunk chunk;
protected virtual void Update(){
if(rigidbody!=null){
pos=transform.position;
if(pos!=pos_Pre){
coord=ChunkManager.PosToCoord(pos);idx=ChunkManager.GetIdx(coord.x,coord.y);
    if(pos.y<-128||!ChunkManager.main.Chunks.TryGetValue(idx,out chunk)||!chunk.Built){
        rigidbody.velocity=Vector3.zero;
        rigidbody.angularVelocity=Vector3.zero;
        pos=transform.position=pos_Pre;
    }
    pos_Pre=pos;
}
}
}
#if UNITY_EDITOR
protected virtual void OnDrawGizmos(){
}
#endif
}