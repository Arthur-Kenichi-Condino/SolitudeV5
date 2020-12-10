using System;
using System.Collections;
using System.Collections.Generic;
//using TreeEditor;
using UnityEngine;
namespace AKCondinoO.Species.Plants{
public class Plant:MonoBehaviour{
[NonSerialized]public static readonly Common common=new Common();
[NonSerialized]protected Tree tree;
//[NonSerialized]protected TreeData data;
protected virtual void Awake(){
if(common.Random[0]==null){
common.Seed=0;
}


            Debug.LogWarning("common.Seed:"+common.Seed);


tree=GetComponent<Tree>();//data=tree.data as TreeData;


//data.root.seed=common.Random[0].Next(0,1000000);
//data.UpdateMesh(tree.transform.worldToLocalMatrix,out _);


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