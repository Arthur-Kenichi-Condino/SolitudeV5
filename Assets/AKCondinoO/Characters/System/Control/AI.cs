using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ActorManagementMentana;
public class AI:Pathfinder{
public int Id{get;internal set;}public int TypeId{get;internal set;}[SerializeField]public Roles Role;
[NonSerialized]protected Sight MySight;
[NonSerialized]protected CharSFX sfx;
protected override void Awake(){
if(manager==null){OutOfSight_v=false;}
    //Debug.LogWarning("here");
                   base.Awake();
MySight=GetComponentInChildren<Sight>();
    sfx=GetComponent<CharSFX>();
MyAttackRange=MyAttackRange*BodyRadius;
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

                
    Debug.LogWarning("OutOfSight");


OutOfSight_disable=true;
}
OutOfSight_v=value;
}
}
#endregion
[NonSerialized]protected bool actorTouchingMe;
[NonSerialized]protected bool enemyTouchingMe;
[NonSerialized]bool firstLoop=true;protected override void Update(){
                                                      base.Update();
attackHitboxColliders=null;
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
gameObject.transform.root.gameObject.SetActive(false);
if(GetActors.ContainsKey(Id)){GetActors.Remove(Id);
if(LOG&&LOG_LEVEL<=0)Debug.Log("disable OutOfSight actor and add to inactive queue");
InactiveActorsByTypeId[TypeId].AddLast(this);
}else{
if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("OutOfSight actor wasn't marked to be active so it should already be in its InactiveActorsByTypeId queue");
}
    OutOfSight_disable=false;
return;
}
#endregion


if(this is _3DSprite){
if(sfx!=null){if(MyMotion==Motions.MOTION_STAND){sfx.Play((int)ActorSounds._IDLE);}else if(MyMotion==Motions.MOTION_MOVE){sfx.Play((int)ActorSounds._MOVE);}}
}


if(DEBUG_ATTACK){Attack(null);}if(DEBUG_GETHIT){DEBUG_GETHIT=false;TakeDamage(null);}if(DEBUG_DIE){DEBUG_DIE=false;Die();}


if(LOG&&LOG_LEVEL<=0)Debug.Log("MySight.IsInHearingSight.Count:"+MySight.IsInHearingSight.Count+";MySight.IsInVisionSight.Count:"+MySight.IsInVisionSight.Count);
GetTargets();

actorTouchingMe=false;
enemyTouchingMe=false;
if(collisions.Count>0){foreach(var collision in collisions){
    if(collision.Key.gameObject.CompareTag("Player")){
actorTouchingMe=true;
    }
    if(MyEnemy!=null&&collision.Key.gameObject.CompareTag("Player")&&collision.Key.gameObject.GetComponent<AI>()==MyEnemy){
enemyTouchingMe=true;
    }
if(actorTouchingMe&&enemyTouchingMe)break;//  Break when all checks are true, so only on O(n) operation is processed for any needed check
}}

//Debug.LogWarning("nextAttackTimer:"+nextAttackTimer);
if(nextAttackTimer>0){
    nextAttackTimer-=Time.deltaTime;
if(LOG&&LOG_LEVEL<=-110)Debug.Log("waiting for nextAttackTimer to get to zero ["+this,this);
}
if(Autonomous<=0){
WALK_PATH();

    
doingAttackMoveAway=false;
if(MyState==State.EXCUSE_ST){OnEXCUSE_ST();}
if(MyState==State.FOLLOW_ST){OnFOLLOW_ST();}
if(MyState==State.IDLE_ST){OnIDLE_ST();}
if(MyState==State.CHASE_ST){OnCHASE_ST();}
if(MyState==State.ATTACK_ST){OnATTACK_ST();}
if(MyState==State.AVOID_ST){OnAVOID_ST();}
if(MyState==State.DODGE_ST){OnDODGE_ST();}
if(MyState==State.FLEE_ST){OnFLEE_ST();}
if(MyState==State.SKILL_OBJECT_ST){OnSKILL_OBJECT_ST();}
evadedFromAlly=false;

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
GetEnemiesAttackingMe[i]=tuple;if(GetEnemiesAttackingMe[i].timeout<=0||GetEnemiesAttackingMe[i].actor.GetMotion==Motions.MOTION_DEAD||GetEnemiesAttackingMe[i].actor.OutOfSight){GetEnemiesAttackingMe.Remove(i);}
}
for(int k=GetEnemiesAttackingAlly.Keys.Count-1;k>=0;k--){var i=GetEnemiesAttackingAlly.Keys.ElementAt(k);
var tuple=GetEnemiesAttackingAlly[i];
    tuple.timeout-=Time.deltaTime;
GetEnemiesAttackingAlly[i]=tuple;if(GetEnemiesAttackingAlly[i].timeout<=0||GetEnemiesAttackingAlly[i].actor.GetMotion==Motions.MOTION_DEAD||GetEnemiesAttackingAlly[i].actor.OutOfSight){GetEnemiesAttackingAlly.Remove(i);}
}
MyPossibleTargets.Clear();
foreach(var actor in GetActors){var i=actor.Key;var v=actor.Value;
if(i!=this.Id&&v.GetMotion!=Motions.MOTION_DEAD&&!v.OutOfSight){
if(v.Target==this){//  This following mode of detecting targets does not take into consideration the stealth status of enemies
Vector3 pos;float dis;
if(MyMotion==Motions.MOTION_HIT){pos=v.collider.bounds.center;
addPossibleTarget();attackingMe();
if(LOG&&LOG_LEVEL<=-10)Debug.Log(GetType()+":I'm under attack",this);
}else if(MySight.IsInVisionSight.ContainsKey(i)&&MySight.IsInVisionSight[i].directSight){pos=MySight.IsInVisionSight[i].pos;
addPossibleTarget();attackingMe();
if(LOG&&LOG_LEVEL<=-10)Debug.Log(GetType()+":enemy approaching my position",this);
}
void addPossibleTarget(){
dis=Vector3.Distance(collider.bounds.center,pos);
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
}else if(v.Target!=null&&IsAllyTo(v.Target)){var ally=v.Target;
Vector3 pos;float dis;
if(TypeId.ActorType()==TypeIds._EIRA){pos=v.collider.bounds.center;


Debug.LogWarning("ally in danger;[my type:"+TypeId.ActorType());
addPossibleTarget();attackingAlly();


}else if(MySight.IsInVisionSight.ContainsKey(ally.Id)&&MySight.IsInVisionSight[ally.Id].directSight&&MySight.IsInVisionSight.ContainsKey(i)&&MySight.IsInVisionSight[i].directSight){pos=MySight.IsInVisionSight[i].pos;
//if(MySight.IsInVisionSight.ContainsKey(i)&&MySight.IsInVisionSight[i].directSight){pos=MySight.IsInVisionSight[i].pos;

Debug.LogWarning("ally in danger;[my type:"+TypeId.ActorType());
addPossibleTarget();attackingAlly();

//}else{


//Debug.LogWarning("ally in danger;but I can't see their enemy yet;[my type:"+TypeId.ActorType());


//}
}else if(MySight.IsInHearingSight.ContainsKey(ally.Id)&&(
    ally.GetMotion==Motions.MOTION_HIT||
        v.GetMotion==Motions.MOTION_ATTACK||
        v.GetMotion==Motions.MOTION_ATTACK2)){pos=MySight.IsInHearingSight[ally.Id].pos;

                        
Debug.LogWarning("ally in danger;[my type:"+TypeId.ActorType());
addPossibleTarget();attackingAlly();


}else{


Debug.LogWarning("ally in danger;but I can't see them yet;[my type:"+TypeId.ActorType());


}
void addPossibleTarget(){
dis=Vector3.Distance(collider.bounds.center,pos);
MyPossibleTargets.Add(i,(v,pos,dis));
}
void attackingAlly(){
if(!GetEnemiesAttackingAlly.ContainsKey(i)){
GetEnemiesAttackingAlly.Add(i,(v,-1,0));
}
var tuple=GetEnemiesAttackingAlly[i];
    tuple.dis=dis;
    tuple.timeout=5f;
GetEnemiesAttackingAlly[i]=tuple;
}
}
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
if(MyEnemy==null){    
float dis=-1;
foreach(var kvp in GetEnemiesAttackingAlly){var i=kvp.Key;var v=kvp.Value.actor;var d=kvp.Value.dis;
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
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":chase",this);
MyState=State.CHASE_ST;
return;
}else{
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":attack",this);
STOP();
MyState=State.ATTACK_ST;sinceLastHitTimer=0;
return;
}
}
}
protected virtual void OnCHASE_ST(){
if(LOG&&LOG_LEVEL<=0)Debug.Log(GetType()+":OnCHASE_ST",this);
if(MyEnemy==null){
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":idle",this);
STOP();
MyState=State.IDLE_ST;
return;
}
if(IsInAttackSight(MyEnemy)){
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":attack",this);
STOP();
MyState=State.ATTACK_ST;sinceLastHitTimer=0;
return;
}
doChase();
}
protected virtual void doChase(){
if(tracing||GoToQueue.Count>0)return;
if(!destSet||CurPathTgt==null||Vector3.Distance(MyDest,MyEnemy.collider.bounds.center)>MyAttackRange){
    if(!actorTouchingMe&&Vector3.Distance(MyEnemy.collider.bounds.center,collider.bounds.center)>BodyRange){
    MyDest=MyEnemy.collider.bounds.center;
    }else{
    MyDest=MyEnemy.collider.bounds.center;var dir=collider.bounds.center-MyEnemy.collider.bounds.center;dir.y=0;dir=Quaternion.Euler(0,(float)mathrandom.NextDouble()*360,0)*dir.normalized;MyDest+=dir*(MyEnemy.BodyRange+BodyRange+MyAttackRange);
    }
if(DRAW_LEVEL<=0)Debug.DrawLine(collider.bounds.center,MyDest,Color.blue,1f);
GoTo(new Ray(MyDest,Vector3.down));
}
}
protected virtual void doChasingMoveAway(){
if(tracing||GoToQueue.Count>0)return;
if(!destSet||CurPathTgt==null){
    MyDest=MyEnemy.collider.bounds.center;var dir=collider.bounds.center-MyEnemy.collider.bounds.center;dir.y=0;dir=Quaternion.Euler(0,(float)mathrandom.NextDouble()*360,0)*dir.normalized;MyDest+=dir*(MyEnemy.BodyRange+BodyRange);
if(DRAW_LEVEL<=0)Debug.DrawLine(collider.bounds.center,MyDest,Color.blue,1f);
GoTo(new Ray(MyDest,Vector3.down));
}
}
[NonSerialized]float sinceLastHitTimer;[NonSerialized]float hitDetectionReactionTick=6f;
protected virtual void OnATTACK_ST(){
if(LOG&&LOG_LEVEL<=0)Debug.Log(GetType()+":OnATTACK_ST",this);
if(MyEnemy==null){
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":idle",this);
STOP();
MyState=State.IDLE_ST;
return;
}


if(evadedFromAlly){
            Debug.LogWarning("evadedFromAlly:"+evadedFromAlly,this);

if(mathrandom.NextDouble()<=.25f){
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":avoid",this);
STOP();
MyState=State.AVOID_ST;
return;
}


}
if(avoid){
avoid=false;
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":avoid",this);
STOP();
MyState=State.AVOID_ST;
return;
}


int r;
if((r=doAttackingMoveAway())!=1){
doAttack();
if(!IsInAttackSight(MyEnemy)){
if(LOG&&LOG_LEVEL<=-1)Debug.Log(GetType()+":chase",this);
STOP(true);
MyState=State.CHASE_ST;
return;
}
}
if(r==2){}


}
[NonSerialized]bool doingAttackMoveAway;protected virtual int doAttackingMoveAway(){
if(tracing||GoToQueue.Count>0)goto _End;
if((MyMotion==Motions.MOTION_HIT||actorTouchingMe)&&sinceLastHitTimer<=0&&MyEnemy.AttackRange+MyEnemy.BodyRadius<=MyAttackRange+BodyRadius&&mathrandom.NextDouble()<.05){
STOP(true);
    sinceLastHitTimer=hitDetectionReactionTick;
    Debug.LogWarning("OnATTACK_ST: hitDetectionReactionTick:"+hitDetectionReactionTick);
}else if(sinceLastHitTimer>0){
    sinceLastHitTimer-=Time.deltaTime;
if(actorTouchingMe){
if(CurPathTgt!=null){
STOP();
}
return 0;
}
if(CurPathTgt==null){
return 0;
}
doingAttackMoveAway=true;
return 1;
}
if(!destSet||sinceLastHitTimer==hitDetectionReactionTick){
    Debug.LogWarning("OnATTACK_ST: move away GoTo");
    MyDest=MyEnemy.collider.bounds.center;var dir=collider.bounds.center-MyEnemy.collider.bounds.center;dir.y=0;dir=Quaternion.Euler(0,(float)(mathrandom.NextDouble()*2-1)*90,0)*dir.normalized;MyDest+=dir*(MyEnemy.BodyRange*2+BodyRange*2);
Debug.DrawLine(collider.bounds.center,MyDest,Color.blue,1f);
GoTo(new Ray(MyDest,Vector3.down));
doingAttackMoveAway=true;
return 2;}
_End:{}
if(CurPathTgt!=null){
STOP();
}
return 0;}
[NonSerialized]bool willHitEnemy,willHitAlly;[NonSerialized]bool avoid;void doAttack(){
if(attackHitboxColliders==null){
OverlappedCollidersOnAttack();
}


bool cancel;TestHit(out cancel);


Debug.LogWarning("willHitEnemy:"+willHitEnemy+";willHitAlly:"+willHitAlly+" ["+this,this);


if(!cancel){
Attack(MyEnemy);
}else{


//  TO DO: fazer aliado, ou eu mesmo, se mover: usar AVOID_ST em mim se aliado também está atacando, usar AVOID_ST no aliado se ele usar EVADE quando for leva dano "On Will Take Damage"            
if(mathrandom.NextDouble()<=.25f){
avoid=true;
}
Attack(MyEnemy);


}


//attackHitboxColliders=null;
}
void TestHit(out bool cancel){
willHitEnemy=false;willHitAlly=false;cancel=false;
if(attackHitboxColliders!=null){
for(int i=0;i<attackHitboxColliders.Length;i++){var collider=attackHitboxColliders[i];
AI actor;
if(collider.CompareTag("Player")&&(actor=collider.GetComponent<AI>())!=null){
if(IsAllyTo(actor)){
    Debug.LogWarning("cancel attack and move, or ally may be hit:"+collider.name+"; tag:"+collider.tag,this);
cancel=true;
willHitAlly=true;
}
if(actor==MyEnemy){
willHitEnemy=true;
}
if(willHitEnemy&&willHitAlly){
break;
}
}
}
}
}
[NonSerialized]protected float avoidanceTimeout=.5f;[NonSerialized]protected float avoidanceTime;[NonSerialized]protected bool avoided=false;
protected virtual void OnAVOID_ST(){
if(MyEnemy==null){
avoidanceTime=0f;avoided=false;
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":idle",this);
STOP();
MyState=State.IDLE_ST;
return;
}


if(avoidanceTime>avoidanceTimeout){
avoidanceTime=0f;avoided=false;
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":chase",this);
STOP();
MyState=State.CHASE_ST;
return;
}
avoidanceTime+=Time.deltaTime;


if(tracing||GoToQueue.Count>0){
}else if(!destSet||CurPathTgt==null){


if(avoided){
avoidanceTime=0f;avoided=false;
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":chase",this);
STOP();
MyState=State.CHASE_ST;
return;
}


avoided=true;
    MyDest=MyEnemy.collider.bounds.center;var dir=collider.bounds.center-MyEnemy.collider.bounds.center;dir.y=0;dir=Quaternion.Euler(0,(float)(mathrandom.NextDouble()*2-1)*90,0)*dir.normalized;MyDest+=dir*(float)(mathrandom.NextDouble()+1);


Debug.DrawLine(collider.bounds.center,MyDest,Color.blue,avoidanceTimeout);

            
GoTo(new Ray(MyDest,Vector3.down));
}


}
protected virtual void OnDODGE_ST(){
}
protected virtual void OnFLEE_ST(){
}
protected virtual void OnSKILL_OBJECT_ST(){}
[NonSerialized]protected float MyAttackRange=1.5f;public float AttackRange{get{return MyAttackRange;}}
protected virtual bool IsInAttackSight(AI enemy){
if(Vector3.Distance(collider.bounds.center,enemy.collider.bounds.center)-(BodyRadius+enemy.BodyRadius)<=MyAttackRange){
return true;
}
return false;}
[NonSerialized]protected float attackInterval=.25f;[SerializeField]protected float attackWaitForSoundTime=0;[NonSerialized]protected float nextAttackTimer=0;
[NonSerialized]protected AttackModes MyAttackMode=AttackModes.Ghost;public enum AttackModes{Ghost,Physical}[NonSerialized]bool testHitBeforeAttacking=true;
protected virtual bool Attack(AI enemy){bool result=false;
    Debug.LogWarning("trying attack stance ["+this);


if(attackHitboxColliders==null&&testHitBeforeAttacking){
OverlappedCollidersOnAttack();
TestHit(out _);
}


if(willHitEnemy||(!testHitBeforeAttacking&&attackHitboxColliders==null)){
if(attackStance==-1){
if(deadStance!=-1||hitStance!=-1){
//Debug.LogWarning("attack stance failed: hit or dying");
}else{
result=true;
    Debug.LogWarning("new attack started: set to do damage next animation");
    didDamage=false;
    nextAttackTimer=(attackInterval/Attributes.Aspd)+attackWaitForSoundTime;
    Debug.LogWarning("nextAttackTimer:"+nextAttackTimer+";attackInterval:"+attackInterval+";Attributes.Aspd:"+Attributes.Aspd+";attackWaitForSoundTime:"+attackWaitForSoundTime);
}
}else{
//Debug.LogWarning("attack stance failed: already attacking["+this);
}
}else{
Debug.LogWarning("attack stance failed: will not hit enemy["+this);
}
if(enemy!=null){
inputViewRotationEuler.y=Quaternion.LookRotation((enemy.collider.bounds.center-collider.bounds.center).normalized).eulerAngles.y-transform.eulerAngles.y;
}
if(testHitBeforeAttacking&&attackHitboxColliders!=null){
//attackHitboxColliders=null;
}
return result;}
[NonSerialized]Vector3 attackHitboxHalfSize;[NonSerialized]protected Collider[]attackHitboxColliders=null;
protected virtual void OverlappedCollidersOnAttack(){

    
if(Vector3.Angle((MyEnemy.collider.bounds.center-collider.bounds.center).normalized,transform.forward)>15f){
attackHitboxColliders=new Collider[0];
return;
}
attackHitboxHalfSize.x=collider.bounds.extents.x+MyAttackRange;
attackHitboxHalfSize.z=collider.bounds.extents.z+MyAttackRange;
attackHitboxHalfSize.y=collider.bounds.extents.y+MyAttackRange;
attackHitboxHalfSize*=RangeMultiplier;
var dest=transform.forward*(collider.bounds.extents.z+attackHitboxHalfSize.z);
attackHitboxColliders=Physics.BoxCastAll(collider.bounds.center,attackHitboxHalfSize,dest.normalized,transform.rotation,collider.bounds.extents.z+attackHitboxHalfSize.z).Select(v=>v.collider).ToArray();
if(attackHitboxColliders.Length==0){//  TO DO: se ghost attack, OverlapBox, ou se perto demais, também overlap box ou usar Avoid
attackHitboxColliders=Physics.OverlapBox(collider.bounds.center+dest,attackHitboxHalfSize,transform.rotation);
}
Debug.DrawRay(collider.bounds.center,dest,Color.red,.5f);


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
if(collider.CompareTag("Player")&&(enemy=collider.GetComponent<AI>())!=this&&enemy!=null){
    Debug.LogWarning("collider hit:"+collider.name+"; tag:"+collider.tag,this);
    enemy.TakeDamage(this);
}
}
}
//attackHitboxColliders=null;


    didDamage=true;
}
}
[NonSerialized]protected float damage;[NonSerialized]protected bool evadedFromAlly;
protected virtual void TakeDamage(AI fromEnemy){
if(fromEnemy!=null){
damage=fromEnemy.Attributes.ATK-Attributes.DEF;
}

        
bool evaded=false;
var skill=Skill.GetBest(this,Skill.When.onWillTakeDamage);if(skill!=null){


    Debug.LogWarning(skill);
if(skill is _EVADE evade&&fromEnemy!=null){evaded=evade.DoSkill(this,fromEnemy);

                
    Debug.LogWarning("evaded:"+evaded+";evade.Result:"+evade.Result);
if(evaded){
if(IsAllyTo(fromEnemy)){evadedFromAlly=true;}
inputViewRotationEuler.y=Quaternion.LookRotation((fromEnemy.collider.bounds.center-collider.bounds.center).normalized).eulerAngles.y-transform.eulerAngles.y;
STOP();
}


}


}


if(evaded)damage=0;else if(damage<=1)damage=1;Attributes.CurStamina-=damage;if(Attributes.CurStamina<=0){Attributes.CurStamina=0;Die();}else if(damage>0){
    Debug.LogWarning("reset nextAttackTimer for ["+this);
    nextAttackTimer=0;
}
}
protected virtual void Die(){
if(deadStance==-1){Dying=DeadForGoodDelay;}
}
protected Vector3 MyDest{get{return dest;}set{dest=value;destSet=true;}}[NonSerialized]Vector3 dest;protected bool destSet{get;private set;}public Vector3 Dest{get{return dest;}}
[NonSerialized]public Vector3 ReachedTgtDisThreshold;
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
ReachedTgtDisThreshold.y=colliderHalfExtents.y;
ReachedTgtDisThreshold.x=colliderHalfExtents.x;
ReachedTgtDisThreshold.z=colliderHalfExtents.z;
_axisDiff.y=CurPathTgt.Value.pos.y-collider.bounds.center.y;_axisDist.y=Mathf.Abs(_axisDiff.y);
_axisDiff.x=CurPathTgt.Value.pos.x-collider.bounds.center.x;_axisDist.x=Mathf.Abs(_axisDiff.x);
_axisDiff.z=CurPathTgt.Value.pos.z-collider.bounds.center.z;_axisDist.z=Mathf.Abs(_axisDiff.z);      
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
    if(Mathf.Abs(collider.bounds.center.y-_movementSnapshotPos.y)>.1f||
       Mathf.Abs(collider.bounds.center.x-_movementSnapshotPos.x)>.1f||
       Mathf.Abs(collider.bounds.center.z-_movementSnapshotPos.z)>.1f){
if(LOG&&LOG_LEVEL<=0)Debug.Log("normal movement detected");
renew_movementWasDetectedTimer();
    }else{
if(LOG&&LOG_LEVEL<=0)Debug.Log("I am stuck in this position!");
    }
_movementSnapshotPos=collider.bounds.center;_movementSnapshotTimer=DoMovementSnapshotTime*(float)(mathrandom.NextDouble()+.5f);
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
inputMoveSpeed.z=-MaxMoveSpeed.z*MaxMoveSpeedBackwardMultiplier;
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
inputMoveSpeed.x=MaxMoveSpeed.x*moveCircularlyAroundTgt_dir;
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
inputMoveSpeed.x=MaxMoveSpeed.x*moveSidewaysRandomDir_dir;
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
inputMoveSpeed.z=MaxMoveSpeed.z;
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
inputMoveSpeed.y=MaxMoveSpeed.y;
}else{
inputMoveSpeed.y=0;
}
#endregion
}
break;}
#endregion
#region none [not stuck]
default:{
if(_axisDist.y>ReachedTgtDisThreshold.y&&
   _axisDist.x<=ReachedTgtDisThreshold.x&&
   _axisDist.z<=ReachedTgtDisThreshold.z){   
if(collider.bounds.center.y>=CurPathTgt.Value.pos.y+.1f){
    if(CurPathTgt.Value.mode!=Node.PreferredReachableMode.fall){
    var cur=CurPathTgt.Value;cur.mode=Node.PreferredReachableMode.fall;
    CurPathTgt=cur;
    }
}else if(collider.bounds.center.y<CurPathTgt.Value.pos.y+.1f||rigidbody.velocity.y<=float.Epsilon){
    if(CurPathTgt.Value.mode!=Node.PreferredReachableMode.jump){
    var cur=CurPathTgt.Value;cur.mode=Node.PreferredReachableMode.jump;
    CurPathTgt=cur;
    }
}
}
if(doingAttackMoveAway){_dir=-_dir;}
if(_axisDist.x>float.Epsilon||
   _axisDist.z>float.Epsilon){
inputViewRotationEuler.y=Quaternion.LookRotation(_dir).eulerAngles.y-transform.eulerAngles.y;
}
inputMoveSpeed.x=0;
if(!canFly){
if(CurPathTgt.Value.mode==Node.PreferredReachableMode.jump&&collider.bounds.center.y<CurPathTgt.Value.pos.y+.1f){
#region necessário pular
inputMoveSpeed.z=0;
if(IsGrounded){
Jump=true;
inputMoveSpeed.y=MaxMoveSpeed.y;
}else{
inputMoveSpeed.y=0;
}
#endregion
}else{
#region ir para o destino normalmente
if((IsGrounded||!HittingWall)&&
  (_axisDist.x>ReachedTgtDisThreshold.x||
   _axisDist.z>ReachedTgtDisThreshold.z)){
inputMoveSpeed.z+=InputMoveAcceleration.z*Time.deltaTime;if(inputMoveSpeed.z>MaxMoveSpeed.z){inputMoveSpeed.z=MaxMoveSpeed.z;}/*inputMoveSpeed.z=MaxMoveSpeed.z;*/if(doingAttackMoveAway){inputMoveSpeed.z=-inputMoveSpeed.z*MaxMoveSpeedBackwardMultiplier;}
}else{
inputMoveSpeed.z=0;
}
inputMoveSpeed.y=0;
#endregion
}
}else{


    Debug.LogWarning(CurPathTgt.Value.mode);
#region ir para o destino normalmente
if((IsGrounded||!HittingWall)&&
  (_axisDist.x>ReachedTgtDisThreshold.x||
   _axisDist.z>ReachedTgtDisThreshold.z)){
inputMoveSpeed.z+=InputMoveAcceleration.z*Time.deltaTime;if(inputMoveSpeed.z>MaxMoveSpeed.z){inputMoveSpeed.z=MaxMoveSpeed.z;}/*inputMoveSpeed.z=MaxMoveSpeed.z;*/if(doingAttackMoveAway){inputMoveSpeed.z=-inputMoveSpeed.z*MaxMoveSpeedBackwardMultiplier;}
}else{
inputMoveSpeed.z=0;
}
if(_axisDist.y>ReachedTgtDisThreshold.y){
if(_axisDiff.y<0){
    Debug.LogWarning("down");
inputMoveSpeed.y=-MaxMoveSpeed.y;
}else{
    Debug.LogWarning("up");
inputMoveSpeed.y=MaxMoveSpeed.y;
}
}else{
inputMoveSpeed.y=0;
}
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
AVOID_ST=20,
DODGE_ST=21,
FLEE_ST=22,
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