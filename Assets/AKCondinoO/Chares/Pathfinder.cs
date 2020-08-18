using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public class Pathfinder:SimActor{
[NonSerialized]Vector2Int AStarDistance=new Vector2Int(10,10);[NonSerialized]int AStarVerticalHits=10;[NonSerialized]Vector2Int gridResolution;[NonSerialized]Node[]Nodes;
protected override void Awake(){
                   base.Awake();
gridResolution=new Vector2Int(AStarDistance.x*2+1,AStarDistance.y*2+1);
Nodes=new Node[gridResolution.x*gridResolution.y*AStarVerticalHits];
if(LOG&&LOG_LEVEL<=2)Debug.Log("gridResolution:"+gridResolution+";Nodes:"+Nodes.Length);
waitUntil2=new WaitUntil(()=>backgroundDataSet2.WaitOne(0));
waitUntil3=new WaitUntil(()=>backgroundDataSet3.WaitOne(0));
}
[NonSerialized]Coroutine cr;[NonSerialized]Task task;[NonSerialized]readonly AutoResetEvent foregroundDataSet1=new AutoResetEvent(false);[NonSerialized]readonly ManualResetEvent backgroundDataSet1=new ManualResetEvent(true);[NonSerialized]readonly AutoResetEvent foregroundDataSet2=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet2=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent foregroundDataSet3=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet3=new AutoResetEvent(false);
protected override void OnEnable(){
                   base.OnEnable();
cr=StartCoroutine(CRDoRaycasts());
Stop=false;task=Task.Factory.StartNew(BG,new object[]{LOG,LOG_LEVEL,},TaskCreationOptions.LongRunning);
}
bool Stop{
    get{bool tmp;lock(Stop_Syn){tmp=Stop_v;      }return tmp;}
    set{         lock(Stop_Syn){    Stop_v=value;}if(value){foregroundDataSet1.Set();foregroundDataSet2.Set();foregroundDataSet3.Set();}}
}[NonSerialized]readonly object Stop_Syn=new object();[NonSerialized]bool Stop_v;
protected override void OnDisable(){
StopCoroutine(cr);
Stop=true;try{task.Wait();}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}
                   base.OnDisable();
}
protected override void OnDestroy(){if(Stop){
if(LOG&&LOG_LEVEL<=2)Debug.Log("dispose");
backgroundDataSet1.Dispose();foregroundDataSet1.Dispose();
backgroundDataSet2.Dispose();foregroundDataSet2.Dispose();
backgroundDataSet3.Dispose();foregroundDataSet3.Dispose();
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

    
            backgroundDataSet1.Reset();foregroundDataSet1.Set();
        }
    }




                   base.Update();
}
[NonSerialized]WaitUntil waitUntil2;
[NonSerialized]WaitUntil waitUntil3;
IEnumerator CRDoRaycasts(){
if(LOG&&LOG_LEVEL<=2)Debug.Log("begin");
_loop:{}
yield return waitUntil2;
if(LOG&&LOG_LEVEL<=1)Debug.Log("do raycasts 2");




foregroundDataSet2.Set();
yield return waitUntil3;
if(LOG&&LOG_LEVEL<=1)Debug.Log("do raycasts 3");




foregroundDataSet3.Set();
if(LOG&&LOG_LEVEL<=1)Debug.Log("loop");
goto _loop;
}


    public bool DEBUG_GOTO;

    
[NonSerialized]Vector3 NodeHalfSize;
[NonSerialized]Vector3 NodeSize;
[NonSerialized]RaycastHit target;[NonSerialized]Vector3 startPos;[NonSerialized]Vector3 boundsExtents;
void BG(object state){Thread.CurrentThread.IsBackground=false;Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;try{
    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL){
        while(!Stop){foregroundDataSet1.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("begin pathfind");
            NodeHalfSize=boundsExtents;
            NodeHalfSize.x+=.1f;
            NodeHalfSize.z+=.1f;
            NodeSize=NodeHalfSize*2;
for(Vector2Int gcoord=new Vector2Int(-AStarDistance.x,-AStarDistance.y);gcoord.x<=AStarDistance.x;gcoord.x++){
for(gcoord.y=-AStarDistance.y                                          ;gcoord.y<=AStarDistance.y;gcoord.y++){


}}

            


            backgroundDataSet2.Set();foregroundDataSet2.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 2");
            backgroundDataSet3.Set();foregroundDataSet3.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 3");
backgroundDataSet1.Set();}
int GetNodeIndex(int x,int y,int z){return z*gridResolution.x+x*AStarVerticalHits+y;}
        _Stop:{
        }
if(LOG&&LOG_LEVEL<=2)Debug.Log("end");
    }
}catch(Exception e){Debug.LogError(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);}}



    
#if UNITY_EDITOR
protected override void OnDrawGizmos(){
                   base.OnDrawGizmos();
}
#endif
[Serializable]public class Node:IHeapItem<Node>{
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
public Vector3 Position{get;set;}
public override int GetHashCode(){return Position.GetHashCode();}public override bool Equals(object obj){if(ReferenceEquals(this,obj))return true;if(!(obj is Node node))return false;return(Position==node.Position);}
public Node Parent;
}
}