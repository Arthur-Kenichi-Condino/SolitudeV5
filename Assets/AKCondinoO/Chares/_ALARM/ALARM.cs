using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ActorManagementMentana;
public class ALARM:_3DSprite{
public override void InitAttributes(bool random=true){
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":init attributes");
Attributes.STR=mathrandom.Next(12,73);
Attributes.VIT=mathrandom.Next(12,25);
Attributes.INT=mathrandom.Next(1,25);
Attributes.AGI=mathrandom.Next(49,73);
Attributes.DEX=mathrandom.Next(49,73);
Attributes.LUK=mathrandom.Next(1,25);
    Attributes.BaseMaxStamina=Attributes.CurStamina=GetBaseMaxStamina();
       Attributes.BaseMaxFocus=Attributes.CurFocus=GetBaseMaxFocus();
                    Attributes.BaseAspd=GetBaseAspd();
ValidateAttributesSet(1);
}
protected override void Awake(){
    //Debug.LogWarning("here");
                   base.Awake();   
}
[NonSerialized]public float TryIdleActionInterval=1f;[NonSerialized]float NextIdleActionTimer;[NonSerialized]public float Boredom;public enum CreativeIdleness:int{MOVE_RANDOM=0,RANDOM_SKILL=1,}[NonSerialized]readonly int CreativeIdlenessActionsCount=Enum.GetValues(typeof(CreativeIdleness)).Length;
protected override void OnIDLE_ST(){
if(MyEnemy!=null){
if(!IsInAttackSight(MyEnemy)){
if(LOG&&LOG_LEVEL<=-1)Debug.Log(GetType()+":chase",this);
MyState=State.CHASE_ST;
return;
}else{
if(LOG&&LOG_LEVEL<=-1)Debug.Log(GetType()+":attack",this);
STOP();
Attack(MyEnemy);
MyState=State.ATTACK_ST;
return;
}
}
if(NextIdleActionTimer<=0){
if(IsGrounded){
if(mathrandom.NextDouble()<=Boredom){
var action=(CreativeIdleness)mathrandom.Next(0,CreativeIdlenessActionsCount);
if(LOG&&LOG_LEVEL<=-1)Debug.Log(GetType()+":CreativeIdleness action:"+action,this);
    switch(action){
        case(CreativeIdleness.MOVE_RANDOM):{
            MoveToRandom(mathrandom);
        break;}
    }
Boredom=0;
}else{
Boredom+=0.1f;
}
        NextIdleActionTimer=TryIdleActionInterval;
}
}else{
    NextIdleActionTimer-=Time.deltaTime;
}
}
[NonSerialized]float enemyTouchedReactionTimeout;[NonSerialized]float enemyTouchedReactTime=2f;[NonSerialized]float enemyTouchedReposTimeout;[NonSerialized]float enemyTouchedReposTime=2f;
protected override void OnCHASE_ST(){
if(LOG&&LOG_LEVEL<=-10)Debug.Log(GetType()+":OnCHASE_ST",this);
if(MyEnemy==null){
if(LOG&&LOG_LEVEL<=-1)Debug.Log(GetType()+":idle",this);
STOP();
MyState=State.IDLE_ST;
return;
}
if(enemyTouchedReposTimeout>0){
    enemyTouchedReposTimeout-=Time.deltaTime;
}
if(enemyTouchedReactionTimeout>0){
if(!enemyTouchingMe){
        enemyTouchedReactionTimeout=0;
}else{
        enemyTouchedReactionTimeout-=Time.deltaTime;
}
}
if(enemyTouchedReactionTimeout<=0){
if(IsInAttackSight(MyEnemy)){
if(LOG&&LOG_LEVEL<=-1)Debug.Log(GetType()+":attack",this);
STOP();
Attack(MyEnemy);
MyState=State.ATTACK_ST;
return;
}
if(enemyTouchingMe){
        enemyTouchedReactionTimeout=enemyTouchedReactTime;enemyTouchedReposTimeout=enemyTouchedReposTime;
}
}
if(enemyTouchedReactionTimeout<=0&&enemyTouchedReposTimeout<=0){
if(LOG&&LOG_LEVEL<=-10)Debug.Log(GetType()+":OnCHASE_ST: GoTo",this);
doChase();
}else{
if(LOG&&LOG_LEVEL<=-10)Debug.Log(GetType()+":OnCHASE_ST: move away GoTo",this);
doChasingMoveAway();
}
}
protected override void OnATTACK_ST(){
if(LOG&&LOG_LEVEL<=-10)Debug.Log(GetType()+":OnATTACK_ST",this);
if(MyEnemy==null){
if(LOG&&LOG_LEVEL<=-1)Debug.Log(GetType()+":idle",this);
STOP();
MyState=State.IDLE_ST;
return;
}
if(CurPathTgt!=null){
STOP();
}
Attack(MyEnemy);
if(!IsInAttackSight(MyEnemy)){
if(LOG&&LOG_LEVEL<=-1)Debug.Log(GetType()+":chase",this);
STOP(true);
MyState=State.CHASE_ST;
return;
}
}
protected override void GetTargets(){
for(int k=AsAggroEnemies.Keys.Count-1;k>=0;k--){var i=AsAggroEnemies.Keys.ElementAt(k);
var tuple=AsAggroEnemies[i];
    tuple.timeout-=Time.deltaTime;
AsAggroEnemies[i]=tuple;if(AsAggroEnemies[i].timeout<=0||AsAggroEnemies[i].actor.GetMotion==Motions.MOTION_DEAD||AsAggroEnemies[i].actor.OutOfSight){AsAggroEnemies.Remove(i);}
}
MyPossibleTargets.Clear();
foreach(var kvp in MySight.IsInVisionSight){var i=kvp.Key;var v=kvp.Value.actor;bool detected=kvp.Value.directSight;
if(i!=this.Id&&v.GetMotion!=Motions.MOTION_DEAD&&!v.OutOfSight){
if(detected){Vector3 pos=kvp.Value.pos;
if(v.HasPassiveRole()){    
if(LOG&&LOG_LEVEL<=-20)Debug.Log("me "+this.Id+", my TypeId:"+TypeToTypeId[GetType()]+"; possible target "+i+", TypeId:"+TypeToTypeId[v.GetType()],v);
var dis=Vector3.Distance(transform.position,pos);
MyPossibleTargets.Add(i,(v,pos,dis));
if(!AsAggroEnemies.ContainsKey(i)){
AsAggroEnemies.Add(i,(v,-1,0));
}
var tuple=AsAggroEnemies[i];
    tuple.dis=dis;
    tuple.timeout=5f;
AsAggroEnemies[i]=tuple;//  At this time, ALARM is just a killing machine; target marked for a 5 s timeout
}
}
}
}
MyEnemy=null;
if(MyEnemy==null){
float dis=-1;
foreach(var kvp in AsAggroEnemies){var i=kvp.Key;var v=kvp.Value.actor;var d=kvp.Value.dis;
if(dis==-1||dis>d){
MyEnemy=v;
    dis=d;
}
}
}
if(MyEnemy!=null){
if(LOG&&LOG_LEVEL<=-10)Debug.Log("Me:"+this+"; MyEnemy:"+MyEnemy,MyEnemy);
}
}
protected override bool IsInAttackSight(AI enemy){
            return base.IsInAttackSight(enemy);
}
protected override void Attack(AI enemy){
if(nextAttackTimer>0)return;
                   base.Attack(enemy);
if(deadStance!=-1||hitStance!=-1)return;if(attackStance==-1){attackStance=0;curAnimTime=0;
if(sfx!=null){sfx.Play((int)ActorSounds._ATTACK,true);}
}
}
protected override void TakeDamage(AI fromEnemy){
                   base.TakeDamage(fromEnemy);
if(damage<=0)return;
if(deadStance!=-1)return;attackStance=-1;hitStance=0;curAnimTime=0;
if(sfx!=null){sfx.Play((int)ActorSounds._HIT,true);}
}
protected override void Die(){
                   base.Die();
attackStance=-1;hitStance=-1;if(deadStance==-1){deadStance=0;curAnimTime=0;
if(sfx!=null){sfx.Play((int)ActorSounds._DEAD,true);}
}
}
}