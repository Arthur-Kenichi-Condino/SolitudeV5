﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ActorManagementMentana;
public class ALARM:_3DSprite{
[NonSerialized]public float TryIdleActionInterval=1f;[NonSerialized]float NextIdleActionTimer;[NonSerialized]public float Boredom;public enum CreativeIdleness:int{MOVE_RANDOM=0,RANDOM_SKILL=1,}[NonSerialized]readonly int CreativeIdlenessActionsCount=Enum.GetValues(typeof(CreativeIdleness)).Length;
protected override void OnIDLE_ST(){
                   base.OnIDLE_ST();

if(MyEnemy!=null){
    Debug.LogWarning("attack or chase?");
}


if(NextIdleActionTimer<=0){
if(IsGrounded){
if(mathrandom.NextDouble()<=Boredom){
var action=(CreativeIdleness)mathrandom.Next(0,CreativeIdlenessActionsCount);
if(LOG&&LOG_LEVEL<=1)Debug.Log("CreativeIdleness action: "+action);
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
protected override void GetTargets(){


for(int k=AsAggroEnemies.Keys.Count-1;k>=0;k--){var i=AsAggroEnemies.Keys.ElementAt(k);
var tuple=AsAggroEnemies[i];
    tuple.timeout-=Time.deltaTime;
AsAggroEnemies[i]=tuple;if(AsAggroEnemies[i].timeout<=0){AsAggroEnemies.Remove(i);}
}
MyPossibleTargets.Clear();
foreach(var kvp in MySight.IsInVisionSight){var i=kvp.Key;var v=kvp.Value.actor;bool detected=kvp.Value.directSight;
if(i!=this.Id){
if(detected){Vector3 pos=kvp.Value.pos;
if(v.HasPassiveRole()){

    Debug.LogWarning(this.Id+", my TypeId:"+TypeToTypeId[GetType()]+"; possible target "+i+", TypeId:"+TypeToTypeId[v.GetType()],v);

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
if(dis==-1||dis>(dis=d)){
MyEnemy=v;
}
}
}
if(MyEnemy!=null){
    Debug.LogWarning("Me:"+this+"; MyEnemy:"+MyEnemy,MyEnemy);
}

//foreach(var actor in GetActors){var i=actor.Key;var v=actor.Value;
//if(i!=this.Id){
//if(v.HasPassiveRole()){
//if(MySight.IsInHearingSight.ContainsKey(i)){
//    Debug.LogWarning("my TypeId:"+TypeToTypeId[GetType()]+"; possible target TypeId:"+TypeToTypeId[v.GetType()]);
//}
//}
//}
//}
}
protected override void Attack(AI enemy){
                   base.Attack(enemy);
if(deadStance!=-1||hitStance!=-1)return;if(attackStance==-1){attackStance=0;curAnimTime=0;}
}
protected override void TakeDamage(AI fromEnemy){
                   base.TakeDamage(fromEnemy);
if(deadStance!=-1)return;attackStance=-1;hitStance=0;curAnimTime=0;
}
protected override void Die(){
                   base.Die();
attackStance=-1;hitStance=-1;if(deadStance==-1){deadStance=0;curAnimTime=0;}
}
}