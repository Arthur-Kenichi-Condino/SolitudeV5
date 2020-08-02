using UnityEngine;
using static AKCondinoO.Voxels.TerrainChunk;
namespace AKCondinoO.Voxels{public class Biome{
    public virtual void v(Vector3 noiseInput,ref Voxel v){
if(noiseInput.y<1){v=Voxel.Bedrock;return;}

if(noiseInput.x>=10.5&&noiseInput.z>=10.5){v=new Voxel(51,Vector3.zero,MaterialId.Rock);return;}

if(noiseInput.y<128){if(noiseInput.x==1)v=new Voxel(51,Vector3.zero,MaterialId.Rock);else v=new Voxel(51,Vector3.zero,MaterialId.Dirt);return;}

v=Voxel.Air;}




    //public virtual void SetV(Vector3 noiseInput,double[]noiseCache1,int noiseCache1Index,ref Voxel v){
    //    if(noiseInput.y<1){
    //        v=Voxel.Bedrock;
    //return;
    //    }



    //    if(noiseInput.x>=.5&&noiseInput.z>=.5){v=Voxel.Bedrock;}



    //}
}}