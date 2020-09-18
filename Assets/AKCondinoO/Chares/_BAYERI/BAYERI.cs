using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BAYERI:_3DSprite{
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