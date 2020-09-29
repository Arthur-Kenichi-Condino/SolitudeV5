using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ActorManagementMentana;
public class AI:Pathfinder{[NonSerialized]protected System.Random mathrandom=new System.Random();
public int Id{get;internal set;}public int TypeId{get;internal set;}[SerializeField]public Roles Role;
[NonSerialized]protected Sight MySight;
protected override void Awake(){
                   base.Awake();
MySight=GetComponentInChildren<Sight>();
}
protected override void OnEnable(){
                   base.OnEnable();
NoMovementDetectionTime=MovementQualityEvaluationTimeReferenceValue;DoMovementSnapshotTime=NoMovementDetectionTime*.21f;MaxGetUnstuckActionTime=NoMovementDetectionTime*.32f;
attackStance=-1;hitStance=-1;deadStance=-1;
}
protected override void OnDisable(){
                   base.OnDisable();
}
protected float Autonomous=0;public float AutonomyDelayAfterControl=30;protected float Dying=0;protected float DeadForGoodDelay=10;
protected State MyState=State.IDLE_ST;
#region OutOfSight result set
public override bool OutOfSight{get{return OutOfSight_v;}set{
if(value&&OutOfSight_v!=value){
OutOfSight_disable=true;
}
OutOfSight_v=value;
}
}
#endregion
[NonSerialized]bool firstLoop=true;protected override void Update(){
                                                      base.Update();
if(!OutOfSight_v&&
   deadStance!=-1){
    Debug.LogWarning("I'm dead! :(");
if(Dying<=0){
setOutOfSight();
}else{
    Dying-=Time.deltaTime;
}
}
    
#region Init    
if(firstLoop&&!Contains(this)){
if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("unregistered actor id detected processed by the manager; actor isn't marked to be active but is enabled anyway: this causes it to be ignored by other actors: actors must be enabled by their manager",this.gameObject);
}
#endregion
#region OutOfSight response
if(OutOfSight_disable){
gameObject.SetActive(false);
if(GetActors.ContainsKey(Id)){GetActors.Remove(Id);
if(LOG&&LOG_LEVEL<=0)Debug.Log("disable OutOfSight actor and add to inactive queue");
InactiveActorsByTypeId[TypeId].AddLast(this);
}else{
if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("OutOfSight actor wasn't marked to be active so it should already be in its InactiveActorsByTypeId queue");
}
    OutOfSight_disable=false;
}
#endregion


if(DEBUG_ATTACK){Attack(null);}if(DEBUG_GETHIT){DEBUG_GETHIT=false;TakeDamage(null);}if(DEBUG_DIE){DEBUG_DIE=false;Die();}


GetTargets();
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
firstLoop=false;}
protected AI MyEnemy=null;public AI Target{get{return MyEnemy;}}[NonSerialized]protected readonly Dictionary<int,(AI actor,Vector3 pos,float dis)>MyPossibleTargets=new Dictionary<int,(AI,Vector3,float)>();[NonSerialized]protected readonly Dictionary<int,(AI actor,float dis,float timeout)>AsAggroEnemies=new Dictionary<int,(AI,float,float)>();
[NonSerialized]protected readonly Dictionary<int,(AI actor,float dis,float timeout)>GetEnemiesAttackingMe=new Dictionary<int,(AI,float,float)>();
[NonSerialized]protected readonly Dictionary<int,(AI actor,float dis,float timeout)>GetEnemiesAttackingAlly=new Dictionary<int,(AI,float,float)>();
protected virtual void GetTargets(){

    
for(int k=GetEnemiesAttackingMe.Keys.Count-1;k>=0;k--){var i=GetEnemiesAttackingMe.Keys.ElementAt(k);
var tuple=GetEnemiesAttackingMe[i];
    tuple.timeout-=Time.deltaTime;
GetEnemiesAttackingMe[i]=tuple;if(GetEnemiesAttackingMe[i].timeout<=0){GetEnemiesAttackingMe.Remove(i);}
}
MyPossibleTargets.Clear();
foreach(var actor in GetActors){var i=actor.Key;var v=actor.Value;
if(i!=this.Id){


if(v.Target==this){//  This following mode of detecting targets does not take into consideration the stealth status of enemies
Vector3 pos;float dis;
if(MyMotion==Motions.MOTION_HIT){pos=v.transform.position;
addPossibleTarget();attackingMe();
    Debug.LogWarning("I'm under attack",this);
}else if(MySight.IsInVisionSight.ContainsKey(i)&&MySight.IsInVisionSight[i].directSight){pos=MySight.IsInVisionSight[i].pos;
addPossibleTarget();attackingMe();
    Debug.LogWarning("enemy approaching my position",this);
}
void addPossibleTarget(){
dis=Vector3.Distance(transform.position,pos);
MyPossibleTargets.Add(i,(v,pos,dis));
}
void attackingMe(){
if(!GetEnemiesAttackingMe.ContainsKey(i)){
GetEnemiesAttackingMe.Add(i,(v,-1,0));
}
var tuple=GetEnemiesAttackingMe[i];
    tuple.dis=dis;
    tuple.timeout=5f;
GetEnemiesAttackingMe[i]=tuple;
}
}
//if(v.HasPassiveRole()){
//if(MySight.IsInHearingSight.ContainsKey(i)){
//    Debug.LogWarning("my TypeId:"+TypeToTypeId[GetType()]+"; possible target TypeId:"+TypeToTypeId[v.GetType()]);
//}
//}


}
}
MyEnemy=null;
if(MyEnemy==null){

    
float dis=-1;
foreach(var kvp in GetEnemiesAttackingMe){var i=kvp.Key;var v=kvp.Value.actor;var d=kvp.Value.dis;
if(dis==-1||dis>d){
MyEnemy=v;
    dis=d;
}
}


}


}
protected Motions MyMotion=Motions.MOTION_STAND;public Motions GetMotion{get{return MyMotion;}}[NonSerialized]protected int attackStance=-1;[SerializeField]protected float attackStanceRhythmMultiplier=1f;[SerializeField]protected float attackStanceDamageTime=.5f;[NonSerialized]protected int hitStance=-1;[SerializeField]protected float hitStanceRhythmMultiplier=2f;[NonSerialized]protected int deadStance=-1;[SerializeField]protected float deadStanceRhythmMultiplier=1f;
protected virtual void OnEXCUSE_ST(){}
protected virtual void OnFOLLOW_ST(){}
protected virtual void OnIDLE_ST(){

    
if(MyEnemy!=null){
if(!IsInAttackSight(MyEnemy)){
    Debug.LogWarning("chase");
MyState=State.CHASE_ST;
return;
}else{
    Debug.LogWarning("attack");
STOP();
MyState=State.ATTACK_ST;
return;
}
}


}
protected virtual void OnCHASE_ST(){
    Debug.LogWarning("OnCHASE_ST");
if(MyEnemy==null){
    Debug.LogWarning("idle");
STOP();
MyState=State.IDLE_ST;
return;
}
if(IsInAttackSight(MyEnemy)){
    Debug.LogWarning("attack");
STOP();
MyState=State.ATTACK_ST;
return;
}


if(!destSet||CurPath.Count<=0||Vector3.Distance(MyDest,MyEnemy.transform.position)>MyAttackRange){
    Debug.LogWarning("OnCHASE_ST: GoTo");
    MyDest=MyEnemy.transform.position;
GoTo(new Ray(MyDest,Vector3.down));
}


}
[NonSerialized]float sinceLastHitTimer;[NonSerialized]float hitDetectionReactionTick=1f;
protected virtual void OnATTACK_ST(){
    Debug.LogWarning("OnATTACK_ST");
if(MyEnemy==null){
    Debug.LogWarning("idle");
STOP();
MyState=State.IDLE_ST;
return;
}


if(MyMotion==Motions.MOTION_HIT&&sinceLastHitTimer<=0){
    sinceLastHitTimer=hitDetectionReactionTick;
    Debug.LogWarning("OnATTACK_ST: hitDetectionReactionTick:"+hitDetectionReactionTick);
}else if(sinceLastHitTimer>0){
    sinceLastHitTimer-=Time.deltaTime;
}
if(!destSet||sinceLastHitTimer==hitDetectionReactionTick){
    Debug.LogWarning("OnATTACK_ST: move away GoTo");
    MyDest=MyEnemy.transform.position;var dir=Quaternion.Euler(0,(float)(mathrandom.NextDouble()*2-1)*90,0)*(transform.position-MyEnemy.transform.position).normalized;MyDest+=dir*(MyEnemy.BodyRadius+BodyRadius);
Debug.DrawLine(MyEnemy.transform.position,MyDest,Color.blue,1f);
GoTo(new Ray(MyDest,Vector3.down));
}


}
protected virtual void OnSKILL_OBJECT_ST(){}
[NonSerialized]protected float MyAttackRange=.1f;
protected virtual bool IsInAttackSight(AI enemy){
if(Vector3.Distance(transform.position,enemy.transform.position)-(BodyRadius+enemy.BodyRadius)<=MyAttackRange){
return true;
}
return false;}
[NonSerialized]protected AttackModes MyAttackMode=AttackModes.Ghost;public enum AttackModes{Ghost,Physical}
protected virtual void Attack(AI enemy){
if(attackStance==-1){
    Debug.LogWarning("new attack started: set to do damage next animation");
    didDamage=false;
}
if(enemy!=null){
inputViewRotationEuler.y=Quaternion.LookRotation((enemy.transform.position-transform.position).normalized).eulerAngles.y-transform.eulerAngles.y;
}
}
[NonSerialized]Vector3 attackHitboxHalfSize;[NonSerialized]protected Collider[]attackHitboxColliders=null;
protected virtual void OverlappedCollidersOnAttack(){

    
attackHitboxHalfSize.x=collider.bounds.extents.x;
attackHitboxHalfSize.z=collider.bounds.extents.z+MyAttackRange;
attackHitboxHalfSize.y=collider.bounds.extents.y;
attackHitboxColliders=Physics.OverlapBox(transform.position+transform.forward*(collider.bounds.extents.z+attackHitboxHalfSize.z),attackHitboxHalfSize,transform.rotation);
Debug.DrawRay(transform.position,transform.forward*(collider.bounds.extents.z+attackHitboxHalfSize.z),Color.white,.1f);


}
[NonSerialized]protected bool didDamage;
protected virtual void DoDamageHitbox(){//  This default function is like a "ghost" attack. It hits everything in front of the actor. Use Boxcast so the attack stops when a target is hit
if(!didDamage){


    Debug.LogWarning("do damage");
if(attackHitboxColliders==null){
OverlappedCollidersOnAttack();
}


if(attackHitboxColliders!=null){
    Debug.LogWarning("attackHitboxColliders.Length:"+attackHitboxColliders.Length);
for(int i=0;i<attackHitboxColliders.Length;i++){var collider=attackHitboxColliders[i];
AI enemy;
if(collider.CompareTag("Player")&&(enemy=collider.GetComponent<AI>())!=null){
    Debug.LogWarning("collider hit:"+collider.name+"; tag:"+collider.tag,this);
    enemy.TakeDamage(this);
}
}
}
attackHitboxColliders=null;


    didDamage=true;
}
}
protected virtual void TakeDamage(AI fromEnemy){}
protected virtual void Die(){
if(deadStance==-1){Dying=DeadForGoodDelay;}
}
protected Vector3 MyDest{get{return dest;}set{dest=value;destSet=true;}}[NonSerialized]Vector3 dest;protected bool destSet{get;private set;}public Vector3 Dest{get{return dest;}}
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
public enum Motions:int{
MOTION_STAND  =0,
MOTION_MOVE   =1,
MOTION_HIT    =2,
MOTION_ATTACK =3,
MOTION_ATTACK2=4,
MOTION_DEAD   =5,
}


[SerializeField]internal bool DEBUG_ATTACK;
[SerializeField]internal bool DEBUG_GETHIT;
[SerializeField]internal bool DEBUG_DIE;

    
public bool HasPassiveRole(){
return(Role==Roles.HomunculusPassive||
       Role==Roles.HomunculusAggressive||
       Role==Roles.HumanPassive||
       Role==Roles.HumanAggressive||
       Role==Roles.Neutral);
}
public bool IsAllyTo(AI other){
if((this.Role==Roles.HomunculusPassive||
    this.Role==Roles.HomunculusAggressive)&&
   (other.Role==Roles.HomunculusPassive||
    other.Role==Roles.HomunculusAggressive||
    other.Role==Roles.HumanPassive||
    other.Role==Roles.HumanAggressive)){
return(true);
}
if((this.Role==Roles.HumanPassive||
    this.Role==Roles.HumanAggressive)&&
   (other.Role==Roles.HumanPassive||
    other.Role==Roles.HumanAggressive||
    other.Role==Roles.HomunculusPassive||
    other.Role==Roles.HomunculusAggressive)){
return(true);
}
return(this.Role==Roles.Neutral||other.Role==Roles.Neutral);
}
}