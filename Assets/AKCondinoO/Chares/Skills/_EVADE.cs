using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class _EVADE:Passive{
public override bool DoSkill(AI actor,AI target){


        Debug.LogWarning("DoSkill");
        //Vector3.Distance();
        //actor.Teleport();


         return base.DoSkill(actor,target);}
}