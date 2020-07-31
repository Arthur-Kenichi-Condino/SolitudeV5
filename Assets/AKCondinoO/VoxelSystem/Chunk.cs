using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace AKCondinoO.Voxels{public class Chunk:MonoBehaviour{
public bool LOG=false;public int LOG_LEVEL=1;public int DRAW_LEVEL=1;
public LinkedListNode<Chunk>ExpropriationNode=null;
protected virtual void OnEnable(){}
protected virtual void OnDisable(){}
protected virtual void Update(){}
public Vector2Int Coord{private set;get;}public Vector2Int Rgn{private set;get;}public int Idx{private set;get;}[NonSerialized]protected bool needsRebuild;
public void OnRebuildRequest(Vector2Int coord,Vector2Int rgn,int idx){
    Coord=coord;Rgn=rgn;Idx=idx;needsRebuild=true;transform.position=new Vector3(Rgn.x,0,Rgn.y);name=transform.position.ToString();
}
public const ushort Height=(256);
public const ushort Width=(16);
public const ushort Depth=(16);
public const ushort FlattenOffset=(Width*Depth);
public static int GetIdx(int vcx,int vcy,int vcz){return vcy*FlattenOffset+vcx*Width+vcz;}
public const int VoxelsPerChunk=(FlattenOffset*Height);
#region ValidateCoord
public static void ValidateCoord(ref Vector2Int region,ref Vector3Int vxlCoord){int a,c;
a=region.x;c=vxlCoord.x;ValidateCoordAxis(ref a,ref c,Width);region.x=a;vxlCoord.x=c;
a=region.y;c=vxlCoord.z;ValidateCoordAxis(ref a,ref c,Depth);region.y=a;vxlCoord.z=c;
}
public static void ValidateCoordAxis(ref int axis,ref int coord,int axisLength){
      if(coord<0){          axis-=axisLength*Mathf.CeilToInt (Math.Abs(coord)/(float)axisLength);coord=(coord%axisLength)+axisLength;
}else if(coord>=axisLength){axis+=axisLength*Mathf.FloorToInt(Math.Abs(coord)/(float)axisLength);coord=(coord%axisLength);}
}
#endregion




//public LinkedListNode<Chunk>ExpropriationNode=null;
//protected virtual void OnEnable(){}
//protected virtual void OnDisable(){}
//protected virtual void Update(){}
//public Vector2Int Coord{private set;get;}public Vector2Int Rgn{private set;get;}public int Idx{private set;get;}[NonSerialized]protected bool needsRebuild;
//public void OnRebuildRequest(Vector2Int coord,Vector2Int rgn,int idx){
//    Coord=coord;Rgn=rgn;Idx=idx;needsRebuild=true;transform.position=new Vector3(Rgn.x,0,Rgn.y);
//}
//public const ushort Height=(256);
//public const ushort Width=(16);
//public const ushort Depth=(16);
//public const ushort FlattenOffset=(Width*Depth);
//public static int GetIdx(int vcx,int vcy,int vcz){return vcy*FlattenOffset+vcx*Width+vcz;}
//public const int VoxelsPerChunk=(FlattenOffset*Height);
//#region ValidateCoord
//public static void ValidateCoord(ref Vector2Int region,ref Vector3Int vxlCoord){int a,c;
//a=region.x;c=vxlCoord.x;ValidateCoordAxis(ref a,ref c,Width);region.x=a;vxlCoord.x=c;
//a=region.y;c=vxlCoord.z;ValidateCoordAxis(ref a,ref c,Depth);region.y=a;vxlCoord.z=c;
//}
//public static void ValidateCoordAxis(ref int axis,ref int coord,int axisLength){
//      if(coord<0){          axis-=axisLength*Mathf.CeilToInt (Math.Abs(coord)/(float)axisLength);coord=(coord%axisLength)+axisLength;
//}else if(coord>=axisLength){axis+=axisLength*Mathf.FloorToInt(Math.Abs(coord)/(float)axisLength);coord=(coord%axisLength);}
//}
//#endregion




//public LinkedListNode<Chunk>ExpropriationNode=null;
//    protected virtual void OnEnable(){}
//    protected virtual void OnDisable(){}
//    protected virtual void Update(){}
//    public Vector2Int Coord{private set;get;}public Vector2Int Rgn{private set;get;}public int Idx{private set;get;}[NonSerialized]protected bool needsRebuild;
//    public void OnRebuildRequest(Vector2Int coord,Vector2Int rgn,int idx){
//        Coord=coord;Rgn=rgn;Idx=idx;needsRebuild=true;transform.position=new Vector3(Rgn.x,0,Rgn.y);
//    }
//public const ushort Height=(256);
//public const ushort Width=(16);
//public const ushort Depth=(16);
//public const ushort FlattenOffset=(Width*Depth);
//public static int GetIdx(int vcx,int vcy,int vcz){return vcy*FlattenOffset+vcx*Width+vcz;}
//public const int VoxelsPerChunk=(FlattenOffset*Height);
//#region ValidateCoord
//public static void ValidateCoord(ref Vector2Int region,ref Vector3Int vxlCoord){int a,c;
//a=region.x;c=vxlCoord.x;ValidateCoordAxis(ref a,ref c,Width);region.x=a;vxlCoord.x=c;
//a=region.y;c=vxlCoord.z;ValidateCoordAxis(ref a,ref c,Depth);region.y=a;vxlCoord.z=c;
//}
//public static void ValidateCoordAxis(ref int axis,ref int coord,int axisLength){
//      if(coord<0){          axis-=axisLength*Mathf.CeilToInt (Math.Abs(coord)/(float)axisLength);coord=(coord%axisLength)+axisLength;
//}else if(coord>=axisLength){axis+=axisLength*Mathf.FloorToInt(Math.Abs(coord)/(float)axisLength);coord=(coord%axisLength);}
//}
//#endregion
////public LinkedListNode<Chunk>ExpropriationNode=null;
////    protected virtual void OnEnable(){}
////[NonSerialized]public new MeshRenderer renderer=null;[NonSerialized]protected MeshFilter meshFilter=null;[NonSerialized]protected Mesh mesh=null;
////    protected virtual void OnBuild(object state){}
////    protected virtual void BG(object state){}
////public const ushort Height=(256);
////public const ushort Width=(16);
////public const ushort Depth=(16);
////public const ushort FlattenOffset=(Width*Depth);
////public static int GetIdx(int vcx,int vcy,int vcz){return vcy*FlattenOffset+vcx*Width+vcz;}
////public const int VoxelsPerChunk=(FlattenOffset*Height);
////#region ValidateCoord
////public static void ValidateCoord(ref Vector2Int region,ref Vector3Int vxlCoord){int a,c;
////a=region.x;c=vxlCoord.x;ValidateCoordAxis(ref a,ref c,Width);region.x=a;vxlCoord.x=c;
////a=region.y;c=vxlCoord.z;ValidateCoordAxis(ref a,ref c,Depth);region.y=a;vxlCoord.z=c;
////}
////public static void ValidateCoordAxis(ref int axis,ref int coord,int axisLength){
////    if(coord<0){
////        axis-=axisLength*Mathf.CeilToInt (Math.Abs(coord)/(float)axisLength);coord=(coord%axisLength)+axisLength;
////    }else if(coord>=axisLength){
////        axis+=axisLength*Mathf.FloorToInt(Math.Abs(coord)/(float)axisLength);coord=(coord%axisLength);
////    }
////}
////#endregion
}}