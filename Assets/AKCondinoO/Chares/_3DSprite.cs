using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class _3DSprite:AI{
[NonSerialized]protected Animator animator;
protected override void Awake(){
                   base.Awake();
animator=GetComponentInChildren<Animator>();
}
protected override void LateUpdate(){
                   base.LateUpdate();
animator.transform.rotation=Quaternion.LookRotation((Camera.main.transform.position-animator.transform.position).normalized,Vector3.up);
}
}