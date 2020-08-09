using paulbourke.MarchingCubes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
namespace AKCondinoO.Voxels{public class TerrainChunk:Chunk{
[NonSerialized]Task task;[NonSerialized]readonly AutoResetEvent foregroundDataSet=new AutoResetEvent(false);[NonSerialized]public readonly ManualResetEvent backgroundDataSet=new ManualResetEvent(true);
protected override void OnEnable(){
    if(mesh==null){
        mesh=new Mesh(){bounds=new Bounds(Vector3.zero,new Vector3(Width,Height,Depth))};gameObject.GetComponent<MeshFilter>().sharedMesh=mesh;renderer=gameObject.GetComponent<MeshRenderer>();collider=gameObject.GetComponent<MeshCollider>();collider.sharedMesh=mesh;
    }
Stop=false;task=Task.Factory.StartNew(BG,new object[]{LOG,LOG_LEVEL,TempVer=new NativeList<Vertex>(Allocator.Persistent),TempTriangles=new NativeList<ushort>(Allocator.Persistent),new System.Random(),ChunkManager.saveSubfolder,},TaskCreationOptions.LongRunning);
}
bool Stop{
    get{bool tmp;lock(Stop_Syn){tmp=Stop_v;      }return tmp;}
    set{         lock(Stop_Syn){    Stop_v=value;}if(value){foregroundDataSet.Set();}}
}[NonSerialized]readonly object Stop_Syn=new object();[NonSerialized]bool Stop_v;
protected override void OnDisable(){
Stop=true;try{task.Wait();}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
}
[NonSerialized]bool hasBuildData;
protected override void Update(){
    if(baking){
        if(bakingHandle.IsCompleted){
bakingHandle.Complete();baking=false;
if(LOG&&LOG_LEVEL<=2)Debug.Log("mesh baked");
    collider.sharedMesh=mesh;
        }else return;
    }
    if(backgroundDataSet.WaitOne(0)){
        if(hasBuildData){
            hasBuildData=false;
if(LOG&&LOG_LEVEL<=2)Debug.Log("did job now build");
            OnBuild();
        }
    if(baking)return;
if(DRAW_LEVEL<=-100)for(int i=0;i<TempVer.Length;i++){Debug.DrawRay(TempVer[i].pos,TempVer[i].normal,Color.green);}
        if(needsRebuild){
            needsRebuild=false;
            cCoord1=Coord;
            cnkRgn1=Rgn;
            backgroundDataSet.Reset();foregroundDataSet.Set();
        }
    }
}
[NonSerialized]NativeList<Vertex>TempVer;
[NonSerialized]NativeList<ushort>TempTriangles;
[NonSerialized]public new MeshRenderer renderer=null;[NonSerialized]Mesh mesh=null;[NonSerialized]public new MeshCollider collider=null;
void OnBuild(){
if(LOG&&LOG_LEVEL<=2)Debug.Log("TempVer.Length:"+TempVer.Length+";TempTriangles.Length:"+TempTriangles.Length);
    bool resize;var flags=MeshUpdateFlags.DontValidateIndices|MeshUpdateFlags.DontNotifyMeshUsers|MeshUpdateFlags.DontRecalculateBounds;
#region VertexBuffer
    if(resize=TempVer.Length>mesh.vertexCount)
        mesh.SetVertexBufferParams(TempVer.Length,layout);
    mesh.SetVertexBufferData(TempVer.AsArray(),0,0,TempVer.Length,0,flags);
#endregion 
#region IndexBuffer
    if(resize)
        mesh.SetIndexBufferParams(TempTriangles.Length,IndexFormat.UInt16);
    mesh.SetIndexBufferData(TempTriangles.AsArray(),0,0,TempTriangles.Length,flags);
#endregion 
#region SubMesh
        mesh.subMeshCount=1;
    mesh.SetSubMesh(0,new SubMeshDescriptor(0,TempTriangles.Length),flags);
#endregion 
baking=true;bakingHandle=new BakerJob(){meshId=mesh.GetInstanceID(),}.Schedule();
}
[NonSerialized]bool baking;[NonSerialized]JobHandle bakingHandle;struct BakerJob:IJob{public int meshId;public void Execute(){Physics.BakeMesh(meshId,false);}}
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct Vertex{
    public Vector3 pos;
    public Vector3 normal;
    public Color color;
    public Vector2 texCoord0;
    public Vector2 texCoord1;
       public Vertex(Vector3 p,Vector3 n,Vector2 uv0){
        pos=p;
        normal=n;
        color=new Color(1f,0f,0f,0f);
        texCoord0=uv0;
        texCoord1=new Vector2(-1f,-1f);
       }
}
[NonSerialized]static readonly VertexAttributeDescriptor[]layout=new[]{
    new VertexAttributeDescriptor(VertexAttribute.Position ,VertexAttributeFormat.Float32,3),
    new VertexAttributeDescriptor(VertexAttribute.Normal   ,VertexAttributeFormat.Float32,3),
    new VertexAttributeDescriptor(VertexAttribute.Color    ,VertexAttributeFormat.Float32,4),
    new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32,2),
    new VertexAttributeDescriptor(VertexAttribute.TexCoord1,VertexAttributeFormat.Float32,2),
};
[NonSerialized]static readonly object tasksBusyCount_Syn=new object();[NonSerialized]static int tasksBusyCount=0;[NonSerialized]static readonly AutoResetEvent queue=new AutoResetEvent(true);
[NonSerialized]public readonly object load_Syn=new object();
[NonSerialized]readonly Voxel[]voxels=new Voxel[VoxelsPerChunk];
[NonSerialized]Vector2Int cCoord1;
[NonSerialized]Vector2Int cnkRgn1;
void BG(object state){Thread.CurrentThread.IsBackground=false;Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;try{
    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL&&parameters[2]is NativeList<Vertex>TempVer&&parameters[3]is NativeList<ushort>TempTriangles&&parameters[4]is System.Random random&&parameters[5]is string[]saveSubfolder){
DataContractSerializer saveContract=new DataContractSerializer(typeof(Dictionary<Vector3Int,(double density,MaterialId material)>));
Voxel[]polygonCell=new Voxel[8];
        while(!Stop){foregroundDataSet.WaitOne();if(Stop)goto _Stop;
lock(tasksBusyCount_Syn){tasksBusyCount++;}queue.WaitOne(tasksBusyCount*5000);
if(LOG&&LOG_LEVEL<=2)Debug.Log("do job ["+cnkRgn1);var watch=System.Diagnostics.Stopwatch.StartNew();
Array.Clear(voxels,0,voxels.Length);TempVer.Clear();TempTriangles.Clear();var neighbors=new Dictionary<int,Voxel>[8];for(int i=0;i<8;i++){neighbors[i]=new Dictionary<int,Voxel>();}Dictionary<Vector3,List<Vector2>>UVsByVertex=new Dictionary<Vector3,List<Vector2>>();
            lock(load_Syn){
var fileName=string.Format(saveSubfolder[0],ChunkManager.GetIdx(cCoord1.x,cCoord1.y));


        //Debug.LogWarning(fileName);

        
if(File.Exists(fileName)){
int loadTries=30;bool loaded=false;while(!loaded){
FileStream file=null;
try{

        
using(file=new FileStream(fileName,FileMode.Open,FileAccess.Read,FileShare.Read)){
if(file.Length>0){
if(LOG&&LOG_LEVEL<=1)Debug.Log("file has data, loading it before building the mesh:fileName:"+fileName);
if(saveContract.ReadObject(file)is Dictionary<Vector3Int,(double density,MaterialId material)>fileData){

foreach(var voxelData in fileData){
voxels[GetIdx(voxelData.Key.x,voxelData.Key.y,voxelData.Key.z)]=new Voxel(voxelData.Value.density,Vector3.zero,voxelData.Value.material);
}

}}
}
loaded=true;
if(LOG&&LOG_LEVEL<=1)Debug.Log("successfully loaded edits from:"+fileName);
        //Debug.LogWarning(loadTries);


}catch(IOException e){Debug.LogWarning("file access failed:try load again after delay:fileName:"+fileName+"\n"+e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
}catch(Exception e1){Debug.LogError("unknown error:ignore the file that may be broken:fileName:"+fileName+"\n"+e1?.Message+"\n"+e1?.StackTrace+"\n"+e1?.Source);
break;
}finally{
dispose();
}
void dispose(){
try{
if(file!=null)
   file.Dispose();
}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
}
if(!loaded){if(--loadTries<=0||Main.Stop){if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("failed to load from: "+fileName);break;}else{
Thread.Yield();Thread.Sleep(random.Next(500,1001));
}}}
}

for(int x=-1;x<=1;x++){
for(int z=-1;z<=1;z++){
if(x==0&&z==0)continue;

Vector2Int nCoord1=cCoord1;nCoord1.x+=x;nCoord1.y+=z;int ngbIdx1=ChunkManager.GetIdx(nCoord1.x,nCoord1.y);int i1=index(nCoord1-cCoord1)-1;
fileName=string.Format(saveSubfolder[0],ngbIdx1);



if(File.Exists(fileName)){
int loadTries=30;bool loaded=false;while(!loaded){
FileStream file=null;
try{

        
using(file=new FileStream(fileName,FileMode.Open,FileAccess.Read,FileShare.Read)){
if(file.Length>0){
if(LOG&&LOG_LEVEL<=1)Debug.Log("file has data, loading it before building the mesh:fileName:"+fileName);
if(saveContract.ReadObject(file)is Dictionary<Vector3Int,(double density,MaterialId material)>fileData){
        
foreach(var voxelData in fileData){
neighbors[i1][GetIdx(voxelData.Key.x,voxelData.Key.y,voxelData.Key.z)]=new Voxel(voxelData.Value.density,Vector3.zero,voxelData.Value.material);
}

}}
}
loaded=true;
if(LOG&&LOG_LEVEL<=1)Debug.Log("successfully loaded edits from:"+fileName);


}catch(IOException e){Debug.LogWarning("file access failed:try load again after delay:fileName:"+fileName+"\n"+e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
}catch(Exception e1){Debug.LogError("unknown error:ignore the file that may be broken:fileName:"+fileName+"\n"+e1?.Message+"\n"+e1?.StackTrace+"\n"+e1?.Source);
break;
}finally{
dispose();
}
void dispose(){
try{
if(file!=null)
   file.Dispose();
}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
}
if(!loaded){if(--loadTries<=0||Main.Stop){if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("failed to load from: "+fileName);break;}else{
Thread.Yield();Thread.Sleep(random.Next(500,1001));
}}}
}

}}

            }
double[][]noiseCache1=new double[9][];
Voxel[][][]voxelsBuffer1=new Voxel[3][][]{new Voxel[1][]{new Voxel[4],},new Voxel[Depth][],new Voxel[FlattenOffset][],};for(int i=0;i<voxelsBuffer1[2].Length;++i){voxelsBuffer1[2][i]=new Voxel[4];if(i<voxelsBuffer1[1].Length){voxelsBuffer1[1][i]=new Voxel[4];}}Voxel[][]voxelsBuffer2=new Voxel[3][]{new Voxel[1],new Voxel[Depth],new Voxel[FlattenOffset],};
Vector3[][][]verticesBuffer=new Vector3[3][][]{new Vector3[1][]{new Vector3[4],},new Vector3[Depth][],new Vector3[FlattenOffset][],};for(int i=0;i<verticesBuffer[2].Length;++i){verticesBuffer[2][i]=new Vector3[4];if(i<verticesBuffer[1].Length){verticesBuffer[1][i]=new Vector3[4];}}
ushort vertexCount=0;
Vector2Int posOffset=Vector2Int.zero;
Vector2Int crdOffset=Vector2Int.zero;
Vector3Int vCoord1;
for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
Polygonise();
}}}
void Polygonise(){
int corner=0;Vector3Int vCoord2=vCoord1;                        if(vCoord1.z>0)polygonCell[corner]=voxelsBuffer1[0][0][0];else if(vCoord1.x>0)polygonCell[corner]=voxelsBuffer1[1][vCoord1.z][0];else if(vCoord1.y>0)polygonCell[corner]=voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][0];else SetVoxel();
corner++;vCoord2=vCoord1;vCoord2.x+=1;                          if(vCoord1.z>0)polygonCell[corner]=voxelsBuffer1[0][0][1];                                                                       else if(vCoord1.y>0)polygonCell[corner]=voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][1];else SetVoxel();
corner++;vCoord2=vCoord1;vCoord2.x+=1;vCoord2.y+=1;             if(vCoord1.z>0)polygonCell[corner]=voxelsBuffer1[0][0][2];                                                                                                                                                              else SetVoxel();
corner++;vCoord2=vCoord1;             vCoord2.y+=1;             if(vCoord1.z>0)polygonCell[corner]=voxelsBuffer1[0][0][3];else if(vCoord1.x>0)polygonCell[corner]=voxelsBuffer1[1][vCoord1.z][1];                                                                                       else SetVoxel();
corner++;vCoord2=vCoord1;                          vCoord2.z+=1;                                                               if(vCoord1.x>0)polygonCell[corner]=voxelsBuffer1[1][vCoord1.z][2];else if(vCoord1.y>0)polygonCell[corner]=voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][2];else SetVoxel();
corner++;vCoord2=vCoord1;vCoord2.x+=1;             vCoord2.z+=1;                                                                                                                                      if(vCoord1.y>0)polygonCell[corner]=voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][3];else SetVoxel();
corner++;vCoord2=vCoord1;vCoord2.x+=1;vCoord2.y+=1;vCoord2.z+=1;                                                                                                                                                                                                                             SetVoxel();
corner++;vCoord2=vCoord1;             vCoord2.y+=1;vCoord2.z+=1;                                                               if(vCoord1.x>0)polygonCell[corner]=voxelsBuffer1[1][vCoord1.z][3];                                                                                       else SetVoxel();
voxelsBuffer1[0][0][0]=polygonCell[4];
voxelsBuffer1[0][0][1]=polygonCell[5];
voxelsBuffer1[0][0][2]=polygonCell[6];
voxelsBuffer1[0][0][3]=polygonCell[7];
voxelsBuffer1[1][vCoord1.z][0]=polygonCell[1];
voxelsBuffer1[1][vCoord1.z][1]=polygonCell[2];
voxelsBuffer1[1][vCoord1.z][2]=polygonCell[5];
voxelsBuffer1[1][vCoord1.z][3]=polygonCell[6];
voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][0]=polygonCell[3];
voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][1]=polygonCell[2];
voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][2]=polygonCell[7];
voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][3]=polygonCell[6];
    void SetVoxel(){
        if(vCoord2.y<0){
            polygonCell[corner]=Voxel.Bedrock;
        }else if(vCoord2.y>=Height){
            polygonCell[corner]=Voxel.Air;
        }else{
Vector2Int cnkRgn2=cnkRgn1;Vector2Int cCoord2=cCoord1;
if(vCoord2.x<0||vCoord2.x>=Width||
   vCoord2.z<0||vCoord2.z>=Depth){ 
ValidateCoord(ref cnkRgn2,ref vCoord2);cCoord2=ChunkManager.RgnToCoord(cnkRgn2);
}var vxlIdx2=GetIdx(vCoord2.x,vCoord2.y,vCoord2.z);
            int i2=index(cCoord2-cCoord1);if(i2==0&&voxels[vxlIdx2].IsCreated)
            polygonCell[corner]=voxels[vxlIdx2];
            else if(i2>0&&neighbors[i2-1].ContainsKey(vxlIdx2))
            polygonCell[corner]=neighbors[i2-1][vxlIdx2];
            else{
Vector3 noiseInput=vCoord2;noiseInput.x+=cnkRgn2.x;
                           noiseInput.z+=cnkRgn2.y;
ChunkManager.biome.v(noiseInput,ref polygonCell[corner],ref noiseCache1[i2],vCoord2.z+vCoord2.x*Width);
            }
if(polygonCell[corner].Normal==Vector3.zero){
int tmpIdx=0;Voxel[]tmp=new Voxel[6];Vector3Int vCoord3=vCoord2;vCoord3.x++;TmpVoxel();
tmpIdx++;vCoord3=vCoord2;                                       vCoord3.x--;if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&voxelsBuffer2[1][vCoord2.z].IsCreated)tmp[tmpIdx]=voxelsBuffer2[1][vCoord2.z];else TmpVoxel();
tmpIdx++;vCoord3=vCoord2;                                       vCoord3.y++;TmpVoxel();
tmpIdx++;vCoord3=vCoord2;                                       vCoord3.y--;if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&voxelsBuffer2[2][vCoord2.z+vCoord2.x*Width].IsCreated)tmp[tmpIdx]=voxelsBuffer2[2][vCoord2.z+vCoord2.x*Width];else TmpVoxel();
tmpIdx++;vCoord3=vCoord2;                                       vCoord3.z++;TmpVoxel();
tmpIdx++;vCoord3=vCoord2;                                       vCoord3.z--;if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&voxelsBuffer2[0][0].IsCreated)tmp[tmpIdx]=voxelsBuffer2[0][0];else TmpVoxel();
    void TmpVoxel(){
        if(vCoord3.y<0){
            tmp[tmpIdx]=Voxel.Bedrock;
        }else if(vCoord3.y>=Height){
            tmp[tmpIdx]=Voxel.Air;
        }else{
Vector2Int cnkRgn3=cnkRgn2;Vector2Int cCoord3=cCoord2;
if(vCoord3.x<0||vCoord3.x>=Width||
   vCoord3.z<0||vCoord3.z>=Depth){ 
ValidateCoord(ref cnkRgn3,ref vCoord3);cCoord3=ChunkManager.RgnToCoord(cnkRgn3);
}var vxlIdx3=GetIdx(vCoord3.x,vCoord3.y,vCoord3.z);
            int i3=index(cCoord3-cCoord1);if(i3==0&&voxels[vxlIdx3].IsCreated)
            tmp[tmpIdx]=voxels[vxlIdx3];
            else if(i3>0&&neighbors[i3-1].ContainsKey(vxlIdx3))
            tmp[tmpIdx]=neighbors[i3-1][vxlIdx3];
            else{
Vector3 noiseInput=vCoord3;noiseInput.x+=cnkRgn3.x;
                           noiseInput.z+=cnkRgn3.y;
ChunkManager.biome.v(noiseInput,ref tmp[tmpIdx],ref noiseCache1[i3],vCoord3.z+vCoord3.x*Width);
            if(i3==0)
            voxels[vxlIdx3]=tmp[tmpIdx];
            else if(i3>0)
            neighbors[i3-1][vxlIdx3]=tmp[tmpIdx];
            }
        }
    }
polygonCell[corner].Normal.x=(float)(tmp[0].Density-tmp[1].Density);
polygonCell[corner].Normal.y=(float)(tmp[2].Density-tmp[3].Density);
polygonCell[corner].Normal.z=(float)(tmp[4].Density-tmp[5].Density);
if(polygonCell[corner].Normal!=Vector3.zero){
    polygonCell[corner].Normal.Normalize();
}
            if(i2==0)
            voxels[vxlIdx2]=polygonCell[corner];
            else if(i2>0)
            neighbors[i2-1][vxlIdx2]=polygonCell[corner];
}
voxelsBuffer2[0][0]=polygonCell[corner];
voxelsBuffer2[1][vCoord2.z]=polygonCell[corner];
voxelsBuffer2[2][vCoord2.z+vCoord2.x*Width]=polygonCell[corner];
        }
    }
#region MarchingCubes
int edgeIndex;
/*
    Determine the index into the edge table which
    tells us which vertices are inside of the surface
*/
                                    edgeIndex =  0;
if(-polygonCell[0].Density<IsoLevel)edgeIndex|=  1;
if(-polygonCell[1].Density<IsoLevel)edgeIndex|=  2;
if(-polygonCell[2].Density<IsoLevel)edgeIndex|=  4;
if(-polygonCell[3].Density<IsoLevel)edgeIndex|=  8;
if(-polygonCell[4].Density<IsoLevel)edgeIndex|= 16;
if(-polygonCell[5].Density<IsoLevel)edgeIndex|= 32;
if(-polygonCell[6].Density<IsoLevel)edgeIndex|= 64;
if(-polygonCell[7].Density<IsoLevel)edgeIndex|=128;
if(Tables.EdgeTable[edgeIndex]==0){/*  Cube is entirely in/out of the surface  */
    return;
}
MaterialId[]materials=new MaterialId[12];
   Vector3[] vertices=new Vector3[12];
   Vector3[]  normals=new Vector3[12];
//  Use buffered data if available
vertices[ 0]=(vCoord1.z>0?verticesBuffer[0][0][0]:(vCoord1.y>0?verticesBuffer[2][vCoord1.z+vCoord1.x*Width][0]:Vector3.zero));
vertices[ 1]=(vCoord1.z>0?verticesBuffer[0][0][1]:Vector3.zero);
vertices[ 2]=(vCoord1.z>0?verticesBuffer[0][0][2]:Vector3.zero);
vertices[ 3]=(vCoord1.z>0?verticesBuffer[0][0][3]:(vCoord1.x>0?verticesBuffer[1][vCoord1.z][0]:Vector3.zero));
vertices[ 4]=(vCoord1.y>0?verticesBuffer[2][vCoord1.z+vCoord1.x*Width][1]:Vector3.zero);
vertices[ 7]=(vCoord1.x>0?verticesBuffer[1][vCoord1.z][1]:Vector3.zero);
vertices[ 8]=(vCoord1.x>0?verticesBuffer[1][vCoord1.z][2]:(vCoord1.y>0?verticesBuffer[2][vCoord1.z+vCoord1.x*Width][3]:Vector3.zero));
vertices[ 9]=(vCoord1.y>0?verticesBuffer[2][vCoord1.z+vCoord1.x*Width][2]:Vector3.zero);
vertices[11]=(vCoord1.x>0?verticesBuffer[1][vCoord1.z][3]:Vector3.zero);
if(0!=(Tables.EdgeTable[edgeIndex]&   1)){vertexInterp(0,1,ref vertices[ 0],ref normals[ 0],ref materials[ 0]);}
if(0!=(Tables.EdgeTable[edgeIndex]&   2)){vertexInterp(1,2,ref vertices[ 1],ref normals[ 1],ref materials[ 1]);}
if(0!=(Tables.EdgeTable[edgeIndex]&   4)){vertexInterp(2,3,ref vertices[ 2],ref normals[ 2],ref materials[ 2]);}
if(0!=(Tables.EdgeTable[edgeIndex]&   8)){vertexInterp(3,0,ref vertices[ 3],ref normals[ 3],ref materials[ 3]);}
if(0!=(Tables.EdgeTable[edgeIndex]&  16)){vertexInterp(4,5,ref vertices[ 4],ref normals[ 4],ref materials[ 4]);}
if(0!=(Tables.EdgeTable[edgeIndex]&  32)){vertexInterp(5,6,ref vertices[ 5],ref normals[ 5],ref materials[ 5]);}
if(0!=(Tables.EdgeTable[edgeIndex]&  64)){vertexInterp(6,7,ref vertices[ 6],ref normals[ 6],ref materials[ 6]);}
if(0!=(Tables.EdgeTable[edgeIndex]& 128)){vertexInterp(7,4,ref vertices[ 7],ref normals[ 7],ref materials[ 7]);}
if(0!=(Tables.EdgeTable[edgeIndex]& 256)){vertexInterp(0,4,ref vertices[ 8],ref normals[ 8],ref materials[ 8]);}
if(0!=(Tables.EdgeTable[edgeIndex]& 512)){vertexInterp(1,5,ref vertices[ 9],ref normals[ 9],ref materials[ 9]);}
if(0!=(Tables.EdgeTable[edgeIndex]&1024)){vertexInterp(2,6,ref vertices[10],ref normals[10],ref materials[10]);}
if(0!=(Tables.EdgeTable[edgeIndex]&2048)){vertexInterp(3,7,ref vertices[11],ref normals[11],ref materials[11]);}
    void vertexInterp(int c0,int c1,ref Vector3 p,ref Vector3 n,ref MaterialId m){
double[]density=new double[2]{-polygonCell[c0].Density,-polygonCell[c1].Density};Vector3[]vertex=new Vector3[2]{Corners[c0],Corners[c1]};
if(p!=Vector3.zero){goto _Normal;}
if(Math.Abs(IsoLevel-density[0])<double.Epsilon){p=vertex[0];goto _Normal;}
if(Math.Abs(IsoLevel-density[1])<double.Epsilon){p=vertex[1];goto _Normal;}
if(Math.Abs(density[0]-density[1])<double.Epsilon){p=vertex[0];goto _Normal;}
        double marchingUnit=(IsoLevel-density[0])/(density[1]-density[0]);
        p.x=(float)(vertex[0].x+marchingUnit*(vertex[1].x-vertex[0].x));
        p.y=(float)(vertex[0].y+marchingUnit*(vertex[1].y-vertex[0].y));
        p.z=(float)(vertex[0].z+marchingUnit*(vertex[1].z-vertex[0].z));
_Normal:{
float[]distance=new float[2];Vector3[]normal=new Vector3[2];
        distance[0]=Vector3.Distance(vertex[0],vertex[1]);
        distance[1]=Vector3.Distance(vertex[1],p);
        n=Vector3.Lerp(
            normal[1]=polygonCell[c1].Normal,
            normal[0]=polygonCell[c0].Normal,distance[1]/distance[0]);
        n=n!=Vector3.zero?n.normalized:Vector3.down;
}
MaterialId[]material=new MaterialId[2]{polygonCell[c0].Material,polygonCell[c1].Material};
        m=material[0];if(density[1]<density[0]){m=material[1];}else if(density[1]==density[0]&&(int)material[1]>(int)material[0]){m=material[1];}
    }
//  Buffer the data
verticesBuffer[0][0][0]=vertices[ 4]+Vector3.back;//  Adiciona um valor negativo porque o voxelCoord próximo vai usar esse valor mais o de sua posição (próprio voxelCoord novo)
verticesBuffer[0][0][1]=vertices[ 5]+Vector3.back;
verticesBuffer[0][0][2]=vertices[ 6]+Vector3.back;
verticesBuffer[0][0][3]=vertices[ 7]+Vector3.back;
verticesBuffer[1][vCoord1.z][0]=vertices[ 1]+Vector3.left;
verticesBuffer[1][vCoord1.z][1]=vertices[ 5]+Vector3.left;
verticesBuffer[1][vCoord1.z][2]=vertices[ 9]+Vector3.left;
verticesBuffer[1][vCoord1.z][3]=vertices[10]+Vector3.left;
verticesBuffer[2][vCoord1.z+vCoord1.x*Width][0]=vertices[ 2]+Vector3.down;
verticesBuffer[2][vCoord1.z+vCoord1.x*Width][1]=vertices[ 6]+Vector3.down;
verticesBuffer[2][vCoord1.z+vCoord1.x*Width][2]=vertices[10]+Vector3.down;
verticesBuffer[2][vCoord1.z+vCoord1.x*Width][3]=vertices[11]+Vector3.down;
/*  Create the triangle  */
for(int i=0;Tables.TriangleTable[edgeIndex][i]!=-1;i+=3){
TempTriangles.Add((ushort)(vertexCount+2));
TempTriangles.Add((ushort)(vertexCount+1));
TempTriangles.Add(         vertexCount   );
int[]idx=new int[3];
Vector3 pos=vCoord1-TrianglePosAdj;pos.x+=posOffset.x;
                                   pos.z+=posOffset.y;
Vector3[]verPos=new Vector3[3]{pos+vertices[idx[0]=Tables.TriangleTable[edgeIndex][i  ]],
                               pos+vertices[idx[1]=Tables.TriangleTable[edgeIndex][i+1]],
                               pos+vertices[idx[2]=Tables.TriangleTable[edgeIndex][i+2]]};
MaterialId material=                                         materials[idx[0]];
           material=(MaterialId)Mathf.Max((int)material,(int)materials[idx[1]]);
           material=(MaterialId)Mathf.Max((int)material,(int)materials[idx[2]]);
   Vector2 materialUV=AtlasHelper.GetUV(material);
TempVer.Add(new Vertex(verPos[0],-normals[idx[0]],materialUV));if(!UVsByVertex.ContainsKey(verPos[0])){UVsByVertex.Add(verPos[0],new List<Vector2>());}UVsByVertex[verPos[0]].Add(materialUV);
TempVer.Add(new Vertex(verPos[1],-normals[idx[1]],materialUV));if(!UVsByVertex.ContainsKey(verPos[1])){UVsByVertex.Add(verPos[1],new List<Vector2>());}UVsByVertex[verPos[1]].Add(materialUV);
TempVer.Add(new Vertex(verPos[2],-normals[idx[2]],materialUV));if(!UVsByVertex.ContainsKey(verPos[2])){UVsByVertex.Add(verPos[2],new List<Vector2>());}UVsByVertex[verPos[2]].Add(materialUV);
vertexCount+=3;
}
#endregion
}
for(crdOffset.y=0,
    posOffset.y=0,
    vCoord1.y=0;vCoord1.y<Height;vCoord1.y++){
for(vCoord1.z=0;vCoord1.z<Depth ;vCoord1.z++){
    vCoord1.x=0;
crdOffset.x=1;
posOffset.x=Width;
PseudoPolygonise();
    vCoord1.x=Width-1;
crdOffset.x=-1;
posOffset.x=-Width;
PseudoPolygonise();
}}
for(crdOffset.x=0,
    posOffset.x=0,
    vCoord1.y=0;vCoord1.y<Height;vCoord1.y++){
for(vCoord1.x=0;vCoord1.x<Width ;vCoord1.x++){
    vCoord1.z=0;
crdOffset.y=1;
posOffset.y=Depth;
PseudoPolygonise();
    vCoord1.z=Depth-1;
crdOffset.y=-1;
posOffset.y=-Depth;
PseudoPolygonise();
}}
void PseudoPolygonise(){
int corner=0;Vector3Int vCoord2=vCoord1;                        SetVoxel();
corner++;vCoord2=vCoord1;vCoord2.x+=1;                          SetVoxel();
corner++;vCoord2=vCoord1;vCoord2.x+=1;vCoord2.y+=1;             SetVoxel();
corner++;vCoord2=vCoord1;             vCoord2.y+=1;             SetVoxel();
corner++;vCoord2=vCoord1;                          vCoord2.z+=1;SetVoxel();
corner++;vCoord2=vCoord1;vCoord2.x+=1;             vCoord2.z+=1;SetVoxel();
corner++;vCoord2=vCoord1;vCoord2.x+=1;vCoord2.y+=1;vCoord2.z+=1;SetVoxel();
corner++;vCoord2=vCoord1;             vCoord2.y+=1;vCoord2.z+=1;SetVoxel();
    void SetVoxel(){
        if(vCoord2.y<0){
            polygonCell[corner]=Voxel.Bedrock;
        }else if(vCoord2.y>=Height){
            polygonCell[corner]=Voxel.Air;
        }else{
Vector2Int cnkRgn2=cnkRgn1+posOffset;Vector2Int cCoord2=cCoord1+crdOffset;
if(vCoord2.x<0||vCoord2.x>=Width||
   vCoord2.z<0||vCoord2.z>=Depth){ 
ValidateCoord(ref cnkRgn2,ref vCoord2);cCoord2=ChunkManager.RgnToCoord(cnkRgn2);
}var vxlIdx2=GetIdx(vCoord2.x,vCoord2.y,vCoord2.z);
            int i2=index(cCoord2-cCoord1);if(i2==0&&voxels[vxlIdx2].IsCreated)
            polygonCell[corner]=voxels[vxlIdx2];
            else if(i2>0&&neighbors[i2-1].ContainsKey(vxlIdx2))
            polygonCell[corner]=neighbors[i2-1][vxlIdx2];
            else{
Vector3 noiseInput=vCoord2;noiseInput.x+=cnkRgn2.x;
                           noiseInput.z+=cnkRgn2.y;
ChunkManager.biome.v(noiseInput,ref polygonCell[corner],ref noiseCache1[i2],vCoord2.z+vCoord2.x*Width);
            }
        }
    }
int edgeIndex;
/*
    Determine the index into the edge table which
    tells us which vertices are inside of the surface
*/
                                    edgeIndex =  0;
if(-polygonCell[0].Density<IsoLevel)edgeIndex|=  1;
if(-polygonCell[1].Density<IsoLevel)edgeIndex|=  2;
if(-polygonCell[2].Density<IsoLevel)edgeIndex|=  4;
if(-polygonCell[3].Density<IsoLevel)edgeIndex|=  8;
if(-polygonCell[4].Density<IsoLevel)edgeIndex|= 16;
if(-polygonCell[5].Density<IsoLevel)edgeIndex|= 32;
if(-polygonCell[6].Density<IsoLevel)edgeIndex|= 64;
if(-polygonCell[7].Density<IsoLevel)edgeIndex|=128;
if(Tables.EdgeTable[edgeIndex]==0){/*  Cube is entirely in/out of the surface  */
    return;
}
MaterialId[]materials=new MaterialId[12];
   Vector3[] vertices=new Vector3[12];
if(0!=(Tables.EdgeTable[edgeIndex]&   1)){vertexInterp(0,1,ref vertices[ 0],ref materials[ 0]);}
if(0!=(Tables.EdgeTable[edgeIndex]&   2)){vertexInterp(1,2,ref vertices[ 1],ref materials[ 1]);}
if(0!=(Tables.EdgeTable[edgeIndex]&   4)){vertexInterp(2,3,ref vertices[ 2],ref materials[ 2]);}
if(0!=(Tables.EdgeTable[edgeIndex]&   8)){vertexInterp(3,0,ref vertices[ 3],ref materials[ 3]);}
if(0!=(Tables.EdgeTable[edgeIndex]&  16)){vertexInterp(4,5,ref vertices[ 4],ref materials[ 4]);}
if(0!=(Tables.EdgeTable[edgeIndex]&  32)){vertexInterp(5,6,ref vertices[ 5],ref materials[ 5]);}
if(0!=(Tables.EdgeTable[edgeIndex]&  64)){vertexInterp(6,7,ref vertices[ 6],ref materials[ 6]);}
if(0!=(Tables.EdgeTable[edgeIndex]& 128)){vertexInterp(7,4,ref vertices[ 7],ref materials[ 7]);}
if(0!=(Tables.EdgeTable[edgeIndex]& 256)){vertexInterp(0,4,ref vertices[ 8],ref materials[ 8]);}
if(0!=(Tables.EdgeTable[edgeIndex]& 512)){vertexInterp(1,5,ref vertices[ 9],ref materials[ 9]);}
if(0!=(Tables.EdgeTable[edgeIndex]&1024)){vertexInterp(2,6,ref vertices[10],ref materials[10]);}
if(0!=(Tables.EdgeTable[edgeIndex]&2048)){vertexInterp(3,7,ref vertices[11],ref materials[11]);}
    void vertexInterp(int c0,int c1,ref Vector3 p,ref MaterialId m){
double[]density=new double[2]{-polygonCell[c0].Density,-polygonCell[c1].Density};Vector3[]vertex=new Vector3[2]{Corners[c0],Corners[c1]};
if(Math.Abs(IsoLevel-density[0])<double.Epsilon){p=vertex[0];goto _Material;}
if(Math.Abs(IsoLevel-density[1])<double.Epsilon){p=vertex[1];goto _Material;}
if(Math.Abs(density[0]-density[1])<double.Epsilon){p=vertex[0];goto _Material;}
        double marchingUnit=(IsoLevel-density[0])/(density[1]-density[0]);
        p.x=(float)(vertex[0].x+marchingUnit*(vertex[1].x-vertex[0].x));
        p.y=(float)(vertex[0].y+marchingUnit*(vertex[1].y-vertex[0].y));
        p.z=(float)(vertex[0].z+marchingUnit*(vertex[1].z-vertex[0].z));
_Material:{
MaterialId[]material=new MaterialId[2]{polygonCell[c0].Material,polygonCell[c1].Material};
        m=material[0];if(density[1]<density[0]){m=material[1];}else if(density[1]==density[0]&&(int)material[1]>(int)material[0]){m=material[1];}
}
    }
/*  Create the triangle  */
for(int i=0;Tables.TriangleTable[edgeIndex][i]!=-1;i+=3){
int[]idx=new int[3];
Vector3 pos=vCoord1-TrianglePosAdj;pos.x+=posOffset.x;
                                   pos.z+=posOffset.y;
Vector3[]verPos=new Vector3[3]{pos+vertices[idx[0]=Tables.TriangleTable[edgeIndex][i  ]],
                               pos+vertices[idx[1]=Tables.TriangleTable[edgeIndex][i+1]],
                               pos+vertices[idx[2]=Tables.TriangleTable[edgeIndex][i+2]]};
MaterialId material=                                         materials[idx[0]];
           material=(MaterialId)Mathf.Max((int)material,(int)materials[idx[1]]);
           material=(MaterialId)Mathf.Max((int)material,(int)materials[idx[2]]);
   Vector2 materialUV=AtlasHelper.GetUV(material);
if(!UVsByVertex.ContainsKey(verPos[0])){UVsByVertex.Add(verPos[0],new List<Vector2>());}UVsByVertex[verPos[0]].Add(materialUV);
if(!UVsByVertex.ContainsKey(verPos[1])){UVsByVertex.Add(verPos[1],new List<Vector2>());}UVsByVertex[verPos[1]].Add(materialUV);
if(!UVsByVertex.ContainsKey(verPos[2])){UVsByVertex.Add(verPos[2],new List<Vector2>());}UVsByVertex[verPos[2]].Add(materialUV);
}
}
for(int i=0;i<TempVer.Length/3;i++){int[]idx=new int[3]{i*3,i*3+1,i*3+2};Vector3[]verPos=new Vector3[3];
for(int j=0;j<3;j++){
var MaterialIdGroupsOrdered=UVsByVertex[verPos[j]=TempVer[idx[j]].pos].ToArray().Select(uv=>{return AtlasHelper.GetMaterial(uv);}).GroupBy(value=>value).OrderByDescending(group=>group.Key).OrderByDescending(group=>group.Count());var weights=new Dictionary<int,int>(4);int total=0;Vector2 uv0=TempVer[idx[j]].texCoord0;
foreach(var MaterialIdgroup in MaterialIdGroupsOrdered){bool add;Vector2 uv=AtlasHelper.GetUV(MaterialIdgroup.First());
    if(uv0==uv){
        total+=weights[0]=MaterialIdgroup.Count();
    }else if((add=TempVer[idx[j]].texCoord1==_emptyUV)||TempVer[idx[j]].texCoord1==uv){
        if(add){var v1=TempVer[idx[0]];v1.texCoord1=uv;TempVer[idx[0]]=v1;
                    v1=TempVer[idx[1]];v1.texCoord1=uv;TempVer[idx[1]]=v1;
                    v1=TempVer[idx[2]];v1.texCoord1=uv;TempVer[idx[2]]=v1;
        }
        total+=weights[1]=MaterialIdgroup.Count();
    }
}
if(weights.Count>1){var v2=TempVer[idx[j]];
 Color col=v2.color;col.r=(weights[0]/(float)total);
                    col.g=(weights[1]/(float)total);
           v2.color=col;TempVer[idx[j]]=v2;
}
}}
if(LOG&&LOG_LEVEL<=2)Debug.Log("job done "+watch.ElapsedMilliseconds+" ms ["+cnkRgn1);
lock(tasksBusyCount_Syn){tasksBusyCount--;}queue.Set();
hasBuildData=true;backgroundDataSet.Set();}
        int index(Vector2Int offset){
            if(offset.x== 0&&offset.y== 0)return 0;
            if(offset.x==-1&&offset.y== 0)return 1;
            if(offset.x== 1&&offset.y== 0)return 2;
            if(offset.x== 0&&offset.y==-1)return 3;
            if(offset.x==-1&&offset.y==-1)return 4;
            if(offset.x== 1&&offset.y==-1)return 5;
            if(offset.x== 0&&offset.y== 1)return 6;
            if(offset.x==-1&&offset.y== 1)return 7;
            if(offset.x== 1&&offset.y== 1)return 8;
        return -1;}
        _Stop:{
            TempVer.Dispose();TempTriangles.Dispose();
if(LOG&&LOG_LEVEL<=2)Debug.Log("end");
        }
#pragma warning disable CS8321 
        void assert(bool condition,string msg="",bool stop=true){
#pragma warning restore CS8321 
            if(!condition){try{throw new Exception(string.IsNullOrEmpty(msg)?"assert failed":msg);}catch{if(stop)throw;}finally{if(TempVer.IsCreated)TempVer.Dispose();if(TempTriangles.IsCreated)TempTriangles.Dispose();}}
        }
    }
}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);try{if(TempVer.IsCreated)TempVer.Dispose();}finally{}try{if(TempTriangles.IsCreated)TempTriangles.Dispose();}finally{}}}
public struct Voxel{
       public Voxel(double d,Vector3 n,MaterialId m){
        Density=d;Normal=n;Material=m;IsCreated=true;
       }
public double Density;public Vector3 Normal;public MaterialId Material;public bool IsCreated;
public static Voxel Air{get;}=new Voxel(0.0,Vector3.up,MaterialId.Air);
public static Voxel Bedrock{get;}=new Voxel(101.0,Vector3.up,MaterialId.Bedrock);
}
public static readonly ReadOnlyCollection<Vector3>Corners=new ReadOnlyCollection<Vector3>(new Vector3[8]{
new Vector3(-.5f,-.5f,-.5f),
new Vector3( .5f,-.5f,-.5f),
new Vector3( .5f, .5f,-.5f),
new Vector3(-.5f, .5f,-.5f),
new Vector3(-.5f,-.5f, .5f),
new Vector3( .5f,-.5f, .5f),
new Vector3( .5f, .5f, .5f),
new Vector3(-.5f, .5f, .5f),
});
public static Vector3 TrianglePosAdj{get;}=new Vector3((Width/2.0f)-0.5f,(Height/2.0f)-0.5f,(Depth/2.0f)-0.5f);//  Ajuste para que o mesh do chunk fique centralizado, com pivot em 0,0,0
[NonSerialized]public const double IsoLevel=-50.0d;Vector2 _emptyUV{get;}=new Vector2(-1,-1);




//[NonSerialized]Task task;[NonSerialized]readonly AutoResetEvent foregroundDataSet=new AutoResetEvent(false);[NonSerialized]readonly ManualResetEvent backgroundDataSet=new ManualResetEvent(true);
//protected override void OnEnable(){
//                   base.OnEnable();
//    if(mesh==null){
//        mesh=gameObject.GetComponent<MeshFilter>().mesh;mesh.bounds=new Bounds(Vector3.zero,new Vector3(Width,Height,Depth));renderer=gameObject.GetComponent<MeshRenderer>();
//    }
//Stop=false;task=Task.Factory.StartNew(BG,new object[]{LOG,LOG_LEVEL,TempVer=new NativeList<Vertex>(Allocator.Persistent),TempTriangles=new NativeList<ushort>(Allocator.Persistent),},TaskCreationOptions.LongRunning);
//}
//bool Stop{
//    get{bool tmp;lock(Stop_Syn){tmp=Stop_v;      }return tmp;}
//    set{         lock(Stop_Syn){    Stop_v=value;}if(value){foregroundDataSet.Set();}}
//}[NonSerialized]readonly object Stop_Syn=new object();[NonSerialized]bool Stop_v;
//protected override void OnDisable(){
//Stop=true;try{task.Wait();}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
//                   base.OnDisable();
//}
//[NonSerialized]bool hasBuildData;
//protected override void Update(){
//                   base.Update();
//    if(backgroundDataSet.WaitOne(0)){
//        if(hasBuildData){
//            hasBuildData=false;
//if(LOG&&LOG_LEVEL<=2)Debug.Log("did job now build");
//            OnBuild();
//        }
//        if(needsRebuild){
//            needsRebuild=false;
//            cCoord1=Coord;
//            cnkRgn1=Rgn;
//            backgroundDataSet.Reset();foregroundDataSet.Set();
//        }




//        //if(DEBUG_START){DEBUG_START=false;backgroundDataSet.Reset();foregroundDataSet.Set();}




//    }
//}
//[NonSerialized]NativeList<Vertex>TempVer;
//[NonSerialized]NativeList<ushort>TempTriangles;
//[NonSerialized]public new MeshRenderer renderer=null;[NonSerialized]Mesh mesh=null;
//void OnBuild(){
//if(LOG&&LOG_LEVEL<=2)Debug.Log("TempVer.Length:"+TempVer.Length+";TempTriangles.Length:"+TempTriangles.Length);
//    bool resize;
//#region VertexBuffer
//    if(resize=TempVer.Length>mesh.vertexCount)
//        mesh.SetVertexBufferParams(TempVer.Length,layout);
//    mesh.SetVertexBufferData(TempVer.AsArray(),0,0,TempVer.Length,0,MeshUpdateFlags.DontValidateIndices|MeshUpdateFlags.DontNotifyMeshUsers|MeshUpdateFlags.DontRecalculateBounds);
//#endregion 
//#region IndexBuffer
//    if(resize)
//        mesh.SetIndexBufferParams(TempTriangles.Length,IndexFormat.UInt16);
//    mesh.SetIndexBufferData(TempTriangles.AsArray(),0,0,TempTriangles.Length,MeshUpdateFlags.DontValidateIndices|MeshUpdateFlags.DontNotifyMeshUsers|MeshUpdateFlags.DontRecalculateBounds);
//#endregion 
//#region SubMesh
//        mesh.subMeshCount=1;
//    mesh.SetSubMesh(0,new SubMeshDescriptor(0,TempTriangles.Length),MeshUpdateFlags.DontValidateIndices|MeshUpdateFlags.DontNotifyMeshUsers|MeshUpdateFlags.DontRecalculateBounds);
//#endregion 
//}
//[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
//public struct Vertex{
//    public Vector3 pos;
//    public Vector3 normal;
//       public Vertex(Vector3 p,Vector3 n){
//        pos=p;
//        normal=n;
//       }
//}
//[NonSerialized]static readonly VertexAttributeDescriptor[]layout=new[]{
//    new VertexAttributeDescriptor(VertexAttribute.Position ,VertexAttributeFormat.Float32,3),
//    new VertexAttributeDescriptor(VertexAttribute.Normal   ,VertexAttributeFormat.Float32,3),
//};
//[NonSerialized]readonly Voxel[]voxels=new Voxel[VoxelsPerChunk];
//[NonSerialized]Vector2Int cCoord1;
//[NonSerialized]Vector2Int cnkRgn1;
//void BG(object state){Thread.CurrentThread.IsBackground=false;try{
//    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL&&parameters[2]is NativeList<Vertex>TempVer&&parameters[3]is NativeList<ushort>TempTriangles){
//#region SetPolygonCell Variables
//Dictionary<int,Voxel>[]neighbors=new Dictionary<int,Voxel>[8];for(int i=0;i<8;i++){neighbors[i]=new Dictionary<int,Voxel>();}
//PolygonCell polygonCell=new PolygonCell();
//Voxel[][][]_voxelsBuffer1=new Voxel[3][][]{new Voxel[1][]{new Voxel[4],},new Voxel[Depth][],new Voxel[FlattenOffset][],};for(int i=0;i<_voxelsBuffer1[2].Length;++i){_voxelsBuffer1[2][i]=new Voxel[4];if(i<_voxelsBuffer1[1].Length){_voxelsBuffer1[1][i]=new Voxel[4];}}
//  Voxel[][]_voxelsBuffer2=new Voxel[3][]{new Voxel[1],new Voxel[Depth],new Voxel[FlattenOffset],};
//Voxel[]_tempVoxels=new Voxel[6];
//Vector3 noiseInput;double[][]_noiseCache1=new double[9][];for(int i=0;i<9;++i){_noiseCache1[i]=new double[FlattenOffset];}//  Calcular a altura uma vez e usar este valor para este x e este z, onde, se abaixo da altura, sempre será com densidade, e se acima, não terá densidade (por isso não precisa calcular toda vez que usar este mesmo x e mesmo z, ao trocar de y).
//#endregion
//#region Polygoniser Variables
//Vector3[][][]_verticesBuffer=new Vector3[3][][]{new Vector3[1][]{new Vector3[4],},new Vector3[Depth][],new Vector3[FlattenOffset][],};for(int i=0;i<_verticesBuffer[2].Length;++i){_verticesBuffer[2][i]=new Vector3[4];if(i<_verticesBuffer[1].Length){_verticesBuffer[1][i]=new Vector3[4];}}
//MaterialId[]materials=new MaterialId[12];
//   Vector3[] vertices=new Vector3[12];
//   Vector3[]  normals=new Vector3[12];
//ushort vertexCount=0;
//#endregion
//        while(!Stop){foregroundDataSet.WaitOne();if(Stop)goto _Stop;
//if(LOG&&LOG_LEVEL<=2)Debug.Log("do job");
//Array.Clear(voxels,0,voxels.Length);TempVer.Clear();TempTriangles.Clear();



        
//Vector3Int vCoord1,vCoord2,vCoord3;
//int corner;
//Vector2Int cnkRgn2,cnkRgn3;
//Vector2Int cCoord3;
//int vxlIdx3;
//Vector2Int preOffset=Vector2Int.zero;
//Vector2Int posOffset=Vector2Int.zero;
//for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
//for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
//for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
//SetPolygonCell();Polygonise();
//}}}
//#region SetPolygonCell
//void SetPolygonCell(){
//    corner=0;
//    _Cycle:{
//        cnkRgn2=cnkRgn1;
//        vCoord2=vCoord1;
//        switch(corner){
//            case(0):{goto _0;}
//            case(1):{goto _1;}
//            case(2):{goto _2;}
//            case(3):{goto _3;}
//            case(4):{goto _4;}
//            case(5):{goto _5;}
//            case(6):{goto _6;}
//            case(7):{goto _7;}
//            default:{goto _End;}
//        }
//    }
//    _0:{
//              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][0];
//        }else if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][0];
//        }else if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][0];
//        }else{ 
//GetVoxel();}
//        goto _Next;
//    }
//    _1:{
//              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][1];
//        }else if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][1];
//        }else{
//        vCoord2.x+=1;
//GetVoxel();}
//        goto _Next;
//    }
//    _2:{
//              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][2];
//        }else{
//        vCoord2.x+=1;
//        vCoord2.y+=1;
//GetVoxel();}
//        goto _Next;
//    }
//    _3:{
//              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][3];
//        }else if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][1];
//        }else{
//        vCoord2.y+=1;
//GetVoxel();}
//        goto _Next;
//    }
//    _4:{
//              if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][2];
//        }else if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][2];
//        }else{
//        vCoord2.z+=1;
//GetVoxel();}
//        goto _Next;
//    }
//    _5:{
//              if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][3];
//        }else{
//        vCoord2.x+=1;
//        vCoord2.z+=1;
//GetVoxel();}
//        goto _Next;
//    }
//    _6:{
//        vCoord2.x+=1;
//        vCoord2.y+=1;
//        vCoord2.z+=1;
//GetVoxel();
//        goto _Next;
//    }
//    _7:{
//              if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][3];
//        }else{
//        vCoord2.y+=1;
//        vCoord2.z+=1;
//GetVoxel();}
//        goto _Next;
//    }
//    _Next:{
//        ++corner;
//        goto _Cycle;
//    }
//    _End:{
//_voxelsBuffer1[0][0][0]=polygonCell.Voxels[4];
//_voxelsBuffer1[0][0][1]=polygonCell.Voxels[5];
//_voxelsBuffer1[0][0][2]=polygonCell.Voxels[6];
//_voxelsBuffer1[0][0][3]=polygonCell.Voxels[7];
//_voxelsBuffer1[1][vCoord1.z][0]=polygonCell.Voxels[1];
//_voxelsBuffer1[1][vCoord1.z][1]=polygonCell.Voxels[2];
//_voxelsBuffer1[1][vCoord1.z][2]=polygonCell.Voxels[5];
//_voxelsBuffer1[1][vCoord1.z][3]=polygonCell.Voxels[6];
//_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][0]=polygonCell.Voxels[3];
//_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][1]=polygonCell.Voxels[2];
//_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][2]=polygonCell.Voxels[7];
//_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][3]=polygonCell.Voxels[6];
//    }
//#region GetVoxel
//    void GetVoxel(){
//        if(vCoord2.y<0){
//            polygonCell.Voxels[corner]=Voxel.Bedrock;
//        }else if(vCoord2.y>=Height){
//            polygonCell.Voxels[corner]=Voxel.Air;
//        }else{
//            if(vCoord2.x<0||vCoord2.x>=Width||
//               vCoord2.z<0||vCoord2.z>=Depth){ 
//ValidateCoord(ref cnkRgn2,ref vCoord2);
//            }
//            ComputeDensityNormal();
//        }
//    }
//#endregion
//}
//#region ComputeDensityNormal
//void ComputeDensityNormal(){
//    int step=-1;
//    _Cycle:{
//        cCoord3=cCoord1;
//        cnkRgn3=cnkRgn2;
//        vCoord3=vCoord2;
//        switch(step){
//            case(-1):{goto _neg1;}
//            case(0):{goto _0;}
//            case(1):{goto _1;}
//            case(2):{goto _2;}
//            case(3):{goto _3;}
//            case(4):{goto _4;}
//            case(5):{goto _5;}
//            default:{goto _End;}
//        }
//    }
//    _neg1:{
//SetVoxel(ref polygonCell.Voxels[corner]);if(polygonCell.Voxels[corner].Normal!=Vector3.zero){goto _Skip;}
//        goto _Next;
//    }
//    _0:{
//        vCoord3.x++;
//SetVoxel(ref _tempVoxels[step]);
//        goto _Next;
//    }
//    _1:{
//        if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&(_tempVoxels[step]=_voxelsBuffer2[1][vCoord2.z]).IsCreated){
//        }else{
//        vCoord3.x--;
//SetVoxel(ref _tempVoxels[step]);}
//        goto _Next;
//    }
//    _2:{
//        vCoord3.y++;
//SetVoxel(ref _tempVoxels[step]);
//        goto _Next;
//    }
//    _3:{
//        if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&(_tempVoxels[step]=_voxelsBuffer2[2][vCoord2.z+vCoord2.x*Width]).IsCreated){
//        }else{
//        vCoord3.y--;
//SetVoxel(ref _tempVoxels[step]);}
//        goto _Next;
//    }
//    _4:{
//        vCoord3.z++;
//SetVoxel(ref _tempVoxels[step]);
//        goto _Next;
//    }
//    _5:{
//        if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&(_tempVoxels[step]=_voxelsBuffer2[0][0]).IsCreated){
//        }else{
//        vCoord3.z--;
//SetVoxel(ref _tempVoxels[step]);}
//        goto _Next;
//    }
//    _Next:{
//        ++step;
//        goto _Cycle;
//    }
//    _End:{
//        polygonCell.Voxels[corner].Normal.x=(float)(_tempVoxels[0].Density-_tempVoxels[1].Density);
//        polygonCell.Voxels[corner].Normal.y=(float)(_tempVoxels[2].Density-_tempVoxels[3].Density);
//        polygonCell.Voxels[corner].Normal.z=(float)(_tempVoxels[4].Density-_tempVoxels[5].Density);
//        if(polygonCell.Voxels[corner].Normal!=Vector3.zero){
//            polygonCell.Voxels[corner].Normal.Normalize();
//        }
//    }
//    _Skip:{}
////_voxelsBuffer2[0][0]=polygonCell.Voxels[corner];
////_voxelsBuffer2[1][vCoord2.z]=polygonCell.Voxels[corner];
////_voxelsBuffer2[2][vCoord2.z+vCoord2.x*Width]=polygonCell.Voxels[corner];
//#region SetVoxel
//    void SetVoxel(ref Voxel v){
//        if(vCoord3.y<0){
//            v=Voxel.Bedrock;
//        }else if(vCoord3.y>=Height){
//            v=Voxel.Air;
//        }else{
//            if(vCoord3.x<0||vCoord3.x>=Width||
//               vCoord3.z<0||vCoord3.z>=Depth){ 
//ValidateCoord(ref cnkRgn3,ref vCoord3);cCoord3=ChunkManager.RgnToCoord(cnkRgn3);
//            }
//            vxlIdx3=GetIdx(vCoord3.x,vCoord3.y,vCoord3.z);
//            noiseInput=vCoord3;noiseInput.x+=cnkRgn3.x;
//                               noiseInput.z+=cnkRgn3.y;
////            Voxel[]vxls;
//            int idx=index(preOffset+(cCoord3-cCoord1));if(idx==0){
//                if(voxels[vxlIdx3].IsCreated){v=voxels[vxlIdx3];}else{
//ChunkManager.biome.SetV(noiseInput+_deround,_noiseCache1[idx  ],vCoord3.z+vCoord3.x*Width,ref voxels[vxlIdx3]);v=voxels[vxlIdx3];

//                }
//            }else{
//                idx--;if(neighbors[idx].ContainsKey(vxlIdx3)){v=neighbors[idx][vxlIdx3];}else{Voxel vxl=new Voxel();
//ChunkManager.biome.SetV(noiseInput+_deround,_noiseCache1[idx+1],vCoord3.z+vCoord3.x*Width,ref vxl);v=neighbors[idx][vxlIdx3]=vxl;




//                }
//            }
////            if(cCoord3==cCoord1){
////                vxls=voxels[idx=index(preOffset)];
////            }else{
////                vxls=voxels[idx=index(preOffset+(cCoord3-cCoord1))];
////            }
////            if(vxls[vxlIdx3].IsCreated)v=vxls[vxlIdx3];else{
////                noiseInput=vCoord3;noiseInput.x+=cnkRgn3.x;
////                                   noiseInput.z+=cnkRgn3.y;




////        if(noiseInput.y<1){
////            v=Voxel.Bedrock;
////    return;
////        }




////            }
//        }
//    }
//#endregion
//}
//#endregion
//#endregion
//#region Polygoniser
//void Polygonise(){
//int edgeIndex;
//    /*
//        Determine the index into the edge table which
//        tells us which vertices are inside of the surface
//    */
//                                edgeIndex =  0;
//    if(polygonCell[0]<_IsoLevel)edgeIndex|=  1;
//    if(polygonCell[1]<_IsoLevel)edgeIndex|=  2;
//    if(polygonCell[2]<_IsoLevel)edgeIndex|=  4;
//    if(polygonCell[3]<_IsoLevel)edgeIndex|=  8;
//    if(polygonCell[4]<_IsoLevel)edgeIndex|= 16;
//    if(polygonCell[5]<_IsoLevel)edgeIndex|= 32;
//    if(polygonCell[6]<_IsoLevel)edgeIndex|= 64;
//    if(polygonCell[7]<_IsoLevel)edgeIndex|=128;
//    if(Tables.EdgeTable[edgeIndex]==0){/*  Cube is entirely in/out of the surface  */
//        return;
//    }
//    //  Use buffered data if available
//    vertices[ 0]=(vCoord1.z>0?_verticesBuffer[0][0][0]:(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][0]:Vector3.zero));
//    vertices[ 1]=(vCoord1.z>0?_verticesBuffer[0][0][1]:Vector3.zero);
//    vertices[ 2]=(vCoord1.z>0?_verticesBuffer[0][0][2]:Vector3.zero);
//    vertices[ 3]=(vCoord1.z>0?_verticesBuffer[0][0][3]:(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][0]:Vector3.zero));
//    vertices[ 4]=(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][1]:Vector3.zero);
//    vertices[ 5]=(Vector3.zero);
//    vertices[ 6]=(Vector3.zero);
//    vertices[ 7]=(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][1]:Vector3.zero);
//    vertices[ 8]=(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][2]:(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][3]:Vector3.zero));
//    vertices[ 9]=(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][2]:Vector3.zero);
//    vertices[10]=(Vector3.zero);
//    vertices[11]=(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][3]:Vector3.zero);
//    //  Compute vertices
//    if(0!=(Tables.EdgeTable[edgeIndex]&   1)){vertexInterp(0,1,ref normals[ 0],ref vertices[ 0],ref materials[ 0],vertices[ 0]!=Vector3.zero);}
//    if(0!=(Tables.EdgeTable[edgeIndex]&   2)){vertexInterp(1,2,ref normals[ 1],ref vertices[ 1],ref materials[ 1],vertices[ 1]!=Vector3.zero);}
//    if(0!=(Tables.EdgeTable[edgeIndex]&   4)){vertexInterp(2,3,ref normals[ 2],ref vertices[ 2],ref materials[ 2],vertices[ 2]!=Vector3.zero);}
//    if(0!=(Tables.EdgeTable[edgeIndex]&   8)){vertexInterp(3,0,ref normals[ 3],ref vertices[ 3],ref materials[ 3],vertices[ 3]!=Vector3.zero);}
//    if(0!=(Tables.EdgeTable[edgeIndex]&  16)){vertexInterp(4,5,ref normals[ 4],ref vertices[ 4],ref materials[ 4],vertices[ 4]!=Vector3.zero);}
//    if(0!=(Tables.EdgeTable[edgeIndex]&  32)){vertexInterp(5,6,ref normals[ 5],ref vertices[ 5],ref materials[ 5],vertices[ 5]!=Vector3.zero);}
//    if(0!=(Tables.EdgeTable[edgeIndex]&  64)){vertexInterp(6,7,ref normals[ 6],ref vertices[ 6],ref materials[ 6],vertices[ 6]!=Vector3.zero);}
//    if(0!=(Tables.EdgeTable[edgeIndex]& 128)){vertexInterp(7,4,ref normals[ 7],ref vertices[ 7],ref materials[ 7],vertices[ 7]!=Vector3.zero);}
//    if(0!=(Tables.EdgeTable[edgeIndex]& 256)){vertexInterp(0,4,ref normals[ 8],ref vertices[ 8],ref materials[ 8],vertices[ 8]!=Vector3.zero);}
//    if(0!=(Tables.EdgeTable[edgeIndex]& 512)){vertexInterp(1,5,ref normals[ 9],ref vertices[ 9],ref materials[ 9],vertices[ 9]!=Vector3.zero);}
//    if(0!=(Tables.EdgeTable[edgeIndex]&1024)){vertexInterp(2,6,ref normals[10],ref vertices[10],ref materials[10],vertices[10]!=Vector3.zero);}
//    if(0!=(Tables.EdgeTable[edgeIndex]&2048)){vertexInterp(3,7,ref normals[11],ref vertices[11],ref materials[11],vertices[11]!=Vector3.zero);}
//#region vertexInterp
//    void vertexInterp(int c0,int c1,ref Vector3 n,ref Vector3 p,ref MaterialId m,bool skipToNormal){double[]density=new double[2];Vector3[]vertex=new Vector3[2];float[]distance=new float[2];float marchingUnit;Vector3[]normal=new Vector3[2];MaterialId[]material=new MaterialId[2];
//        density[0]=polygonCell[c0];
//        density[1]=polygonCell[c1];
//        vertex[0]=PolygonCell.Corners[c0];
//        vertex[1]=PolygonCell.Corners[c1];
//if(skipToNormal){goto _Normal;}
//if(Math.Abs(_IsoLevel-density[0])<_Epsilon){p=vertex[0];goto _Normal;}
//if(Math.Abs(_IsoLevel-density[1])<_Epsilon){p=vertex[1];goto _Normal;}
//if(Math.Abs(density[0]-density[1])<_Epsilon){p=vertex[0];goto _Normal;}
//        marchingUnit=(float)((_IsoLevel-density[0])/(density[1]-density[0]));
//        p.x=vertex[0].x+marchingUnit*(vertex[1].x-vertex[0].x);
//        p.y=vertex[0].y+marchingUnit*(vertex[1].y-vertex[0].y);
//        p.z=vertex[0].z+marchingUnit*(vertex[1].z-vertex[0].z);
//_Normal:{
//        distance[0]=Vector3.Distance(vertex[0],vertex[1]);
//        distance[1]=Vector3.Distance(vertex[1],p);
//        n=Vector3.Lerp(
//            normal[1]=polygonCell.Voxels[c1].Normal,
//            normal[0]=polygonCell.Voxels[c0].Normal,distance[1]/distance[0]);
//        n=n!=Vector3.zero?n.normalized:(
//            normal[0]!=Vector3.zero?normal[0]:
//            normal[1]!=Vector3.zero?normal[1]: 
//            Vector3.down);
//}
//goto _UV;
//_UV:{
//        material[0]=polygonCell.Voxels[c0].Material;
//        material[1]=polygonCell.Voxels[c1].Material;
//        m=material[0];
//        if      (density[1]< density[0]){
//        m=material[1];
//        }else if(density[1]==density[0]&&(int)material[1]>(int)material[0]){
//        m=material[1];
//        }
//}
//    }
//#endregion
//    //  Buffer the data
//    _verticesBuffer[0][0][0]=vertices[ 4]+Vector3.back;//  Adiciona um valor negativo porque o voxelCoord próximo vai usar esse valor mais o de sua posição (próprio voxelCoord novo)
//    _verticesBuffer[0][0][1]=vertices[ 5]+Vector3.back;
//    _verticesBuffer[0][0][2]=vertices[ 6]+Vector3.back;
//    _verticesBuffer[0][0][3]=vertices[ 7]+Vector3.back;
//    //
//    _verticesBuffer[1][vCoord1.z][0]=vertices[ 1]+Vector3.left;
//    _verticesBuffer[1][vCoord1.z][1]=vertices[ 5]+Vector3.left;
//    _verticesBuffer[1][vCoord1.z][2]=vertices[ 9]+Vector3.left;
//    _verticesBuffer[1][vCoord1.z][3]=vertices[10]+Vector3.left;
//    //
//    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][0]=vertices[ 2]+Vector3.down;
//    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][1]=vertices[ 6]+Vector3.down;
//    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][2]=vertices[10]+Vector3.down;
//    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][3]=vertices[11]+Vector3.down;
//    /*  Create the triangle  */
//    for(int i=0;Tables.TriangleTable[edgeIndex][i]!=-1;i+=3){
//#region Indices
//        TempTriangles.Add((ushort)(vertexCount+2));
//        TempTriangles.Add((ushort)(vertexCount+1));
//        TempTriangles.Add(         vertexCount  );
//#endregion
//#region Vertices
//int[]tIdx=new int[3];
//Vector3 tPos=vCoord1-TrianglePosAdj;tPos.x+=posOffset.x;
//                                    tPos.z+=posOffset.y;
//Vector3[]verPos=new Vector3[3]{tPos+vertices[tIdx[0]=Tables.TriangleTable[edgeIndex][i  ]],
//                               tPos+vertices[tIdx[1]=Tables.TriangleTable[edgeIndex][i+1]],
//                               tPos+vertices[tIdx[2]=Tables.TriangleTable[edgeIndex][i+2]]};
//#region Normals
//Vector3 faceNorm=Vector3.zero;
//Vector3[]norm=new Vector3[3]{normals[tIdx[0]]!=Vector3.zero?normals[tIdx[0]]:(                                (faceNorm=Vector3.Cross(verPos[1]-verPos[0],verPos[2]-verPos[0]).normalized)),
//                             normals[tIdx[1]]!=Vector3.zero?normals[tIdx[1]]:(faceNorm!=Vector3.zero?faceNorm:(faceNorm=Vector3.Cross(verPos[1]-verPos[0],verPos[2]-verPos[0]).normalized)),
//                             normals[tIdx[2]]!=Vector3.zero?normals[tIdx[2]]:(faceNorm!=Vector3.zero?faceNorm:(         Vector3.Cross(verPos[1]-verPos[0],verPos[2]-verPos[0]).normalized))};
//        if(norm[0]==Vector3.zero){norm[0]=norm[1]!=Vector3.zero?norm[1]:norm[2];}
//        if(norm[1]==Vector3.zero){norm[1]=norm[0]!=Vector3.zero?norm[0]:norm[2];}
//        if(norm[2]==Vector3.zero){norm[2]=norm[0]!=Vector3.zero?norm[0]:norm[1];}
//#endregion
//#region Material
//        //  TO DO
//#endregion
//        TempVer.Add(new Vertex(verPos[0],-norm[0]));
//        TempVer.Add(new Vertex(verPos[1],-norm[1]));
//        TempVer.Add(new Vertex(verPos[2],-norm[2]));
//#endregion
//        vertexCount+=3;
//    }
//}
//#endregion
////Thread.Sleep(3000);




//            hasBuildData=true;backgroundDataSet.Set();
//Thread.Yield();
//for(int i=0;i<8;i++){neighbors[i].Clear();}Array.Clear(_voxelsBuffer1[0][0],0,_voxelsBuffer1[0][0].Length);for(int i=0;i<_voxelsBuffer1[2].Length;++i){Array.Clear(_voxelsBuffer1[2][i],0,_voxelsBuffer1[2][i].Length);if(i<_voxelsBuffer1[1].Length){Array.Clear(_voxelsBuffer1[1][i],0,_voxelsBuffer1[1][i].Length);}}Array.Clear(_voxelsBuffer2[0],0,_voxelsBuffer2[0].Length);Array.Clear(_voxelsBuffer2[1],0,_voxelsBuffer2[1].Length);Array.Clear(_voxelsBuffer2[2],0,_voxelsBuffer2[2].Length);Array.Clear(_tempVoxels,0,_tempVoxels.Length);for(int i=0;i<9;++i){Array.Clear(_noiseCache1[i],0,_noiseCache1[i].Length);}Array.Clear(_verticesBuffer[0][0],0,_verticesBuffer[0][0].Length);for(int i=0;i<_verticesBuffer[2].Length;++i){Array.Clear(_verticesBuffer[2][i],0,_verticesBuffer[2][i].Length);if(i<_verticesBuffer[1].Length){Array.Clear(_verticesBuffer[1][i],0,_verticesBuffer[1][i].Length);}}vertexCount=0;
//        }
//        _Stop:{
//            TempVer.Dispose();TempTriangles.Dispose();
//if(LOG&&LOG_LEVEL<=2)Debug.Log("end");
//        }
//        int index(Vector2Int offset){
//            if(offset.x== 0&&offset.y== 0)return 0;
//            if(offset.x==-1&&offset.y== 0)return 1;
//            if(offset.x== 1&&offset.y== 0)return 2;
//            if(offset.x== 0&&offset.y==-1)return 3;
//            if(offset.x==-1&&offset.y==-1)return 4;
//            if(offset.x== 1&&offset.y==-1)return 5;
//            if(offset.x== 0&&offset.y== 1)return 6;
//            if(offset.x==-1&&offset.y== 1)return 7;
//            if(offset.x== 1&&offset.y== 1)return 8;
//        return -1;}
//    }
//}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}}
//public class PolygonCell{
//    public readonly Voxel[]Voxels=new Voxel[8];
//    public double this[int i]{
//        get{switch(i){
//                case(0):{return-Voxels[0].Density;}
//                case(1):{return-Voxels[1].Density;}
//                case(2):{return-Voxels[2].Density;}
//                case(3):{return-Voxels[3].Density;}
//                case(4):{return-Voxels[4].Density;}
//                case(5):{return-Voxels[5].Density;}
//                case(6):{return-Voxels[6].Density;}
//                case(7):{return-Voxels[7].Density;}
//                default:{return 0;}
//        }}
//    }
//public static readonly ReadOnlyCollection<Vector3>Corners=new ReadOnlyCollection<Vector3>(new Vector3[8]{
//new Vector3(-.5f,-.5f,-.5f),
//new Vector3( .5f,-.5f,-.5f),
//new Vector3( .5f, .5f,-.5f),
//new Vector3(-.5f, .5f,-.5f),
//new Vector3(-.5f,-.5f, .5f),
//new Vector3( .5f,-.5f, .5f),
//new Vector3( .5f, .5f, .5f),
//new Vector3(-.5f, .5f, .5f),
//});
//}
//[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
//public struct Voxel{
//       public Voxel(double d,Vector3 n,MaterialId m){
//        Density=d;Normal=n;Material=m;IsCreated=true;
//       }
//public double Density;public Vector3 Normal;public MaterialId Material;public bool IsCreated;
//public static Voxel Air{get;}=new Voxel(0.0,Vector3.up,MaterialId.Air);
//public static Voxel Bedrock{get;}=new Voxel(101.0,Vector3.up,MaterialId.Bedrock);
//}
//Vector3 _deround{get;}=new Vector3(.5f,.5f,.5f);
//public const double _Epsilon=0.00001;
//public const double _IsoLevel=-50.0d;
//public static Vector3 TrianglePosAdj{get;}=new Vector3((Width/2.0f)-0.5f,(Height/2.0f)-0.5f,(Depth/2.0f)-0.5f);//  Ajuste para que o mesh do chunk fique centralizado, com pivot em 0,0,0




////[NonSerialized]Task task;[NonSerialized]readonly AutoResetEvent foregroundDataSet=new AutoResetEvent(false);[NonSerialized]readonly ManualResetEvent backgroundDataSet=new ManualResetEvent(true);
////    protected override void OnEnable(){
////                       base.OnEnable();
////        if(mesh==null){
////            mesh=gameObject.GetComponent<MeshFilter>().mesh;mesh.bounds=new Bounds(Vector3.zero,new Vector3(Width,Height,Depth));renderer=gameObject.GetComponent<MeshRenderer>();
////        }
////Stop=false;task=Task.Factory.StartNew(BG,new object[]{LOG,LOG_LEVEL,TempVer=new NativeList<Vertex>(Allocator.Persistent),TempTriangles=new NativeList<ushort>(Allocator.Persistent),},TaskCreationOptions.LongRunning);
////    }
////bool Stop{
////    get{bool tmp;lock(Stop_Syn){tmp=Stop_v;      }return tmp;}
////    set{         lock(Stop_Syn){    Stop_v=value;}if(value){foregroundDataSet.Set();}}
////}[NonSerialized]readonly object Stop_Syn=new object();[NonSerialized]bool Stop_v;
////    protected override void OnDisable(){
////Stop=true;try{task.Wait();}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
////                       base.OnDisable();
////    }
////[NonSerialized]bool hasBuildData;
////    protected override void Update(){
////                       base.Update();
////        if(backgroundDataSet.WaitOne(0)){
////            if(hasBuildData){
////                hasBuildData=false;
////if(LOG&&LOG_LEVEL<=2)Debug.Log("did job now build");
////                OnBuild();
////            }
////            if(needsRebuild){
////                needsRebuild=false;
////                cCoord1=Coord;
////                cnkRgn1=Rgn;
////                backgroundDataSet.Reset();foregroundDataSet.Set();
////            }




////            //if(DEBUG_START){DEBUG_START=false;}//




////        }
////    }
////[NonSerialized]NativeList<Vertex>TempVer;
////[NonSerialized]NativeList<ushort>TempTriangles;
////[NonSerialized]public new MeshRenderer renderer=null;[NonSerialized]Mesh mesh=null;
////    void OnBuild(){
////if(LOG&&LOG_LEVEL<=2)Debug.Log("TempVer.Length:"+TempVer.Length+";TempTriangles.Length:"+TempTriangles.Length);
////        bool resize;
////#region VertexBuffer
////        if(resize=TempVer.Length>mesh.vertexCount)
////            mesh.SetVertexBufferParams(TempVer.Length,layout);
////        mesh.SetVertexBufferData(TempVer.AsArray(),0,0,TempVer.Length,0,MeshUpdateFlags.DontValidateIndices|MeshUpdateFlags.DontRecalculateBounds);
////#endregion 
////#region IndexBuffer
////        if(resize)
////            mesh.SetIndexBufferParams(TempTriangles.Length,IndexFormat.UInt16);
////        mesh.SetIndexBufferData(TempTriangles.AsArray(),0,0,TempTriangles.Length,MeshUpdateFlags.DontValidateIndices|MeshUpdateFlags.DontRecalculateBounds);
////#endregion 
////#region SubMesh
////            mesh.subMeshCount=1;
////        mesh.SetSubMesh(0,new SubMeshDescriptor(0,TempTriangles.Length),MeshUpdateFlags.DontValidateIndices|MeshUpdateFlags.DontRecalculateBounds);
////#endregion 
////    }
////    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
////    public struct Vertex{
////        public Vector3 pos;
////        public Vector3 normal;
////           public Vertex(Vector3 p,Vector3 n){
////            pos=p;
////            normal=n;
////           }
////    }
////[NonSerialized]static readonly VertexAttributeDescriptor[]layout=new[]{
////        new VertexAttributeDescriptor(VertexAttribute.Position ,VertexAttributeFormat.Float32,3),
////        new VertexAttributeDescriptor(VertexAttribute.Normal   ,VertexAttributeFormat.Float32,3),
////    };




//        public bool DEBUG_START=false;
//////        //Task.Factory.StartNew(BG,new object[]{LOG,LOG_LEVEL,0f,new NativeList<Vertex>(Allocator.TempJob),new NativeList<ushort>(Allocator.TempJob),new Vector2Int(),new Vector2Int()},TaskCreationOptions.PreferFairness);
//////    }
//////    protected override void OnBuild(object state){
//////        if(state is object[]parameters&&parameters[0]is float time&&parameters[1]is NativeList<Vertex>TempVer&&parameters[2]is NativeList<ushort>TempTriangles){
//////            if(mesh==null){
//////                mesh=(meshFilter=gameObject.AddComponent<MeshFilter>()).mesh;mesh.bounds=new Bounds(Vector3.zero,new Vector3(16,256,16));renderer=gameObject.AddComponent<MeshRenderer>();
//////            }
//////            bool resize;
//////#region VertexBuffer
//////            if(resize=TempVer.Length>mesh.vertexCount)
//////                mesh.SetVertexBufferParams(TempVer.Length,layout);
//////            mesh.SetVertexBufferData(TempVer.AsArray(),0,0,TempVer.Length,0,MeshUpdateFlags.DontValidateIndices|MeshUpdateFlags.DontRecalculateBounds);
//////#endregion 
//////#region IndexBuffer
//////            if(resize)
//////                mesh.SetIndexBufferParams(TempTriangles.Length,IndexFormat.UInt16);
//////            mesh.SetIndexBufferData(TempTriangles.AsArray(),0,0,TempTriangles.Length,MeshUpdateFlags.DontValidateIndices|MeshUpdateFlags.DontRecalculateBounds);
//////#endregion 
//////#region SubMesh
//////                mesh.subMeshCount=1;
//////            mesh.SetSubMesh(0,new SubMeshDescriptor(0,TempTriangles.Length),MeshUpdateFlags.DontValidateIndices|MeshUpdateFlags.DontRecalculateBounds);
//////#endregion 
//////            Debug.LogWarning("dispose:"+TempVer.Length+";"+TempTriangles.Length);
//////            TempVer.Dispose();TempTriangles.Dispose();
//////        }
//////    }
//////    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
//////    public struct Vertex{
//////        public Vector3 pos;
//////        public Vector3 normal;
//////           public Vertex(Vector3 p,Vector3 n){
//////            pos=p;
//////            normal=n;
//////           }
//////    }
//////[NonSerialized]static readonly VertexAttributeDescriptor[]layout=new[]{
//////        new VertexAttributeDescriptor(VertexAttribute.Position ,VertexAttributeFormat.Float32,3),
//////        new VertexAttributeDescriptor(VertexAttribute.Normal   ,VertexAttributeFormat.Float32,3),
//////    };
////[NonSerialized]readonly Voxel[][]voxels=new Voxel[9][];
////[NonSerialized]Vector2Int cCoord1;
////[NonSerialized]Vector2Int cnkRgn1;
////    void BG(object state){Thread.CurrentThread.IsBackground=false;try{
////        if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL&&parameters[2]is NativeList<Vertex>TempVer&&parameters[3]is NativeList<ushort>TempTriangles){
////voxels[0]=new Voxel[VoxelsPerChunk];



////            while(!Stop){foregroundDataSet.WaitOne();if(Stop)goto _Stop;


////            queue.WaitOne();lock(TasksBusyCount_Syn){TasksBusyCount++;if(LOG&&LOG_LEVEL<=1)Debug.Log("TasksBusyCount:"+TasksBusyCount);if(TasksBusyCount<=0)queue.Set();}
//////SpinWait.SpinUntil(()=>{lock(TasksBusyCount_Syn){if(TasksBusyCount<=0)return true;}return false;});


//////lock(TasksBusyCount_Syn){if(LOG&&LOG_LEVEL<=1)Debug.Log("TasksBusyCount:"+TasksBusyCount);TasksBusyCount++;}
////if(LOG&&LOG_LEVEL<=2)Debug.Log("do job");
////Array.Clear(voxels[0],0,voxels[0].Length);for(int i=1;i<9;++i){voxels[i]=new Voxel[VoxelsPerChunk];}
////#region SetPolygonCell Variables
////PolygonCell polygonCell=new PolygonCell();
////Voxel[][][]_voxelsBuffer1=new Voxel[3][][]{new Voxel[1][]{new Voxel[4],},new Voxel[Depth][],new Voxel[FlattenOffset][],};for(int i=0;i<_voxelsBuffer1[2].Length;++i){_voxelsBuffer1[2][i]=new Voxel[4];if(i<_voxelsBuffer1[1].Length){_voxelsBuffer1[1][i]=new Voxel[4];}}
////  Voxel[][]_voxelsBuffer2=new Voxel[3][]{new Voxel[1],new Voxel[Depth],new Voxel[FlattenOffset],};
////Voxel[]_tempVoxels=new Voxel[6];
////Vector3 noiseInput;double[][]_noiseCache1=new double[9][];for(int i=0;i<9;++i){_noiseCache1[i]=new double[FlattenOffset];}//  Calcular a altura uma vez e usar este valor para este x e este z, onde, se abaixo da altura, sempre será com densidade, e se acima, não terá densidade (por isso não precisa calcular toda vez que usar este mesmo x e mesmo z, ao trocar de y).
////#endregion
////#region Polygoniser Variables
////Vector3[][][]_verticesBuffer=new Vector3[3][][]{new Vector3[1][]{new Vector3[4],},new Vector3[Depth][],new Vector3[FlattenOffset][],};for(int i=0;i<_verticesBuffer[2].Length;++i){_verticesBuffer[2][i]=new Vector3[4];if(i<_verticesBuffer[1].Length){_verticesBuffer[1][i]=new Vector3[4];}}
////MaterialId[]materials=new MaterialId[12];
////   Vector3[] vertices=new Vector3[12];
////   Vector3[]  normals=new Vector3[12];
////ushort vertexCount=0;
////#endregion


////TempVer.Clear();TempTriangles.Clear();
////Vector3Int vCoord1,vCoord2,vCoord3;
////int corner;
////Vector2Int cnkRgn2,cnkRgn3;
////Vector2Int cCoord3;
////int vxlIdx3;
////Vector2Int preOffset=Vector2Int.zero;
////Vector2Int posOffset=Vector2Int.zero;
////for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
////for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
////for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
////SetPolygonCell();Polygonise();
////}}}
////#region SetPolygonCell
////void SetPolygonCell(){
////    corner=0;
////    _Cycle:{
////        cnkRgn2=cnkRgn1;
////        vCoord2=vCoord1;
////        switch(corner){
////            case(0):{goto _0;}
////            case(1):{goto _1;}
////            case(2):{goto _2;}
////            case(3):{goto _3;}
////            case(4):{goto _4;}
////            case(5):{goto _5;}
////            case(6):{goto _6;}
////            case(7):{goto _7;}
////            default:{goto _End;}
////        }
////    }
////    _0:{
////              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][0];
////        }else if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][0];
////        }else if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][0];
////        }else{ 
////GetVoxel();}
////        goto _Next;
////    }
////    _1:{
////              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][1];
////        }else if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][1];
////        }else{
////        vCoord2.x+=1;
////GetVoxel();}
////        goto _Next;
////    }
////    _2:{
////              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][2];
////        }else{
////        vCoord2.x+=1;
////        vCoord2.y+=1;
////GetVoxel();}
////        goto _Next;
////    }
////    _3:{
////              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][3];
////        }else if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][1];
////        }else{
////        vCoord2.y+=1;
////GetVoxel();}
////        goto _Next;
////    }
////    _4:{
////              if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][2];
////        }else if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][2];
////        }else{
////        vCoord2.z+=1;
////GetVoxel();}
////        goto _Next;
////    }
////    _5:{
////              if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][3];
////        }else{
////        vCoord2.x+=1;
////        vCoord2.z+=1;
////GetVoxel();}
////        goto _Next;
////    }
////    _6:{
////        vCoord2.x+=1;
////        vCoord2.y+=1;
////        vCoord2.z+=1;
////GetVoxel();
////        goto _Next;
////    }
////    _7:{
////              if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][3];
////        }else{
////        vCoord2.y+=1;
////        vCoord2.z+=1;
////GetVoxel();}
////        goto _Next;
////    }
////    _Next:{
////        ++corner;
////        goto _Cycle;
////    }
////    _End:{
////_voxelsBuffer1[0][0][0]=polygonCell.Voxels[4];
////_voxelsBuffer1[0][0][1]=polygonCell.Voxels[5];
////_voxelsBuffer1[0][0][2]=polygonCell.Voxels[6];
////_voxelsBuffer1[0][0][3]=polygonCell.Voxels[7];
////_voxelsBuffer1[1][vCoord1.z][0]=polygonCell.Voxels[1];
////_voxelsBuffer1[1][vCoord1.z][1]=polygonCell.Voxels[2];
////_voxelsBuffer1[1][vCoord1.z][2]=polygonCell.Voxels[5];
////_voxelsBuffer1[1][vCoord1.z][3]=polygonCell.Voxels[6];
////_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][0]=polygonCell.Voxels[3];
////_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][1]=polygonCell.Voxels[2];
////_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][2]=polygonCell.Voxels[7];
////_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][3]=polygonCell.Voxels[6];
////    }
////#region GetVoxel
////    void GetVoxel(){
////        if(vCoord2.y<0){
////            polygonCell.Voxels[corner]=Voxel.Bedrock;
////        }else if(vCoord2.y>=Height){
////            polygonCell.Voxels[corner]=Voxel.Air;
////        }else{
////            if(vCoord2.x<0||vCoord2.x>=Width||
////               vCoord2.z<0||vCoord2.z>=Depth){ 
////ValidateCoord(ref cnkRgn2,ref vCoord2);
////            }
////            ComputeDensityNormal();
////        }
////    }
////#endregion
////}
////#region ComputeDensityNormal
////void ComputeDensityNormal(){
////    int step=-1;
////    _Cycle:{
////        cCoord3=cCoord1;
////        cnkRgn3=cnkRgn2;
////        vCoord3=vCoord2;
////        switch(step){
////            case(-1):{goto _neg1;}
////            case(0):{goto _0;}
////            case(1):{goto _1;}
////            case(2):{goto _2;}
////            case(3):{goto _3;}
////            case(4):{goto _4;}
////            case(5):{goto _5;}
////            default:{goto _End;}
////        }
////    }
////    _neg1:{
////SetVoxel(ref polygonCell.Voxels[corner]);if(polygonCell.Voxels[corner].Normal!=Vector3.zero){goto _Skip;}
////        goto _Next;
////    }
////    _0:{
////        vCoord3.x++;
////SetVoxel(ref _tempVoxels[step]);
////        goto _Next;
////    }
////    _1:{
////        if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&(_tempVoxels[step]=_voxelsBuffer2[1][vCoord2.z]).IsCreated){
////        }else{
////        vCoord3.x--;
////SetVoxel(ref _tempVoxels[step]);}
////        goto _Next;
////    }
////    _2:{
////        vCoord3.y++;
////SetVoxel(ref _tempVoxels[step]);
////        goto _Next;
////    }
////    _3:{
////        if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&(_tempVoxels[step]=_voxelsBuffer2[2][vCoord2.z+vCoord2.x*Width]).IsCreated){
////        }else{
////        vCoord3.y--;
////SetVoxel(ref _tempVoxels[step]);}
////        goto _Next;
////    }
////    _4:{
////        vCoord3.z++;
////SetVoxel(ref _tempVoxels[step]);
////        goto _Next;
////    }
////    _5:{
////        if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&(_tempVoxels[step]=_voxelsBuffer2[0][0]).IsCreated){
////        }else{
////        vCoord3.z--;
////SetVoxel(ref _tempVoxels[step]);}
////        goto _Next;
////    }
////    _Next:{
////        ++step;
////        goto _Cycle;
////    }
////    _End:{
////        polygonCell.Voxels[corner].Normal.x=(float)(_tempVoxels[0].Density-_tempVoxels[1].Density);
////        polygonCell.Voxels[corner].Normal.y=(float)(_tempVoxels[2].Density-_tempVoxels[3].Density);
////        polygonCell.Voxels[corner].Normal.z=(float)(_tempVoxels[4].Density-_tempVoxels[5].Density);
////        if(polygonCell.Voxels[corner].Normal!=Vector3.zero){
////            polygonCell.Voxels[corner].Normal.Normalize();
////        }
////    }
////    _Skip:{}
////_voxelsBuffer2[0][0]=polygonCell.Voxels[corner];
////_voxelsBuffer2[1][vCoord2.z]=polygonCell.Voxels[corner];
////_voxelsBuffer2[2][vCoord2.z+vCoord2.x*Width]=polygonCell.Voxels[corner];
////#region SetVoxel
////    void SetVoxel(ref Voxel v){
////        if(vCoord3.y<0){
////            v=Voxel.Bedrock;
////        }else if(vCoord3.y>=Height){
////            v=Voxel.Air;
////        }else{
////            if(vCoord3.x<0||vCoord3.x>=Width||
////               vCoord3.z<0||vCoord3.z>=Depth){ 
////ValidateCoord(ref cnkRgn3,ref vCoord3);cCoord3=ChunkManager.RgnToCoord(cnkRgn3);
////            }
////            vxlIdx3=GetIdx(vCoord3.x,vCoord3.y,vCoord3.z);
////            Voxel[]vxls;
////            int idx;
////            if(cCoord3==cCoord1){
////                vxls=voxels[idx=index(preOffset)];
////            }else{
////                vxls=voxels[idx=index(preOffset+(cCoord3-cCoord1))];
////            }
////            if(vxls[vxlIdx3].IsCreated)v=vxls[vxlIdx3];else{
////                noiseInput=vCoord3;noiseInput.x+=cnkRgn3.x;
////                                   noiseInput.z+=cnkRgn3.y;




////        if(noiseInput.y<1){
////            v=Voxel.Bedrock;
////    return;
////        }




////            }
////        }
////    }
////#endregion
////}
////#endregion
////#endregion
////#region Polygoniser
////void Polygonise(){
////int edgeIndex;
////    /*
////        Determine the index into the edge table which
////        tells us which vertices are inside of the surface
////    */
////                                edgeIndex =  0;
////    if(polygonCell[0]<_IsoLevel)edgeIndex|=  1;
////    if(polygonCell[1]<_IsoLevel)edgeIndex|=  2;
////    if(polygonCell[2]<_IsoLevel)edgeIndex|=  4;
////    if(polygonCell[3]<_IsoLevel)edgeIndex|=  8;
////    if(polygonCell[4]<_IsoLevel)edgeIndex|= 16;
////    if(polygonCell[5]<_IsoLevel)edgeIndex|= 32;
////    if(polygonCell[6]<_IsoLevel)edgeIndex|= 64;
////    if(polygonCell[7]<_IsoLevel)edgeIndex|=128;
////    if(Tables.EdgeTable[edgeIndex]==0){/*  Cube is entirely in/out of the surface  */
////        return;
////    }
////    //  Use buffered data if available
////    vertices[ 0]=(vCoord1.z>0?_verticesBuffer[0][0][0]:(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][0]:Vector3.zero));
////    vertices[ 1]=(vCoord1.z>0?_verticesBuffer[0][0][1]:Vector3.zero);
////    vertices[ 2]=(vCoord1.z>0?_verticesBuffer[0][0][2]:Vector3.zero);
////    vertices[ 3]=(vCoord1.z>0?_verticesBuffer[0][0][3]:(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][0]:Vector3.zero));
////    vertices[ 4]=(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][1]:Vector3.zero);
////    vertices[ 5]=(Vector3.zero);
////    vertices[ 6]=(Vector3.zero);
////    vertices[ 7]=(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][1]:Vector3.zero);
////    vertices[ 8]=(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][2]:(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][3]:Vector3.zero));
////    vertices[ 9]=(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][2]:Vector3.zero);
////    vertices[10]=(Vector3.zero);
////    vertices[11]=(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][3]:Vector3.zero);
////    //  Compute vertices
////    if(0!=(Tables.EdgeTable[edgeIndex]&   1)){vertexInterp(0,1,ref normals[ 0],ref vertices[ 0],ref materials[ 0],vertices[ 0]!=Vector3.zero);}
////    if(0!=(Tables.EdgeTable[edgeIndex]&   2)){vertexInterp(1,2,ref normals[ 1],ref vertices[ 1],ref materials[ 1],vertices[ 1]!=Vector3.zero);}
////    if(0!=(Tables.EdgeTable[edgeIndex]&   4)){vertexInterp(2,3,ref normals[ 2],ref vertices[ 2],ref materials[ 2],vertices[ 2]!=Vector3.zero);}
////    if(0!=(Tables.EdgeTable[edgeIndex]&   8)){vertexInterp(3,0,ref normals[ 3],ref vertices[ 3],ref materials[ 3],vertices[ 3]!=Vector3.zero);}
////    if(0!=(Tables.EdgeTable[edgeIndex]&  16)){vertexInterp(4,5,ref normals[ 4],ref vertices[ 4],ref materials[ 4],vertices[ 4]!=Vector3.zero);}
////    if(0!=(Tables.EdgeTable[edgeIndex]&  32)){vertexInterp(5,6,ref normals[ 5],ref vertices[ 5],ref materials[ 5],vertices[ 5]!=Vector3.zero);}
////    if(0!=(Tables.EdgeTable[edgeIndex]&  64)){vertexInterp(6,7,ref normals[ 6],ref vertices[ 6],ref materials[ 6],vertices[ 6]!=Vector3.zero);}
////    if(0!=(Tables.EdgeTable[edgeIndex]& 128)){vertexInterp(7,4,ref normals[ 7],ref vertices[ 7],ref materials[ 7],vertices[ 7]!=Vector3.zero);}
////    if(0!=(Tables.EdgeTable[edgeIndex]& 256)){vertexInterp(0,4,ref normals[ 8],ref vertices[ 8],ref materials[ 8],vertices[ 8]!=Vector3.zero);}
////    if(0!=(Tables.EdgeTable[edgeIndex]& 512)){vertexInterp(1,5,ref normals[ 9],ref vertices[ 9],ref materials[ 9],vertices[ 9]!=Vector3.zero);}
////    if(0!=(Tables.EdgeTable[edgeIndex]&1024)){vertexInterp(2,6,ref normals[10],ref vertices[10],ref materials[10],vertices[10]!=Vector3.zero);}
////    if(0!=(Tables.EdgeTable[edgeIndex]&2048)){vertexInterp(3,7,ref normals[11],ref vertices[11],ref materials[11],vertices[11]!=Vector3.zero);}
////#region vertexInterp
////    void vertexInterp(int c0,int c1,ref Vector3 n,ref Vector3 p,ref MaterialId m,bool skipToNormal){double[]density=new double[2];Vector3[]vertex=new Vector3[2];float[]distance=new float[2];float marchingUnit;Vector3[]normal=new Vector3[2];MaterialId[]material=new MaterialId[2];
////        density[0]=polygonCell[c0];
////        density[1]=polygonCell[c1];
////        vertex[0]=PolygonCell.Corners[c0];
////        vertex[1]=PolygonCell.Corners[c1];
////if(skipToNormal){goto _Normal;}
////if(Math.Abs(_IsoLevel-density[0])<_Epsilon){p=vertex[0];goto _Normal;}
////if(Math.Abs(_IsoLevel-density[1])<_Epsilon){p=vertex[1];goto _Normal;}
////if(Math.Abs(density[0]-density[1])<_Epsilon){p=vertex[0];goto _Normal;}
////        marchingUnit=(float)((_IsoLevel-density[0])/(density[1]-density[0]));
////        p.x=vertex[0].x+marchingUnit*(vertex[1].x-vertex[0].x);
////        p.y=vertex[0].y+marchingUnit*(vertex[1].y-vertex[0].y);
////        p.z=vertex[0].z+marchingUnit*(vertex[1].z-vertex[0].z);
////_Normal:{
////        distance[0]=Vector3.Distance(vertex[0],vertex[1]);
////        distance[1]=Vector3.Distance(vertex[1],p);
////        n=Vector3.Lerp(
////            normal[1]=polygonCell.Voxels[c1].Normal,
////            normal[0]=polygonCell.Voxels[c0].Normal,distance[1]/distance[0]);
////        n=n!=Vector3.zero?n.normalized:(
////            normal[0]!=Vector3.zero?normal[0]:
////            normal[1]!=Vector3.zero?normal[1]: 
////            Vector3.down);
////}
////goto _UV;
////_UV:{
////        material[0]=polygonCell.Voxels[c0].Material;
////        material[1]=polygonCell.Voxels[c1].Material;
////        m=material[0];
////        if      (density[1]< density[0]){
////        m=material[1];
////        }else if(density[1]==density[0]&&(int)material[1]>(int)material[0]){
////        m=material[1];
////        }
////}
////    }
////#endregion
////    //  Buffer the data
////    _verticesBuffer[0][0][0]=vertices[ 4]+Vector3.back;//  Adiciona um valor negativo porque o voxelCoord próximo vai usar esse valor mais o de sua posição (próprio voxelCoord novo)
////    _verticesBuffer[0][0][1]=vertices[ 5]+Vector3.back;
////    _verticesBuffer[0][0][2]=vertices[ 6]+Vector3.back;
////    _verticesBuffer[0][0][3]=vertices[ 7]+Vector3.back;
////    //
////    _verticesBuffer[1][vCoord1.z][0]=vertices[ 1]+Vector3.left;
////    _verticesBuffer[1][vCoord1.z][1]=vertices[ 5]+Vector3.left;
////    _verticesBuffer[1][vCoord1.z][2]=vertices[ 9]+Vector3.left;
////    _verticesBuffer[1][vCoord1.z][3]=vertices[10]+Vector3.left;
////    //
////    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][0]=vertices[ 2]+Vector3.down;
////    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][1]=vertices[ 6]+Vector3.down;
////    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][2]=vertices[10]+Vector3.down;
////    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][3]=vertices[11]+Vector3.down;
////    /*  Create the triangle  */
////    for(int i=0;Tables.TriangleTable[edgeIndex][i]!=-1;i+=3){
////#region Indices
////        TempTriangles.Add((ushort)(vertexCount+2));
////        TempTriangles.Add((ushort)(vertexCount+1));
////        TempTriangles.Add(         vertexCount  );
////#endregion
////#region Vertices
////int[]tIdx=new int[3];
////Vector3 tPos=vCoord1-TrianglePosAdj;tPos.x+=posOffset.x;
////                                    tPos.z+=posOffset.y;
////Vector3[]verPos=new Vector3[3]{tPos+vertices[tIdx[0]=Tables.TriangleTable[edgeIndex][i  ]],
////                               tPos+vertices[tIdx[1]=Tables.TriangleTable[edgeIndex][i+1]],
////                               tPos+vertices[tIdx[2]=Tables.TriangleTable[edgeIndex][i+2]]};
////#region Normals
////Vector3 faceNorm=Vector3.zero;
////Vector3[]norm=new Vector3[3]{normals[tIdx[0]]!=Vector3.zero?normals[tIdx[0]]:(                                (faceNorm=Vector3.Cross(verPos[1]-verPos[0],verPos[2]-verPos[0]).normalized)),
////                             normals[tIdx[1]]!=Vector3.zero?normals[tIdx[1]]:(faceNorm!=Vector3.zero?faceNorm:(faceNorm=Vector3.Cross(verPos[1]-verPos[0],verPos[2]-verPos[0]).normalized)),
////                             normals[tIdx[2]]!=Vector3.zero?normals[tIdx[2]]:(faceNorm!=Vector3.zero?faceNorm:(         Vector3.Cross(verPos[1]-verPos[0],verPos[2]-verPos[0]).normalized))};
////        if(norm[0]==Vector3.zero){norm[0]=norm[1]!=Vector3.zero?norm[1]:norm[2];}
////        if(norm[1]==Vector3.zero){norm[1]=norm[0]!=Vector3.zero?norm[0]:norm[2];}
////        if(norm[2]==Vector3.zero){norm[2]=norm[0]!=Vector3.zero?norm[0]:norm[1];}
////#endregion
////#region Material
////        //  TO DO
////#endregion
////        TempVer.Add(new Vertex(verPos[0],-norm[0]));
////        TempVer.Add(new Vertex(verPos[1],-norm[1]));
////        TempVer.Add(new Vertex(verPos[2],-norm[2]));
////#endregion
////        vertexCount+=3;
////    }
////}
////#endregion


////            //Thread.Sleep(3000);


////                hasBuildData=true;backgroundDataSet.Set();


////for(int i=1;i<9;++i){voxels[i]=null;}
////polygonCell=null;
////_voxelsBuffer1=null;
////_voxelsBuffer2=null;
////_tempVoxels=null;
////_noiseCache1=null;
////_verticesBuffer=null;
////materials=null;
////vertices=null;
////normals=null;
////lock(ChunkManager.GC_Syn){
//////GCSettings.LargeObjectHeapCompactionMode=GCLargeObjectHeapCompactionMode.CompactOnce;
//////GC.Collect(GC.MaxGeneration,GCCollectionMode.Forced,true,true);
//////GC.WaitForPendingFinalizers();
////}
////lock(TasksBusyCount_Syn){TasksBusyCount--;}queue.Set();
////            }
////            int index(Vector2Int offset){
////                if(offset.x== 0&&offset.y== 0)return 0;
////                if(offset.x==-1&&offset.y== 0)return 1;
////                if(offset.x== 1&&offset.y== 0)return 2;
////                if(offset.x== 0&&offset.y==-1)return 3;
////                if(offset.x==-1&&offset.y==-1)return 4;
////                if(offset.x== 1&&offset.y==-1)return 5;
////                if(offset.x== 0&&offset.y== 1)return 6;
////                if(offset.x==-1&&offset.y== 1)return 7;
////                if(offset.x== 1&&offset.y== 1)return 8;
////            return -1;}
////            _Stop:{
////                TempVer.Dispose();TempTriangles.Dispose();
////if(LOG&&LOG_LEVEL<=2)Debug.Log("end");
////            }
////        }
////    }catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}}
////    public class PolygonCell{
////        public readonly Voxel[]Voxels=new Voxel[8];
////        public double this[int i]{
////            get{switch(i){
////                    case(0):{return-Voxels[0].Density;}
////                    case(1):{return-Voxels[1].Density;}
////                    case(2):{return-Voxels[2].Density;}
////                    case(3):{return-Voxels[3].Density;}
////                    case(4):{return-Voxels[4].Density;}
////                    case(5):{return-Voxels[5].Density;}
////                    case(6):{return-Voxels[6].Density;}
////                    case(7):{return-Voxels[7].Density;}
////                    default:{return 0;}
////            }}
////        }
////public static readonly ReadOnlyCollection<Vector3>Corners=new ReadOnlyCollection<Vector3>(new Vector3[8]{
////    new Vector3(-.5f,-.5f,-.5f),
////    new Vector3( .5f,-.5f,-.5f),
////    new Vector3( .5f, .5f,-.5f),
////    new Vector3(-.5f, .5f,-.5f),
////    new Vector3(-.5f,-.5f, .5f),
////    new Vector3( .5f,-.5f, .5f),
////    new Vector3( .5f, .5f, .5f),
////    new Vector3(-.5f, .5f, .5f),
////});
////    }
////    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
////    public struct Voxel{
////           public Voxel(double d,Vector3 n,MaterialId m){
////            Density=d;Normal=n;Material=m;IsCreated=true;
////           }
////public double Density;public Vector3 Normal;public MaterialId Material;public bool IsCreated;
////public static Voxel Air{get;}=new Voxel(0.0,Vector3.up,MaterialId.Air);
////public static Voxel Bedrock{get;}=new Voxel(101.0,Vector3.up,MaterialId.Bedrock);
////    }
//////    protected override void BG(object state){try{
//////        if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL&&parameters[2]is float time&&parameters[3]is NativeList<Vertex>TempVer&&parameters[4]is NativeList<ushort>TempTriangles){
//////            if(parameters[5]is Vector2Int cCoord1&&parameters[6]is Vector2Int cnkRgn1){
//////Voxel[][]voxels=new Voxel[9][];for(int i=0;i<9;++i){voxels[i]=new Voxel[VoxelsPerChunk];}
//////#region SetPolygonCell Variables
//////PolygonCell polygonCell=new PolygonCell();
//////Voxel[][][]_voxelsBuffer1=new Voxel[3][][]{new Voxel[1][]{new Voxel[4],},new Voxel[Depth][],new Voxel[FlattenOffset][],};for(int i=0;i<_voxelsBuffer1[2].Length;++i){_voxelsBuffer1[2][i]=new Voxel[4];if(i<_voxelsBuffer1[1].Length){_voxelsBuffer1[1][i]=new Voxel[4];}}
//////  Voxel[][]_voxelsBuffer2=new Voxel[3][]{new Voxel[1],new Voxel[Depth],new Voxel[FlattenOffset],};
//////Voxel[]_tempVoxels=new Voxel[6];
//////Vector3 noiseInput;double[][]_noiseCache1=new double[9][];//  Calcular a altura uma vez e usar este valor para este x e este z, onde, se abaixo da altura, sempre será com densidade, e se acima, não terá densidade (por isso não precisa calcular toda vez que usar este mesmo x e mesmo z, ao trocar de y).
//////#endregion
//////#region Polygoniser Variables
//////Vector3[][][]_verticesBuffer=new Vector3[3][][]{new Vector3[1][]{new Vector3[4],},new Vector3[Depth][],new Vector3[FlattenOffset][],};for(int i=0;i<_verticesBuffer[2].Length;++i){_verticesBuffer[2][i]=new Vector3[4];if(i<_verticesBuffer[1].Length){_verticesBuffer[1][i]=new Vector3[4];}}
//////MaterialId[]materials=new MaterialId[12];
//////   Vector3[] vertices=new Vector3[12];
//////   Vector3[]  normals=new Vector3[12];
//////ushort vertexCount=0;
//////#endregion
//////Vector3Int vCoord1,vCoord2,vCoord3;
//////int corner;
//////Vector2Int cnkRgn2,cnkRgn3;
//////Vector2Int cCoord3;
//////int vxlIdx3;
//////Vector2Int preOffset=Vector2Int.zero;
//////Vector2Int posOffset=Vector2Int.zero;
//////for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
//////for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
//////for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
//////SetPolygonCell();Polygonise();
//////}}}
//////#region SetPolygonCell
//////void SetPolygonCell(){
//////    corner=0;
//////    _Cycle:{
//////        cnkRgn2=cnkRgn1;
//////        vCoord2=vCoord1;
//////        switch(corner){
//////            case(0):{goto _0;}
//////            case(1):{goto _1;}
//////            case(2):{goto _2;}
//////            case(3):{goto _3;}
//////            case(4):{goto _4;}
//////            case(5):{goto _5;}
//////            case(6):{goto _6;}
//////            case(7):{goto _7;}
//////            default:{goto _End;}
//////        }
//////    }
//////    _0:{
//////              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][0];
//////        }else if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][0];
//////        }else if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][0];
//////        }else{ 
//////GetVoxel();}
//////        goto _Next;
//////    }
//////    _1:{
//////              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][1];
//////        }else if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][1];
//////        }else{
//////        vCoord2.x+=1;
//////GetVoxel();}
//////        goto _Next;
//////    }
//////    _2:{
//////              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][2];
//////        }else{
//////        vCoord2.x+=1;
//////        vCoord2.y+=1;
//////GetVoxel();}
//////        goto _Next;
//////    }
//////    _3:{
//////              if(vCoord1.z>0){polygonCell.Voxels[corner]=_voxelsBuffer1[0][0][3];
//////        }else if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][1];
//////        }else{
//////        vCoord2.y+=1;
//////GetVoxel();}
//////        goto _Next;
//////    }
//////    _4:{
//////              if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][2];
//////        }else if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][2];
//////        }else{
//////        vCoord2.z+=1;
//////GetVoxel();}
//////        goto _Next;
//////    }
//////    _5:{
//////              if(vCoord1.y>0){polygonCell.Voxels[corner]=_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][3];
//////        }else{
//////        vCoord2.x+=1;
//////        vCoord2.z+=1;
//////GetVoxel();}
//////        goto _Next;
//////    }
//////    _6:{
//////        vCoord2.x+=1;
//////        vCoord2.y+=1;
//////        vCoord2.z+=1;
//////GetVoxel();
//////        goto _Next;
//////    }
//////    _7:{
//////              if(vCoord1.x>0){polygonCell.Voxels[corner]=_voxelsBuffer1[1][vCoord1.z][3];
//////        }else{
//////        vCoord2.y+=1;
//////        vCoord2.z+=1;
//////GetVoxel();}
//////        goto _Next;
//////    }
//////    _Next:{
//////        ++corner;
//////        goto _Cycle;
//////    }
//////    _End:{
//////_voxelsBuffer1[0][0][0]=polygonCell.Voxels[4];
//////_voxelsBuffer1[0][0][1]=polygonCell.Voxels[5];
//////_voxelsBuffer1[0][0][2]=polygonCell.Voxels[6];
//////_voxelsBuffer1[0][0][3]=polygonCell.Voxels[7];
//////_voxelsBuffer1[1][vCoord1.z][0]=polygonCell.Voxels[1];
//////_voxelsBuffer1[1][vCoord1.z][1]=polygonCell.Voxels[2];
//////_voxelsBuffer1[1][vCoord1.z][2]=polygonCell.Voxels[5];
//////_voxelsBuffer1[1][vCoord1.z][3]=polygonCell.Voxels[6];
//////_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][0]=polygonCell.Voxels[3];
//////_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][1]=polygonCell.Voxels[2];
//////_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][2]=polygonCell.Voxels[7];
//////_voxelsBuffer1[2][vCoord1.z+vCoord1.x*Width][3]=polygonCell.Voxels[6];
//////    }
//////#region GetVoxel
//////    void GetVoxel(){
//////        if(vCoord2.y<0){
//////            polygonCell.Voxels[corner]=Voxel.Bedrock;
//////        }else if(vCoord2.y>=Height){
//////            polygonCell.Voxels[corner]=Voxel.Air;
//////        }else{
//////            if(vCoord2.x<0||vCoord2.x>=Width||
//////               vCoord2.z<0||vCoord2.z>=Depth){ 
//////ValidateCoord(ref cnkRgn2,ref vCoord2);
//////            }
//////            ComputeDensityNormal();
//////        }
//////    }
//////#endregion
//////}
//////#region ComputeDensityNormal
//////void ComputeDensityNormal(){
//////    int step=-1;
//////    _Cycle:{
//////        cCoord3=cCoord1;
//////        cnkRgn3=cnkRgn2;
//////        vCoord3=vCoord2;
//////        switch(step){
//////            case(-1):{goto _neg1;}
//////            case(0):{goto _0;}
//////            case(1):{goto _1;}
//////            case(2):{goto _2;}
//////            case(3):{goto _3;}
//////            case(4):{goto _4;}
//////            case(5):{goto _5;}
//////            default:{goto _End;}
//////        }
//////    }
//////    _neg1:{
//////SetVoxel(ref polygonCell.Voxels[corner]);if(polygonCell.Voxels[corner].Normal!=Vector3.zero){goto _Skip;}
//////        goto _Next;
//////    }
//////    _0:{
//////        vCoord3.x++;
//////SetVoxel(ref _tempVoxels[step]);
//////        goto _Next;
//////    }
//////    _1:{
//////        if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&(_tempVoxels[step]=_voxelsBuffer2[1][vCoord2.z]).IsCreated){
//////        }else{
//////        vCoord3.x--;
//////SetVoxel(ref _tempVoxels[step]);}
//////        goto _Next;
//////    }
//////    _2:{
//////        vCoord3.y++;
//////SetVoxel(ref _tempVoxels[step]);
//////        goto _Next;
//////    }
//////    _3:{
//////        if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&(_tempVoxels[step]=_voxelsBuffer2[2][vCoord2.z+vCoord2.x*Width]).IsCreated){
//////        }else{
//////        vCoord3.y--;
//////SetVoxel(ref _tempVoxels[step]);}
//////        goto _Next;
//////    }
//////    _4:{
//////        vCoord3.z++;
//////SetVoxel(ref _tempVoxels[step]);
//////        goto _Next;
//////    }
//////    _5:{
//////        if(vCoord2.z>1&&vCoord2.x>1&&vCoord2.y>1&&(_tempVoxels[step]=_voxelsBuffer2[0][0]).IsCreated){
//////        }else{
//////        vCoord3.z--;
//////SetVoxel(ref _tempVoxels[step]);}
//////        goto _Next;
//////    }
//////    _Next:{
//////        ++step;
//////        goto _Cycle;
//////    }
//////    _End:{
//////        polygonCell.Voxels[corner].Normal.x=(float)(_tempVoxels[0].Density-_tempVoxels[1].Density);
//////        polygonCell.Voxels[corner].Normal.y=(float)(_tempVoxels[2].Density-_tempVoxels[3].Density);
//////        polygonCell.Voxels[corner].Normal.z=(float)(_tempVoxels[4].Density-_tempVoxels[5].Density);
//////        if(polygonCell.Voxels[corner].Normal!=Vector3.zero){
//////            polygonCell.Voxels[corner].Normal.Normalize();
//////        }
//////    }
//////    _Skip:{}
//////_voxelsBuffer2[0][0]=polygonCell.Voxels[corner];
//////_voxelsBuffer2[1][vCoord2.z]=polygonCell.Voxels[corner];
//////_voxelsBuffer2[2][vCoord2.z+vCoord2.x*Width]=polygonCell.Voxels[corner];
//////#region SetVoxel
//////    void SetVoxel(ref Voxel v){
//////        if(vCoord3.y<0){
//////            v=Voxel.Bedrock;
//////        }else if(vCoord3.y>=Height){
//////            v=Voxel.Air;
//////        }else{
//////            if(vCoord3.x<0||vCoord3.x>=Width||
//////               vCoord3.z<0||vCoord3.z>=Depth){ 
//////ValidateCoord(ref cnkRgn3,ref vCoord3);cCoord3=ChunkManager.RgnToCoord(cnkRgn3);
//////            }
//////            vxlIdx3=GetIdx(vCoord3.x,vCoord3.y,vCoord3.z);
//////            Voxel[]vxls;
//////            int idx;
//////            if(cCoord3==cCoord1){
//////                vxls=voxels[idx=index(preOffset)];
//////            }else{
//////                vxls=voxels[idx=index(preOffset+(cCoord3-cCoord1))];
//////            }
//////            if(vxls[vxlIdx3].IsCreated)v=vxls[vxlIdx3];else{
//////                noiseInput=vCoord3;noiseInput.x+=cnkRgn3.x;
//////                                   noiseInput.z+=cnkRgn3.y;




//////        if(noiseInput.y<1){
//////            v=Voxel.Bedrock;
//////    return;
//////        }




//////            }
//////        }
//////    }
//////#endregion
//////}
//////#endregion
//////#endregion
//////#region Polygoniser
//////void Polygonise(){
//////int edgeIndex;
//////    /*
//////        Determine the index into the edge table which
//////        tells us which vertices are inside of the surface
//////    */
//////                                edgeIndex =  0;
//////    if(polygonCell[0]<_IsoLevel)edgeIndex|=  1;
//////    if(polygonCell[1]<_IsoLevel)edgeIndex|=  2;
//////    if(polygonCell[2]<_IsoLevel)edgeIndex|=  4;
//////    if(polygonCell[3]<_IsoLevel)edgeIndex|=  8;
//////    if(polygonCell[4]<_IsoLevel)edgeIndex|= 16;
//////    if(polygonCell[5]<_IsoLevel)edgeIndex|= 32;
//////    if(polygonCell[6]<_IsoLevel)edgeIndex|= 64;
//////    if(polygonCell[7]<_IsoLevel)edgeIndex|=128;
//////    if(Tables.EdgeTable[edgeIndex]==0){/*  Cube is entirely in/out of the surface  */
//////        return;
//////    }
//////    //  Use buffered data if available
//////    vertices[ 0]=(vCoord1.z>0?_verticesBuffer[0][0][0]:(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][0]:Vector3.zero));
//////    vertices[ 1]=(vCoord1.z>0?_verticesBuffer[0][0][1]:Vector3.zero);
//////    vertices[ 2]=(vCoord1.z>0?_verticesBuffer[0][0][2]:Vector3.zero);
//////    vertices[ 3]=(vCoord1.z>0?_verticesBuffer[0][0][3]:(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][0]:Vector3.zero));
//////    vertices[ 4]=(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][1]:Vector3.zero);
//////    vertices[ 5]=(Vector3.zero);
//////    vertices[ 6]=(Vector3.zero);
//////    vertices[ 7]=(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][1]:Vector3.zero);
//////    vertices[ 8]=(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][2]:(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][3]:Vector3.zero));
//////    vertices[ 9]=(vCoord1.y>0?_verticesBuffer[2][vCoord1.z+vCoord1.x*Width][2]:Vector3.zero);
//////    vertices[10]=(Vector3.zero);
//////    vertices[11]=(vCoord1.x>0?_verticesBuffer[1][vCoord1.z][3]:Vector3.zero);
//////    //  Compute vertices
//////    if(0!=(Tables.EdgeTable[edgeIndex]&   1)){vertexInterp(0,1,ref normals[ 0],ref vertices[ 0],ref materials[ 0],vertices[ 0]!=Vector3.zero);}
//////    if(0!=(Tables.EdgeTable[edgeIndex]&   2)){vertexInterp(1,2,ref normals[ 1],ref vertices[ 1],ref materials[ 1],vertices[ 1]!=Vector3.zero);}
//////    if(0!=(Tables.EdgeTable[edgeIndex]&   4)){vertexInterp(2,3,ref normals[ 2],ref vertices[ 2],ref materials[ 2],vertices[ 2]!=Vector3.zero);}
//////    if(0!=(Tables.EdgeTable[edgeIndex]&   8)){vertexInterp(3,0,ref normals[ 3],ref vertices[ 3],ref materials[ 3],vertices[ 3]!=Vector3.zero);}
//////    if(0!=(Tables.EdgeTable[edgeIndex]&  16)){vertexInterp(4,5,ref normals[ 4],ref vertices[ 4],ref materials[ 4],vertices[ 4]!=Vector3.zero);}
//////    if(0!=(Tables.EdgeTable[edgeIndex]&  32)){vertexInterp(5,6,ref normals[ 5],ref vertices[ 5],ref materials[ 5],vertices[ 5]!=Vector3.zero);}
//////    if(0!=(Tables.EdgeTable[edgeIndex]&  64)){vertexInterp(6,7,ref normals[ 6],ref vertices[ 6],ref materials[ 6],vertices[ 6]!=Vector3.zero);}
//////    if(0!=(Tables.EdgeTable[edgeIndex]& 128)){vertexInterp(7,4,ref normals[ 7],ref vertices[ 7],ref materials[ 7],vertices[ 7]!=Vector3.zero);}
//////    if(0!=(Tables.EdgeTable[edgeIndex]& 256)){vertexInterp(0,4,ref normals[ 8],ref vertices[ 8],ref materials[ 8],vertices[ 8]!=Vector3.zero);}
//////    if(0!=(Tables.EdgeTable[edgeIndex]& 512)){vertexInterp(1,5,ref normals[ 9],ref vertices[ 9],ref materials[ 9],vertices[ 9]!=Vector3.zero);}
//////    if(0!=(Tables.EdgeTable[edgeIndex]&1024)){vertexInterp(2,6,ref normals[10],ref vertices[10],ref materials[10],vertices[10]!=Vector3.zero);}
//////    if(0!=(Tables.EdgeTable[edgeIndex]&2048)){vertexInterp(3,7,ref normals[11],ref vertices[11],ref materials[11],vertices[11]!=Vector3.zero);}
//////#region vertexInterp
//////    void vertexInterp(int c0,int c1,ref Vector3 n,ref Vector3 p,ref MaterialId m,bool skipToNormal){double[]density=new double[2];Vector3[]vertex=new Vector3[2];float[]distance=new float[2];float marchingUnit;Vector3[]normal=new Vector3[2];MaterialId[]material=new MaterialId[2];
//////        density[0]=polygonCell[c0];
//////        density[1]=polygonCell[c1];
//////        vertex[0]=PolygonCell.Corners[c0];
//////        vertex[1]=PolygonCell.Corners[c1];
//////if(skipToNormal){goto _Normal;}
//////if(Math.Abs(_IsoLevel-density[0])<_Epsilon){p=vertex[0];goto _Normal;}
//////if(Math.Abs(_IsoLevel-density[1])<_Epsilon){p=vertex[1];goto _Normal;}
//////if(Math.Abs(density[0]-density[1])<_Epsilon){p=vertex[0];goto _Normal;}
//////        marchingUnit=(float)((_IsoLevel-density[0])/(density[1]-density[0]));
//////        p.x=vertex[0].x+marchingUnit*(vertex[1].x-vertex[0].x);
//////        p.y=vertex[0].y+marchingUnit*(vertex[1].y-vertex[0].y);
//////        p.z=vertex[0].z+marchingUnit*(vertex[1].z-vertex[0].z);
//////_Normal:{
//////        distance[0]=Vector3.Distance(vertex[0],vertex[1]);
//////        distance[1]=Vector3.Distance(vertex[1],p);
//////        n=Vector3.Lerp(
//////            normal[1]=polygonCell.Voxels[c1].Normal,
//////            normal[0]=polygonCell.Voxels[c0].Normal,distance[1]/distance[0]);
//////        n=n!=Vector3.zero?n.normalized:(
//////            normal[0]!=Vector3.zero?normal[0]:
//////            normal[1]!=Vector3.zero?normal[1]: 
//////            Vector3.down);
//////}
//////goto _UV;
//////_UV:{
//////        material[0]=polygonCell.Voxels[c0].Material;
//////        material[1]=polygonCell.Voxels[c1].Material;
//////        m=material[0];
//////        if      (density[1]< density[0]){
//////        m=material[1];
//////        }else if(density[1]==density[0]&&(int)material[1]>(int)material[0]){
//////        m=material[1];
//////        }
//////}
//////    }
//////#endregion
//////    //  Buffer the data
//////    _verticesBuffer[0][0][0]=vertices[ 4]+Vector3.back;//  Adiciona um valor negativo porque o voxelCoord próximo vai usar esse valor mais o de sua posição (próprio voxelCoord novo)
//////    _verticesBuffer[0][0][1]=vertices[ 5]+Vector3.back;
//////    _verticesBuffer[0][0][2]=vertices[ 6]+Vector3.back;
//////    _verticesBuffer[0][0][3]=vertices[ 7]+Vector3.back;
//////    //
//////    _verticesBuffer[1][vCoord1.z][0]=vertices[ 1]+Vector3.left;
//////    _verticesBuffer[1][vCoord1.z][1]=vertices[ 5]+Vector3.left;
//////    _verticesBuffer[1][vCoord1.z][2]=vertices[ 9]+Vector3.left;
//////    _verticesBuffer[1][vCoord1.z][3]=vertices[10]+Vector3.left;
//////    //
//////    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][0]=vertices[ 2]+Vector3.down;
//////    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][1]=vertices[ 6]+Vector3.down;
//////    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][2]=vertices[10]+Vector3.down;
//////    _verticesBuffer[2][vCoord1.z+vCoord1.x*Width][3]=vertices[11]+Vector3.down;
//////    /*  Create the triangle  */
//////    for(int i=0;Tables.TriangleTable[edgeIndex][i]!=-1;i+=3){
//////#region Indices
//////        TempTriangles.Add((ushort)(vertexCount+2));
//////        TempTriangles.Add((ushort)(vertexCount+1));
//////        TempTriangles.Add(         vertexCount  );
//////#endregion
//////#region Vertices
//////int[]tIdx=new int[3];
//////Vector3 tPos=vCoord1-TrianglePosAdj;tPos.x+=posOffset.x;
//////                                    tPos.z+=posOffset.y;
//////Vector3[]verPos=new Vector3[3]{tPos+vertices[tIdx[0]=Tables.TriangleTable[edgeIndex][i  ]],
//////                               tPos+vertices[tIdx[1]=Tables.TriangleTable[edgeIndex][i+1]],
//////                               tPos+vertices[tIdx[2]=Tables.TriangleTable[edgeIndex][i+2]]};
//////#region Normals
//////Vector3 faceNorm=Vector3.zero;
//////Vector3[]norm=new Vector3[3]{normals[tIdx[0]]!=Vector3.zero?normals[tIdx[0]]:(                                (faceNorm=Vector3.Cross(verPos[1]-verPos[0],verPos[2]-verPos[0]).normalized)),
//////                             normals[tIdx[1]]!=Vector3.zero?normals[tIdx[1]]:(faceNorm!=Vector3.zero?faceNorm:(faceNorm=Vector3.Cross(verPos[1]-verPos[0],verPos[2]-verPos[0]).normalized)),
//////                             normals[tIdx[2]]!=Vector3.zero?normals[tIdx[2]]:(faceNorm!=Vector3.zero?faceNorm:(         Vector3.Cross(verPos[1]-verPos[0],verPos[2]-verPos[0]).normalized))};
//////        if(norm[0]==Vector3.zero){norm[0]=norm[1]!=Vector3.zero?norm[1]:norm[2];}
//////        if(norm[1]==Vector3.zero){norm[1]=norm[0]!=Vector3.zero?norm[0]:norm[2];}
//////        if(norm[2]==Vector3.zero){norm[2]=norm[0]!=Vector3.zero?norm[0]:norm[1];}
//////#endregion
//////#region Material
//////        //  TO DO
//////#endregion
//////        TempVer.Add(new Vertex(verPos[0],-norm[0]));
//////        TempVer.Add(new Vertex(verPos[1],-norm[1]));
//////        TempVer.Add(new Vertex(verPos[2],-norm[2]));
//////#endregion
//////        vertexCount+=3;
//////    }
//////}
//////#endregion
//////if(LOG&&LOG_LEVEL<=2)Debug.Log("end");
//////                buildData.Enqueue(new object[]{time,TempVer,TempTriangles});
//////            }
//////            int index(Vector2Int offset){
//////                if(offset.x== 0&&offset.y== 0)return 0;
//////                if(offset.x==-1&&offset.y== 0)return 1;
//////                if(offset.x== 1&&offset.y== 0)return 2;
//////                if(offset.x== 0&&offset.y==-1)return 3;
//////                if(offset.x==-1&&offset.y==-1)return 4;
//////                if(offset.x== 1&&offset.y==-1)return 5;
//////                if(offset.x== 0&&offset.y== 1)return 6;
//////                if(offset.x==-1&&offset.y== 1)return 7;
//////                if(offset.x== 1&&offset.y== 1)return 8;
//////            return -1;}
//////        }
//////    }catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}}
//////    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
//////    public struct Voxel{
//////           public Voxel(double d,Vector3 n,MaterialId m){
//////            Density=d;Normal=n;Material=m;IsCreated=true;
//////           }
//////public double Density;public Vector3 Normal;public MaterialId Material;public bool IsCreated;
//////public static Voxel Air{get;}=new Voxel(0.0,Vector3.up,MaterialId.Air);
//////public static Voxel Bedrock{get;}=new Voxel(101.0,Vector3.up,MaterialId.Bedrock);
//////    }
//////    public class PolygonCell{
//////        public readonly Voxel[]Voxels=new Voxel[8];
//////        public double this[int i]{
//////            get{switch(i){
//////                    case(0):{return-Voxels[0].Density;}
//////                    case(1):{return-Voxels[1].Density;}
//////                    case(2):{return-Voxels[2].Density;}
//////                    case(3):{return-Voxels[3].Density;}
//////                    case(4):{return-Voxels[4].Density;}
//////                    case(5):{return-Voxels[5].Density;}
//////                    case(6):{return-Voxels[6].Density;}
//////                    case(7):{return-Voxels[7].Density;}
//////                    default:{return 0;}
//////            }}
//////        }
//////public static readonly ReadOnlyCollection<Vector3>Corners=new ReadOnlyCollection<Vector3>(new Vector3[8]{
//////    new Vector3(-.5f,-.5f,-.5f),
//////    new Vector3( .5f,-.5f,-.5f),
//////    new Vector3( .5f, .5f,-.5f),
//////    new Vector3(-.5f, .5f,-.5f),
//////    new Vector3(-.5f,-.5f, .5f),
//////    new Vector3( .5f,-.5f, .5f),
//////    new Vector3( .5f, .5f, .5f),
//////    new Vector3(-.5f, .5f, .5f),
//////});
//////    }
////[NonSerialized]public static readonly object TasksBusyCount_Syn=new object();[NonSerialized]public static int TasksBusyCount=0;[NonSerialized]static readonly AutoResetEvent queue=new AutoResetEvent(true);
////public const double _Epsilon=0.00001;
////public const double _IsoLevel=-50.0d;
////public static Vector3 TrianglePosAdj{get;}=new Vector3((Width/2.0f)-0.5f,(Height/2.0f)-0.5f,(Depth/2.0f)-0.5f);//  Ajuste para que o mesh do chunk fique centralizado, com pivot em 0,0,0
}}