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
}