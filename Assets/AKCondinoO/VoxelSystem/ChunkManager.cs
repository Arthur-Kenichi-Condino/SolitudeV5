using System;
using System.Collections.Generic;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace AKCondinoO.Voxels{public class ChunkManager:MonoBehaviour{ 
public bool LOG=false;public int LOG_LEVEL=1;
public GameObject ChunkPrefab;
[NonSerialized]public Vector2Int expropriationDistance=new Vector2Int(5,5);[NonSerialized]public readonly LinkedList<Chunk>ChunksPool=new LinkedList<Chunk>();
[NonSerialized]public Vector2Int instantiationDistance=new Vector2Int(0,0);[NonSerialized]public readonly Dictionary<int,Chunk>Chunks=new Dictionary<int,Chunk>();
protected virtual void Awake(){
AtlasHelper.GetAtlasData(ChunkPrefab.GetComponent<MeshRenderer>().sharedMaterial);
var maxChunks=(expropriationDistance.x*2+1)*(expropriationDistance.y*2+1);
if(LOG&&LOG_LEVEL<=100)Debug.Log("The number of processors on this computer is "+Environment.ProcessorCount);
ThreadPool.GetAvailableThreads(out int worker ,out int io         );if(LOG&&LOG_LEVEL<=100)Debug.Log("Thread pool threads available at startup: Worker threads: "+worker+" Asynchronous I/O threads: "+io);
ThreadPool.GetMaxThreads(out int workerThreads,out int portThreads);if(LOG&&LOG_LEVEL<=100)Debug.Log("Maximum worker threads: "+workerThreads+" Maximum completion port threads: "+portThreads);
ThreadPool.GetMinThreads(out int minWorker    ,out int minIOC     );if(LOG&&LOG_LEVEL<=100)Debug.Log("minimum number of worker threads: "+minWorker+" minimum asynchronous I/O: "+minIOC);
var idealMin=(maxChunks+Environment.ProcessorCount);if(minWorker!=idealMin){
if(ThreadPool.SetMinThreads(idealMin,minIOC)){
if(LOG&&LOG_LEVEL<=100)Debug.Log("changed minimum number of worker threads to:"+(idealMin));
}else{
if(LOG&&LOG_LEVEL<=100)Debug.Log("SetMinThreads failed");
}
}
GCSettings.LatencyMode=GCLatencyMode.SustainedLowLatency;
    for(int i=maxChunks-1;i>=0;--i){
        GameObject obj=Instantiate(ChunkPrefab);Chunk scr=obj.GetComponent<Chunk>();ChunksPool.AddLast(scr);scr.ExpropriationNode=ChunksPool.Last;
    }
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
[NonSerialized]public static readonly Biome biome=new Biome();
public static Vector2Int RgnToCoord(Vector2Int region){return new Vector2Int(region.x/Chunk.Width,region.y/Chunk.Depth);}public static Vector2Int CoordToRgn(Vector2Int coord){return new Vector2Int(coord.x*Chunk.Width,coord.y*Chunk.Depth);}
public static int GetIdx(int cx,int cy){return cy+cx*(Width+1);}
public const int Width=6250;
public const int Depth=6250;




protected virtual void OnEnable(){




    Build();




}
//[NonSerialized]public Vector2Int expropriationDistance=new Vector2Int(5,5);[NonSerialized]public readonly LinkedList<Chunk>ChunksPool=new LinkedList<Chunk>();
//[NonSerialized]public Vector2Int instantiationDistance=new Vector2Int(5,5);[NonSerialized]public readonly Dictionary<int,Chunk>Chunks=new Dictionary<int,Chunk>();
//protected virtual void Awake(){
//GCSettings.LatencyMode=GCLatencyMode.SustainedLowLatency;
//    for(int i=(expropriationDistance.x*2+1)*(expropriationDistance.y*2+1)-1;i>=0;--i){
//        GameObject obj=Instantiate(ChunkPrefab);Chunk scr=obj.GetComponent<Chunk>();
//        ChunksPool.AddLast(scr);scr.ExpropriationNode=ChunksPool.Last;
//    }
//}
//protected virtual void OnEnable(){




//    Instantiation();




//}
//protected virtual void Update(){
//}
//[NonSerialized]protected static Vector2Int aCoord;
//void Instantiation(){
//for(Vector2Int coord=new Vector2Int(),cCoord1=new Vector2Int();coord.y<=instantiationDistance.y;coord.y++){for(cCoord1.y=-coord.y+aCoord.y;cCoord1.y<=coord.y+aCoord.y;cCoord1.y+=coord.y*2){
//for(coord.x=0                                                 ;coord.x<=instantiationDistance.x;coord.x++){for(cCoord1.x=-coord.x+aCoord.x;cCoord1.x<=coord.x+aCoord.x;cCoord1.x+=coord.x*2){
//if(LOG&&LOG_LEVEL<=1)Debug.Log("try build chunk:"+cCoord1);
//if(Math.Abs(cCoord1.x)>=Width||
//   Math.Abs(cCoord1.y)>=Depth){
//if(LOG&&LOG_LEVEL<=2)Debug.Log("do not build out of world chunk");
//goto _skip;
//}
//    int cnkIdx1=GetIdx(cCoord1.x,cCoord1.y);if(!Chunks.ContainsKey(cnkIdx1)){
//if(LOG&&LOG_LEVEL<=1)Debug.Log("build chunk:"+cnkIdx1+";ChunksPool.Count:"+ChunksPool.Count);
//        Chunk scr=ChunksPool.First.Value;ChunksPool.RemoveFirst();scr.ExpropriationNode=(null);Chunks.Add(cnkIdx1,scr);scr.OnRebuildRequest(cCoord1,CoordToRgn(cCoord1),cnkIdx1);
//    }else{
//if(LOG&&LOG_LEVEL<=1)Debug.Log("chunk already built:"+cnkIdx1);
//    }
//_skip:{}
//if(coord.x==0){break;}}}
//if(coord.y==0){break;}}}
//}
//[NonSerialized]public static readonly Biome biome=new Biome();
//public static Vector2Int RgnToCoord(Vector2Int region){return new Vector2Int(region.x/Chunk.Width,region.y/Chunk.Depth);}public static Vector2Int CoordToRgn(Vector2Int coord){return new Vector2Int(coord.x*Chunk.Width,coord.y*Chunk.Depth);}
//public static int GetIdx(int cx,int cy){return cy+cx*(Width+1);}
//public const int Width=6250;
//public const int Depth=6250;




//public GameObject ChunkPrefab;
//[NonSerialized]public Vector2Int expropriationDistance=new Vector2Int(5,5);[NonSerialized]public readonly LinkedList<Chunk>ChunksPool=new LinkedList<Chunk>();
//[NonSerialized]public Vector2Int instantiationDistance=new Vector2Int(5,5);[NonSerialized]public readonly Dictionary<int,Chunk>Chunks=new Dictionary<int,Chunk>();
//    protected virtual void Awake(){
////GCSettings.LatencyMode=GCLatencyMode.Batch;
//GCSettings.LatencyMode=GCLatencyMode.SustainedLowLatency;
//        for(int i=(expropriationDistance.x*2+1)*(expropriationDistance.y*2+1)-1;i>=0;--i){
//            GameObject obj=Instantiate(ChunkPrefab);Chunk scr=obj.GetComponent<Chunk>();
//            ChunksPool.AddLast(scr);scr.ExpropriationNode=ChunksPool.Last;
//        }
//    }
//    protected virtual void OnEnable(){




//        Instantiation();




//    }
//    protected virtual void Update(){
//    }
//[NonSerialized]protected static Vector2Int aCoord;
//    void Instantiation(){
//        for(Vector2Int coord=new Vector2Int(),cCoord1=new Vector2Int();coord.y<=instantiationDistance.y;coord.y++){for(cCoord1.y=-coord.y+aCoord.y;cCoord1.y<=coord.y+aCoord.y;cCoord1.y+=coord.y*2){
//        for(coord.x=0                                                 ;coord.x<=instantiationDistance.x;coord.x++){for(cCoord1.x=-coord.x+aCoord.x;cCoord1.x<=coord.x+aCoord.x;cCoord1.x+=coord.x*2){
//if(LOG&&LOG_LEVEL<=1)Debug.Log("try build chunk cCoord1:"+cCoord1);
//            if(Math.Abs(cCoord1.x)>=Width||
//               Math.Abs(cCoord1.y)>=Depth){
//if(LOG&&LOG_LEVEL<=2)Debug.Log("do not add or build out of world chunk");
//                goto _skip;
//            }
//            int cnkIdx1=GetIdx(cCoord1.x,cCoord1.y);if(!Chunks.ContainsKey(cnkIdx1)){
//                Chunk scr=ChunksPool.First.Value;ChunksPool.RemoveFirst();scr.ExpropriationNode=(null);Chunks.Add(cnkIdx1,scr);scr.OnRebuildRequest(cCoord1,CoordToRgn(cCoord1),cnkIdx1);
//if(LOG&&LOG_LEVEL<=1)Debug.Log("add and build chunk:"+cnkIdx1+";ChunksPool.Count:"+ChunksPool.Count);
//            }else{
//if(LOG&&LOG_LEVEL<=1)Debug.Log("chunk already built and set:"+cnkIdx1);
//            }
//            _skip:{}
//        if(coord.x==0){break;}}}
//        if(coord.y==0){break;}}}
//    }
//    public static void Edit(){
//    }




//        public bool DEBUG_EDIT=false;
////[NonSerialized]Task task;
////    protected virtual void OnEnable(){
////        task=Task.Factory.StartNew(BG,new object[]{},TaskCreationOptions.LongRunning);




////        Instantiation();




////    }
////    protected virtual void OnDisable(){
////    }
////[NonSerialized]protected static Vector2Int aCoord;
////[NonSerialized]public Vector2Int expropriationDistance=new Vector2Int(5,5);[NonSerialized]public readonly LinkedList<Chunk>ChunksPool=new LinkedList<Chunk>();
////[NonSerialized]public Vector2Int instantiationDistance=new Vector2Int(1,1);[NonSerialized]public readonly Dictionary<int,Chunk>Chunks=new Dictionary<int,Chunk>();
////    void Instantiation(){
////        for(Vector2Int coord=new Vector2Int(),cCoord1=new Vector2Int();coord.y<=instantiationDistance.y;coord.y++){for(cCoord1.y=-coord.y+aCoord.y;cCoord1.y<=coord.y+aCoord.y;cCoord1.y+=coord.y*2){
////        for(coord.x=0                                                 ;coord.x<=instantiationDistance.x;coord.x++){for(cCoord1.x=-coord.x+aCoord.x;cCoord1.x<=coord.x+aCoord.x;cCoord1.x+=coord.x*2){
////if(LOG&&LOG_LEVEL<=1)Debug.Log("try build chunk cCoord1:"+cCoord1);
////            if(Math.Abs(cCoord1.x)>=Width||
////               Math.Abs(cCoord1.y)>=Depth){
////if(LOG&&LOG_LEVEL<=2)Debug.Log("do not add or build out of world chunk");
////                goto _skip;
////            }
////            int cnkIdx1=GetIdx(cCoord1.x,cCoord1.y);if(!Chunks.ContainsKey(cnkIdx1)){
////if(LOG&&LOG_LEVEL<=1)Debug.Log("add and build chunk:"+cnkIdx1);




////                Chunk scr=ChunksPool.First.Value;ChunksPool.RemoveFirst();scr.ExpropriationNode=(null);Chunks.Add(cnkIdx1,scr);
                




////            }else{
////if(LOG&&LOG_LEVEL<=1)Debug.Log("chunk already built and set:"+cnkIdx1);
////            }
////             _skip:{}
////        if(coord.x==0){break;}}}
////        if(coord.y==0){break;}}}
////    }
////    void BG(object state){
////        Debug.LogWarning("long running");
////    }
//[NonSerialized]public static readonly object GC_Syn=new object();
//public static Vector2Int RgnToCoord(Vector2Int region){return new Vector2Int(region.x/Chunk.Width,region.y/Chunk.Depth);}public static Vector2Int CoordToRgn(Vector2Int coord){return new Vector2Int(coord.x*Chunk.Width,coord.y*Chunk.Depth);}
//public static int GetIdx(int cx,int cy){return cy+cx*(Width+1);}
//public const int Width=6250;
//public const int Depth=6250;
}}