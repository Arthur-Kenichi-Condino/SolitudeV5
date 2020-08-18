using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels{public class Chunk:MonoBehaviour{
public bool LOG=false;public int LOG_LEVEL=1;public int DRAW_LEVEL=1;public bool DISABLE=false;
public LinkedListNode<Chunk>ExpropriationNode=null;
protected virtual void OnEnable(){}
protected virtual void OnDisable(){}
protected virtual void OnDestroy(){}
protected virtual void Update(){}
public Vector2Int Coord{private set;get;}public Vector2Int Rgn{private set;get;}public int Idx{private set;get;}public bool Initialized{private set;get;}[NonSerialized]public bool needsRebuild;public bool Built{protected set;get;}
public void OnRebuildRequest(Vector2Int coord,Vector2Int rgn,int idx){
    Coord=coord;Rgn=rgn;Idx=idx;Initialized=true;needsRebuild=true;transform.position=new Vector3(Rgn.x,0,Rgn.y);name=transform.position.ToString();
}
public const ushort Height=(256);
public const ushort Width=(16);
public const ushort Depth=(16);
public const ushort FlattenOffset=(Width*Depth);
public static int GetIdx(int vcx,int vcy,int vcz){return vcy*FlattenOffset+vcx*Depth+vcz;}
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
public static Vector3Int PosToCoord(Vector3 pos){
Vector2Int rgn=ChunkManager.PosToRgn(pos);
pos.x=(pos.x>0)?(pos.x-(int)pos.x==0.5f?Mathf.FloorToInt(pos.x):Mathf.RoundToInt(pos.x)):(int)Math.Round(pos.x,MidpointRounding.AwayFromZero);
pos.y=(pos.y>0)?(pos.y-(int)pos.y==0.5f?Mathf.FloorToInt(pos.y):Mathf.RoundToInt(pos.y)):(int)Math.Round(pos.y,MidpointRounding.AwayFromZero);
pos.z=(pos.z>0)?(pos.z-(int)pos.z==0.5f?Mathf.FloorToInt(pos.z):Mathf.RoundToInt(pos.z)):(int)Math.Round(pos.z,MidpointRounding.AwayFromZero);
Vector3Int coord=new Vector3Int((int)pos.x-rgn.x,(int)pos.y,(int)pos.z-rgn.y);
coord.x+=Mathf.FloorToInt(Width/2.0f );coord.x=Mathf.Clamp(coord.x,0,Width-1 );
coord.y+=Mathf.FloorToInt(Height/2.0f);coord.y=Mathf.Clamp(coord.y,0,Height-1);
coord.z+=Mathf.FloorToInt(Depth/2.0f );coord.z=Mathf.Clamp(coord.z,0,Depth-1 );
return coord;}
}}