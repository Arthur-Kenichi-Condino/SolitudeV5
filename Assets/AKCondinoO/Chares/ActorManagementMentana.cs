using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ActorManagementMentana:MonoBehaviour{
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
[NonSerialized]public static readonly Dictionary<int,AI>GetActors=new Dictionary<int,AI>();[NonSerialized]public static readonly Dictionary<int,LinkedList<AI>>InactiveActorsByTypeId=new Dictionary<int,LinkedList<AI>>();
void Update(){

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
//for(int i=0;i<actorsPrefabs){}
    //  no actor: se id dele no dicionario actors != dele mesmo, destruir ele;
    //  queue de inactive actors pra dar spawn
    //  OutOfSight=true remove de getactors e adiciona pra queue
    //  spawn remove da queue, adiciona pra getactors e coloca outofsight como false

}
}