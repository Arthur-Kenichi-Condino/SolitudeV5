using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Sight:MonoBehaviour{
protected void OnTriggerStay(Collider other){
AI actor;
if((actor=other.GetComponent<AI>())!=null){
    Debug.LogWarning(other.name+" is in hearing range");    
}
}
}