﻿using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ActorManagementMentana:MonoBehaviour{[NonSerialized]protected System.Random mathrandom=new System.Random();
public bool LOG=false;public int LOG_LEVEL=1;public int DRAW_LEVEL=1;
public static bool Contains(AI actor){if(!Actors.ContainsKey(actor.Id)||Actors[actor.Id]!=actor){unregistered.Add(actor);return(false);}return(true);}[NonSerialized]static readonly List<AI>unregistered=new List<AI>();[NonSerialized]public static readonly Dictionary<int,AI>Actors=new Dictionary<int,AI>();[NonSerialized]public static readonly Dictionary<int,List<AI>>ActorsByTypeId=new Dictionary<int,List<AI>>();
[SerializeField]AI[]actorsPrefabs;[SerializeField]int[]actorsMaxInstantiations;[NonSerialized]int nextActorId;
void Awake(){
if(actorsPrefabs!=null&&actorsMaxInstantiations!=null){
for(int i=0;i<actorsPrefabs.Length;i++){if(i>=actorsMaxInstantiations.Length){break;}var prefab=actorsPrefabs[i];if(prefab==null){break;}var amount=actorsMaxInstantiations[i];
ActorsByTypeId.Add(i,new List<AI>(amount));InactiveActorsByTypeId.Add(i,new LinkedList<AI>());for(int j=0;j<amount;j++){
var typeId=i;var id=i+j;
var aI=Instantiate(prefab);
    aI.Id=id;aI.TypeId=typeId;
var gO=aI.gameObject;gO.name=prefab.name+"("+typeId+":"+id+")";if(gO.activeSelf){gO.SetActive(false);if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("please keep the prefab disabled or the objects will be initialized and deinitialized at instantiation");}
Actors.Add(id,aI);ActorsByTypeId[typeId].Add(aI);InactiveActorsByTypeId[typeId].AddLast(aI);
nextActorId=id+1;}
}
}
}
[SerializeField]int[]monsterTypeIds;
[NonSerialized]public static readonly Dictionary<int,AI>GetActors=new Dictionary<int,AI>();[NonSerialized]public static readonly Dictionary<int,LinkedList<AI>>InactiveActorsByTypeId=new Dictionary<int,LinkedList<AI>>();
[NonSerialized]protected static Vector3 actPos,center,size;
[NonSerialized]protected static Vector2Int actReg;
bool firstLoop=true;void Update(){

if(firstLoop||actPos!=Camera.main.transform.position){
    actPos=Camera.main.transform.position;
    actReg=ChunkManager.PosToRgn(actPos);
    Debug.LogWarning(actPos+" "+actReg);
    center=new Vector3(actReg.x,0,actReg.y);size=new Vector3(Chunk.Width*(ChunkManager.main.instantiationDistance.x*2+1),Chunk.Height,Chunk.Depth*(ChunkManager.main.instantiationDistance.y*2+1));
}

for(int u=unregistered.Count-1;u>=0;u--){for(int i=0;i<actorsPrefabs.Length;i++){if(actorsPrefabs[i].GetType()==unregistered[u].GetType()){var prefab=actorsPrefabs[i];

    
if(LOG&&LOG_LEVEL<=0)Debug.Log("register actor of type:"+unregistered[u].GetType());
var typeId=i;var id=nextActorId++;
var aI=unregistered[u];
    aI.Id=id;aI.TypeId=typeId;
var gO=aI.gameObject;gO.name=prefab.name+"("+typeId+":"+id+")";if(gO.activeSelf){gO.SetActive(false);}
Actors.Add(id,aI);ActorsByTypeId[typeId].Add(aI);InactiveActorsByTypeId[typeId].AddLast(aI);


unregistered.RemoveAt(u);goto _continue;}}
var toDestroy=unregistered[u].gameObject;
if(LOG&&LOG_LEVEL<=0)Debug.Log("destroy unavailable actor of type:"+unregistered[u].GetType());
unregistered.RemoveAt(u);Destroy(toDestroy);
_continue:{}}


if(NextActorStagingTimer<=0){
if(mathrandom.NextDouble()<=ChanceToStage){
    
var action=(CreativeIdleness)mathrandom.Next(0,CreativeIdlenessActionsCount);
    Debug.LogWarning("do staging action:"+action);
switch(action){
    case(CreativeIdleness.SpawnEnemy):{
        if(GetValidRandomPos(out Vector3 pos)){
            if(monsterTypeIds!=null){
var typeId=mathrandom.Next(0,monsterTypeIds.Length);
                Debug.LogWarning("spawn monster of type id:"+typeId);
            }
        }
    break;}
    case(CreativeIdleness.CreateAlly):{
        if(GetValidRandomPos(out Vector3 pos)){
        }
    break;}
}

ChanceToStage=0;
}else{

    Debug.LogWarning("staging failed");

ChanceToStage+=0.1f;
}
    NextActorStagingTimer=TryActorStagingInterval;
}else{
    NextActorStagingTimer-=Time.deltaTime;
}
//for(int i=0;i<actorsPrefabs){}
    //  no actor: se id dele no dicionario actors != dele mesmo, destruir ele;
    //  queue de inactive actors pra dar spawn
    //  OutOfSight=true remove de getactors e adiciona pra queue
    //  spawn remove da queue, adiciona pra getactors e coloca outofsight como false


firstLoop=false;}


bool GetValidRandomPos(out Vector3 pos){
var ray=new Ray(center+new Vector3((float)(mathrandom.NextDouble()*2f-1)*(size.x/2f),size.y/2f,(float)(mathrandom.NextDouble()*2f-1)*(size.z/2f)),Vector3.down);
    Debug.DrawRay(ray.origin,Vector3.down*Chunk.Height,Color.white,1f);
if(Physics.Raycast(ray,out RaycastHit hit,Chunk.Height)){
pos=hit.point;
return true;
}
pos=default;
return false;}


[NonSerialized]public float TryActorStagingInterval=1f;[NonSerialized]float NextActorStagingTimer;[NonSerialized]public float ChanceToStage;public enum CreativeIdleness:int{CreateAlly,SpawnEnemy,}[NonSerialized]readonly int CreativeIdlenessActionsCount=Enum.GetValues(typeof(CreativeIdleness)).Length;
#if UNITY_EDITOR
[NonSerialized]Color gizmosAreaCubeColor=new Color(1,1,1,0.125f);
protected void OnDrawGizmos(){
if(DRAW_LEVEL<=1){
//Debug.LogWarning(center);
var oldColor=Gizmos.color;
Gizmos.color=gizmosAreaCubeColor;
Gizmos.DrawCube(center,size);
Gizmos.color=oldColor;
}
}
#endif
}