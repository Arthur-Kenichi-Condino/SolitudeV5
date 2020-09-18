using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActorManagementMentana;
public class AI:Pathfinder{[NonSerialized]protected System.Random mathrandom=new System.Random();
public int Id{get;internal set;}public int TypeId{get;internal set;}
protected override void OnEnable(){
                   base.OnEnable();
NoMovementDetectionTime=MovementQualityEvaluationTimeReferenceValue;DoMovementSnapshotTime=NoMovementDetectionTime*.21f;MaxGetUnstuckActionTime=NoMovementDetectionTime*.32f;
}
protected override void OnDisable(){
                   base.OnDisable();
}
protected float Autonomous=0;public float AutonomyDelayAfterControl=30;
protected State MyState=State.IDLE_ST;
public override bool OutOfSight{get{return OutOfSight_v;}set{OutOfSight_v=value;
if(value){
if(GetActors.ContainsKey(Id)){GetActors.Remove(Id);
gameObject.SetActive(false);
if(LOG&&LOG_LEVEL<=0)Debug.Log("disable OutOfSight actor and add to inactive queue");
InactiveActorsByTypeId[TypeId].AddLast(this);
}else{
if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("OutOfSight actor wasn't marked to be active so it should already be in its InactiveActorsByTypeId queue");
}
}
}
}
protected override void Update(){
                   base.Update();
    
    
if(!Contains(this)){if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("unregistered actor detected");}
if(main.gameObject.activeSelf&&!GetActors.ContainsKey(Id)){
gameObject.SetActive(false);
if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("actor isn't marked to be active but is enabled anyway: this causes it to be ignored by other actors: actors must be enabled by their manager");
}


    if(DEBUG_ATTACK){Attack(null);}
    if(DEBUG_GETHIT){DEBUG_GETHIT=false;TakeDamage(null);}
    if(DEBUG_DIE){DEBUG_DIE=false;Die();}


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
protected virtual void TakeDamage(AI fromEnemy){}
protected virtual void Die(){}


    public bool DEBUG_ATTACK;
    public bool DEBUG_GETHIT;
    public bool DEBUG_DIE;


[NonSerialized]public Vector3 ReachedTgtDisThreshold=new Vector3(.1f,.1f,.1f);
[NonSerialized]protected bool BlockMovement;
[NonSerialized]protected float MovementQualityEvaluationTimeReferenceValue=2.27f;[NonSerialized]float _movementSnapshotTimer;[NonSerialized]Vector3 _movementSnapshotPos;[NonSerialized]protected float DoMovementSnapshotTime;[NonSerialized]float _movementWasDetectedTimer;[NonSerialized]protected float NoMovementDetectionTime;[NonSerialized]protected GetUnstuckActions _noMovementGetUnstuckAction=GetUnstuckActions.none;[NonSerialized]protected float _noMovementGetUnstuckTimer=0;[NonSerialized]protected float MaxGetUnstuckActionTime;[NonSerialized]int moveSidewaysRandomDir_dir=1;[NonSerialized]int moveCircularlyAroundTgt_dir=1;public enum GetUnstuckActions:int{none=-1,jumpAllWayUp=0,moveSidewaysRandomDir=1,moveCircularlyAroundTgt=2,moveBackwards=3,moveLooselyToRandomDir=4,}[NonSerialized]readonly int GetUnstuckActionsCount=Enum.GetValues(typeof(GetUnstuckActions)).Length-1;
[NonSerialized]Vector3 _axisDiff,_dir;
[NonSerialized]Vector3 _axisDist;
void WALK_PATH(){
if(CurPath.Count>0&&CurPathTgt==null){
    CurPathTgt=CurPath.Dequeue();
if(LOG&&LOG_LEVEL<=0)Debug.Log("WALK_PATH new dest:"+CurPathTgt.Value.pos+","+CurPathTgt.Value.mode);
_movementWasDetectedTimer=NoMovementDetectionTime*(float)(mathrandom.NextDouble()+.5f);_noMovementGetUnstuckAction=GetUnstuckActions.none;
}
if(CurPathTgt.HasValue){
ReachedTgtDisThreshold.y=colliderHalfExtents.y*.25f;
ReachedTgtDisThreshold.x=colliderHalfExtents.x*.25f;
ReachedTgtDisThreshold.z=colliderHalfExtents.z*.25f;
_axisDiff.y=CurPathTgt.Value.pos.y-transform.position.y;_axisDist.y=Mathf.Abs(_axisDiff.y);
_axisDiff.x=CurPathTgt.Value.pos.x-transform.position.x;_axisDist.x=Mathf.Abs(_axisDiff.x);
_axisDiff.z=CurPathTgt.Value.pos.z-transform.position.z;_axisDist.z=Mathf.Abs(_axisDiff.z);      
_dir=_axisDiff.normalized;
if(_axisDist.y<=ReachedTgtDisThreshold.y&&
   _axisDist.x<=ReachedTgtDisThreshold.x&&
   _axisDist.z<=ReachedTgtDisThreshold.z){    
    CurPathTgt=null;
if(LOG&&LOG_LEVEL<=0)Debug.Log("WALK_PATH dest reached");
return;
}
if(!BlockMovement){
void renew_movementWasDetectedTimer(){_movementWasDetectedTimer=NoMovementDetectionTime*(float)(mathrandom.NextDouble()+.5f);}
_movementWasDetectedTimer-=Time.deltaTime;
if(_movementSnapshotTimer<=0){
if(LOG&&LOG_LEVEL<=0)Debug.Log("movement snapshot");
    if(Mathf.Abs(transform.position.y-_movementSnapshotPos.y)>.1f||
       Mathf.Abs(transform.position.x-_movementSnapshotPos.x)>.1f||
       Mathf.Abs(transform.position.z-_movementSnapshotPos.z)>.1f){
if(LOG&&LOG_LEVEL<=0)Debug.Log("normal movement detected");
renew_movementWasDetectedTimer();
    }else{
if(LOG&&LOG_LEVEL<=0)Debug.Log("I am stuck in this position!");
    }
_movementSnapshotPos=transform.position;_movementSnapshotTimer=DoMovementSnapshotTime*(float)(mathrandom.NextDouble()+.5f);
}else{
_movementSnapshotTimer-=Time.deltaTime;
}
if(_movementWasDetectedTimer<=0){
mathrandomGetUnstuckAction();
if(LOG&&LOG_LEVEL<=0)Debug.Log("I've been stuck for long enough! Try something new! _noMovementGetUnstuckAction:"+_noMovementGetUnstuckAction);
renew_movementWasDetectedTimer();
}else if(_noMovementGetUnstuckAction!=GetUnstuckActions.none){
_noMovementGetUnstuckTimer-=Time.deltaTime;
    if(_noMovementGetUnstuckTimer<=0){
mathrandomGetUnstuckAction();
if(LOG&&LOG_LEVEL<=0)Debug.Log("my action to get unstuck didn't get me to target on a good fashioned time! Try something else! _noMovementGetUnstuckAction:"+_noMovementGetUnstuckAction);
    }
}
void mathrandomGetUnstuckAction(){_noMovementGetUnstuckAction=(GetUnstuckActions)mathrandom.Next(-1,GetUnstuckActionsCount);_noMovementGetUnstuckTimer=MaxGetUnstuckActionTime*(float)(mathrandom.NextDouble()+.5f);if(_noMovementGetUnstuckAction==GetUnstuckActions.moveSidewaysRandomDir){moveSidewaysRandomDir_dir=mathrandom.Next(0,2)==1?1:-1;}if(_noMovementGetUnstuckAction==GetUnstuckActions.moveCircularlyAroundTgt){moveCircularlyAroundTgt_dir=mathrandom.Next(0,2)==1?1:-1;}}
switch(_noMovementGetUnstuckAction){ 
#region moveBackwards
case(GetUnstuckActions.moveBackwards):{
if(_axisDist.x>float.Epsilon||
   _axisDist.z>float.Epsilon){
inputViewRotationEuler.y=Quaternion.LookRotation(_dir).eulerAngles.y-transform.eulerAngles.y;
}
inputMoveSpeed.x=0;
if((IsGrounded||!HittingWall)&&
  (_axisDist.x>ReachedTgtDisThreshold.x||
   _axisDist.z>ReachedTgtDisThreshold.z)){
inputMoveSpeed.z=-InputMaxMoveSpeed.z;
}else{
inputMoveSpeed.z=0;
}
inputMoveSpeed.y=0;
break;}
#endregion
#region moveCircularlyAroundTgt
case(GetUnstuckActions.moveCircularlyAroundTgt):{
if(_axisDist.x>float.Epsilon||
   _axisDist.z>float.Epsilon){
inputViewRotationEuler.y=Quaternion.LookRotation(_dir).eulerAngles.y-transform.eulerAngles.y;
}
if((IsGrounded||!HittingWall)&&
  (_axisDist.x>ReachedTgtDisThreshold.x||
   _axisDist.z>ReachedTgtDisThreshold.z)){
inputMoveSpeed.x=InputMaxMoveSpeed.x*moveCircularlyAroundTgt_dir;
}else{
inputMoveSpeed.x=0;
}
inputMoveSpeed.z=0;
inputMoveSpeed.y=0;
break;}
#endregion
#region moveSidewaysRandomDir
case(GetUnstuckActions.moveSidewaysRandomDir):{
if((IsGrounded||!HittingWall)&&
  (_axisDist.x>ReachedTgtDisThreshold.x||
   _axisDist.z>ReachedTgtDisThreshold.z)){
inputMoveSpeed.x=InputMaxMoveSpeed.x*moveSidewaysRandomDir_dir;
}else{
inputMoveSpeed.x=0;
}
inputMoveSpeed.z=0;
inputMoveSpeed.y=0;
break;}
#endregion
#region jumpAllWayUp
case(GetUnstuckActions.jumpAllWayUp):{
if(_axisDist.x>float.Epsilon||
   _axisDist.z>float.Epsilon){
inputViewRotationEuler.y=Quaternion.LookRotation(_dir).eulerAngles.y-transform.eulerAngles.y;
}
inputMoveSpeed.x=0;
if(!IsGrounded&&!HittingWall){
#region estou no ar, não estou tocando nada; se atingi a altitude máxima: mover para destino; caso contrário, estou subindo ainda
if(rigidbody.velocity.y<=float.Epsilon&&
  (_axisDist.x>ReachedTgtDisThreshold.x||
   _axisDist.z>ReachedTgtDisThreshold.z)){
inputMoveSpeed.z=InputMaxMoveSpeed.z;
}else{
inputMoveSpeed.z=0;
}
inputMoveSpeed.y=0;
#endregion
}else{
#region se estou no chão, pular
inputMoveSpeed.z=0;
if(IsGrounded){
Jump=true;
inputMoveSpeed.y=InputMaxMoveSpeed.y;
}else{
inputMoveSpeed.y=0;
}
#endregion
}
break;}
#endregion

#region none
default:{
if(CurPathTgt.Value.mode!=Node.PreferredReachableMode.jump&&
   _axisDist.y>ReachedTgtDisThreshold.y&&(transform.position.y<CurPathTgt.Value.pos.y+.1f||rigidbody.velocity.y<=float.Epsilon)&&
   _axisDist.x<=ReachedTgtDisThreshold.x&&
   _axisDist.z<=ReachedTgtDisThreshold.z){   
    var cur=CurPathTgt.Value;cur.mode=Node.PreferredReachableMode.jump;
    CurPathTgt=cur;
}
if(_axisDist.x>float.Epsilon||
   _axisDist.z>float.Epsilon){
inputViewRotationEuler.y=Quaternion.LookRotation(_dir).eulerAngles.y-transform.eulerAngles.y;
}
inputMoveSpeed.x=0;
if(CurPathTgt.Value.mode==Node.PreferredReachableMode.jump&&transform.position.y<CurPathTgt.Value.pos.y+.1f){
#region necessário pular
inputMoveSpeed.z=0;
if(IsGrounded){
Jump=true;
inputMoveSpeed.y=InputMaxMoveSpeed.y;
}else{
inputMoveSpeed.y=0;
}
#endregion
}else{
#region ir para o destino normalmente
if((IsGrounded||!HittingWall)&&
  (_axisDist.x>ReachedTgtDisThreshold.x||
   _axisDist.z>ReachedTgtDisThreshold.z)){
inputMoveSpeed.z=InputMaxMoveSpeed.z;
}else{
inputMoveSpeed.z=0;
}
inputMoveSpeed.y=0;
#endregion
}
break;}
#endregion
}
}else{
inputMoveSpeed=Vector3.zero;
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