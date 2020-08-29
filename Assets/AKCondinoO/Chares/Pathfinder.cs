using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
public class Pathfinder:SimActor{
[NonSerialized]Vector2Int AStarDistance=new Vector2Int(5,5);[NonSerialized]int AStarVerticalHits=3;[NonSerialized]Vector2Int gridResolution;[NonSerialized]Node[]Nodes;[NonSerialized]Node originNode;[NonSerialized]Node targetNode;
protected override void Awake(){
                   base.Awake();
waitUntil2=new WaitUntil(()=>backgroundDataSet2.WaitOne(0));
waitUntil3=new WaitUntil(()=>backgroundDataSet3.WaitOne(0));
waitUntil4=new WaitUntil(()=>backgroundDataSet4.WaitOne(0));
}
[NonSerialized]Coroutine cr;[NonSerialized]Task task;[NonSerialized]readonly AutoResetEvent foregroundDataSet1=new AutoResetEvent(false);[NonSerialized]readonly ManualResetEvent backgroundDataSet1=new ManualResetEvent(true);[NonSerialized]readonly AutoResetEvent foregroundDataSet2=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet2=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent foregroundDataSet3=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet3=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent foregroundDataSet4=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet4=new AutoResetEvent(false);
protected override void OnEnable(){
                   base.OnEnable();
noCharLayer=~(1<<LayerMask.NameToLayer("Char"));
gridResolution=new Vector2Int(AStarDistance.x*2+1,AStarDistance.y*2+1);
Nodes=new Node[gridResolution.x*gridResolution.y*AStarVerticalHits];for(int i=0;i<Nodes.Length;i++)Nodes[i]=new Node();
if(LOG&&LOG_LEVEL<=2)Debug.Log("gridResolution:"+gridResolution+";Nodes:"+Nodes.Length);
cr=StartCoroutine(CRDoRaycasts());
int raycasts=gridResolution.x*gridResolution.y;
Stop=false;task=Task.Factory.StartNew(BG,new object[]{LOG,LOG_LEVEL,ToSetGridVerRaycasts=new NativeList<RaycastCommand>(raycasts,Allocator.Persistent),ToSetGridVerHitsResultsBuffer=new NativeArray<RaycastHit>(raycasts,Allocator.Persistent,NativeArrayOptions.UninitializedMemory),},TaskCreationOptions.LongRunning);
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
if(LOG&&LOG_LEVEL<=2)Debug.Log("GoTo:"+hit.point);
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

    
            backgroundDataSet1.Reset();foregroundDataSet1.Set();
        }
    }
                   base.Update();
}
[NonSerialized]WaitUntil waitUntil2;
[NonSerialized]WaitUntil waitUntil3;
[NonSerialized]WaitUntil waitUntil4;
IEnumerator CRDoRaycasts(){
if(LOG&&LOG_LEVEL<=2)Debug.Log("begin");
_loop:{}
if(LOG&&LOG_LEVEL<=2)Debug.Log("ToSetGridVerHits.Count before phase id '2':"+ToSetGridVerHits.Count);
int vHits=0;
do{
yield return waitUntil2;
if(LOG&&LOG_LEVEL<=0)Debug.Log("current vertical hit test:"+vHits);
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
yield return waitUntil3;
if(LOG&&LOG_LEVEL<=2)Debug.Log("do raycasts 3;ToSetGridVerHits.Count after phase id '2' should be the same as before:"+ToSetGridVerHits.Count);




foregroundDataSet3.Set();
yield return waitUntil4;
if(LOG&&LOG_LEVEL<=2)Debug.Log("do raycasts 4");




foregroundDataSet4.Set();
if(LOG&&LOG_LEVEL<=2)Debug.Log("cr loop end");
goto _loop;
}


    public bool DEBUG_GOTO;

    
[NonSerialized]JobHandle handle2;[NonSerialized]NativeList<RaycastCommand>ToSetGridVerRaycasts;[NonSerialized]NativeArray<RaycastHit>ToSetGridVerHitsResultsBuffer;[NonSerialized]readonly Dictionary<int,RaycastHit[]>ToSetGridVerHits=new Dictionary<int,RaycastHit[]>();
[NonSerialized]Vector3 NodeHalfSize;
[NonSerialized]Vector3 NodeSize;
[NonSerialized]RaycastHit target;[NonSerialized]Vector3 startPos;[NonSerialized]Vector3 boundsExtents;
void BG(object state){Thread.CurrentThread.IsBackground=false;Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;try{
    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL&&parameters[2]is NativeList<RaycastCommand>ToSetGridVerRaycasts&&parameters[3]is NativeArray<RaycastHit>ToSetGridVerHitsResultsBuffer){
List<RaycastHit[]>ToSetGridVerHitsResults=new List<RaycastHit[]>();
int i=0,j=0;
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
}
i+=AStarVerticalHits;j++;}}
for(int n=0;n<Nodes.Length;n++){var node=Nodes[n];for(int v=0;v<node.neighbours.Count;v++){var neighbour=node.neighbours[v].node;
int indexOfMe=-1;for(int vv=0;vv<neighbour.neighbours.Count;vv++){if(neighbour.neighbours[vv].node==node)indexOfMe=vv;}node.indexOfMe.Add(indexOfMe);
}}
        while(!Stop){foregroundDataSet1.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=2)Debug.Log("begin pathfind");
            NodeHalfSize=boundsExtents;
            NodeHalfSize.x+=.1f;
            NodeHalfSize.z+=.1f;
            NodeSize=NodeHalfSize*2;
ToSetGridVerHits.Clear();var gridStartHeight=startPos.y+(AStarVerticalHits/2f)*NodeSize.y;var disableCommandHeight=-Mathf.Ceil(Chunk.Height/2f)-1;
if(LOG&&LOG_LEVEL<=1)Debug.Log("disableCommandHeight value:"+disableCommandHeight);
int vHits=0;
do{
if(LOG&&LOG_LEVEL<=0)Debug.Log("current vertical hit test:"+vHits);
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
i=0;j=0;foreach(var result in ToSetGridVerHits){
for(int ridx=0;ridx<AStarVerticalHits;ridx++){int nodeIdx=i+ridx;var hit=result.Value[ridx];
if(hit.normal==Vector3.zero){
Nodes[nodeIdx].valid=false;
}else{
Nodes[nodeIdx].valid=true;
Nodes[nodeIdx].Position=hit.point+Vector3.up*NodeHalfSize.y;Nodes[nodeIdx].Normal=hit.normal.normalized;
}
}
i+=AStarVerticalHits;j++;}
originNode=GetNodeAt(startPos);
targetNode=GetNodeAt(target.point+Vector3.up*NodeHalfSize.y);




            backgroundDataSet3.Set();foregroundDataSet3.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 3");




            backgroundDataSet4.Set();foregroundDataSet4.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 4");




backgroundDataSet1.Set();}
int GetNodeIndex(int cz,int vy,int cx){return (cx+AStarDistance.x)*gridResolution.y*AStarVerticalHits+(cz+AStarDistance.y)*AStarVerticalHits+vy;}Node GetNodeAt(Vector3 position){Node result=null;float minDis=-1,dis;for(int n=0;n<Nodes.Length;n++){var node=Nodes[n];dis=Vector3.Distance(position,node.Position);if(minDis==-1||dis<minDis){result=node;minDis=dis;}}return result;}
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
var emptyColor=new Color(1,1,1,.25f);
var originColor=new Color(0,0,1,.25f);
var targetColor=new Color(0,0,1,.25f);
if(Nodes!=null)foreach(var node in Nodes){
if(node.valid){
     if(node==originNode)
Gizmos.color=originColor;
else if(node==targetNode)
Gizmos.color=targetColor;
else
Gizmos.color=emptyColor;
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
[NonSerialized]int noCharLayer;
[Serializable]public class Node:IHeapItem<Node>{
public bool valid{get;set;}public int Idx{get;set;}public readonly List<(int idx,Node node)>neighbours=new List<(int,Node)>();public readonly List<int>indexOfMe=new List<int>();
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