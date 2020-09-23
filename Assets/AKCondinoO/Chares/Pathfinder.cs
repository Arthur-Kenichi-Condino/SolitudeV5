using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
public class Pathfinder:SimActor{
[NonSerialized]Vector2Int AStarDistance=new Vector2Int(10,10);[NonSerialized]int AStarVerticalHits=3;[NonSerialized]Vector2Int gridResolution;[NonSerialized]Node[]Nodes;[NonSerialized]Node originNode;[NonSerialized]Node targetNode;
protected override void Awake(){
                   base.Awake();
waitUntil2=new WaitUntil(()=>backgroundDataSet2.WaitOne(0));
waitUntil3a=new WaitUntil(()=>backgroundDataSet3a.WaitOne(0));
waitUntil3b=new WaitUntil(()=>backgroundDataSet3b.WaitOne(0));
waitUntil3c=new WaitUntil(()=>backgroundDataSet3c.WaitOne(0));
waitUntil4=new WaitUntil(()=>backgroundDataSet4.WaitOne(0));
}
[NonSerialized]Coroutine cr;[NonSerialized]Task task;[NonSerialized]readonly AutoResetEvent foregroundDataSet1=new AutoResetEvent(false);[NonSerialized]readonly ManualResetEvent backgroundDataSet1=new ManualResetEvent(true);[NonSerialized]readonly AutoResetEvent foregroundDataSet2=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet2=new AutoResetEvent(false);
[NonSerialized]readonly AutoResetEvent foregroundDataSet3a=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet3a=new AutoResetEvent(false);
[NonSerialized]readonly AutoResetEvent foregroundDataSet3b=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet3b=new AutoResetEvent(false);
[NonSerialized]readonly AutoResetEvent foregroundDataSet3c=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet3c=new AutoResetEvent(false);
[NonSerialized]readonly AutoResetEvent foregroundDataSet4=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet4=new AutoResetEvent(false);
protected override void OnEnable(){
                   base.OnEnable();
backgroundDataSet1.Set();foregroundDataSet1.Reset();
backgroundDataSet2.Reset();foregroundDataSet2.Reset();
backgroundDataSet3a.Reset();foregroundDataSet3a.Reset();
backgroundDataSet3b.Reset();foregroundDataSet3b.Reset();
backgroundDataSet3c.Reset();foregroundDataSet3c.Reset();
backgroundDataSet4.Reset();foregroundDataSet4.Reset();
noCharLayer=~(1<<LayerMask.NameToLayer("Char")|1<<LayerMask.NameToLayer("Ignore Raycast"));
if(LOG&&LOG_LEVEL<=2)Debug.Log("noCharLayer:"+noCharLayer+"[1<<LayerMask.NameToLayer(\"Ignore Raycast\")=="+(1<<LayerMask.NameToLayer("Ignore Raycast"))+";1<<LayerMask.NameToLayer(\"Char\")=="+(1<<LayerMask.NameToLayer("Char")));
gridResolution=new Vector2Int(AStarDistance.x*2+1,AStarDistance.y*2+1);
Nodes=new Node[gridResolution.x*gridResolution.y*AStarVerticalHits];for(int i=0;i<Nodes.Length;i++)Nodes[i]=new Node();
ClosedNodes=new Heap<Node>(Nodes.Length);ClosedNodes.LOG=LOG;ClosedNodes.LOG_LEVEL=LOG_LEVEL;
  OpenNodes=new Heap<Node>(Nodes.Length);  OpenNodes.LOG=LOG;  OpenNodes.LOG_LEVEL=LOG_LEVEL;
int raycasts=gridResolution.x*gridResolution.y;int resultsBufferSize=raycasts*AStarVerticalHits;
if(LOG&&LOG_LEVEL<=2)Debug.Log("gridResolution:"+gridResolution+";Nodes:"+Nodes.Length+";raycasts:"+raycasts+";resultsBufferSize:"+resultsBufferSize);
resultsManaged3a.Clear();resultsManaged3a.Capacity=resultsBufferSize;
resultsManaged3b.Clear();resultsManaged3b.Capacity=resultsBufferSize;
cr=StartCoroutine(CRDoRaycasts());
Stop=false;task=Task.Factory.StartNew(BG,new object[]{LOG,LOG_LEVEL,ToSetGridVerRaycasts=new NativeList<RaycastCommand>(raycasts,Allocator.Persistent),ToSetGridVerHitsResultsBuffer=new NativeArray<RaycastHit>(raycasts,Allocator.Persistent,NativeArrayOptions.UninitializedMemory),
commands3a=new NativeList<BoxcastCommand>(resultsBufferSize,Allocator.Persistent),results3a=new NativeArray<RaycastHit>(resultsBufferSize,Allocator.Persistent,NativeArrayOptions.UninitializedMemory),
commands3b=new NativeList<BoxcastCommand>(resultsBufferSize,Allocator.Persistent),results3b=new NativeArray<RaycastHit>(resultsBufferSize,Allocator.Persistent,NativeArrayOptions.UninitializedMemory),
commands3ciA=new NativeList<BoxcastCommand>(Allocator.Persistent),results3ciA=new NativeList<RaycastHit>(Allocator.Persistent),
commands3ciB=new NativeList<RaycastCommand>(Allocator.Persistent),results3ciB=new NativeList<RaycastHit>(Allocator.Persistent),
commands3ciiA1=new NativeList<BoxcastCommand>(Allocator.Persistent),results3ciiA1=new NativeList<RaycastHit>(Allocator.Persistent),
commands3ciiB1=new NativeList<RaycastCommand>(Allocator.Persistent),results3ciiB1=new NativeList<RaycastHit>(Allocator.Persistent),
commands3ciiA2=new NativeList<BoxcastCommand>(Allocator.Persistent),results3ciiA2=new NativeList<RaycastHit>(Allocator.Persistent),
commands3ciiB2=new NativeList<RaycastCommand>(Allocator.Persistent),results3ciiB2=new NativeList<RaycastHit>(Allocator.Persistent),},TaskCreationOptions.LongRunning);
}
bool Stop{
    get{bool tmp;lock(Stop_Syn){tmp=Stop_v;      }return tmp;}
    set{         lock(Stop_Syn){    Stop_v=value;}if(value){foregroundDataSet1.Set();foregroundDataSet2.Set();
foregroundDataSet3a.Set();
foregroundDataSet3b.Set();
foregroundDataSet3c.Set();
foregroundDataSet4.Set();}}
}[NonSerialized]readonly object Stop_Syn=new object();[NonSerialized]bool Stop_v;
protected override void OnDisable(){
StopCoroutine(cr);
handle2.Complete();
handle3a.Complete();
handle3b.Complete();
handle3ciA.Complete();
handle3ciB.Complete();
handle3ciiA1.Complete();
handle3ciiB1.Complete();
handle3ciiA2.Complete();
handle3ciiB2.Complete();
Stop=true;try{task.Wait();}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
try{if(ToSetGridVerRaycasts.IsCreated)ToSetGridVerRaycasts.Dispose();}finally{}try{if(ToSetGridVerHitsResultsBuffer.IsCreated)ToSetGridVerHitsResultsBuffer.Dispose();}finally{}
try{if(commands3a.IsCreated)commands3a.Dispose();}finally{}try{if(results3a.IsCreated)results3a.Dispose();}finally{}
try{if(commands3b.IsCreated)commands3b.Dispose();}finally{}try{if(results3b.IsCreated)results3b.Dispose();}finally{}
try{if(commands3ciA.IsCreated)commands3ciA.Dispose();}finally{}try{if(results3ciA.IsCreated)results3ciA.Dispose();}finally{}
try{if(commands3ciB.IsCreated)commands3ciB.Dispose();}finally{}try{if(results3ciB.IsCreated)results3ciB.Dispose();}finally{}
try{if(commands3ciiA1.IsCreated)commands3ciiA1.Dispose();}finally{}try{if(results3ciiA1.IsCreated)results3ciiA1.Dispose();}finally{}
try{if(commands3ciiB1.IsCreated)commands3ciiB1.Dispose();}finally{}try{if(results3ciiB1.IsCreated)results3ciiB1.Dispose();}finally{}
try{if(commands3ciiA2.IsCreated)commands3ciiA2.Dispose();}finally{}try{if(results3ciiA2.IsCreated)results3ciiA2.Dispose();}finally{}
try{if(commands3ciiB2.IsCreated)commands3ciiB2.Dispose();}finally{}try{if(results3ciiB2.IsCreated)results3ciiB2.Dispose();}finally{}
                   base.OnDisable();
}
protected override void OnDestroy(){if(Stop){
if(LOG&&LOG_LEVEL<=2)Debug.Log("dispose");
backgroundDataSet1.Dispose();foregroundDataSet1.Dispose();
backgroundDataSet2.Dispose();foregroundDataSet2.Dispose();
backgroundDataSet3a.Dispose();foregroundDataSet3a.Dispose();
backgroundDataSet3b.Dispose();foregroundDataSet3b.Dispose();
backgroundDataSet3c.Dispose();foregroundDataSet3c.Dispose();
backgroundDataSet4.Dispose();foregroundDataSet4.Dispose();
}
                   base.OnDestroy();
}
public LinkedListNode<RaycastHit>GoTo(Ray tgtDir,float maxDis=1000){
if(Physics.Raycast(tgtDir,out RaycastHit hit,maxDis)){
if(LOG&&LOG_LEVEL<=2)Debug.Log("GoTo:"+hit.point);
return GoToQueue.AddLast(hit);
}
return null;}    
protected void MoveToRandom(System.Random mathrandom){
    GoTo(new Ray(transform.position+new Vector3(mathrandom.Next(-16,17),16,mathrandom.Next(-16,17)),Vector3.down));
}
[NonSerialized]readonly protected Queue<(Vector3 pos,Node.PreferredReachableMode mode)>CurPath=new Queue<(Vector3,Node.PreferredReachableMode)>();[NonSerialized]protected(Vector3 pos,Node.PreferredReachableMode mode)?CurPathTgt=null;
[NonSerialized]readonly LinkedList<RaycastHit>GoToQueue=new LinkedList<RaycastHit>();[NonSerialized]bool tracing;
protected override void Update(){


    //if(Input.GetKeyDown(KeyCode.C))DEBUG_GOTO=true;
    if(DEBUG_GOTO){DEBUG_GOTO=false;GoTo(new Ray(new Vector3(1,0,-10),Vector3.down));//GoToQueue.AddLast(new RaycastHit());
        Debug.DrawRay(new Vector3(1,0,-10),Vector3.down*1000,Color.white,5f);
    }


    if(backgroundDataSet1.WaitOne(0)){
        if(tracing){
if(LOG&&LOG_LEVEL<=1)Debug.Log("path traced, set CurPath");
            CurPathTgt=null;CurPath.Clear();for(int p=0;p<resultPath.Count;p++){CurPath.Enqueue((resultPath[p].Position,resultPath[p].Mode));}
            tracing=false;
        }
        if(GoToQueue.Count>0){
if(LOG&&LOG_LEVEL<=1)Debug.Log("dequeue");


            tracing=true;
            target=GoToQueue.First.Value;GoToQueue.RemoveFirst();
            startPos=transform.position;startForward=transform.forward;
            boundsExtents=collider.bounds.extents;

    
if(commands3a.IsCreated)commands3a.Clear();
if(commands3b.IsCreated)commands3b.Clear();
if(commands3ciA.IsCreated)commands3ciA.Clear();if(results3ciA.IsCreated)results3ciA.Clear();
if(commands3ciB.IsCreated)commands3ciB.Clear();if(results3ciB.IsCreated)results3ciB.Clear();
if(commands3ciiA1.IsCreated)commands3ciiA1.Clear();if(results3ciiA1.IsCreated)results3ciiA1.Clear();
if(commands3ciiB1.IsCreated)commands3ciiB1.Clear();if(results3ciiB1.IsCreated)results3ciiB1.Clear();
if(commands3ciiA2.IsCreated)commands3ciiA2.Clear();if(results3ciiA2.IsCreated)results3ciiA2.Clear();
if(commands3ciiB2.IsCreated)commands3ciiB2.Clear();if(results3ciiB2.IsCreated)results3ciiB2.Clear();
            backgroundDataSet1.Reset();foregroundDataSet1.Set();
        }
    }
                   base.Update();
}
protected virtual void STOP(bool clearQueue=true){
CurPathTgt=null;CurPath.Clear();if(clearQueue)GoToQueue.Clear();
}
[NonSerialized]WaitUntil waitUntil2;
[NonSerialized]WaitUntil waitUntil3a;
[NonSerialized]WaitUntil waitUntil3b;
[NonSerialized]WaitUntil waitUntil3c;
[NonSerialized]WaitUntil waitUntil4;
IEnumerator CRDoRaycasts(){
if(LOG&&LOG_LEVEL<=2)Debug.Log("begin");
_loop:{}
if(LOG&&LOG_LEVEL<=2)Debug.Log("ToSetGridVerHits.Count before phase id '2':"+ToSetGridVerHits.Count);
int vHits=0;
do{
yield return waitUntil2;
if(LOG&&LOG_LEVEL<=1)Debug.Log("current vertical hit test:"+vHits);
if(DRAW_LEVEL<=-100)foreach(var raycast in ToSetGridVerRaycasts){Debug.DrawRay(raycast.from,raycast.direction*raycast.distance,Color.white,1f);}
handle2=RaycastCommand.ScheduleBatch(ToSetGridVerRaycasts,ToSetGridVerHitsResultsBuffer,1,default(JobHandle));//  Schedule the batch of raycasts
while(!handle2.IsCompleted)yield return null;handle2.Complete();//  Wait for the batch processing job to complete
for(int i=0,j=0;j<ToSetGridVerRaycasts.Length;i+=AStarVerticalHits,j++){var raycast=ToSetGridVerRaycasts[j];var result=ToSetGridVerHitsResultsBuffer[j];
if(DRAW_LEVEL<=-100)Debug.DrawRay(result.point,result.normal,Color.green,1f);
if(result.collider!=null){
ToSetGridVerHits[j][vHits]=result;}else{ 
ToSetGridVerHits[j][vHits]=default(RaycastHit);}
}
ToSetGridVerRaycasts.Clear();
foregroundDataSet2.Set();
}while(++vHits<AStarVerticalHits);    
yield return waitUntil3a;
if(LOG&&LOG_LEVEL<=2)Debug.Log("do raycasts 3a;commands3a.Length/results3a.Length:"+commands3a.Length+"/"+results3a.Length+";ToSetGridVerHits.Count after phase id '2' when different from 0 should then stay constant:"+ToSetGridVerHits.Count);
handle3a=BoxcastCommand.ScheduleBatch(commands3a,results3a,1,default(JobHandle));
while(!handle3a.IsCompleted)yield return null;handle3a.Complete();
resultsManaged3a.Clear();for(int c=0;c<commands3a.Length;c++)resultsManaged3a.Add((results3a[c],results3a[c].collider!=null));
foregroundDataSet3a.Set();
yield return waitUntil3b;
if(LOG&&LOG_LEVEL<=2)Debug.Log("do raycasts 3b");
handle3b=BoxcastCommand.ScheduleBatch(commands3b,results3b,1,default(JobHandle));
while(!handle3b.IsCompleted)yield return null;handle3b.Complete();
resultsManaged3b.Clear();for(int c=0;c<commands3b.Length;c++)resultsManaged3b.Add((results3b[c],results3b[c].collider!=null));




foregroundDataSet3b.Set();
yield return waitUntil3c;
if(LOG&&LOG_LEVEL<=2)Debug.Log("do raycasts 3c");


handle3ciA=BoxcastCommand.ScheduleBatch(commands3ciA,results3ciA,1,default(JobHandle));
handle3ciB=RaycastCommand.ScheduleBatch(commands3ciB,results3ciB,1,default(JobHandle));
handle3ciiA1=BoxcastCommand.ScheduleBatch(commands3ciiA1,results3ciiA1,1,default(JobHandle));
handle3ciiB1=RaycastCommand.ScheduleBatch(commands3ciiB1,results3ciiB1,1,default(JobHandle));
handle3ciiA2=BoxcastCommand.ScheduleBatch(commands3ciiA2,results3ciiA2,1,default(JobHandle));
handle3ciiB2=RaycastCommand.ScheduleBatch(commands3ciiB2,results3ciiB2,1,default(JobHandle));
while(!handle3ciA.IsCompleted)yield return null;handle3ciA.Complete();
while(!handle3ciB.IsCompleted)yield return null;handle3ciB.Complete();
while(!handle3ciiA1.IsCompleted)yield return null;handle3ciiA1.Complete();
while(!handle3ciiB1.IsCompleted)yield return null;handle3ciiB1.Complete();
while(!handle3ciiA2.IsCompleted)yield return null;handle3ciiA2.Complete();
while(!handle3ciiB2.IsCompleted)yield return null;handle3ciiB2.Complete();
int loopLimitToYield=Mathf.Max(gridResolution.x,gridResolution.y)*AStarVerticalHits*26,countToYield=0;//  <- This keeps FPS in a good mood
if(LOG&&LOG_LEVEL<=2)Debug.Log("yield every "+loopLimitToYield+" objects processed");var watch=System.Diagnostics.Stopwatch.StartNew();
resultsManaged3ciA.Clear();
resultsManaged3ciB.Clear();
resultsManaged3ciiA1.Clear();
resultsManaged3ciiB1.Clear();
resultsManaged3ciiA2.Clear();
resultsManaged3ciiB2.Clear();for(int c=0;c<commands3ciA.Length;c++){resultsManaged3ciA.Add((results3ciA[c],results3ciA[c].collider!=null));
                                                                    resultsManaged3ciB.Add((results3ciB[c],results3ciB[c].collider!=null));
                                                                    resultsManaged3ciiA1.Add((results3ciiA1[c],results3ciiA1[c].collider!=null));
                                                                    resultsManaged3ciiB1.Add((results3ciiB1[c],results3ciiB1[c].collider!=null));
                                                                    resultsManaged3ciiA2.Add((results3ciiA2[c],results3ciiA2[c].collider!=null));
                                                                    resultsManaged3ciiB2.Add((results3ciiB2[c],results3ciiB2[c].collider!=null));
if(++countToYield>loopLimitToYield){yield return null;countToYield=0;}}
if(LOG&&LOG_LEVEL<=2)Debug.Log("took "+watch.ElapsedMilliseconds+" ms");


foregroundDataSet3c.Set();
yield return waitUntil4;
if(LOG&&LOG_LEVEL<=2)Debug.Log("do raycasts 4");




foregroundDataSet4.Set();
if(LOG&&LOG_LEVEL<=2)Debug.Log("cr loop end");
goto _loop;
}
[NonSerialized]Vector3 NodeHalfSize;
[NonSerialized]Vector3 NodeSize;
[NonSerialized]JobHandle handle2;[NonSerialized]NativeList<RaycastCommand>ToSetGridVerRaycasts;[NonSerialized]NativeArray<RaycastHit>ToSetGridVerHitsResultsBuffer;[NonSerialized]readonly Dictionary<int,RaycastHit[]>ToSetGridVerHits=new Dictionary<int,RaycastHit[]>();
[NonSerialized]JobHandle handle3a;[NonSerialized]NativeList<BoxcastCommand>commands3a;[NonSerialized]NativeArray<RaycastHit>results3a;[NonSerialized]readonly List<(RaycastHit hit,bool colliderNotNull)>resultsManaged3a=new List<(RaycastHit,bool)>();
[NonSerialized]JobHandle handle3b;[NonSerialized]NativeList<BoxcastCommand>commands3b;[NonSerialized]NativeArray<RaycastHit>results3b;[NonSerialized]readonly List<(RaycastHit hit,bool colliderNotNull)>resultsManaged3b=new List<(RaycastHit,bool)>();
[NonSerialized]Dictionary<int,(Node,int)>resultToNodeAndNeighbour;
[NonSerialized]JobHandle handle3ciA;[NonSerialized]NativeList<BoxcastCommand>commands3ciA;[NonSerialized]NativeList<RaycastHit>results3ciA;[NonSerialized]readonly List<(RaycastHit hit,bool colliderNotNull)>resultsManaged3ciA=new List<(RaycastHit,bool)>();
[NonSerialized]JobHandle handle3ciB;[NonSerialized]NativeList<RaycastCommand>commands3ciB;[NonSerialized]NativeList<RaycastHit>results3ciB;[NonSerialized]readonly List<(RaycastHit hit,bool colliderNotNull)>resultsManaged3ciB=new List<(RaycastHit,bool)>();
[NonSerialized]JobHandle handle3ciiA1;[NonSerialized]NativeList<BoxcastCommand>commands3ciiA1;[NonSerialized]NativeList<RaycastHit>results3ciiA1;[NonSerialized]readonly List<(RaycastHit hit,bool colliderNotNull)>resultsManaged3ciiA1=new List<(RaycastHit,bool)>();
[NonSerialized]JobHandle handle3ciiB1;[NonSerialized]NativeList<RaycastCommand>commands3ciiB1;[NonSerialized]NativeList<RaycastHit>results3ciiB1;[NonSerialized]readonly List<(RaycastHit hit,bool colliderNotNull)>resultsManaged3ciiB1=new List<(RaycastHit,bool)>();
[NonSerialized]JobHandle handle3ciiA2;[NonSerialized]NativeList<BoxcastCommand>commands3ciiA2;[NonSerialized]NativeList<RaycastHit>results3ciiA2;[NonSerialized]readonly List<(RaycastHit hit,bool colliderNotNull)>resultsManaged3ciiA2=new List<(RaycastHit,bool)>();
[NonSerialized]JobHandle handle3ciiB2;[NonSerialized]NativeList<RaycastCommand>commands3ciiB2;[NonSerialized]NativeList<RaycastHit>results3ciiB2;[NonSerialized]readonly List<(RaycastHit hit,bool colliderNotNull)>resultsManaged3ciiB2=new List<(RaycastHit,bool)>();
[NonSerialized]readonly List<(int idx,Node node,RaycastHit floorHit)>nodesGrounded=new List<(int,Node,RaycastHit)>();
[NonSerialized]readonly List<(int idx,Node node,RaycastHit floorHit)>nodesWalkable=new List<(int,Node,RaycastHit)>();
[NonSerialized]readonly List<(int idx,Node node,RaycastHit obstacleHit)>nodesObstructed=new List<(int,Node,RaycastHit)>();
[NonSerialized]Heap<Node>ClosedNodes;
[NonSerialized]Heap<Node>OpenNodes;
[NonSerialized]public static readonly object tasksBusyCount_Syn=new object();[NonSerialized]public static int tasksBusyCount=0;[NonSerialized]public static readonly AutoResetEvent queue=new AutoResetEvent(true);
[NonSerialized]RaycastHit target;[NonSerialized]Vector3 startPos;[NonSerialized]Vector3 startForward;[NonSerialized]Vector3 boundsExtents;[NonSerialized]bool preferClimbing=false;[NonSerialized]List<Node>resultPath=new List<Node>(0);
void BG(object state){Thread.CurrentThread.IsBackground=false;Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;try{
    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL&&parameters[2]is NativeList<RaycastCommand>ToSetGridVerRaycasts&&parameters[3]is NativeArray<RaycastHit>ToSetGridVerHitsResultsBuffer
&&parameters[4]is NativeList<BoxcastCommand>commands3a&&parameters[5]is NativeArray<RaycastHit>results3a
&&parameters[6]is NativeList<BoxcastCommand>commands3b&&parameters[7]is NativeArray<RaycastHit>results3b){
List<RaycastHit[]>ToSetGridVerHitsResults=new List<RaycastHit[]>();
int i=0,j=0;int tnc=0;
for(Vector2Int gcoord=new Vector2Int(-AStarDistance.x,-AStarDistance.y);gcoord.x<=AStarDistance.x;gcoord.x++){
for(gcoord.y=-AStarDistance.y                                          ;gcoord.y<=AStarDistance.y;gcoord.y++){
if(j>=ToSetGridVerHitsResults.Count)ToSetGridVerHitsResults.Add(new RaycastHit[AStarVerticalHits]);
for(int gcoordverhit=0;gcoordverhit<AStarVerticalHits;gcoordverhit++){int nodeIdx=i+gcoordverhit;
Nodes[nodeIdx].Idx=nodeIdx;
Nodes[nodeIdx].neighbours.Clear();
int idx;
    if(gcoordverhit>0)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y,gcoordverhit-1,gcoord.x),Nodes[idx]));
    if(gcoordverhit<AStarVerticalHits-1)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y,gcoordverhit+1,gcoord.x),Nodes[idx]));
    if(gcoord.x>-AStarDistance.x){
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y,gcoordverhit,gcoord.x-1),Nodes[idx]));
        if(gcoordverhit>0)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y,gcoordverhit-1,gcoord.x-1),Nodes[idx]));
        if(gcoordverhit<AStarVerticalHits-1)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y,gcoordverhit+1,gcoord.x-1),Nodes[idx]));
    }
    if(gcoord.y>-AStarDistance.y){
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y-1,gcoordverhit,gcoord.x),Nodes[idx]));
        if(gcoordverhit>0)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y-1,gcoordverhit-1,gcoord.x),Nodes[idx]));
        if(gcoordverhit<AStarVerticalHits-1)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y-1,gcoordverhit+1,gcoord.x),Nodes[idx]));
    }
    if(gcoord.x>-AStarDistance.x&&gcoord.y>-AStarDistance.y){
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y-1,gcoordverhit,gcoord.x-1),Nodes[idx]));
        if(gcoordverhit>0)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y-1,gcoordverhit-1,gcoord.x-1),Nodes[idx]));
        if(gcoordverhit<AStarVerticalHits-1)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y-1,gcoordverhit+1,gcoord.x-1),Nodes[idx]));
    }
    if(gcoord.x<AStarDistance.x){
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y,gcoordverhit,gcoord.x+1),Nodes[idx]));
        if(gcoordverhit>0)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y,gcoordverhit-1,gcoord.x+1),Nodes[idx]));
        if(gcoordverhit<AStarVerticalHits-1)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y,gcoordverhit+1,gcoord.x+1),Nodes[idx]));
    }
    if(gcoord.y<AStarDistance.y){
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y+1,gcoordverhit,gcoord.x),Nodes[idx]));
        if(gcoordverhit>0)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y+1,gcoordverhit-1,gcoord.x),Nodes[idx]));
        if(gcoordverhit<AStarVerticalHits-1)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y+1,gcoordverhit+1,gcoord.x),Nodes[idx]));
    }
    if(gcoord.x<AStarDistance.x&&gcoord.y<AStarDistance.y){
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y+1,gcoordverhit,gcoord.x+1),Nodes[idx]));
        if(gcoordverhit>0)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y+1,gcoordverhit-1,gcoord.x+1),Nodes[idx]));
        if(gcoordverhit<AStarVerticalHits-1)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y+1,gcoordverhit+1,gcoord.x+1),Nodes[idx]));
    }
    if(gcoord.x>-AStarDistance.x&&gcoord.y<AStarDistance.y){
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y+1,gcoordverhit,gcoord.x-1),Nodes[idx]));
        if(gcoordverhit>0)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y+1,gcoordverhit-1,gcoord.x-1),Nodes[idx]));
        if(gcoordverhit<AStarVerticalHits-1)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y+1,gcoordverhit+1,gcoord.x-1),Nodes[idx]));
    }
    if(gcoord.y>-AStarDistance.y&&gcoord.x<AStarDistance.x){
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y-1,gcoordverhit,gcoord.x+1),Nodes[idx]));
        if(gcoordverhit>0)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y-1,gcoordverhit-1,gcoord.x+1),Nodes[idx]));
        if(gcoordverhit<AStarVerticalHits-1)
Nodes[nodeIdx].neighbours.Add((idx=GetNodeIndex(gcoord.y-1,gcoordverhit+1,gcoord.x+1),Nodes[idx]));
    }
Nodes[nodeIdx].neighbourCanBeReached.Clear();Nodes[nodeIdx].neighbourCanBeReached.AddRange(Enumerable.Repeat((true,Node.PreferredReachableMode.walk),Nodes[nodeIdx].neighbours.Count));tnc+=Nodes[nodeIdx].neighbours.Count;
}
i+=AStarVerticalHits;j++;}}
resultToNodeAndNeighbour=new Dictionary<int,(Node,int)>(tnc);
commands3ciA.Capacity=tnc;results3ciA.Capacity=tnc;
commands3ciB.Capacity=tnc;results3ciB.Capacity=tnc;
commands3ciiA1.Capacity=tnc;results3ciiA1.Capacity=tnc;
commands3ciiB1.Capacity=tnc;results3ciiB1.Capacity=tnc;
commands3ciiA2.Capacity=tnc;results3ciiA2.Capacity=tnc;
commands3ciiB2.Capacity=tnc;results3ciiB2.Capacity=tnc;
for(int nodeIdx=0;nodeIdx<Nodes.Length;nodeIdx++){
Nodes[nodeIdx].indexOfMe.Clear();var node=Nodes[nodeIdx];
for(int n=0;n<node.neighbours.Count;n++){var neighbour=node.neighbours[n].node;
int indexOfMe=-1;for(int nn=0;nn<neighbour.neighbours.Count;nn++){if(neighbour.neighbours[nn].node==node)indexOfMe=nn;}node.indexOfMe.Add(indexOfMe);
}
}
        while(!Stop){foregroundDataSet1.WaitOne();if(Stop)goto _Stop;
lock(tasksBusyCount_Syn){tasksBusyCount++;}queue.WaitOne(tasksBusyCount*100);
if(LOG&&LOG_LEVEL<=2)Debug.Log("begin pathfind");
            NodeHalfSize=boundsExtents;
            NodeHalfSize.x+=.1f;
            NodeHalfSize.z+=.1f;
            NodeSize=NodeHalfSize*2;
ToSetGridVerHits.Clear();var gridStartHeight=startPos.y+(AStarVerticalHits/2f)*NodeSize.y;var disableCommandHeight=-Mathf.Ceil(Chunk.Height/2f)-1;
if(LOG&&LOG_LEVEL<=1)Debug.Log("disableCommandHeight value:"+disableCommandHeight);
int vHits=0;
do{
if(LOG&&LOG_LEVEL<=1)Debug.Log("current vertical hit test:"+vHits);
i=0;j=0;float fromHeight;
for(Vector2Int gcoord=new Vector2Int(-AStarDistance.x,-AStarDistance.y);gcoord.x<=AStarDistance.x;gcoord.x++){
for(gcoord.y=-AStarDistance.y                                          ;gcoord.y<=AStarDistance.y;gcoord.y++){
if(vHits>0){
if(ToSetGridVerHits[j][vHits-1].normal!=Vector3.zero){
fromHeight=ToSetGridVerHits[j][vHits-1].point.y-.1f;
}else{
fromHeight=disableCommandHeight;
}
}else{
fromHeight=gridStartHeight;
}
Vector2 gridpos=gcoord;gridpos.x*=NodeSize.x;gridpos.y*=NodeSize.z;gridpos.x+=startPos.x;gridpos.y+=startPos.z;var cmd=new RaycastCommand(new Vector3(gridpos.x,fromHeight,gridpos.y),Vector3.down,1000,noCharLayer);ToSetGridVerRaycasts.AddNoResize(cmd);
if(vHits==0)ToSetGridVerHits[j]=ToSetGridVerHitsResults[j];
if(LOG&&LOG_LEVEL<=-100)Debug.Log(i+"=="+GetNodeIndex(gcoord.y,0,gcoord.x)+": "+ToSetGridVerRaycasts[i/AStarVerticalHits].from);
i+=AStarVerticalHits;j++;}}
            backgroundDataSet2.Set();foregroundDataSet2.WaitOne();if(Stop)goto _Stop;
}while(++vHits<AStarVerticalHits);
nodesGrounded.Clear();
i=0;j=0;foreach(var result in ToSetGridVerHits){
for(int ridx=0;ridx<AStarVerticalHits;ridx++){int nodeIdx=i+ridx;var hit=result.Value[ridx];
Nodes[nodeIdx].Parent=null;
if(hit.normal==Vector3.zero){
Nodes[nodeIdx].valid=false;
TellNeighboursReachabilityOf(Nodes[nodeIdx],false);
}else{
Nodes[nodeIdx].valid=true;
Nodes[nodeIdx].Position=hit.point+Vector3.up*(NodeHalfSize.y+Mathf.Tan(Mathf.Deg2Rad*Vector3.Angle(Vector3.up,hit.normal.normalized))*NodeHalfSize.z);Nodes[nodeIdx].Normal=hit.normal.normalized;
TellNeighboursReachabilityOf(Nodes[nodeIdx],true);
nodesGrounded.Add((nodeIdx,Nodes[nodeIdx],hit));
}
}
i+=AStarVerticalHits;j++;}
void TellNeighboursReachabilityOf(Node node,bool yes){
if(LOG&&LOG_LEVEL<=0)Debug.Log("node.neighbours.Count:"+node.neighbours.Count+"=="+node.indexOfMe.Count+":node.indexOfMe.Count;node.neighbourCanBeReached.Count:"+node.neighbourCanBeReached.Count);
for(int n=0;n<node.neighbours.Count;n++){var neighbour=node.neighbours[n].node;var indexOfMe=node.indexOfMe[n];
var reachableState=neighbour.neighbourCanBeReached[indexOfMe];
    reachableState.yes=yes;
    reachableState.mode=yes?Node.PreferredReachableMode.walk:Node.PreferredReachableMode.teleport;
    neighbour.neighbourCanBeReached[indexOfMe]=reachableState;
}
node.Walkable=yes;
}
originNode=GetNodeAt(startPos);
targetNode=GetNodeAt(target.point+Vector3.up*NodeHalfSize.y);
if(LOG&&LOG_LEVEL<=2)Debug.Log("nodesGrounded.Count:"+nodesGrounded.Count);
Vector3 halfExtents3a=NodeHalfSize;
        halfExtents3a.y=.25f;
Quaternion orientation3a=Quaternion.identity;
Vector3 direction3a=Vector3.up;
float dis3a=NodeSize.y+.3f;
for(int g=0;g<nodesGrounded.Count;g++){
Vector3 center3a=nodesGrounded[g].floorHit.point;
        center3a.y-=(.3f);
commands3a.AddNoResize(new BoxcastCommand(center3a,halfExtents3a,orientation3a,direction3a,dis3a,noCharLayer));
}
            backgroundDataSet3a.Set();foregroundDataSet3a.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 3a");


nodesWalkable.Clear();
void SetAsWalkable(int idx,Node node,RaycastHit floorHit){
TellNeighboursReachabilityOf(node,true);
nodesWalkable.Add((idx,node,floorHit));
}
nodesObstructed.Clear();
void SetAsObstructed(int idx,Node node,RaycastHit obstacleHit){
if(LOG&&LOG_LEVEL<=-100)Debug.Log("obstruction found at:"+node.Position);
TellNeighboursReachabilityOf(node,false);
nodesObstructed.Add((idx,node,obstacleHit));
}
for(int r=0;r<resultsManaged3a.Count;r++){var result3a=resultsManaged3a[r];
if(Vector3.Angle(nodesGrounded[r].floorHit.normal,Vector3.up)>60){//  Obstructed! Floor too steep
SetAsObstructed(nodesGrounded[r].idx,nodesGrounded[r].node,result3a.hit);
}else{
    if(result3a.colliderNotNull){
        if(result3a.hit.point.y>=(nodesGrounded[r].floorHit.point.y-.05f)){//  Obstructed! Obstacle above floor hit/detected
SetAsObstructed(nodesGrounded[r].idx,nodesGrounded[r].node,result3a.hit);
        }else{
SetAsWalkable(nodesGrounded[r].idx,nodesGrounded[r].node,nodesGrounded[r].floorHit);
        }
    }else{
SetAsWalkable(nodesGrounded[r].idx,nodesGrounded[r].node,nodesGrounded[r].floorHit);
    }
}
}

    
Vector3 halfExtents3b=NodeHalfSize;
        halfExtents3b.y=.25f;
Quaternion orientation3b=Quaternion.identity;
Vector3 direction3b=Vector3.down;
float dis3b=NodeSize.y-.05f;
for(int o=0;o<nodesObstructed.Count;o++){
Vector3 center3b=nodesObstructed[o].obstacleHit.point+Vector3.up*(NodeSize.y+Mathf.Tan(Mathf.Deg2Rad*Vector3.Angle(Vector3.up,nodesObstructed[o].obstacleHit.normal))*NodeHalfSize.z);
        center3b.x=nodesObstructed[o].node.Position.x;
        center3b.z=nodesObstructed[o].node.Position.z;
commands3b.AddNoResize(new BoxcastCommand(center3b,halfExtents3b,orientation3b,direction3b,dis3b,noCharLayer));
}


            backgroundDataSet3b.Set();foregroundDataSet3b.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 3b");


for(int r=0;r<resultsManaged3b.Count;r++){var result3b=resultsManaged3b[r];
if(Vector3.Angle(result3b.hit.normal,Vector3.up)<=60&&Math.Abs(result3b.hit.point.y-(nodesObstructed[r].node.Position.y-NodeHalfSize.y))<=NodeHalfSize.y){
var pos=nodesObstructed[r].node.Position;pos.y=result3b.hit.point.y+NodeHalfSize.y;nodesObstructed[r].node.Position=pos;
SetAsWalkable(nodesObstructed[r].idx,nodesObstructed[r].node,nodesObstructed[r].obstacleHit);
}
}
resultToNodeAndNeighbour.Clear();
Vector3 halfExtents3c=NodeHalfSize;
for(int w=0,ridx=0;w<nodesWalkable.Count;w++){
    for(int n=0;n<nodesWalkable[w].node.neighbourCanBeReached.Count;n++){var neighbour=nodesWalkable[w].node.neighbours[n];var reachable=nodesWalkable[w].node.neighbourCanBeReached[n];
        if(reachable.yes){
var startPos=nodesWalkable[w].node.Position;
    startPos.y+=.25f;
var endPos=neighbour.node.Position;
    endPos.y+=.25f;
Vector3 direction3c=(endPos-startPos).normalized;
var look=direction3c;
    look.y=0;
Quaternion orientation3c=Quaternion.LookRotation(look!=Vector3.zero?look:startForward,Vector3.up);
results3ciA.AddNoResize(new RaycastHit());commands3ciA.AddNoResize(new BoxcastCommand(startPos,halfExtents3c,orientation3c,direction3c,Vector3.Distance(endPos,startPos),noCharLayer));
results3ciB.AddNoResize(new RaycastHit());commands3ciB.AddNoResize(new RaycastCommand(startPos,direction3c,Vector3.Distance(endPos,startPos),noCharLayer));
Vector3 jumpPos;
if(startPos.y<endPos.y){
        jumpPos=startPos;
        jumpPos.y=endPos.y;
}else{
        jumpPos=endPos;
        jumpPos.y=startPos.y;
}
        direction3c=(jumpPos-startPos).normalized;
results3ciiA1.AddNoResize(new RaycastHit());commands3ciiA1.AddNoResize(new BoxcastCommand(startPos,halfExtents3c,orientation3c,direction3c,Vector3.Distance(jumpPos,startPos),noCharLayer));
results3ciiB1.AddNoResize(new RaycastHit());commands3ciiB1.AddNoResize(new RaycastCommand(startPos,direction3c,Vector3.Distance(jumpPos,startPos),noCharLayer));
        direction3c=(endPos-jumpPos).normalized;
results3ciiA2.AddNoResize(new RaycastHit());commands3ciiA2.AddNoResize(new BoxcastCommand(jumpPos,halfExtents3c,orientation3c,direction3c,Vector3.Distance(endPos,jumpPos),noCharLayer));
results3ciiB2.AddNoResize(new RaycastHit());commands3ciiB2.AddNoResize(new RaycastCommand(jumpPos,direction3c,Vector3.Distance(endPos,jumpPos),noCharLayer));
resultToNodeAndNeighbour[ridx++]=(nodesWalkable[w].node,n);
        }
    }
}
//Debug.LogWarning(nodesWalkable.Count+" "+Nodes.Length);
//Debug.LogWarning(commands3ciA.Capacity);
//Debug.LogWarning(commands3ciA.Length);

            backgroundDataSet3c.Set();foregroundDataSet3c.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 3c");


foreach(var kvp in resultToNodeAndNeighbour){var ridx=kvp.Key;var node=kvp.Value.Item1;var n=kvp.Value.Item2;var neighbour=node.neighbours[n];var reachable=node.neighbourCanBeReached[n];
var result3ciA=resultsManaged3ciA[ridx];
var result3ciB=resultsManaged3ciB[ridx];
var result3ciiA1=resultsManaged3ciiA1[ridx];
var result3ciiB1=resultsManaged3ciiB1[ridx];
var result3ciiA2=resultsManaged3ciiA2[ridx];
var result3ciiB2=resultsManaged3ciiB2[ridx];
if(!result3ciA.colliderNotNull&&
   !result3ciB.colliderNotNull){
    reachable.yes=true;
    reachable.mode=neighbour.node.Position.y!=node.Position.y?Node.PreferredReachableMode.ramp:Node.PreferredReachableMode.walk;
}else if(!result3ciiA1.colliderNotNull&&!result3ciiA2.colliderNotNull&&
         !result3ciiB1.colliderNotNull&&!result3ciiB2.colliderNotNull){
    reachable.yes=true;
    reachable.mode=neighbour.node.Position.y>=node.Position.y?Node.PreferredReachableMode.jump:Node.PreferredReachableMode.fall;
}else{
    reachable.yes=false;
}
node.neighbourCanBeReached[n]=reachable;
}
//for(int w=0,ridx=0;w<nodesWalkable.Count;w++){
//    for(int n=0;n<nodesWalkable[w].node.neighbourCanBeReached.Count;n++,ridx++){var neighbour=nodesWalkable[w].node.neighbours[n];var reachable=nodesWalkable[w].node.neighbourCanBeReached[n];
//        //Debug.LogWarning(ridx);
//var result3ciA=resultsManaged3ciA[ridx];
//var result3ciB=resultsManaged3ciB[ridx];
//    }
//}


            backgroundDataSet4.Set();foregroundDataSet4.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 4");


//Debug.LogWarning(default(Node));
bool pathFound=false;Node lastTestedNode=null;
ClosedNodes.Clear();
  OpenNodes.Clear();
  OpenNodes.Add(originNode);
while(OpenNodes.Count>0){
if(LOG&&LOG_LEVEL<=-100)Debug.Log("OpenNodes.Count:"+OpenNodes.Count);
var current=OpenNodes.RemoveFirst();ClosedNodes.Add(current);lastTestedNode=current;//  Node agora testado!
if(current==targetNode){
if(LOG&&LOG_LEVEL<=0)Debug.Log("_path_found_:currentNode==targetNode");
pathFound=true;
goto _Found;
}
var neighbours=current.neighbours;
if(LOG&&LOG_LEVEL<=-50)Debug.Log("neighbours.Count:"+neighbours.Count);
for(int n=0;n<neighbours.Count;n++){var neighbour=neighbours[n];
if(ClosedNodes.Contains(neighbour.node)){//  Already tested
continue;}
if(!neighbour.node.Walkable){
continue;}
if(!current.neighbourCanBeReached[n].yes){
continue;}
bool inOpenNodes;var G_NewCost=current.G+GetDistance(current,neighbour.node);if(!(inOpenNodes=OpenNodes.Contains(neighbour.node))||G_NewCost<neighbour.node.G){
neighbour.node.G=G_NewCost;//  Vizinho válido para avaliação de encontrar caminho
neighbour.node.H=GetDistance(neighbour.node,targetNode);
neighbour.node.Parent=current; 
if(!inOpenNodes){
OpenNodes.Add(neighbour.node);
}else{
OpenNodes.UpdateItem(neighbour.node);
}
}
}
}
float GetDistance(Node nodeA,Node nodeB){
var dis=Vector3.Distance(nodeA.Position,nodeB.Position);
if(preferClimbing){
}
return(dis);}
_Found:{}
List<Node>path=new List<Node>();
Node last=null;if(pathFound)last=targetNode;else if(ClosedNodes.Count>0)last=ClosedNodes.RemoveFirst();
var step=last;
int c=Nodes.Length;do{
if(step==null){break;}
path.Add(step);
int indexOfMe;if(step.Parent!=null)if((indexOfMe=step.Parent.neighbours.IndexOf((step.Idx,step)))>=0){step.Mode=step.Parent.neighbourCanBeReached[indexOfMe].mode;}else{step.Mode=Node.PreferredReachableMode.jump;}
step=step.Parent;
}while(step!=originNode&&--c>0);
if(LOG&&LOG_LEVEL<=1)Debug.Log("path retraced: retrace count should never reach 0:"+c);
if(path.Count>0)path.Reverse();
resultPath=path;


lock(tasksBusyCount_Syn){tasksBusyCount--;}queue.Set();
backgroundDataSet1.Set();}
int GetNodeIndex(int cz,int vy,int cx){return (cx+AStarDistance.x)*gridResolution.y*AStarVerticalHits+(cz+AStarDistance.y)*AStarVerticalHits+vy;}Node GetNodeAt(Vector3 position){Node result=null;float minDis=-1,dis;for(int nodeIdx=0;nodeIdx<Nodes.Length;nodeIdx++){var node=Nodes[nodeIdx];dis=Vector3.Distance(position,node.Position);if(minDis==-1||dis<minDis){result=node;minDis=dis;}}return result;}
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
var emptyColor=new Color(1,1,1,.25f);var emptyLineColor=new Color(1,1,1,.0625f);
var originColor=new Color(0,0,1,.25f);
var targetColor=new Color(0,0,1,.25f);
var obstructedColor=new Color(1,0,0,.25f);
var pathColor=new Color(0,0,1,.25f);
if(Nodes!=null)foreach(var node in Nodes){
if(node.valid){
     if(node==originNode)
Gizmos.color=originColor;
else if(node==targetNode)
Gizmos.color=targetColor;
else if(!node.Walkable)
Gizmos.color=obstructedColor;
else if(resultPath.Contains(node))
Gizmos.color=pathColor;
else
Gizmos.color=emptyColor;
Gizmos.DrawCube(node.Position,NodeSize);
if(DRAW_LEVEL<=-110)foreach(var neighbour in node.neighbours)if(neighbour.node.valid)Debug.DrawLine(node.Position,neighbour.node.Position,emptyLineColor);
}
}
Gizmos.color=oldcolor;
}
    }
                   base.OnDrawGizmos();
}
#endif
[NonSerialized]int noCharLayer;
[Serializable]public class Node:IHeapItem<Node>{
public bool valid{get;set;}public int Idx{get;set;}public readonly List<(int idx,Node node)>neighbours=new List<(int,Node)>();public readonly List<int>indexOfMe=new List<int>();public readonly List<(bool yes,PreferredReachableMode mode)>neighbourCanBeReached=new List<(bool,PreferredReachableMode)>();
public bool Walkable{get;set;}
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
public Vector3 Position{get;set;}public Vector3 Normal{get;set;}public PreferredReachableMode Mode{get;set;}
public override int GetHashCode(){return Position.GetHashCode();}public override bool Equals(object obj){if(ReferenceEquals(this,obj))return true;if(!(obj is Node node))return false;return(Position==node.Position);}
public Node Parent;
public enum PreferredReachableMode{
jump,
ramp,
walk,
fall,
teleport,
}
}


[SerializeField]internal bool DEBUG_GOTO;

    
}