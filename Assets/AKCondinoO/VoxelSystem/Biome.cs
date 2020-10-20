using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.TerrainChunk;
namespace AKCondinoO.Voxels{public class Biome{
public bool LOG=false;public int LOG_LEVEL=1;
public int Seed{
get{return Seed_v;}
set{       Seed_v=value;
if(LOG&&LOG_LEVEL<=2)Debug.Log("value:"+value);
Random[0]=new System.Random(Seed_v);
Random[1]=new System.Random(Random[0].Next());
SetModules();
}
}int Seed_v;
public virtual int IdxForRnd{get{return 0;}}
public virtual int IdxForHgt{get{return 4;}}//  Base Height Result Module
public ModuleBase _0{get{return Modules[0];}}
public ModuleBase _1{get{return Modules[1];}}
public ModuleBase _Neg1{get{return Modules[2];}}
public ModuleBase _Half{get{return Modules[3];}}
public ModuleBase _128{get{return Modules[4];}}
public readonly List<ModuleBase>Modules=new List<ModuleBase>();
protected virtual void SetModules(){
Modules.Add(new Const( 0));
Modules.Add(new Const( 1));
Modules.Add(new Const(-1));
Modules.Add(new Const(.5));
Modules.Add(new Const(128));
if(LOG&&LOG_LEVEL<=2)Debug.Log("SetModules() at "+GetType()+" resulted in Count:"+Modules.Count);
}
protected(MaterialId,MaterialId)[]MaterialIdsPicking=new(MaterialId,MaterialId)[1]{
(MaterialId.Rock,MaterialId.Dirt),
};
protected Select[]MaterialSelectors=new Select[1];




#region value
public virtual void v(Vector3 noiseInput,ref Voxel v,ref double[]noiseCache1,int noiseCache1Index){if(noiseCache1==null)noiseCache1=new double[Chunk.FlattenOffset];
if(noiseInput.y<1){v=Voxel.Bedrock;return;}
noiseInput+=_deround;
double noiseValue1=noiseCache1[noiseCache1Index]!=0?noiseCache1[noiseCache1Index]:(noiseCache1[noiseCache1Index]=Modules[IdxForHgt].GetValue(noiseInput.z,noiseInput.x,0));
if(noiseInput.y<=noiseValue1){double d;v=new Voxel(d=AddSmoothDensity(50,20,noiseInput,noiseValue1),Vector3.zero,GetMaterial(noiseInput,d));return;}


//if(noiseInput.x>=10.5&&noiseInput.z>=10.5){v=new Voxel(51,Vector3.zero,MaterialId.Rock);return;}

//if(noiseInput.y<128){if(noiseInput.z==Chunk.Depth-2)v=new Voxel(51,Vector3.zero,MaterialId.Rock);else v=new Voxel(51,Vector3.zero,MaterialId.Dirt);return;}


v=Voxel.Air;}
protected Vector3 _deround{get;}=new Vector3(.5f,.5f,.5f);
public readonly System.Random[]Random=new System.Random[2];
#endregion
protected virtual double AddSmoothDensity(double bottomSharpValue,double smoothingHeightDelta,Vector3 noiseInput,double noiseValue1){
double dValue=bottomSharpValue;
double smoothingValue=Math.Abs(1.0-((noiseInput.y-(noiseValue1-smoothingHeightDelta))/smoothingHeightDelta));
       dValue*=smoothingValue;
    if(dValue<1)
       dValue=1;
    if(dValue>100)
       dValue=100;
return dValue;}
protected virtual MaterialId GetMaterial(Vector3 noiseInput,double density){
if(-density>=IsoLevel){return MaterialId.Air;}
return MaterialIdsPicking[0].Item1;}



    //public virtual void SetV(Vector3 noiseInput,double[]noiseCache1,int noiseCache1Index,ref Voxel v){
    //    if(noiseInput.y<1){
    //        v=Voxel.Bedrock;
    //return;
    //    }



    //    if(noiseInput.x>=.5&&noiseInput.z>=.5){v=Voxel.Bedrock;}



    //}
}}