using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActorManagementMentana;
public class ALARM:_3DSprite{
[NonSerialized]public float TryIdleActionInterval=1f;[NonSerialized]float NextIdleActionTimer;[NonSerialized]public float Boredom;public enum CreativeIdleness:int{MOVE_RANDOM=0,RANDOM_SKILL=1,}[NonSerialized]readonly int CreativeIdlenessActionsCount=Enum.GetValues(typeof(CreativeIdleness)).Length;
protected override void OnIDLE_ST(){
                   base.OnIDLE_ST();



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


MyPossibleTargets.Clear();
foreach(var kvp in MySight.IsInVisionSight){var i=kvp.Key;var v=kvp.Value.actor;bool detected=kvp.Value.directSight;
if(i!=this.Id){
if(detected){Vector3 pos=kvp.Value.pos;
if(v.HasPassiveRole()){

    Debug.LogWarning("my TypeId:"+TypeToTypeId[GetType()]+"; possible target TypeId:"+TypeToTypeId[v.GetType()]);

MyPossibleTargets.Add(i,(v,pos,Vector3.Distance(transform.position,pos)));
}
}
}
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