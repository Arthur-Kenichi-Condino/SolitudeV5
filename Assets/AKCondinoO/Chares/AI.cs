using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AI:Pathfinder{
#if UNITY_EDITOR
protected override void OnDrawGizmos(){
                   base.OnDrawGizmos();
}
#endif
}