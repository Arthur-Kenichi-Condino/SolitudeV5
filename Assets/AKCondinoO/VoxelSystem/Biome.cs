using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.TerrainChunk;
namespace AKCondinoO.Voxels{public class Biome{
    public virtual void SetV(Vector3 noiseInput,double[]noiseCache1,int noiseCache1Index,ref Voxel v){
        if(noiseInput.y<1){
            v=Voxel.Bedrock;
    return;
        }
    }
}}