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
[NonSerialized]Vector3 _forward,_cameraForward,_forwardFromCameraToSprite;[NonSerialized]bool _back;
protected override void LateUpdate(){
                   base.LateUpdate();
animator.transform.rotation=Quaternion.LookRotation((Camera.main.transform.position-animator.transform.position).normalized,Vector3.up);
_forward=Vector3.Scale(transform.eulerAngles,Vector3.up);
_forward=Quaternion.Euler(_forward)*Vector3.forward;
_cameraForward=Vector3.Scale(Camera.main.transform.eulerAngles,Vector3.up);
_cameraForward=Quaternion.Euler(_cameraForward)*Vector3.forward;
_forwardFromCameraToSprite=(transform.position-Camera.main.transform.position).normalized;
_back=Vector3.Angle(_forward,_forwardFromCameraToSprite)<=90;
}
}