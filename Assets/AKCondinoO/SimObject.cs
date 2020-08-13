using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimObject:MonoBehaviour{
public bool LOG=false;public int LOG_LEVEL=1;
[NonSerialized]public new Rigidbody rigidbody=null;
protected virtual void Awake(){
rigidbody=GetComponent<Rigidbody>();
}
protected virtual void Update(){}
}