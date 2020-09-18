using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Sight:MonoBehaviour{
protected void OnTriggerStay(Collider other){
if(other.tag=="Player"){
    Debug.LogWarning(other.name+" is in hearing range");    
}
}
}