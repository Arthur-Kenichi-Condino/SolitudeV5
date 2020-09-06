using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ALARM:_3DSprite{[NonSerialized]protected System.Random mathrandom=new System.Random();
[NonSerialized]public float TryIdleActionInterval=1f;[NonSerialized]float NextIdleActionTimer;[NonSerialized]public float Boredom;public enum CreativeIdleness:int{MOVE_RANDOM=0,RANDOM_SKILL=1,}[NonSerialized]int CreativeIdlenessActionsCount=Enum.GetValues(typeof(CreativeIdleness)).Length;
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
}