using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using static MemoryManagement;
namespace AKCondinoO.Voxels{public class ChunkManager:MonoBehaviour{ 
public bool LOG=false;public int LOG_LEVEL=1;
[NonSerialized]public static readonly string saveFolder=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("\\","/").ToString()+"/Solitude/";
        [NonSerialized]public static string[]saveSubfolder=new string[1];
public static string CurrWorldName{private set;get;}
public GameObject ChunkPrefab;
[NonSerialized]public Vector2Int expropriationDistance=new Vector2Int(5,5);[NonSerialized]public readonly LinkedList<Chunk>ChunksPool=new LinkedList<Chunk>();
[NonSerialized]public Vector2Int instantiationDistance=new Vector2Int(1,1);[NonSerialized]public readonly Dictionary<int,Chunk>Chunks=new Dictionary<int,Chunk>();
protected virtual void Awake(){
var maxChunks=(expropriationDistance.x*2+1)*(expropriationDistance.y*2+1);
try{
MainThread=Thread.CurrentThread;GarbageCollector.GCMode=GarbageCollector.Mode.Enabled;StartCoroutine(Unload());
AtlasHelper.GetAtlasData(ChunkPrefab.GetComponent<MeshRenderer>().sharedMaterial);
if(LOG&&LOG_LEVEL<=100)Debug.Log("The number of processors on this computer is "+Environment.ProcessorCount);
ThreadPool.GetAvailableThreads(out int worker ,out int io         );if(LOG&&LOG_LEVEL<=100)Debug.Log("Thread pool threads available at startup: Worker threads: "+worker+" Asynchronous I/O threads: "+io);
ThreadPool.GetMaxThreads(out int workerThreads,out int portThreads);if(LOG&&LOG_LEVEL<=100)Debug.Log("Maximum worker threads: "+workerThreads+" Maximum completion port threads: "+portThreads);
ThreadPool.GetMinThreads(out int minWorker    ,out int minIOC     );if(LOG&&LOG_LEVEL<=100)Debug.Log("minimum number of worker threads: "+minWorker+" minimum asynchronous I/O: "+minIOC);
var idealMin=(maxChunks+1+Environment.ProcessorCount);if(minWorker!=idealMin){
if(ThreadPool.SetMinThreads(idealMin,minIOC)){
if(LOG&&LOG_LEVEL<=100)Debug.Log("changed minimum number of worker threads to:"+(idealMin));
}else{
if(LOG&&LOG_LEVEL<=100)Debug.Log("SetMinThreads failed");
}
}
CurrWorldName="Terra Nova";
biome.LOG=LOG;biome.LOG_LEVEL=LOG_LEVEL;biome.Seed=0;
Directory.CreateDirectory(saveSubfolder[0]=saveFolder+CurrWorldName+"/Chunks/");saveSubfolder[0]+="c{0}.edits";
}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
#if UNITY_STANDALONE
//  Quit the application
Application.Quit();
#endif
#if UNITY_EDITOR
//  Stop playing the scene
UnityEditor.EditorApplication.isPlaying=false;
#endif
return;
}
    for(int i=maxChunks-1;i>=0;--i){
        GameObject obj=Instantiate(ChunkPrefab);Chunk scr=obj.GetComponent<Chunk>();ChunksPool.AddLast(scr);scr.ExpropriationNode=ChunksPool.Last;if(scr is TerrainChunk cnk){load_Syn.Add(cnk.load_Syn);}
    }
}
[NonSerialized]Task task;[NonSerialized]readonly AutoResetEvent foregroundDataSet=new AutoResetEvent(false);[NonSerialized]readonly ManualResetEvent backgroundDataSet=new ManualResetEvent(true);
protected virtual void OnEnable(){
Stop=false;task=Task.Factory.StartNew(BG,new object[]{LOG,LOG_LEVEL,new System.Random(),saveSubfolder},TaskCreationOptions.LongRunning);




    Build();//Build();




}
bool Stop{
    get{bool tmp;lock(Stop_Syn){tmp=Stop_v;      }return tmp;}
    set{         lock(Stop_Syn){    Stop_v=value;}if(value){foregroundDataSet.Set();}}
}[NonSerialized]readonly object Stop_Syn=new object();[NonSerialized]bool Stop_v;
protected virtual void OnDisable(){
Stop=true;try{task.Wait();}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
}
protected virtual void Update(){
    if(backgroundDataSet.WaitOne(0)){
        if(editedDirty.Count>0){for(int i=0;i<editedDirty.Count;i++){
            if(Chunks.ContainsKey(editedDirty[i])){
                Chunk cnk;(cnk=Chunks[editedDirty[i]]).needsRebuild=true;
for(int x=-1;x<=1;x++){
for(int z=-1;z<=1;z++){


Vector2Int nCoord1=cnk.Coord;nCoord1.x+=x;nCoord1.y+=z;int ngbIdx1=GetIdx(nCoord1.x,nCoord1.y);
                if(Chunks.ContainsKey(ngbIdx1)){
                    Chunks[ngbIdx1].needsRebuild=true;
                }

}}
            }
        }editedDirty.Clear();}
    }




        if(DEBUG_EDIT){Edit(new Vector3(0,0,0),new Vector3Int(1,50,1));}




}
[NonSerialized]protected static Vector2Int aCoord;
void Build(){
for(Vector2Int coord=new Vector2Int(),cCoord1=new Vector2Int();coord.y<=instantiationDistance.y;coord.y++){for(cCoord1.y=-coord.y+aCoord.y;cCoord1.y<=coord.y+aCoord.y;cCoord1.y+=coord.y*2){
for(coord.x=0                                                 ;coord.x<=instantiationDistance.x;coord.x++){for(cCoord1.x=-coord.x+aCoord.x;cCoord1.x<=coord.x+aCoord.x;cCoord1.x+=coord.x*2){
if(LOG&&LOG_LEVEL<=1)Debug.Log("try build chunk:"+cCoord1);
if(Math.Abs(cCoord1.x)>=Width||
   Math.Abs(cCoord1.y)>=Depth){
if(LOG&&LOG_LEVEL<=2)Debug.Log("do not build out of world chunk");
goto _skip;
}
    int cnkIdx1=GetIdx(cCoord1.x,cCoord1.y);if(!Chunks.ContainsKey(cnkIdx1)){
if(LOG&&LOG_LEVEL<=1)Debug.Log("build chunk:"+cnkIdx1+"[ChunksPool.Count:"+ChunksPool.Count);
        Chunk scr=ChunksPool.First.Value;ChunksPool.RemoveFirst();scr.ExpropriationNode=(null);Chunks.Add(cnkIdx1,scr);scr.OnRebuildRequest(cCoord1,CoordToRgn(cCoord1),cnkIdx1);
    }else{
if(LOG&&LOG_LEVEL<=1)Debug.Log("chunk already built:"+cnkIdx1);
    }
_skip:{}
if(coord.x==0){break;}}}
if(coord.y==0){break;}}}
}
public void Edit(Vector3 center,Vector3Int size,double density=0){
    if(backgroundDataSet.WaitOne(0)){
Vector2Int cCoord1=PosToCoord(center);
Vector2Int cnkRgn1=CoordToRgn(cCoord1);
Vector3Int vCoord1=Chunk.PosToCoord(center);
if(LOG&&LOG_LEVEL<=2)Debug.Log("edit at:"+center+"; cCoord1:"+cCoord1+"; vCoord1:"+vCoord1);
var offset=new Vector3Int();
for(offset.x=-size.x;offset.x<=size.x;offset.x++){
for(offset.z=-size.z;offset.z<=size.z;offset.z++){
for(offset.y=-size.y;offset.y<=size.y;offset.y++){
Vector3Int vCoord2=vCoord1+offset;
if(vCoord2.y<0||vCoord2.y>=Chunk.Height){
continue;
}
Vector2Int cnkRgn2=cnkRgn1;Vector2Int cCoord2=cCoord1;
if(vCoord2.x<0||vCoord2.x>=Chunk.Width||
   vCoord2.z<0||vCoord2.z>=Chunk.Depth){
Chunk.ValidateCoord(ref cnkRgn2,ref vCoord2);cCoord2=RgnToCoord(cnkRgn2);
}
var cnkIdx2=GetIdx(cCoord2.x,cCoord2.y);if(Chunks.ContainsKey(cnkIdx2)){if((!(Chunks[cnkIdx2]is TerrainChunk cnk))||cnk.needsRebuild||!cnk.backgroundDataSet.WaitOne(0)){Cancel();goto _End;}}
    if(!edtVxlsByCnkIdx.ContainsKey(cnkIdx2)){edtVxlsByCnkIdx.Add(cnkIdx2,new Dictionary<Vector3Int,(double density,MaterialId material)>());}
    edtVxlsByCnkIdx[cnkIdx2].Add(vCoord2,(10,MaterialId.Air));
}}}
            backgroundDataSet.Reset();foregroundDataSet.Set();
_End:{}

        
        DEBUG_EDIT=false;


    }
    void Cancel(){
edtVxlsByCnkIdx.Clear();

        Debug.LogWarning("cancel");

    }
}
//[NonSerialized]public static readonly object load_Syn=new object();
[NonSerialized]readonly List<object>load_Syn=new List<object>();
[NonSerialized]readonly Dictionary<int,Dictionary<Vector3Int,(double density,MaterialId material)>>edtVxlsByCnkIdx=new Dictionary<int,Dictionary<Vector3Int,(double density,MaterialId material)>>();[NonSerialized]readonly List<int>editedDirty=new List<int>();
void BG(object state){Thread.CurrentThread.IsBackground=false;Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;try{
    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL&&parameters[2]is System.Random random&&parameters[3]is string[]saveSubfolder){
DataContractSerializer saveContract=new DataContractSerializer(typeof(Dictionary<Vector3Int,(double density,MaterialId material)>));
        while(!Stop){foregroundDataSet.WaitOne();if(Stop)goto _Stop;
            foreach(var syn in load_Syn)Monitor.Enter(syn);try{
foreach(var cnkIdxEdtsPair in edtVxlsByCnkIdx){
var fileName=string.Format(saveSubfolder[0],cnkIdxEdtsPair.Key);
if(LOG&&LOG_LEVEL<=1)Debug.Log("save edits at: "+fileName);
int saveTries=60;bool saved=false;while(!saved){
FileStream file=null;
try{
using(file=new FileStream(fileName,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None)){
if(file.Length>0){
if(LOG&&LOG_LEVEL<=1)Debug.Log("file has data, load it to merge with new save data:fileName:"+fileName);
if(saveContract.ReadObject(file)is Dictionary<Vector3Int,(double density,MaterialId material)>fileData){
foreach(var voxelData in fileData){
if(!cnkIdxEdtsPair.Value.ContainsKey(voxelData.Key)){
    cnkIdxEdtsPair.Value.Add(voxelData.Key,voxelData.Value);
}
}
}}
file.SetLength(0);
file.Flush(true);
saveContract.WriteObject(file,cnkIdxEdtsPair.Value);
}
editedDirty.Add(cnkIdxEdtsPair.Key);
saved=true;
if(LOG&&LOG_LEVEL<=1)Debug.Log("successfully saved edits at:"+fileName);
}catch(IOException e){Debug.LogWarning("file access failed:try save again after delay:fileName:"+fileName+"\n"+e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
}catch(Exception e1){Debug.LogError("unknown error:rename file to be marked as broken:fileName:"+fileName+"\n"+e1?.Message+"\n"+e1?.StackTrace+"\n"+e1?.Source);
    try{
    new FileInfo(fileName).Rename(Path.GetFileName(fileName),FileInfoExtensions.FileExistsBehavior.RenameOld,".{0}.{1}","broken",null);
    }catch(Exception e2){Debug.LogError(e2?.Message+"\n"+e2?.StackTrace+"\n"+e2?.Source);}
}finally{
dispose();
}
void dispose(){
try{
if(file!=null)
   file.Dispose();
}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
}
if(!saved){if(--saveTries<=0){if(LOG&&LOG_LEVEL<=100)Debug.LogWarning("failed to save at: "+fileName);break;}else{
Thread.Yield();Thread.Sleep(random.Next(100,501));
}}}
}


        


            }catch{throw;}finally{foreach(var syn in load_Syn)Monitor.Exit(syn);}






edtVxlsByCnkIdx.Clear();
backgroundDataSet.Set();}
        _Stop:{
            CallGC();
if(LOG&&LOG_LEVEL<=2)Debug.Log("end");
        }
    }
}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}}
[NonSerialized]public static readonly Biome biome=new Biome();
public static Vector2Int RgnToCoord(Vector2Int region){return new Vector2Int(region.x/Chunk.Width,region.y/Chunk.Depth);}public static Vector2Int CoordToRgn(Vector2Int coord){return new Vector2Int(coord.x*Chunk.Width,coord.y*Chunk.Depth);}
public static Vector2Int PosToCoord(Vector3 pos){
pos.x/=(float)Chunk.Width;
pos.z/=(float)Chunk.Depth;
return new Vector2Int((pos.x>0)?(pos.x-(int)pos.x==0.5f?Mathf.FloorToInt(pos.x):Mathf.RoundToInt(pos.x)):(int)Math.Round(pos.x,MidpointRounding.AwayFromZero),
                      (pos.z>0)?(pos.z-(int)pos.z==0.5f?Mathf.FloorToInt(pos.z):Mathf.RoundToInt(pos.z)):(int)Math.Round(pos.z,MidpointRounding.AwayFromZero));
}
public static Vector2Int PosToRgn(Vector3 pos){Vector2Int coord=PosToCoord(pos);
return new Vector2Int(coord.x*Width,coord.y*Depth);
}
public static int GetIdx(int cx,int cy){return cy+cx*(Width+1);}
public const int Width=6250;
public const int Depth=6250;



        
        public bool DEBUG_EDIT=false;




}}
public static class MemoryManagement{
public static Thread MainThread{get;set;}
[NonSerialized]static readonly object GC_Syn=new object();
public static void CallGC(){lock(GC_Syn){
GCSettings.LatencyMode=GCLatencyMode.Batch;
call();
GCSettings.LatencyMode=GCLatencyMode.SustainedLowLatency;
}}
public static IEnumerator Unload(){lock(GC_Syn){
GCSettings.LatencyMode=GCLatencyMode.Batch;
AsyncOperation asyn=Resources.UnloadUnusedAssets();yield return asyn;
call();
GCSettings.LatencyMode=GCLatencyMode.SustainedLowLatency;
}}
static void call(){
GCSettings.LargeObjectHeapCompactionMode=GCLargeObjectHeapCompactionMode.CompactOnce;
GC.Collect(GC.MaxGeneration,GCCollectionMode.Forced,true,true);
GC.WaitForPendingFinalizers();
}
}