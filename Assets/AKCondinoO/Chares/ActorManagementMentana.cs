using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ActorManagementMentana:MonoBehaviour{
[NonSerialized]public readonly Dictionary<int,AI>Actors=new Dictionary<int,AI>();[NonSerialized]public readonly Dictionary<int,List<AI>>ActorsByTypeId=new Dictionary<int,List<AI>>();
[SerializeField]AI[]actorsPrefabs;[SerializeField]int[]actorsMaxInstantiations;[NonSerialized]int nextActorId;
void Awake(){
if(actorsPrefabs!=null&&actorsMaxInstantiations!=null){
for(int i=0;i<actorsPrefabs.Length;i++){if(i>=actorsMaxInstantiations.Length){break;}var prefab=actorsPrefabs[i];if(prefab==null){break;}var amount=actorsMaxInstantiations[i];
ActorsByTypeId.Add(i,new List<AI>(amount));for(int j=0;j<amount;j++){int id=i+j;var aI=Instantiate(prefab);var gO=aI.gameObject;gO.name=prefab.name+"("+i+":"+id+")";gO.SetActive(false);Actors.Add(id,aI);ActorsByTypeId[i].Add(aI);nextActorId=id+1;}
}
}
}
[NonSerialized]public readonly Dictionary<int,AI>GetActors=new Dictionary<int,AI>();
void Update(){

    //  no actor: se id dele no dicionario actors != dele mesmo, destruir ele;
    //  queue de inactive actors pra dar spawn
    //  OutOfSight=true remove de getactors e adiciona pra queue
    //  spawn remove da queue, adiciona pra getactors e coloca outofsight como false

}
}