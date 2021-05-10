using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnimatorParamChanger:MonoBehaviour{
public bool LOG=false;public int LOG_LEVEL=1;public int DRAW_LEVEL=1;
[NonSerialized]public AI actor;[NonSerialized]public Animator animator;
void OnEnable(){
actor=transform.root.GetComponent<AI>();
}
[NonSerialized]Vector3 _horizontalMoveSpeed;[NonSerialized]Vector3 _forward;[NonSerialized]Vector3 _move;[NonSerialized]float _angle;[NonSerialized]float _turn;[NonSerialized]public float horizontalMoveSensibility=1f/3f;[NonSerialized]public bool backwardAvailable=true;
void Update(){
if(actor.IsUMA&&animator==null){
animator=actor.GetComponentInChildren<Animator>();

    Debug.LogWarning(actor+";animator:"+animator);

}
if(animator!=null){
_horizontalMoveSpeed=actor.rigidbody.velocity;
_horizontalMoveSpeed.x=0;
_horizontalMoveSpeed.y=0;
animator.SetFloat("Forward",(_horizontalMoveSpeed.magnitude*(backwardAvailable&&Vector3.Angle(actor.rigidbody.transform.forward,actor.rigidbody.velocity.normalized)>90?-1:1)*horizontalMoveSensibility)*(actor.Crouching?4f:1f),0.1f,Time.deltaTime);
animator.SetBool("OnGround",actor.OnGround);

            
_turn=Mathf.LerpUnclamped(_turn,Vector3.SignedAngle(_forward,actor.transform.forward,Vector3.up)*Mathf.Deg2Rad*100f,50f*Time.deltaTime);
_forward=actor.transform.forward;

            
//_move=_horizontalMoveSpeed;_angle=0;
//			// convert the world relative moveInput vector into a local-relative
//			// turn amount and forward amount required to head in the desired
//			// direction.
//			if (_move.magnitude > 1f){ _move.Normalize();
//			_move = actor.transform.InverseTransformDirection(_move);
//			_move = Vector3.ProjectOnPlane(_move, Vector3.up);
//			_angle = Mathf.Atan2(_move.x, _move.z);
//if(_angle>0&&_turn>=0&&_angle>_turn){_turn=_angle;
//}else 
//if(_angle<0&&_turn<=0&&_angle<_turn){_turn=_angle;
//}
//			}


Debug.LogWarning("_angle:"+_angle+";_turn:"+_turn);
animator.SetFloat("Turn",_turn,0.1f,Time.deltaTime);
            

if(LOG&&LOG_LEVEL<=-110)Debug.Log("actor.OnGround:"+actor.OnGround);

animator.SetFloat("Jump",actor.rigidbody.velocity.y);
animator.SetBool("Crouch",actor.CanCrouch&&actor.Crouching);
}
}
public void OnAttack(int attackStance){
    Debug.LogWarning("OnAttack(int attackStance):"+attackStance);
}
public void FootR(string s){
    Debug.LogWarning("FootR");
}
public void FootL(string s){
    Debug.LogWarning("FootL");
}
public void Hit(string s){
    Debug.LogWarning("Hit");
}
}