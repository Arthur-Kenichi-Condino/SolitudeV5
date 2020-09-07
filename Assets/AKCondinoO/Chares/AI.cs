using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AI:Pathfinder{
protected float Autonomous=0;public float AutonomyDelayAfterControl=30;
protected State MyState=State.IDLE_ST;
protected override void Update(){
                   base.Update();


    if(DEBUG_ATTACK){DEBUG_ATTACK=false;Attack(null);}


if(Autonomous<=0){
WALK_PATH();

    
if(MyState==State.EXCUSE_ST){OnEXCUSE_ST();}
if(MyState==State.FOLLOW_ST){OnFOLLOW_ST();}
if(MyState==State.IDLE_ST){OnIDLE_ST();}
if(MyState==State.CHASE_ST){OnCHASE_ST();}
if(MyState==State.ATTACK_ST){OnATTACK_ST();}
if(MyState==State.SKILL_OBJECT_ST){OnSKILL_OBJECT_ST();}


}else{
    Autonomous-=Time.deltaTime;
}
}
protected virtual void OnEXCUSE_ST(){}
protected virtual void OnFOLLOW_ST(){}
protected virtual void OnIDLE_ST(){}
protected virtual void OnCHASE_ST(){}
protected virtual void OnATTACK_ST(){}
protected virtual void OnSKILL_OBJECT_ST(){}
protected virtual void Attack(AI enemy){}


    public bool DEBUG_ATTACK;


[NonSerialized]public Vector3 ReachedTgtDisThreshold=new Vector3(.1f,.1f,.1f);
[NonSerialized]Vector3 _axisDiff,_dir;
[NonSerialized]Vector3 _axisDist;
void WALK_PATH(){
if(CurPath.Count>0&&CurPathTgt==null){
    CurPathTgt=CurPath.Dequeue();


        Debug.LogWarning(CurPathTgt.Value.mode);


}
if(CurPathTgt.HasValue){


_axisDiff.y=CurPathTgt.Value.pos.y-transform.position.y;_axisDist.y=Mathf.Abs(_axisDiff.y);
_axisDiff.x=CurPathTgt.Value.pos.x-transform.position.x;_axisDist.x=Mathf.Abs(_axisDiff.x);
_axisDiff.z=CurPathTgt.Value.pos.z-transform.position.z;_axisDist.z=Mathf.Abs(_axisDiff.z);
            
            
_dir=_axisDiff.normalized;


if(_axisDist.y<=ReachedTgtDisThreshold.y&&
   _axisDist.x<=ReachedTgtDisThreshold.x&&
   _axisDist.z<=ReachedTgtDisThreshold.z){    


    CurPathTgt=null;
        Debug.LogWarning("dest reached");


return;
}
if(_axisDist.x>float.Epsilon||
   _axisDist.z>float.Epsilon){
inputViewRotationEuler.y=Quaternion.LookRotation(_dir).eulerAngles.y-transform.eulerAngles.y;
}


if(CurPathTgt.Value.mode==Node.PreferredReachableMode.jump&&transform.position.y<CurPathTgt.Value.pos.y+.1f){

    
inputMoveSpeed.x=0;
inputMoveSpeed.z=0;
if(IsGrounded){
Jump=true;
inputMoveSpeed.y=InputMaxMoveSpeed.y;
}else{
inputMoveSpeed.y=0;
}


}else{
    
    
inputMoveSpeed.x=0;
if(IsGrounded||!HittingWall){
inputMoveSpeed.z=InputMaxMoveSpeed.z;
}
inputMoveSpeed.y=0;


}


}else{
inputMoveSpeed=Vector3.zero;
}
}
#if UNITY_EDITOR
protected override void OnDrawGizmos(){
                   base.OnDrawGizmos();
}
#endif
public enum State:int{
IDLE_ST=0,
STOP_CMD_ST=10,
MOVE_CMD_ST=11,
FOLLOW_ST=1,
EXCUSE_ST=2,
CHASE_ST=3,
ATTACK_ST=4,
SKILL_OBJECT_ST=5,
}
}