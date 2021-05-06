using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class _EVADE:Passive{[NonSerialized]readonly System.Random mathrandom=new System.Random();
public override bool DoSkill(AI actor,AI target){Result=-1;


        Debug.LogWarning("DoSkill");


if(target.IsAllyTo(actor)){
Result=1;
         return(true);
}


float evasionChance=1-Mathf.Clamp01(target.Attributes.Hit-actor.Attributes.Flee);if(evasionChance>mathrandom.NextDouble()){
    float dis=Vector3.Distance(actor.transform.position,target.transform.position)*(target==actor.Target?.5f:.25f);
        float reposAngle=((float)(mathrandom.NextDouble())*2f-1f)*15f;if(reposAngle<0){reposAngle-=5f;}else{reposAngle+=5f;}Vector3 forward=(actor.transform.position-target.transform.position).normalized;Vector3 repos=target.transform.position+((Quaternion.AngleAxis(reposAngle,Vector3.up)*forward)*(dis+actor.BodyRadius));
        actor.Teleport(Quaternion.LookRotation((target.transform.position-actor.transform.position).normalized,actor.transform.up),repos,false);
        Debug.LogWarning("evaded!");
Result=0;
         return(true);
}


         return base.DoSkill(actor,target);}
public int Result=-1;
}