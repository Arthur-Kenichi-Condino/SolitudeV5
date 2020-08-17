﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public class Pathfinder:SimActor{
protected override void Awake(){
                   base.Awake();
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


public void GoTo(Ray tgtDir,float maxDis=1000){
if(Physics.Raycast(tgtDir,out RaycastHit hit,maxDis)){
    Debug.LogWarning(hit.point);
}
}    
[NonSerialized]readonly LinkedList<RaycastHit>GoToQueue=new LinkedList<RaycastHit>();
protected override void Update(){


    if(DEBUG_GOTO){DEBUG_GOTO=false;GoTo(new Ray(new Vector3(1,10,-10),Vector3.down));//GoToQueue.AddLast(new RaycastHit());
    }


    if(backgroundDataSet1.WaitOne(0)){
        if(GoToQueue.Count>0){
if(LOG&&LOG_LEVEL<=1)Debug.Log("dequeue");


            GoToQueue.RemoveFirst();

    
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

    
void BG(object state){Thread.CurrentThread.IsBackground=false;Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;try{
    if(state is object[]parameters&&parameters[0]is bool LOG&&parameters[1]is int LOG_LEVEL){
        while(!Stop){foregroundDataSet1.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("begin pathfind");
            backgroundDataSet2.Set();foregroundDataSet2.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 2");
            backgroundDataSet3.Set();foregroundDataSet3.WaitOne();if(Stop)goto _Stop;
if(LOG&&LOG_LEVEL<=1)Debug.Log("use raycasts results 3");
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