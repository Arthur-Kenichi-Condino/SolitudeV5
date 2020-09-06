using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AI:Pathfinder{
protected override void Update(){
                   base.Update();
WALK_PATH();
}
[NonSerialized]public Vector3 ReachedTgtDisThreshold=new Vector3(.1f,.1f,.1f);
[NonSerialized]Vector3 _axisDiff,_dir;
[NonSerialized]Vector3 _axisDist;
void WALK_PATH(){
if(CurPath.Count>0&&CurPathTgt==null){
    CurPathTgt=CurPath.Dequeue();


        Debug.LogWarning(CurPathTgt.Value.mode);


}
if(CurPathTgt.HasValue){


_axisDiff.y=CurPathTgt.Value.pos.y-transform.position.y;_axisDist.y=Mathf.Abs(_axisDiff.y);
_axisDiff.x=CurPathTgt.Value.pos.x-transform.position.x;_axisDist.x=Mathf.Abs(_axisDiff.x);
_axisDiff.z=CurPathTgt.Value.pos.z-transform.position.z;_axisDist.z=Mathf.Abs(_axisDiff.z);
            
            
_dir=_axisDiff.normalized;


if(_axisDist.y<=ReachedTgtDisThreshold.y&&
   _axisDist.x<=ReachedTgtDisThreshold.x&&
   _axisDist.z<=ReachedTgtDisThreshold.z){    


    CurPathTgt=null;
        Debug.LogWarning("dest reached");


return;
}
if(_axisDist.y>float.Epsilon||
   _axisDist.x>float.Epsilon||
   _axisDist.z>float.Epsilon){
inputViewRotationEuler.y=Quaternion.LookRotation(_dir).eulerAngles.y-transform.eulerAngles.y;
}


inputMoveSpeed.x=0;
inputMoveSpeed.z=InputMaxMoveSpeed.z;


}
}
#if UNITY_EDITOR
protected override void OnDrawGizmos(){
                   base.OnDrawGizmos();
}
#endif
}