using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public class Pathfinder:SimActor{
protected override void Awake(){
                   base.Awake();
waitUntil=new WaitUntil(()=>backgroundDataSet2.WaitOne(0));
}
[NonSerialized]Coroutine cr;[NonSerialized]Task task;[NonSerialized]readonly AutoResetEvent foregroundDataSet1=new AutoResetEvent(false);[NonSerialized]readonly ManualResetEvent backgroundDataSet1=new ManualResetEvent(true);[NonSerialized]readonly AutoResetEvent foregroundDataSet2=new AutoResetEvent(false);[NonSerialized]readonly AutoResetEvent backgroundDataSet2=new AutoResetEvent(false);
protected override void OnEnable(){
                   base.OnEnable();
cr=StartCoroutine(CRDoRaycasts());
Stop=false;task=Task.Factory.StartNew(BG,new object[]{LOG,LOG_LEVEL,},TaskCreationOptions.LongRunning);
}
bool Stop{
    get{bool tmp;lock(Stop_Syn){tmp=Stop_v;      }return tmp;}
    set{         lock(Stop_Syn){    Stop_v=value;}if(value){foregroundDataSet1.Set();foregroundDataSet2.Set();}}
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
}
                   base.OnDestroy();
}

    
[NonSerialized]readonly LinkedList<RaycastHit>GoToQueue=new LinkedList<RaycastHit>();
protected override void Update(){


    if(DEBUG_GOTO){DEBUG_GOTO=false;}


    if(backgroundDataSet1.WaitOne(0)){
        if(GoToQueue.Count>0){
if(LOG&&LOG_LEVEL<=1)Debug.Log("dequeue");



    
            backgroundDataSet1.Reset();foregroundDataSet1.Set();
        }
    }




                   base.Update();
}
[NonSerialized]WaitUntil waitUntil;
IEnumerator CRDoRaycasts(){
if(LOG&&LOG_LEVEL<=2)Debug.Log("begin");
_loop:{}
yield return waitUntil;
if(LOG&&LOG_LEVEL<=1)Debug.Log("do raycasts");




foregroundDataSet2.Set();
if(LOG&&LOG_LEVEL<=1)Debug.Log("loop");
goto _loop;
}


    public bool DEBUG_GOTO;

    
void BG(object state){Thread.CurrentThread.IsBackground=false;Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;try{
    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL){
        while(!Stop){foregroundDataSet1.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("begin pathfind");
            backgroundDataSet2.Set();foregroundDataSet2.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results");
backgroundDataSet1.Set();}
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
}