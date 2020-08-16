using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimObject:MonoBehaviour{
public bool LOG=false;public int LOG_LEVEL=1;
[NonSerialized]public new Rigidbody rigidbody=null;
protected virtual void Awake(){
rigidbody=GetComponent<Rigidbody>();
pos=pos_Pre=transform.position;
}
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
}