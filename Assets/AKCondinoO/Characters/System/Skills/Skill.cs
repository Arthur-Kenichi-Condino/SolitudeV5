using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Skill{
public virtual bool DoSkill(AI actor,AI target){
return(false);}
public static Skill GetBest(AI actor,When moment){Skill result=null;
switch(moment){
case(When.onWillTakeDamage):{
    

    Debug.LogWarning("When.onWillTakeDamage");
    if(actor.onWillTakeDamageSkills.Length>0){
    if(actor.onWillTakeDamageSkills.Contains(SkillIds._EVADE)){


        //actor.Skills[SkillIds._EVADE].DoSkill();
actor.Skills.TryGetValue(SkillIds._EVADE,out result);


    }
    }


break;}
}
return(result);}
public enum When{
onWillTakeDamage,
onTookDamage,
}
}
public class Passive:Skill{
}
public class Active:Skill{
}
public enum SkillIds:int{
_EVADE=0,
}