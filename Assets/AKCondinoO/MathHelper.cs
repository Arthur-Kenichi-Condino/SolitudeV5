using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class MathHelper{
/// <summary>
///  [https://answers.unity.com/questions/532297/rotate-a-vector-around-a-certain-point.html]
/// </summary>
/// <param name="point"></param>
/// <param name="pivot"></param>
/// <param name="rotation"></param>
/// <returns></returns>
public static Vector3 RotatePointAroundPivot(Vector3 point,Vector3 pivot,Quaternion rotation){
//  Get point direction, relative to pivot:(point-pivot);
// rotate it:rotation*(point-pivot);
// calculate rotated point:rotation*(point-pivot)+pivot; and
// return it
return rotation*(point-pivot)+pivot;}
}