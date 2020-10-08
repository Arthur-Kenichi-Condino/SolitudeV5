using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActorManagementMentana;
public class BAYERI:_3DSprite{
public override void InitAttributes(bool random=true){
if(LOG&&LOG_LEVEL<=1)Debug.Log(GetType()+":init attributes");
Attributes.FOR=mathrandom.Next(52,73);
Attributes.VIT=mathrandom.Next(52,73);
    Attributes.MaxStamina=Attributes.Stamina=GetMaxStamina();
Attributes.INT=mathrandom.Next(88,100);
    Attributes.MaxFocus=Attributes.Focus=GetMaxFocus();
Attributes.AGI=mathrandom.Next(49,73);
Attributes.DEX=mathrandom.Next(66,100);
    Attributes.BaseAspd=GetAspd();
}
protected override void Attack(AI enemy){
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