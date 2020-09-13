using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ActorManagementMentana:MonoBehaviour{
[SerializeField]GameObject[]actorsPrefabs;[SerializeField]int[]actorsMaxInstantiations;
void Awake(){
if(actorsPrefabs!=null&&actorsMaxInstantiations!=null){
}
}
}