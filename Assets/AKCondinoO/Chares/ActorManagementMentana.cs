using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ActorManagementMentana:MonoBehaviour{[NonSerialized]protected System.Random mathrandom=new System.Random();
public bool LOG=false;public int LOG_LEVEL=1;public int DRAW_LEVEL=1;
public static bool Contains(AI actor){if(!Actors.ContainsKey(actor.Id)||Actors[actor.Id]!=actor){unregistered.Add(actor);return(false);}return(true);}[NonSerialized]static readonly List<AI>unregistered=new List<AI>();[NonSerialized]public static readonly Dictionary<int,AI>Actors=new Dictionary<int,AI>();[NonSerialized]public static readonly Dictionary<int,List<AI>>ActorsByTypeId=new Dictionary<int,List<AI>>();
[SerializeField]AI[]actorsPrefabs;[SerializeField]int[]actorsMaxInstantiations;[NonSerialized]int nextActorId;
public static ActorManagementMentana main{get;private set;}
void Awake(){
main=this;
if(actorsPrefabs!=null&&actorsMaxInstantiations!=null){
for(int i=0;i<actorsPrefabs.Length;i++){if(i>=actorsMaxInstantiations.Length){break;}var prefab=actorsPrefabs[i];if(prefab==null){break;}var amount=actorsMaxInstantiations[i];
ActorsByTypeId.Add(i,new List<AI>(amount));InactiveActorsByTypeId.Add(i,new LinkedList<AI>());for(int j=0;j<amount;j++){
var typeId=i;var id=nextActorId++;
var aI=Instantiate(prefab);
    aI.Id=id;aI.TypeId=typeId;
var gO=aI.gameObject;gO.name=prefab.name+"("+typeId+":"+id+")";if(gO.activeSelf){gO.SetActive(false);if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("please keep the prefab disabled or the objects will be initialized and deinitialized at instantiation");}
Actors.Add(id,aI);ActorsByTypeId[typeId].Add(aI);InactiveActorsByTypeId[typeId].AddLast(aI);
}
}
}
}
[SerializeField]int[]monsterTypeIds;[SerializeField]int[]homunculusTypeIds;
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

if(DEBUG_SPAWN_ENEMY!=-1){
NextActorStagingTimer=0;ChanceToStage=1;
}

if(NextActorStagingTimer<=0){
if(mathrandom.NextDouble()<=ChanceToStage){
    
var action=DEBUG_SPAWN_ENEMY!=-1?CreativeIdleness.SpawnEnemy:(CreativeIdleness)mathrandom.Next(0,CreativeIdlenessActionsCount);
    Debug.LogWarning("do staging action:"+action);
switch(action){
    case(CreativeIdleness.SpawnEnemy):{
if(monsterTypeIds!=null){
var typeId=DEBUG_SPAWN_ENEMY!=-1?DEBUG_SPAWN_ENEMY:monsterTypeIds[mathrandom.Next(0,monsterTypeIds.Length)];if(InactiveActorsByTypeId.ContainsKey(typeId)&&InactiveActorsByTypeId[typeId].Count>0){var actorCast=InactiveActorsByTypeId[typeId].First.Value;
    if(GetValidRandomPos(actorCast,out RaycastHit hitInfo,out Vector3 pos)){
        Debug.LogWarning("spawn monster of type id:"+typeId);
InactiveActorsByTypeId[typeId].RemoveFirst();StageActor(actorCast,hitInfo,pos);
    }else{
        Debug.LogWarning("no valid position found for monster of type id:"+typeId);
    }
}else{
    Debug.LogWarning("spawn limit reached for monster of type id:"+typeId);
}
}
//        if(GetValidRandomPos(out Vector3 pos)){
//            if(monsterTypeIds!=null){
//var typeId=mathrandom.Next(0,monsterTypeIds.Length);
//                if(InactiveActorsByTypeId[typeId].Count>0){
//var actorCast=InactiveActorsByTypeId[typeId].First;InactiveActorsByTypeId[typeId].RemoveFirst();StageActor(actorCast.Value,pos);
//                    
//                }else{
//                    Debug.LogWarning("spawn limit reached for monster of type id:"+typeId);
//                }
//            }
//        }
    break;}
    case(CreativeIdleness.CreateAlly):{
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

DEBUG_SPAWN_ENEMY=-1;

firstLoop=false;}

    
void StageActor(AI actor,RaycastHit hitInfo,Vector3 pos){
    Debug.DrawRay(hitInfo.point,hitInfo.normal,Color.white,5);
var angle=Vector3.Angle(Vector3.up,hitInfo.normal);var tan=Mathf.Tan(Mathf.Deg2Rad*angle);
    Debug.LogWarning("staging actor of type:"+actor.GetType()+"[angle:"+angle+";tan:"+tan);
pos.y+=actor.collider.bounds.extents.y+tan*actor.BodyRadius+.1f;actor.transform.position=pos;actor.OutOfSight=false;GetActors.Add(actor.Id,actor);actor.gameObject.SetActive(true);
}
//void StageActor(AI actor,Vector3 pos){
//    Debug.LogWarning("staging actor of type:"+actor.GetType());
//pos.y+=actor.collider.bounds.extents.y*2;actor.transform.position=pos;actor.OutOfSight=false;GetActors.Add(actor.Id,actor);actor.gameObject.SetActive(true);
//}

    
bool GetValidRandomPos(AI actor,out RaycastHit hitInfo,out Vector3 pos){
var c=center+new Vector3((float)(mathrandom.NextDouble()*2f-1)*(size.x*.5f),size.y*.5f,
                         (float)(mathrandom.NextDouble()*2f-1)*(size.z*.5f));
    Debug.DrawRay(c,Vector3.down*Chunk.Height,Color.white,1f);
bool result=Physics.BoxCast(c,actor.collider.bounds.extents,Vector3.down,out hitInfo,Quaternion.identity,size.y);
pos=hitInfo.point+hitInfo.normal*actor.BodyRadius;
return result;}
//bool GetValidRandomPos(out Vector3 pos){
//var ray=new Ray(center+new Vector3((float)(mathrandom.NextDouble()*2f-1)*(size.x/2f),size.y/2f,(float)(mathrandom.NextDouble()*2f-1)*(size.z/2f)),Vector3.down);
//    Debug.DrawRay(ray.origin,Vector3.down*Chunk.Height,Color.white,1f);
//if(Physics.Raycast(ray,out RaycastHit hit,Chunk.Height)){
//pos=hit.point;
//return true;
//}
//pos=default;
//return false;}
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


[SerializeField]internal int DEBUG_SPAWN_ENEMY=-1;
[SerializeField]internal int DEBUG_CREATE_ALLY=-1;

    
[Serializable]public enum Roles{
Neutral,
MonsterPassive,
MonsterAggressive,
HomunculusPassive,
HomunculusAggressive,
HumanPassive,
HumanAggressive,
}
}