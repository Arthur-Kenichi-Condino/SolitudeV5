using AKCondinoO.Species.Plants;
using MessagePack;
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
namespace AKCondinoO.Voxels{public class ChunkManager:MonoBehaviour{//  valores:[teste:(1,1)]:[expropriationDistance=(7,7);instantiationDistance=(5,5);]
public bool LOG=false;public int LOG_LEVEL=1;
[NonSerialized]public static readonly string saveFolder=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("\\","/").ToString()+"/Solitude/";[NonSerialized]public static string[]saveSubfolder=new string[1];
public static string CurrWorldName{private set;get;}
public GameObject ChunkPrefab;
[NonSerialized]public Vector2Int expropriationDistance=new Vector2Int(1,1);[NonSerialized]public readonly LinkedList<Chunk>ChunksPool=new LinkedList<Chunk>();
[NonSerialized]public Vector2Int instantiationDistance=new Vector2Int(1,1);[NonSerialized]public readonly Dictionary<int,Chunk>Chunks=new Dictionary<int,Chunk>();
public static ChunkManager main{get;private set;}
protected virtual void Awake(){
main=this;
var maxChunks=(expropriationDistance.x*2+1)*(expropriationDistance.y*2+1);
try{
MainThread=Thread.CurrentThread;GarbageCollector.GCMode=GarbageCollector.Mode.Enabled;StartCoroutine(Unload());
AtlasHelper.GetAtlasData(ChunkPrefab.GetComponent<MeshRenderer>().sharedMaterial);
if(LOG&&LOG_LEVEL<=100)Debug.Log("The number of processors on this computer is "+Environment.ProcessorCount);
ThreadPool.GetAvailableThreads(out int worker ,out int io         );if(LOG&&LOG_LEVEL<=100)Debug.Log("Thread pool threads available at startup: Worker threads: "+worker+" Asynchronous I/O threads: "+io);
ThreadPool.GetMaxThreads(out int workerThreads,out int portThreads);if(LOG&&LOG_LEVEL<=100)Debug.Log("Maximum worker threads: "+workerThreads+" Maximum completion port threads: "+portThreads);
ThreadPool.GetMinThreads(out int minWorker    ,out int minIOC     );if(LOG&&LOG_LEVEL<=100)Debug.Log("minimum number of worker threads: "+minWorker+" minimum asynchronous I/O: "+minIOC);
var idealMin=(maxChunks+2+Environment.ProcessorCount);if(minWorker!=idealMin){
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

    
Vector3 fadeQStart,fadeQEnd;
AtlasHelper.Material.SetVector(AtlasHelper._Shader_Input[5],fadeQStart=new Vector3(Chunk.Width/2f+Chunk.Width*instantiationDistance.x-Chunk.Width,
                                                                                   Chunk.Height/2f-8f,
                                                                                   Chunk.Depth/2f+Chunk.Depth*instantiationDistance.y-Chunk.Depth));
AtlasHelper.Material.SetVector(AtlasHelper._Shader_Input[6],fadeQEnd  =new Vector3(Chunk.Width/2f+Chunk.Width*instantiationDistance.x-Chunk.Width/2f,
                                                                                   Chunk.Height/2f,
                                                                                   Chunk.Depth/2f+Chunk.Depth*instantiationDistance.y-Chunk.Depth/2f));
Vector3 fogQStart,fogQEnd;
AtlasHelper.Material.SetVector(AtlasHelper._Shader_Input[4],fogQEnd   =fadeQStart);
                                                            fogQStart =new Vector3(fogQEnd.x-8f,
                                                                                   fogQEnd.y-8f,
                                                                                   fogQEnd.z-8f);
AtlasHelper.Material.SetVector(AtlasHelper._Shader_Input[3],new Vector3(fogQStart.x==0f?4f:fogQStart.x,
                                                                        fogQStart.y,
                                                                        fogQStart.z==0f?4f:fogQStart.z));


}
[NonSerialized]Task task1,task2;[NonSerialized]readonly AutoResetEvent foregroundDataSet1=new AutoResetEvent(false);[NonSerialized]readonly ManualResetEvent foregroundDataSet2=new ManualResetEvent(true);[NonSerialized]readonly ManualResetEvent backgroundDataSet1=new ManualResetEvent(true);
protected virtual void OnEnable(){
backgroundDataSet1.Set();foregroundDataSet1.Reset();
                         foregroundDataSet2.Set();
Stop=false;task1=Task.Factory.StartNew(BG1,new object[]{LOG,LOG_LEVEL,new System.Random(),saveSubfolder},TaskCreationOptions.LongRunning);
           task2=Task.Factory.StartNew(BG2,new object[]{LOG,LOG_LEVEL,}                                 ,TaskCreationOptions.LongRunning);




    //Build();




}
bool Stop{
    get{bool tmp;lock(Stop_Syn){tmp=Stop_v;      }return tmp;}
    set{         lock(Stop_Syn){    Stop_v=value;}if(value){foregroundDataSet1.Set();foregroundDataSet2.Set();}}
}[NonSerialized]readonly object Stop_Syn=new object();[NonSerialized]bool Stop_v;
protected virtual void OnDisable(){
Stop=true;try{task1.Wait();}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
          try{task2.Wait();}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
}
protected virtual void OnDestroy(){if(Stop){
if(LOG&&LOG_LEVEL<=2)Debug.Log("dispose");
backgroundDataSet1.Dispose();foregroundDataSet1.Dispose();
                             foregroundDataSet2.Dispose();
}}
[NonSerialized]float frameTimeVariation;protected float millisecondsPerFrame;
public static float FPS{
    get{float tmp;lock(FPS_Syn)tmp=FPS_v;return tmp;}
    set{          lock(FPS_Syn)FPS_v=value;         }
}[NonSerialized]static readonly object FPS_Syn=new object();[NonSerialized]static float FPS_v;
public static float averageFramerate{
    get{float tmp;lock(averageFramerate_Syn){tmp=averageFramerate_v;  }return tmp;}
    set{          lock(averageFramerate_Syn){averageFramerate_v=value;}           }
}[NonSerialized]static readonly object averageFramerate_Syn=new object();[NonSerialized]static float averageFramerate_v=60;[NonSerialized]int frameCounter;[NonSerialized]float averageFramerateRefreshTimer;[SerializeField]float averageFramerateRefreshTime=1.0f;
bool firstLoop=true;protected virtual void Update(){
#region FPS
frameTimeVariation+=(Time.deltaTime-frameTimeVariation);millisecondsPerFrame=frameTimeVariation*1000.0f;FPS=1.0f/frameTimeVariation;




frameCounter++;averageFramerateRefreshTimer+=Time.deltaTime;
if(averageFramerateRefreshTimer>=averageFramerateRefreshTime){
averageFramerate=frameCounter/averageFramerateRefreshTimer;
frameCounter=0;averageFramerateRefreshTimer=0.0f;
}


        //Debug.LogWarning(FPS+"; "+averageFramerate);


#endregion
    if(backgroundDataSet1.WaitOne(0)){
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




        if(DEBUG_EDIT){Edit(new Vector3(0,10,0),new Vector3Int(8,8,1),51,MaterialId.Dirt,EditMode.Sphere);}




if(firstLoop||actPos!=Camera.main.transform.position){
if(LOG&&LOG_LEVEL<=-80)Debug.Log("actPos anterior:"+actPos+";actPos novo:"+Camera.main.transform.position);
    actPos=Camera.main.transform.position;


AtlasHelper.Material.SetVector(AtlasHelper._Shader_Input[0],actPos);


    if(firstLoop|aCoord!=(aCoord=PosToCoord(actPos))){
if(LOG&&LOG_LEVEL<=1)Debug.Log("aCoord novo:"+aCoord+";aCoord_Pre:"+aCoord_Pre);
Build();aCoord_Pre=aCoord;
    }
}




firstLoop=false;}
[NonSerialized]protected static Vector3 actPos;
[NonSerialized]protected static Vector2Int aCoord,aCoord_Pre;
void Build(){
for(Vector2Int coord=new Vector2Int(),cCoord1=new Vector2Int();coord.y<=expropriationDistance.y;coord.y++){for(cCoord1.y=-coord.y+aCoord_Pre.y;cCoord1.y<=coord.y+aCoord_Pre.y;cCoord1.y+=coord.y*2){
for(coord.x=0                                                 ;coord.x<=expropriationDistance.x;coord.x++){for(cCoord1.x=-coord.x+aCoord_Pre.x;cCoord1.x<=coord.x+aCoord_Pre.x;cCoord1.x+=coord.x*2){
if(LOG&&LOG_LEVEL<=1)Debug.Log("try expropriate chunk:"+cCoord1);        
if(Math.Abs(cCoord1.x)>=Width||
   Math.Abs(cCoord1.y)>=Depth){
if(LOG&&LOG_LEVEL<=2)Debug.Log("do not try to expropriate out of world chunk");
goto _skip;
}
if(Mathf.Abs(cCoord1.x-aCoord.x)>instantiationDistance.x||
   Mathf.Abs(cCoord1.y-aCoord.y)>instantiationDistance.y){


        //if(scr.ExpropriationNode!=null){ChunksPool.Remove(scr.ExpropriationNode);scr.ExpropriationNode=(null);}
    int cnkIdx1=GetIdx(cCoord1.x,cCoord1.y);if(Chunks.ContainsKey(cnkIdx1)){
if(LOG&&LOG_LEVEL<=1)Debug.Log("expropriate chunk:"+cnkIdx1);
        //Chunk scr=Chunks[cnkIdx1];Chunks.Remove(cnkIdx1);ChunksPool.AddLast(scr);scr.ExpropriationNode=ChunksPool.Last;
        Chunk scr=Chunks[cnkIdx1];if(scr.ExpropriationNode==null){ChunksPool.AddLast(scr);scr.ExpropriationNode=ChunksPool.Last;
        }else{
if(LOG&&LOG_LEVEL<=1)Debug.Log("chunk index already expropriated:"+cnkIdx1);
        }
    }else{
if(LOG&&LOG_LEVEL<=1)Debug.Log("no chunk to expropriate at:"+cnkIdx1);
    }




}
_skip:{}
if(coord.x==0){break;}}}
if(coord.y==0){break;}}}
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
        //Chunk scr=ChunksPool.First.Value;ChunksPool.RemoveFirst();scr.ExpropriationNode=(null);Chunks.Add(cnkIdx1,scr);scr.OnRebuildRequest(cCoord1,CoordToRgn(cCoord1),cnkIdx1);
        //Chunk scr=ChunksPool.First.Value;ChunksPool.RemoveFirst();if(scr.Initialized&&Chunks.ContainsKey(scr.Idx))Chunks.Remove(scr.Idx);scr.ExpropriationNode=(null);
        Chunk scr=ChunksPool.First.Value;ChunksPool.RemoveFirst();scr.ExpropriationNode=(null);if(scr.Initialized&&Chunks.ContainsKey(scr.Idx))Chunks.Remove(scr.Idx);Chunks.Add(cnkIdx1,scr);scr.OnRebuildRequest(cCoord1,CoordToRgn(cCoord1),cnkIdx1);
    }else{
if(LOG&&LOG_LEVEL<=1)Debug.Log("chunk already built:"+cnkIdx1);
        Chunk scr=Chunks[cnkIdx1];if(scr.ExpropriationNode!=null){ChunksPool.Remove(scr.ExpropriationNode);scr.ExpropriationNode=(null);}
    }
_skip:{}
if(coord.x==0){break;}}}
if(coord.y==0){break;}}}
}
public enum EditMode{Cube,Sphere}
public void Edit(Vector3 center,Vector3Int size,double tgtDensity=0,MaterialId tgtMaterialId=MaterialId.Air,EditMode mode=EditMode.Cube){
    if(backgroundDataSet1.WaitOne(0)){

     
if(tgtMaterialId==MaterialId.Air){tgtDensity=0;}else{tgtDensity=Math.Max(tgtDensity,81f);tgtDensity=Math.Min(tgtDensity,100f);}
        

Vector3 cornerRadius;
Vector3 cornerRadiusSmoothStep=new Vector3(1f,1f,1f);
switch(mode){
case(EditMode.Sphere):{
size.x=size.y=size.z=Mathf.Max(2,size.x,size.y,size.z);
cornerRadius=new Vector3(size.x,size.x,size.x);
    break;
}
default:{


//radius=Mathf.Sqrt();
size.x=Mathf.Max(2,size.x);//  Troquei o valor de 8 para 2 e funcionou bem, mas não entendi o porquê
size.y=Mathf.Max(2,size.y);//
size.z=Mathf.Max(2,size.z);//
cornerRadius=new Vector3(Mathf.Sqrt(Mathf.Pow(size.x,2)+Mathf.Pow(size.y,2)),
                         Mathf.Sqrt(Mathf.Pow(size.y,2)+Mathf.Pow(size.z,2)),
                         Mathf.Sqrt(Mathf.Pow(size.z,2)+Mathf.Pow(size.x,2)));
cornerRadiusSmoothStep.x=(cornerRadius.x-Mathf.Sqrt(Mathf.Pow(size.x-4f,2)+Mathf.Pow(size.y-4f,2)))/4f;
cornerRadiusSmoothStep.y=(cornerRadius.y-Mathf.Sqrt(Mathf.Pow(size.y-4f,2)+Mathf.Pow(size.z-4f,2)))/4f;
cornerRadiusSmoothStep.z=(cornerRadius.z-Mathf.Sqrt(Mathf.Pow(size.z-4f,2)+Mathf.Pow(size.x-4f,2)))/4f;
Debug.LogWarning(cornerRadiusSmoothStep);

    break;
}
}


Vector2Int cCoord1=PosToCoord(center);
Vector2Int cnkRgn1=CoordToRgn(cCoord1);
Vector3Int vCoord1=Chunk.PosToCoord(center);
if(LOG&&LOG_LEVEL<=2)Debug.Log("edit at:"+center+"; cCoord1:"+cCoord1+"; vCoord1:"+vCoord1);
var offset=new Vector3Int();float smoothValue;
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
    if(!edtVxlsByCnkIdx.ContainsKey(cnkIdx2)){edtVxlsByCnkIdx.Add(cnkIdx2,new Dictionary<Vector3Int,(double density,MaterialId materialId)>());
                                              edtDataByCnkIdx.Add(cnkIdx2,new Dictionary<Vector3Int,(Vector2Int cnkRgn2,float smoothValue)>());}


switch(mode){
case(EditMode.Sphere):{
//smoothValue=Mathf.Clamp01(1f-Mathf.Abs(offset.x)/(float)size.x)*
//            Mathf.Clamp01(1f-Mathf.Abs(offset.y)/(float)size.y)*
//            Mathf.Clamp01(1f-Mathf.Abs(offset.z)/(float)size.z);


//smoothValue=0f;
smoothValue=1f-Mathf.Clamp01(Vector3.Distance(vCoord2,vCoord1)/size.x);
//smoothValue=Mathf.Min(Mathf.Clamp01(1f-Mathf.Abs(offset.x)/(float)size.x),
//                      Mathf.Clamp01(1f-Mathf.Abs(offset.y)/(float)size.y),
//                      Mathf.Clamp01(1f-Mathf.Abs(offset.z)/(float)size.z));


    break;
}
default:{
//smoothValue=Mathf.Clamp01(1f-Vector3.Distance(vCoord2,vCoord1)/Mathf.Max(size.x,size.y,size.z));
//smoothValue=Mathf.Min(1f-Mathf.Abs(vCoord2.x-vCoord1.x)/(float)size.x,
//                      1f-Mathf.Abs(vCoord2.y-vCoord1.y)/(float)size.y,
//                      1f-Mathf.Abs(vCoord2.z-vCoord1.z)/(float)size.z);


//smoothValue=Mathf.Min(1f-Mathf.Abs(offset.x)/(float)size.x,
//                      1f-Mathf.Abs(offset.y)/(float)size.y,
//                      1f-Mathf.Abs(offset.z)/(float)size.z)*.25f;


//smoothValue=(1f-Mathf.Clamp01((float)Mathf.Abs(vCoord2.x-vCoord1.x)/size.x)+
//             1f-Mathf.Clamp01((float)Mathf.Abs(vCoord2.y-vCoord1.y)/size.y)+
//             1f-Mathf.Clamp01((float)Mathf.Abs(vCoord2.z-vCoord1.z)/size.z))/3f;
smoothValue=1f;


//float smoothRadius=4f;


Vector3 smoothByAxis=new Vector3(1f,1f,1f);


int dis1=Mathf.Abs(vCoord2.x-vCoord1.x);
if(dis1>=size.x){
smoothByAxis.x=.004f;
}else if(dis1>=size.x-1){
smoothByAxis.x=.04f;
}else if(dis1>=size.x-2){
smoothByAxis.x=.12f;
}else if(dis1>=size.x-3){
smoothByAxis.x=.24f;
}
float dis2=Mathf.Abs(vCoord2.z-vCoord1.z);
if(dis2>=size.z){
smoothByAxis.z=.004f;
}else if(dis2>=size.z-1){
smoothByAxis.z=.04f;
}else if(dis2>=size.z-2){
smoothByAxis.z=.12f;
}else if(dis2>=size.z-3){
smoothByAxis.z=.24f;
}
float dis3=Mathf.Abs(vCoord2.y-vCoord1.y);
if(dis3>=size.y){
smoothByAxis.y=.004f;
}else if(dis3>=size.y-1){
smoothByAxis.y=.04f;
}else if(dis3>=size.y-2){
smoothByAxis.y=.12f;
}else if(dis3>=size.y-3){
smoothByAxis.y=.24f;
}


Vector3 roundCorner=new Vector3(1f,1f,1f);
float dis4=Mathf.Sqrt(Mathf.Pow(dis1,2)+Mathf.Pow(dis2,2));
Debug.LogWarning(dis4+"/"+cornerRadius.z);
if(dis4>=cornerRadius.z-cornerRadiusSmoothStep.z){
roundCorner.x=.004f;
}else if(dis4>=cornerRadius.z-cornerRadiusSmoothStep.z*2f){
roundCorner.x=.04f;
}else if(dis4>=cornerRadius.z-cornerRadiusSmoothStep.z*3f){
roundCorner.x=.2f;
}else if(dis4>=cornerRadius.z-cornerRadiusSmoothStep.z*4f){
roundCorner.x=.4f;
}
float dis5=Mathf.Sqrt(Mathf.Pow(dis2,2)+Mathf.Pow(dis3,2));
if(dis5>=cornerRadius.y-cornerRadiusSmoothStep.y){
roundCorner.z=.004f;
}else if(dis5>=cornerRadius.y-cornerRadiusSmoothStep.y*2f){
roundCorner.z=.04f;
}else if(dis5>=cornerRadius.y-cornerRadiusSmoothStep.y*3f){
roundCorner.z=.2f;
}else if(dis5>=cornerRadius.y-cornerRadiusSmoothStep.y*4f){
roundCorner.z=.4f;
}
float dis6=Mathf.Sqrt(Mathf.Pow(dis3,2)+Mathf.Pow(dis1,2));
if(dis6>=cornerRadius.x-cornerRadiusSmoothStep.x){
roundCorner.y=.004f;
}else if(dis6>=cornerRadius.x-cornerRadiusSmoothStep.x*2f){
roundCorner.y=.04f;
}else if(dis6>=cornerRadius.x-cornerRadiusSmoothStep.x*3f){
roundCorner.y=.2f;
}else if(dis6>=cornerRadius.x-cornerRadiusSmoothStep.x*4f){
roundCorner.y=.4f;
}


smoothValue=Mathf.Min(Mathf.Min(smoothByAxis.x,smoothByAxis.z,smoothByAxis.y),Mathf.Min(roundCorner.x,roundCorner.z,roundCorner.y));


//float dis1=Vector3.Distance(vCoord2,vCoord1)/*=Mathf.Abs(vCoord2.x-vCoord1.x)*/;float perc1=1f;


//float radius1;
//if((radius1=size.x-dis1)<=4f){
//perc1=radius1/4f;
////smoothByAxis.x=;
//}


//Debug.LogWarning(dis1+" "+perc1);


//float dis1=Mathf.Abs(vCoord2.x-vCoord1.x);
//float dis2=Mathf.Lerp(dis1/10f,dis1,dis1/size.x);
//smoothValue=Mathf.Lerp(.5f,.0125f,dis2/size.x);


//float dis=Mathf.Abs(vCoord2.x-vCoord1.x);
//Debug.LogWarning(dis);
//if(dis>=size.x){
//smoothByAxis.x=.0075f;
//}else if(dis>=size.x-1){
//smoothByAxis.x=.0395f;
//}else if(dis>=size.x-2){
//smoothByAxis.x=.1115f;
//}else if(dis>=size.x-3){
//smoothByAxis.x=.2115f;
//}else if(dis>=size.x-4){
//smoothByAxis.x=.3510f;
//}else if(dis>=size.x-5){
//smoothByAxis.x=.5105f;
//}
//dis=Mathf.Abs(vCoord2.z-vCoord1.z);
//Debug.LogWarning(dis);
//if(dis>=size.z){
//smoothByAxis.z=.0075f;
//}else if(dis>=size.z-1){
//smoothByAxis.z=.0395f;
//}else if(dis>=size.z-2){
//smoothByAxis.z=.1115f;
//}else if(dis>=size.z-3){
//smoothByAxis.z=.2115f;
//}else if(dis>=size.z-4){
//smoothByAxis.z=.3510f;
//}else if(dis>=size.z-5){
//smoothByAxis.z=.5105f;
//}
//dis=Mathf.Abs(vCoord2.y-vCoord1.y);
//Debug.LogWarning(dis);
//if(dis>=size.y){
//smoothByAxis.y=.0075f;
//}else if(dis>=size.y-1){
//smoothByAxis.y=.0395f;
//}else if(dis>=size.y-2){
//smoothByAxis.y=.1115f;
//}else if(dis>=size.y-3){
//smoothByAxis.y=.2115f;
//}else if(dis>=size.y-4){
//smoothByAxis.y=.3510f;
//}else if(dis>=size.y-5){
//smoothByAxis.y=.5105f;
//}


////smoothValue=Mathf.Min(smoothByAxis.x,smoothByAxis.z,smoothByAxis.y);
//smoothValue=(smoothByAxis.x+smoothByAxis.z+smoothByAxis.y)/3f;//Mathf.Min(smoothByAxis.x,smoothByAxis.z,smoothByAxis.y);
//Vector3 smoothByAxis=new Vector3(1f,1f,1f);
//float dis=Mathf.Abs(vCoord2.x-vCoord1.x);
//if(dis>=size.x-smoothRadius){
//smoothByAxis.x=(1f-(dis-1f)/size.x)*.5f;
//Debug.LogWarning(dis+" "+smoothByAxis.x);
//}
////Debug.LogWarning((dis/size.x));
////float dis;float smoothRadius=4f;Vector3 smoothByAxis=new Vector3();
////if((dis=Mathf.Abs(offset.x-size.x))<=smoothRadius){
////smoothByAxis.x=(smoothRadius-dis)/smoothRadius;
////Debug.LogWarning(smoothByAxis.x);
////}


//smoothValue=smoothByAxis.x;
//Debug.LogWarning(smoothValue);


//smoothValue=1f;
//Debug.LogWarning(1f-Mathf.Abs(vCoord2.x-vCoord1.x)/(float)size.x);
//Debug.LogWarning(1f-Mathf.Abs(vCoord2.y-vCoord1.y)/(float)size.y);
//Debug.LogWarning(1f-Mathf.Abs(vCoord2.z-vCoord1.z)/(float)size.z);
        //Debug.LogWarning("dis:"+Vector3.Distance(vCoord2,vCoord1)+" maxDis:"+Mathf.Max(size.x,size.y,size.z)+" smoothValue:"+smoothValue);


    break;
}
}


    edtVxlsByCnkIdx[cnkIdx2].Add(vCoord2,(tgtDensity,tgtMaterialId));edtDataByCnkIdx[cnkIdx2].Add(vCoord2,(cnkRgn2,smoothValue));
}}}
            backgroundDataSet1.Reset();foregroundDataSet1.Set();
_End:{}

        
        DEBUG_EDIT=false;


    }
    void Cancel(){
if(LOG&&LOG_LEVEL<=2)Debug.Log("cancel edit");
edtVxlsByCnkIdx.Clear();
edtDataByCnkIdx.Clear();
    }
}
[NonSerialized]readonly List<object>load_Syn=new List<object>();
[NonSerialized]readonly Dictionary<int,Dictionary<Vector3Int,(double density,MaterialId materialId)>>edtVxlsByCnkIdx=new Dictionary<int,Dictionary<Vector3Int,(double density,MaterialId materialId)>>();[NonSerialized]readonly Dictionary<int,Dictionary<Vector3Int,(Vector2Int cnkRgn2,float smoothValue)>>edtDataByCnkIdx=new Dictionary<int,Dictionary<Vector3Int,(Vector2Int cnkRgn2,float smoothValue)>>();[NonSerialized]readonly List<int>editedDirty=new List<int>();
void BG1(object state){Thread.CurrentThread.IsBackground=false;Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;try{
    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL&&parameters[2]is System.Random random&&parameters[3]is string[]saveSubfolder){
        while(!Stop){foregroundDataSet1.WaitOne();if(Stop)goto _Stop;
            foreach(var syn in load_Syn)Monitor.Enter(syn);try{
foreach(var cnkIdxEdtsPair in edtVxlsByCnkIdx){var vxlData=cnkIdxEdtsPair.Value;var edtData=edtDataByCnkIdx[cnkIdxEdtsPair.Key];
var fileName=string.Format(saveSubfolder[0],cnkIdxEdtsPair.Key);
if(LOG&&LOG_LEVEL<=1)Debug.Log("save edits at: "+fileName);


TerrainChunk.Voxel[]toMerge=new TerrainChunk.Voxel[Chunk.VoxelsPerChunk];bool[]hasMergeData=new bool[Chunk.VoxelsPerChunk];
if(File.Exists(fileName)){
int loadTries=30;bool loaded=false;while(!loaded){
FileStream file=null;


try{


using(file=new FileStream(fileName,FileMode.Open,FileAccess.Read,FileShare.Read)){
if(file.Length>0){
if(LOG&&LOG_LEVEL<=1)Debug.Log("file has data, loading it to merge edits:fileName:"+fileName);
if(MessagePackSerializer.Deserialize(typeof(Dictionary<Vector3Int,(double density,MaterialId material)>),file)is Dictionary<Vector3Int,(double density,MaterialId material)>fileData){
                                                        
foreach(var voxelData in fileData){
int vxlIdx=Chunk.GetIdx(voxelData.Key.x,voxelData.Key.y,voxelData.Key.z);toMerge[vxlIdx]=new TerrainChunk.Voxel(voxelData.Value.density,Vector3.zero,voxelData.Value.material);hasMergeData[vxlIdx]=true;
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


TerrainChunk.Voxel tmpVxl=new TerrainChunk.Voxel();double[]noiseCache=null;
foreach(var edtDataToProcess in edtData){var cnkRgn2=edtDataToProcess.Value.cnkRgn2;var smoothValue=edtDataToProcess.Value.smoothValue;
var v=vxlData[edtDataToProcess.Key];var vCoord2=edtDataToProcess.Key;int vxlIdx2=Chunk.GetIdx(vCoord2.x,vCoord2.y,vCoord2.z);
        

if(hasMergeData[vxlIdx2]){var mergeData=toMerge[vxlIdx2];
tmpVxl.Density=mergeData.Density;tmpVxl.Material=mergeData.Material;tmpVxl.Normal=Vector3.zero;
}else{
Vector3 noiseInput=vCoord2;noiseInput.x+=cnkRgn2.x;
                           noiseInput.z+=cnkRgn2.y;
biome.v(noiseInput,ref tmpVxl,ref noiseCache,vCoord2.z+vCoord2.x*Chunk.Depth);
}


var densityResult=tmpVxl.Density+(v.density-tmpVxl.Density)*smoothValue;//  Lerp
MaterialId materialIdToSet=-densityResult>=TerrainChunk.IsoLevel?MaterialId.Air:(v.materialId==MaterialId.Air?(tmpVxl.Material==MaterialId.Air?MaterialId.Dirt:tmpVxl.Material):v.materialId);


//Debug.LogWarning("smoothValue:"+smoothValue+"; curDensity:"+tmpVxl.Density+"; tgtDensity:"+v.density+"; densityResult:"+densityResult);
v.density=densityResult;v.materialId=materialIdToSet;


vxlData[edtDataToProcess.Key]=v;
}


int saveTries=60;bool saved=false;while(!saved){
FileStream file=null;
try{
using(file=new FileStream(fileName,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None)){
if(file.Length>0){
if(LOG&&LOG_LEVEL<=1)Debug.Log("file has data, load it to merge with new save data:fileName:"+fileName);  
if(MessagePackSerializer.Deserialize(typeof(Dictionary<Vector3Int,(double density,MaterialId material)>),file)is Dictionary<Vector3Int,(double density,MaterialId material)>fileData){
foreach(var voxelData in fileData){
if(!cnkIdxEdtsPair.Value.ContainsKey(voxelData.Key)){
    cnkIdxEdtsPair.Value.Add(voxelData.Key,voxelData.Value);
}
}
}}
file.SetLength(0);
file.Flush(true);
MessagePackSerializer.Serialize(typeof(Dictionary<Vector3Int,(double density,MaterialId material)>),file,cnkIdxEdtsPair.Value);
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
edtDataByCnkIdx.Clear();
backgroundDataSet1.Set();}
        _Stop:{
            CallGC();
if(LOG&&LOG_LEVEL<=2)Debug.Log("end");
        }
    }
}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}}
[NonSerialized]public static readonly Biome biome=new Plains();
[SerializeField]protected BiomePrefabs[]biomePrefabs;
[Serializable]public class BiomePrefabs{
public string Name;
public Plant[]Plants;
public int[]FrequencyMin;
public int[]FrequencyMax;
}
public static Vector2Int RgnToCoord(Vector2Int region){return new Vector2Int(region.x/Chunk.Width,region.y/Chunk.Depth);}public static Vector2Int CoordToRgn(Vector2Int coord){return new Vector2Int(coord.x*Chunk.Width,coord.y*Chunk.Depth);}
public static Vector2Int PosToCoord(Vector3 pos){
pos.x/=(float)Chunk.Width;
pos.z/=(float)Chunk.Depth;
return new Vector2Int((pos.x>0)?(pos.x-(int)pos.x==0.5f?Mathf.FloorToInt(pos.x):Mathf.RoundToInt(pos.x)):(int)Math.Round(pos.x,MidpointRounding.AwayFromZero),
                      (pos.z>0)?(pos.z-(int)pos.z==0.5f?Mathf.FloorToInt(pos.z):Mathf.RoundToInt(pos.z)):(int)Math.Round(pos.z,MidpointRounding.AwayFromZero));
}
public static Vector2Int PosToRgn(Vector3 pos){Vector2Int coord=PosToCoord(pos);
return new Vector2Int(coord.x*Chunk.Width,coord.y*Chunk.Depth);
}
public static int GetIdx(int cx,int cy){return cy+cx*(Depth+1);}
public const int Width=6250;
public const int Depth=6250;
void BG2(object state){Thread.CurrentThread.IsBackground=false;Thread.CurrentThread.Priority=System.Threading.ThreadPriority.Normal;try{
    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL){
int delay=500;
        while(!Stop){foregroundDataSet2.WaitOne();if(Stop)goto _Stop;
int tmp;lock(TerrainChunk.tasksBusyCount_Syn){tmp=TerrainChunk.tasksBusyCount;}Thread.Yield();Thread.Sleep(1+Mathf.Clamp(tmp*delay,0,30000));if(!TerrainChunk.queue.WaitOne(0)){if(averageFramerate>=50&&FPS>=50){TerrainChunk.queue.Set();delay=(int)(delay*.9f);}else{delay=(int)(delay*5f);}delay=Mathf.Clamp(delay,150,5000);if(LOG&&LOG_LEVEL<=2)Debug.Log("new delay value:"+delay);}else{TerrainChunk.queue.Set();}
        }
        _Stop:{
        }

        
if(LOG&&LOG_LEVEL<=2)Debug.Log("end");
    }
}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}}



        
[SerializeField]internal bool DEBUG_EDIT=false;




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