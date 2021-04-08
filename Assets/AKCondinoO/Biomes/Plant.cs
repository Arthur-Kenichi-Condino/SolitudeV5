using System;
using System.Collections;
using System.Collections.Generic;
//using TreeEditor;
using UnityEngine;
namespace AKCondinoO.Species.Plants{
public class Plant:SimObject{
[NonSerialized]public static readonly Common common=new Common();
[NonSerialized]protected Tree tree;
//[NonSerialized]protected TreeData data;
public bool Poisonous=true;
protected override void Awake(){
                   base.Awake();
if(common.Random[0]==null){
common.Seed=0;
}


            Debug.LogWarning("collider:"+collider+";rigidbody:"+rigidbody+";common.Seed:"+common.Seed);


tree=GetComponent<Tree>();//data=tree.data as TreeData;


//data.root.seed=common.Random[0].Next(0,1000000);
//data.UpdateMesh(tree.transform.worldToLocalMatrix,out _);


}
[NonSerialized]bool firstLoop=true;protected override void Update(){
                                                      base.Update();


            //Debug.LogWarning("collisions.Count:"+collisions.Count+";IsGrounded:"+IsGrounded);


}
}
public class Common{
public int Seed{
get{return Seed_v;}
set{       Seed_v=value;
Random[0]=new System.Random(Seed_v);
}
}int Seed_v;
public readonly System.Random[]Random=new System.Random[1];
}
}