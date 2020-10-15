using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ActorManagementMentana;
public class EIRA:_3DSprite{
public override void InitAttributes(bool random=true){
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":init attributes");
Attributes.FOR=mathrandom.Next(22,52);
Attributes.VIT=mathrandom.Next(22,52);
Attributes.INT=mathrandom.Next(88,100);
Attributes.AGI=mathrandom.Next(66,100);
Attributes.DEX=mathrandom.Next(66,100);
Attributes.SOR=mathrandom.Next(1,100);
    Attributes.BaseMaxStamina=Attributes.CurStamina=GetBaseMaxStamina();
       Attributes.BaseMaxFocus=Attributes.CurFocus=GetBaseMaxFocus();
                    Attributes.BaseAspd=GetBaseAspd();
ValidateAttributesSet(1);
}
protected override void Awake(){
                   base.Awake();
canFly=true;
}
protected override void Attack(AI enemy){
if(nextAttackTimer>0)return;
                   base.Attack(enemy);
if(deadStance!=-1||hitStance!=-1)return;if(attackStance==-1){attackStance=mathrandom.Next(0,2);curAnimTime=0;}
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