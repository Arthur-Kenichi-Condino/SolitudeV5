using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ActorManagementMentana;
public class BAYERI:_3DSprite{
public override void InitAttributes(bool random=true){
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":init attributes");
Attributes.STR=mathrandom.Next(52,73);
Attributes.VIT=mathrandom.Next(52,73);
Attributes.INT=mathrandom.Next(88,100);
Attributes.AGI=mathrandom.Next(49,73);
Attributes.DEX=mathrandom.Next(66,100);
Attributes.LUK=mathrandom.Next(1,100);
    Attributes.BaseMaxStamina=Attributes.CurStamina=GetBaseMaxStamina();
       Attributes.BaseMaxFocus=Attributes.CurFocus=GetBaseMaxFocus();
                    Attributes.BaseAspd=GetBaseAspd();
ValidateAttributesSet(1);
}
protected override void Attack(AI enemy){
if(nextAttackTimer>0)return;
                   base.Attack(enemy);
if(deadStance!=-1||hitStance!=-1)return;if(attackStance==-1){attackStance=mathrandom.Next(0,2);curAnimTime=0;}
}
protected override void TakeDamage(AI fromEnemy){
                   base.TakeDamage(fromEnemy);
if(damage<=0)return;
if(deadStance!=-1)return;attackStance=-1;hitStance=0;curAnimTime=0;
}
protected override void Die(){
                   base.Die();
attackStance=-1;hitStance=-1;if(deadStance==-1){deadStance=0;curAnimTime=0;}
}
}