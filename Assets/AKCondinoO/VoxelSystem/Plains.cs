using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels{public class Plains:Biome{
public override int IdxForRnd{get{return 1;}}
public override int IdxForHgt{get{return 5;}}//  Base Height Result Module
protected override void SetModules(){
                   base.SetModules();
#region 1
ModuleBase module1=new Const(5);
#endregion
#region 2
ModuleBase module2a=new RidgedMultifractal(frequency:Mathf.Pow(2,-8),lacunarity:2.0,octaves:6,seed:Random[IdxForRnd].Next(),quality:QualityMode.Low);
ModuleBase module2b=new Turbulence(input:module2a); 
((Turbulence)module2b).Seed=Random[IdxForRnd].Next();
((Turbulence)module2b).Frequency=Mathf.Pow(2,-2);
((Turbulence)module2b).Power=1;
ModuleBase module2c=new ScaleBias(scale:1.0,bias:30.0,input:module2b);  
#endregion
#region 3
ModuleBase module3a=new Billow(frequency:Mathf.Pow(2,-7)*1.6,lacunarity:2.0,persistence:0.5,octaves:8,seed:Random[IdxForRnd].Next(),quality:QualityMode.Low);
ModuleBase module3b=new Turbulence(input:module3a);
((Turbulence)module3b).Seed=Random[IdxForRnd].Next();
((Turbulence)module3b).Frequency=Mathf.Pow(2,-2);  
((Turbulence)module3b).Power=1.8;
ModuleBase module3c=new ScaleBias(scale:1.0,bias:31.0,input:module3b);
#endregion
#region 4
ModuleBase module4a=new Perlin(frequency:Mathf.Pow(2,-6),lacunarity:2.0,persistence:0.5,octaves:6,seed:Random[IdxForRnd].Next(),quality:QualityMode.Low);
ModuleBase module4b=new Select(inputA:module2c,inputB:module3c,controller:module4a);
((Select)module4b).SetBounds(min:-.2,max:.2);
((Select)module4b).FallOff=.25;
ModuleBase module4c=new Multiply(lhs:module4b,rhs:module1);
#endregion
MaterialSelectors[0]=(Select)module4b;
Modules.Add(module4c);
}
protected override MaterialId GetMaterial(Vector3 noiseInput,double density){
double min=MaterialSelectors[0].Minimum;
double max=MaterialSelectors[0].Maximum;
double fallOff=MaterialSelectors[0].FallOff*.5;
var selectValue=MaterialSelectors[0].Controller.GetValue(noiseInput.z,noiseInput.x,0);
if(selectValue<=min-fallOff||selectValue>=max+fallOff){
return MaterialIdsPicking[0].Item2;
}else{
return MaterialIdsPicking[0].Item1;
}
}
}}