using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
public class Pathfinder:SimActor{
[NonSerialized]Vector2Int AStarDistance=new Vector2Int(5,5);[NonSerialized]int AStarVerticalHits=3;[NonSerialized]Vector2Int gridResolution;[NonSerialized]Node[]Nodes;
protected override void Awake(){
                   base.Awake();
waitUntil2=new WaitUntil(()=>backgroundDataSet2.WaitOne(0));
waitUntil3=new WaitUntil(()=>backgroundDataSet3.WaitOne(0));
waitUntil4=new WaitUntil(()=>backgroundDataSet4.WaitOne(0));
}
[NonSerialized]Coroutine cr;[NonSerialized]Task task;[NonSerialized]readonly AutoResetEvent foregroundDataSet1=new AutoResetEvent(false);[NonSerialized]readonly ManualResetEvent backgroundDataSet1=new ManualResetEvent(true);[NonSerialized]readonly AutoResetEvent foregroundDataSet2=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet2=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent foregroundDataSet3=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet3=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent foregroundDataSet4=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet4=new AutoResetEvent(false);
protected override void OnEnable(){
                   base.OnEnable();
gridResolution=new Vector2Int(AStarDistance.x*2+1,AStarDistance.y*2+1);
Nodes=new Node[gridResolution.x*gridResolution.y*AStarVerticalHits];for(int i=0;i<Nodes.Length;i++)Nodes[i]=new Node();
if(LOG&&LOG_LEVEL<=2)Debug.Log("gridResolution:"+gridResolution+";Nodes:"+Nodes.Length);
cr=StartCoroutine(CRDoRaycasts());
int raycasts=gridResolution.x*gridResolution.y;int resultsBufferSize=raycasts*AStarVerticalHits;
Stop=false;task=Task.Factory.StartNew(BG,new object[]{LOG,LOG_LEVEL,ToSetGridVerRaycasts=new NativeList<RaycastCommand>(raycasts,Allocator.Persistent),ToSetGridVerHitsResultsBuffer=new NativeArray<RaycastHit>(resultsBufferSize,Allocator.Persistent,NativeArrayOptions.UninitializedMemory),},TaskCreationOptions.LongRunning);
}
bool Stop{
    get{bool tmp;lock(Stop_Syn){tmp=Stop_v;      }return tmp;}
    set{         lock(Stop_Syn){    Stop_v=value;}if(value){foregroundDataSet1.Set();foregroundDataSet2.Set();foregroundDataSet3.Set();foregroundDataSet4.Set();}}
}[NonSerialized]readonly object Stop_Syn=new object();[NonSerialized]bool Stop_v;
protected override void OnDisable(){
StopCoroutine(cr);
handle2.Complete();
Stop=true;try{task.Wait();}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
try{if(ToSetGridVerRaycasts.IsCreated)ToSetGridVerRaycasts.Dispose();}finally{}try{if(ToSetGridVerHitsResultsBuffer.IsCreated)ToSetGridVerHitsResultsBuffer.Dispose();}finally{}
                   base.OnDisable();
}
protected override void OnDestroy(){if(Stop){
if(LOG&&LOG_LEVEL<=2)Debug.Log("dispose");
backgroundDataSet1.Dispose();foregroundDataSet1.Dispose();
backgroundDataSet2.Dispose();foregroundDataSet2.Dispose();
backgroundDataSet3.Dispose();foregroundDataSet3.Dispose();
backgroundDataSet4.Dispose();foregroundDataSet4.Dispose();
}
                   base.OnDestroy();
}


public LinkedListNode<RaycastHit>GoTo(Ray tgtDir,float maxDis=1000){
if(Physics.Raycast(tgtDir,out RaycastHit hit,maxDis)){


    Debug.LogWarning(hit.point);
return GoToQueue.AddLast(hit);


}
return null;}    
[NonSerialized]readonly LinkedList<RaycastHit>GoToQueue=new LinkedList<RaycastHit>();
protected override void Update(){


    if(DEBUG_GOTO){DEBUG_GOTO=false;GoTo(new Ray(new Vector3(1,10,-10),Vector3.down));//GoToQueue.AddLast(new RaycastHit());
    }


    if(backgroundDataSet1.WaitOne(0)){
        if(GoToQueue.Count>0){
if(LOG&&LOG_LEVEL<=1)Debug.Log("dequeue");


            target=GoToQueue.First.Value;GoToQueue.RemoveFirst();
            startPos=transform.position;
            boundsExtents=collider.bounds.extents;

    
ToSetGridVerRaycasts.Clear();
            backgroundDataSet1.Reset();foregroundDataSet1.Set();
        }
    }




                   base.Update();
}
[NonSerialized]WaitUntil waitUntil3;
[NonSerialized]WaitUntil waitUntil2;
[NonSerialized]WaitUntil waitUntil4;
IEnumerator CRDoRaycasts(){
if(LOG&&LOG_LEVEL<=2)Debug.Log("begin");
_loop:{}

    
int vHits=0;
do{
yield return waitUntil2;
    Debug.LogWarning(vHits);
foregroundDataSet2.Set();
}while(++vHits<AStarVerticalHits);

    
yield return waitUntil3;
if(LOG&&LOG_LEVEL<=1)Debug.Log("do raycasts 2");
//if(DRAW_LEVEL<=-100)foreach(var raycast in ToSetGridVerRaycasts){Debug.DrawRay(raycast.from,raycast.direction*raycast.distance,Color.white,1f);}
//handle2=RaycastCommand.ScheduleBatch(ToSetGridVerRaycasts,ToSetGridVerHitsResultsBuffer,1,default(JobHandle));//  Schedule the batch of raycasts
//while(!handle2.IsCompleted)yield return null;handle2.Complete();//  Wait for the batch processing job to complete




////ToSetGridVerHits.Clear();
//Debug.LogWarning("ToSetGridVerHits.Count:"+ToSetGridVerHits.Count);
//RaycastHit[]hits=new RaycastHit[AStarVerticalHits];


    //foreach(var result in ToSetGridVerHitsResultsBuffer){
    //Debug.DrawRay(result.point,result.normal,Color.green,5f);
    //}


//for(int i=0,j=0;j<ToSetGridVerRaycasts.Length;i+=AStarVerticalHits,j++){var raycast=ToSetGridVerRaycasts[j];

//for(int ridx=0;ridx<AStarVerticalHits;ridx++){var result=ToSetGridVerHitsResultsBuffer[ridx+i];
//if(DRAW_LEVEL<=-100)Debug.DrawRay(result.point,result.normal,Color.green,1f);
//ToSetGridVerHits[raycast][ridx]=result;


//    Debug.LogWarning(result.collider==null);


//}

//}


//for(int i=0;i<ToSetGridVerRaycasts.Length;i++){var raycast=ToSetGridVerRaycasts[i];
//for(int ridx=0;ridx<AStarVerticalHits;ridx++){ToSetGridVerHits[raycast][ridx]=ToSetGridVerHitsResultsBuffer[i+ridx];


    
//    Debug.DrawRay(ToSetGridVerHits[raycast][ridx].point,ToSetGridVerHits[raycast][ridx].normal,Color.green,5f);
////ToSetGridVerHits[raycast]=hits;


//}
//}
//Debug.LogWarning("ToSetGridVerHits.Count:"+ToSetGridVerHits.Count);
    



foregroundDataSet3.Set();
yield return waitUntil4;
if(LOG&&LOG_LEVEL<=1)Debug.Log("do raycasts 4");




foregroundDataSet4.Set();
if(LOG&&LOG_LEVEL<=1)Debug.Log("cr loop end");
goto _loop;
}


    public bool DEBUG_GOTO;

    
[NonSerialized]JobHandle handle2;[NonSerialized]NativeList<RaycastCommand>ToSetGridVerRaycasts;[NonSerialized]NativeArray<RaycastHit>ToSetGridVerHitsResultsBuffer;[NonSerialized]readonly Dictionary<RaycastCommand,RaycastHit[]>ToSetGridVerHits=new Dictionary<RaycastCommand,RaycastHit[]>();
[NonSerialized]Vector3 NodeHalfSize;
[NonSerialized]Vector3 NodeSize;
[NonSerialized]RaycastHit target;[NonSerialized]Vector3 startPos;[NonSerialized]Vector3 boundsExtents;
void BG(object state){Thread.CurrentThread.IsBackground=false;Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;try{
    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL&&parameters[2]is NativeList<RaycastCommand>ToSetGridVerRaycasts&&parameters[3]is NativeArray<RaycastHit>ToSetGridVerHitsResultsBuffer){
int i=0,j=0;
for(Vector2Int gcoord=new Vector2Int(-AStarDistance.x,-AStarDistance.y);gcoord.x<=AStarDistance.x;gcoord.x++){
for(gcoord.y=-AStarDistance.y                                          ;gcoord.y<=AStarDistance.y;gcoord.y++){


    //Debug.LogWarning(i+"=="+GetNodeIndex(gcoord.y,0,gcoord.x));


for(int gcoordverhit=0;gcoordverhit<AStarVerticalHits;gcoordverhit++){int nodeIdx=i+gcoordverhit;
Nodes[nodeIdx].Idx=nodeIdx;
Nodes[nodeIdx].neighbours.Clear();


int idx;
    //Debug.LogWarning(nodeIdx);
    if(gcoord.x+AStarDistance.x>0&&gcoord.y+AStarDistance.y>0){
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y-1,gcoordverhit,gcoord.x-1),Nodes[idx]));
        if(gcoordverhit>0){
//Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y-1,gcoordverhit,gcoord.x-1),Nodes[idx]));
        }
    }


}


i+=AStarVerticalHits;j++;}}
List<RaycastHit[]>toSetGridVerHitsResults=new List<RaycastHit[]>();
        while(!Stop){foregroundDataSet1.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("begin pathfind");
            NodeHalfSize=boundsExtents;
            NodeHalfSize.x+=.1f;
            NodeHalfSize.z+=.1f;
            NodeSize=NodeHalfSize*2;


///*Array.Clear(Nodes,0,Nodes.Length);*/ToSetGridVerHits.Clear();//if(results.Count<)
//i=0;j=0;float fromHeight=startPos.y+(AStarVerticalHits/2f)*NodeSize.y;
//for(Vector2Int gcoord=new Vector2Int(-AStarDistance.x,-AStarDistance.y);gcoord.x<=AStarDistance.x;gcoord.x++){
//for(gcoord.y=-AStarDistance.y                                          ;gcoord.y<=AStarDistance.y;gcoord.y++){
//Vector2 gridpos=gcoord;gridpos.x*=NodeSize.x;gridpos.y*=NodeSize.z;gridpos.x+=startPos.x;gridpos.y+=startPos.z;
//var cmd=new RaycastCommand(new Vector3(gridpos.x,fromHeight,gridpos.y),Vector3.down,1000,-5,AStarVerticalHits);ToSetGridVerRaycasts.AddNoResize(cmd);
//if(j>=toSetGridVerHitsResults.Count)toSetGridVerHitsResults.Add(new RaycastHit[AStarVerticalHits]);
//ToSetGridVerHits[cmd]=toSetGridVerHitsResults[j];
//if(LOG&&LOG_LEVEL<=-100)Debug.Log(i+"=="+GetNodeIndex(gcoord.y,0,gcoord.x)+": "+ToSetGridVerRaycasts[i/AStarVerticalHits].from);


//i+=AStarVerticalHits;j++;}}
//Array.Clear(Nodes,0,Nodes.Length);bool disChanged=false;if(disChanged=AStarDis_Pre!=AStarDistance){ToSetGridVerHits.Clear();AStarDis_Pre=AStarDistance;}
//
//for(Vector2Int gcoord=new Vector2Int(-AStarDistance.x,-AStarDistance.y);gcoord.x<=AStarDistance.x;gcoord.x++){
//for(gcoord.y=-AStarDistance.y                                          ;gcoord.y<=AStarDistance.y;gcoord.y++){
//Vector2 gridpos=gcoord;gridpos.x*=NodeSize.x;gridpos.y*=NodeSize.z;
//var cmd=new RaycastCommand(new Vector3(gridpos.x,fromHeight,gridpos.y),Vector3.down,1000,-5,AStarVerticalHits);ToSetGridVerRaycasts.AddNoResize(cmd);
//if(disChanged)ToSetGridVerHits.Add(cmd,new RaycastHit[AStarVerticalHits])else;
    

//    //Debug.LogWarning(i+" "+gcoord);
//if(LOG&&LOG_LEVEL<=-100)Debug.Log(i+"=="+GetNodeIndex(gcoord.y,0,gcoord.x)+": "+ToSetGridVerRaycasts[i/AStarVerticalHits].from);


//i+=AStarVerticalHits;}}



            
//ToSetGridVerHits.Clear();
//int i=0;RaycastHit[]hits=new RaycastHit[AStarVerticalHits];
//foreach(var raycast in ToSetGridVerRaycasts){
//for(int ridx=0;ridx<AStarVerticalHits;ridx++)hits[ridx]=ToSetGridVerHitsResultsBuffer[i+ridx];
//ToSetGridVerHits.Add(raycast,hits);
//i++;}


int vHits=0;
do{
    Debug.LogWarning(vHits);
            backgroundDataSet2.Set();foregroundDataSet2.WaitOne();if(Stop)goto _Stop;
}while(++vHits<AStarVerticalHits);


            backgroundDataSet3.Set();foregroundDataSet3.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 2");


//i=0;j=0;foreach(var result in ToSetGridVerHits){
//if(LOG&&LOG_LEVEL<=-100)Debug.Log(i+": "+result.Key.from);


//for(int ridx=0;ridx<AStarVerticalHits;ridx++){int nodeIdx=i+ridx;var hit=result.Value[ridx];
//if(float.IsNaN(hit.normal.x)||float.IsNaN(hit.normal.y)||float.IsNaN(hit.normal.z)||hit.normal==Vector3.zero||hit.normal.magnitude>1.00001f||hit.normal.magnitude<0.99999f){
//Nodes[nodeIdx].valid=false;
//}else{
//Nodes[nodeIdx].valid=true;
//Nodes[nodeIdx].Position=hit.point;Nodes[nodeIdx].Normal=hit.normal.normalized;
//}
//}


//i+=AStarVerticalHits;j++;}//for(i=0,j=0;j<ToSetGridVerHits.Count;i+=AStarVerticalHits,j++){


//for(int ridx=0;ridx<AStarVerticalHits;ridx++){
//}


//}//foreach(var result in ToSetGridVerHits){

//if(LOG&&LOG_LEVEL<=-100)Debug.Log(i+": "+result.Key.from);

//RaycastHit[]hits=result.Value;for(int ridx=0;ridx<AStarVerticalHits;ridx++){var hit=hits[ridx];

//    //Debug.LogWarning(hit.normal);
//if(hit.normal==Vector3.zero)continue;if(float.IsNaN(hit.normal.x)||float.IsNaN(hit.normal.y)||float.IsNaN(hit.normal.z))continue;

//    //Debug.LogWarning(hit.point);

//    Nodes[i]=new Node(hit.point);

//i++;}
//i+=AStarVerticalHits;}
//for(int rayidx=0;rayidx<ToSetGridVerRaycasts.Length;rayidx++){
//for(int ridx=0;ridx<AStarVerticalHits;ridx++){
//i++;}

//    Debug.LogWarning(i+": "+ToSetGridVerRaycasts[i/AStarVerticalHits].from);

//}
//i=0;foreach(var result in ToSetGridVerHits){for(int ridx=0;ridx<AStarVerticalHits;ridx++){

//    Debug.LogWarning(i);

//i++;}}


            backgroundDataSet4.Set();foregroundDataSet4.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 4");
backgroundDataSet1.Set();}
int GetNodeIndex(int cz,int vy,int cx){return (cx+AStarDistance.x)*gridResolution.y*AStarVerticalHits+(cz+AStarDistance.y)*AStarVerticalHits+vy;}
        _Stop:{
        }
if(LOG&&LOG_LEVEL<=2)Debug.Log("end");
    }
}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}}



    
#if UNITY_EDITOR
protected override void OnDrawGizmos(){
    if(backgroundDataSet1.WaitOne(0)){
if(DRAW_LEVEL<=-100){
var oldcolor=Gizmos.color;
Gizmos.color=new Color(1,1,1,.25f);
//if(DRAW_LEVEL<=-110)foreach(var node in Nodes)foreach(var neighbour in node.neighbours)if(node.valid&&neighbour.node.valid)Debug.DrawLine(node.Position,neighbour.node.Position,Color.white,1f);
if(Nodes!=null)foreach(var node in Nodes){
if(node.valid){
Gizmos.DrawCube(node.Position,NodeSize);
if(DRAW_LEVEL<=-110)foreach(var neighbour in node.neighbours)if(neighbour.node.valid)Debug.DrawLine(node.Position,neighbour.node.Position,Color.white);
}
}
Gizmos.color=oldcolor;
}
    }
                   base.OnDrawGizmos();
}
#endif
[Serializable]public class Node:IHeapItem<Node>{
public bool valid{get;set;}public int Idx{get;set;}public readonly List<(int idx,Node node)>neighbours=new List<(int,Node)>();
public int HeapIndex{get;set;}
public float F{get;private set;}//  heuristics
public float G{get{return g;}set{g=value;F=g+h;}}float g;//  node dis to start
public float H{get{return h;}set{h=value;F=g+h;}}float h;//  node dis to target
public int CompareTo(Node toCompare){
int comparison=F.CompareTo(toCompare.F);
 if(comparison==0){
    comparison=H.CompareTo(toCompare.H);
 }
return -comparison;}
public Vector3 Position{get;set;}public Vector3 Normal{get;set;}
public override int GetHashCode(){return Position.GetHashCode();}public override bool Equals(object obj){if(ReferenceEquals(this,obj))return true;if(!(obj is Node node))return false;return(Position==node.Position);}
public Node Parent;
}
}