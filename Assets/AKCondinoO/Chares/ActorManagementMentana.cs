using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ActorManagementMentana:MonoBehaviour{
[SerializeField]GameObject[]actorsPrefabs;[SerializeField]int[]actorsMaxInstantiations;[NonSerialized]int nextActorId=0;
void Awake(){
if(actorsPrefabs!=null&&actorsMaxInstantiations!=null){
for(int i=0;i<actorsPrefabs.Length;i++){if(i>=actorsMaxInstantiations.Length)break;var prefab=actorsPrefabs[i];var amount=actorsMaxInstantiations[i];
for(int j=0;j<amount;j++){var gO=Instantiate(prefab);gO.SetActive(false);}
}
}
}
}