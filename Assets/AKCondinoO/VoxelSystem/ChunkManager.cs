using System;
using System.Collections;
using System.Collections.Generic;
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
                      [NonSerialized]string[]saveSubfolder=new string[1];
public static string CurrWorldName{private set;get;}
public GameObject ChunkPrefab;
[NonSerialized]public Vector2Int expropriationDistance=new Vector2Int(5,5);[NonSerialized]public readonly LinkedList<Chunk>ChunksPool=new LinkedList<Chunk>();
[NonSerialized]public Vector2Int instantiationDistance=new Vector2Int(1,1);[NonSerialized]public readonly Dictionary<int,Chunk>Chunks=new Dictionary<int,Chunk>();
protected virtual void Awake(){
MainThread=Thread.CurrentThread;GarbageCollector.GCMode=GarbageCollector.Mode.Enabled;StartCoroutine(Unload());
AtlasHelper.GetAtlasData(ChunkPrefab.GetComponent<MeshRenderer>().sharedMaterial);
var maxChunks=(expropriationDistance.x*2+1)*(expropriationDistance.y*2+1);
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
saveSubfolder[0]=saveFolder+CurrWorldName+"/Chunks/"+"c{0}.edits";
    for(int i=maxChunks-1;i>=0;--i){
        GameObject obj=Instantiate(ChunkPrefab);Chunk scr=obj.GetComponent<Chunk>();ChunksPool.AddLast(scr);scr.ExpropriationNode=ChunksPool.Last;
    }
}
[NonSerialized]Task task;[NonSerialized]readonly AutoResetEvent foregroundDataSet=new AutoResetEvent(false);[NonSerialized]readonly ManualResetEvent backgroundDataSet=new ManualResetEvent(true);
protected virtual void OnEnable(){
Stop=false;task=Task.Factory.StartNew(BG,new object[]{LOG,LOG_LEVEL,saveSubfolder},TaskCreationOptions.LongRunning);




    //Build();




}
bool Stop{
    get{bool tmp;lock(Stop_Syn){tmp=Stop_v;      }return tmp;}
    set{         lock(Stop_Syn){    Stop_v=value;}if(value){foregroundDataSet.Set();}}
}[NonSerialized]readonly object Stop_Syn=new object();[NonSerialized]bool Stop_v;
protected virtual void OnDisable(){
Stop=true;try{task.Wait();}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
}
protected virtual void Update(){




        if(DEBUG_EDIT){Edit(Vector3.zero);}




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
public void Edit(Vector3 center){
    if(backgroundDataSet.WaitOne(0)){
        DEBUG_EDIT=false;
            backgroundDataSet.Reset();foregroundDataSet.Set();Build();
    }
}
[NonSerialized]public static readonly object load_Syn=new object();
[NonSerialized]readonly Dictionary<int,List<(Vector3Int vCoord,double density,MaterialId material)>>edtVxlsByCnkIdx=new Dictionary<int,List<(Vector3Int vCoord,double density,MaterialId material)>>();
void BG(object state){Thread.CurrentThread.IsBackground=false;Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;try{
    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL&&parameters[2]is string[]saveSubfolder){
DataContractSerializer saveContract=new DataContractSerializer(typeof(List<(Vector3Int vCoord,double density,MaterialId material)>));
        while(!Stop){foregroundDataSet.WaitOne();if(Stop)goto _Stop;
            lock(load_Syn){

        
int saveTries=60;
bool saved=false;
//var fileName=String.Format(saveSubfolder[0],0);
        Thread.Sleep(5000);


            }






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