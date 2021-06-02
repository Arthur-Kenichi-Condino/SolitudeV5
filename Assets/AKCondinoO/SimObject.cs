using AKCondinoO.Species.Plants;
using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UMA;
using UnityEngine;
public class SimObject:MonoBehaviour{[NonSerialized]protected System.Random mathrandom=new System.Random();
public bool LOG=false;public int LOG_LEVEL=1;public int DRAW_LEVEL=1;
protected static Vector3 _half_y{get;}=new Vector3(1,.5f,1);
protected static Vector3 _half_xyz{get;}=new Vector3(.5f,.5f,.5f);
[NonSerialized]public new Collider collider=null;[NonSerialized]public new Rigidbody rigidbody=null;[NonSerialized]public new Renderer renderer;
protected virtual void Awake(){
collider=GetComponent<Collider>();rigidbody=GetComponent<Rigidbody>();if(rigidbody!=null){rigidbody.sleepThreshold=0;}
if(LOG&&LOG_LEVEL<=-1)Debug.Log(this.gameObject+" has the collider:"+collider,this);
if(collider!=null){
colliderDefaultSize=collider.bounds.size;colliderDefaultCenter=transform.InverseTransformPoint(collider.bounds.center);
GetColliderData();
if(LOG&&LOG_LEVEL<=-1)Debug.Log("collider not null, so:colliderDefaultSize:"+colliderDefaultSize+";colliderDefaultCenter:"+colliderDefaultCenter,this);
}
if(renderer==null){renderer=GetComponentInChildren<Renderer>();
if(LOG&&LOG_LEVEL<=-1)Debug.Log("renderer registered");
}
if(renderer!=null){
GetRendererData();
if(LOG&&LOG_LEVEL<=-1)Debug.Log("renderer not null:"+renderer,this);
}
pos=pos_Pre=transform.position;
}
[NonSerialized]public bool IsUMA=false;[NonSerialized]protected UMAData umaData;[NonSerialized]protected bool UMADataChanged;[NonSerialized]protected bool UMADataChanged_reload;
public void OnCharacterCompleted(UMAData umaData){
SimObject simObject=umaData.transform.root.gameObject.GetComponent<SimObject>();
       if(simObject!=null){
if(simObject.LOG&&simObject.LOG_LEVEL<=1)Debug.LogWarning("[UMAData] the following is an event-called function:do not use 'this' keyword:'this' will refer to the Prefab asset:use the 'simObject' refered from 'UMAData umaData'",umaData);
       if(simObject.UMADataChanged_reload){


Debug.LogWarning("UMADataChanged_reload");
                

          simObject.UMADataChanged_reload=false;
       return;}
simObject.IsUMA=true;simObject.umaData=umaData;simObject.OnUMADataChanged();simObject.UMADataChanged=true;simObject.UMADataChanged_reload=true;
       }
}
protected void OnUMADataChanged(){
CapsuleCollider capsule=umaData.transform.root.gameObject.GetComponent<CapsuleCollider>();BoxCollider box=umaData.transform.root.gameObject.GetComponent<BoxCollider>();
if(capsule!=null||box!=null){
if(LOG&&LOG_LEVEL<=-1)Debug.Log(gameObject+" has the collider:"+collider,this);
collider=box!=null?box as Collider:capsule as Collider;rigidbody=umaData.transform.root.gameObject.GetComponent<Rigidbody>();
colliderDefaultSize=collider.bounds.size;colliderDefaultCenter=transform.InverseTransformPoint(collider.bounds.center);
GetColliderData();
if(LOG&&LOG_LEVEL<=-1)Debug.Log("collider not null, so:colliderDefaultSize:"+colliderDefaultSize+";colliderDefaultCenter:"+colliderDefaultCenter,this);
renderer=GetComponentInChildren<Renderer>();
GetRendererData();
if(LOG&&LOG_LEVEL<=-1)Debug.Log("renderer not null:"+renderer,this);
}
                 UMADataChanged=false;
}
[NonSerialized]protected Vector3 colliderDefaultSize;[NonSerialized]protected Vector3 colliderDefaultCenter;
[NonSerialized]protected Vector3 colliderHalfExtents_v;protected Vector3 colliderHalfExtents{get{return colliderHalfExtents_v*RangeMultiplier;}set{colliderHalfExtents_v=value;}}
[NonSerialized]protected float colliderShortestExtent;
[NonSerialized]public float BodyRadius;
[NonSerialized]protected float BodyRange_v;public float BodyRange{get{return BodyRange_v*RangeMultiplier;}set{BodyRange_v=value;}}
[NonSerialized]protected float RangeMultiplier=2f;
protected void GetColliderData(){
if(!(this is _3DSpriteCharacter)){RangeMultiplier=1f;}else{
if(LOG&&LOG_LEVEL<=1)Debug.Log("this is _3DSprite:"+(this is _3DSpriteCharacter),this);
}
colliderHalfExtents=collider.bounds.extents/2;
colliderShortestExtent=Mathf.Min(collider.bounds.extents.x,collider.bounds.extents.y,collider.bounds.extents.z);
BodyRange=BodyRadius=Mathf.Max(Mathf.Sqrt(Mathf.Pow(collider.bounds.extents.x,2)*2),
                               Mathf.Sqrt(Mathf.Pow(collider.bounds.extents.z,2)*2));
if(LOG&&LOG_LEVEL<=1)Debug.Log("BodyRadius:"+BodyRange+", name:"+name);
}
protected void GetRendererData(){
if(this is _3DSpriteCharacter){
if(LOG&&LOG_LEVEL<=1)Debug.Log("this is _3DSprite:"+(this is _3DSpriteCharacter),this);
renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
renderer.receiveShadows=true;
}
}
protected virtual void OnEnable(){}
protected virtual void OnDisable(){}
protected virtual void OnDestroy(){}
[NonSerialized]protected bool IsGrounded;public bool OnGround{get{return IsGrounded;}}[NonSerialized]protected bool HittingWall;public bool CanCrouch=false;public bool Crouching{get;protected set;}[NonSerialized]protected bool IsHalf=false;[NonSerialized]protected float ToggleIsHalfTimer=0;[NonSerialized]protected float ToggleIsHalfDelay=.5f;[NonSerialized]protected Vector3 CrouchingScale=new Vector3(1,.5f,1);[NonSerialized]protected Vector3 CrouchingCenterOffset=new Vector3(0,.5f,0);
protected virtual void FixedUpdate(){
if(rigidbody!=null){
IsGrounded=false;HittingWall=false;
if(LOG&&LOG_LEVEL<=0)Debug.Log("collisions.Count:"+collisions.Count+";dirtyCollisions.Count:"+dirtyCollisions.Count,this);
foreach(var collision in collisions){
if(collision.Key.transform.root.gameObject!=transform.root.gameObject)for(int i=0;i<collision.Value.Count;i++){var contact=collision.Value[i];if(contact.normal==Vector3.zero)break;
IsGrounded=IsGrounded||((this is Plant)?(collider.bounds.Contains(contact.point)&&Vector3.Angle(contact.normal,transform.up)<=60):(Vector3.Angle(contact.normal,Vector3.up)<=60&&contact.point.y<=collider.bounds.center.y-collider.bounds.extents.y+.1f/*transform.position.y*//*-(collider.bounds.extents.y/2)*//*-colliderDefaultCenter.y-collider.bounds.extents.y+.1f*/));HittingWall=HittingWall||Vector3.Angle(contact.normal,Vector3.up)>60;


if(LOG&&LOG_LEVEL<=-110)Debug.Log("contact.point.y:"+contact.point.y+" (collider.bounds.center.y-collider.bounds.extents.y+.1f):"+(collider.bounds.center.y-collider.bounds.extents.y+.1f)+" collider.bounds.center.y:"+collider.bounds.center.y+" collider.bounds.extents.y:"+collider.bounds.extents.y);


}
dirtyCollisions[collision.Key]=true;}
if(LOG&&LOG_LEVEL<=-100)Debug.Log("IsGrounded:"+IsGrounded,this);
if(CanCrouch){
if(ToggleIsHalfTimer>0){
    ToggleIsHalfTimer-=Time.deltaTime;
}else{
if(IsGrounded){
if(IsHalf){
if(LOG&&LOG_LEVEL<=0)Debug.Log("IsGrounded and not Crouching:disable IsHalf");
if(collider is BoxCollider box){
var centerOld=box.center;
box.size=colliderDefaultSize;box.center=(transform.TransformPoint(colliderDefaultCenter)-transform.position);
var centerChangeOffset=box.center-centerOld;
Debug.LogWarning("centerChangeOffset:"+centerChangeOffset);
rigidbody.position=new Vector3(
    rigidbody.position.x,
    rigidbody.position.y-centerChangeOffset.y*2f,
    rigidbody.position.z
);
//Debug.LogWarning();
//Debug.LogWarning(collider.bounds.center+" "+colliderDefaultCenter);
}
    ToggleIsHalfTimer=ToggleIsHalfDelay;
IsHalf=false;
}
}else{
if(!IsHalf){
Debug.LogWarning("enable IsHalf");
if(collider is BoxCollider box){
box.size=Vector3.Scale(colliderDefaultSize,CrouchingScale);box.center=(transform.TransformPoint(colliderDefaultCenter+CrouchingCenterOffset)-transform.position);//box.center=Vector3.Scale(box.size,Vector3.up*.5f);
}
    ToggleIsHalfTimer=ToggleIsHalfDelay;
IsHalf=true;
}
}
}
}
}
}
[NonSerialized]protected readonly Dictionary<Collider,List<ContactPoint>>collisions=new Dictionary<Collider,List<ContactPoint>>();[NonSerialized]readonly Dictionary<Collider,bool>dirtyCollisions=new Dictionary<Collider,bool>();[NonSerialized]readonly List<ContactPoint>contacts=new List<ContactPoint>();
void OnCollisionStay(Collision collision){
if(LOG&&LOG_LEVEL<=0)Debug.Log("collision stay:"+collision.collider.name);


if(!collisions.ContainsKey(collision.collider)){collisions.Add(collision.collider,new List<ContactPoint>());dirtyCollisions.Add(collision.collider,false);}else if(dirtyCollisions[collision.collider]){collisions[collision.collider].Clear();dirtyCollisions[collision.collider]=false;}collision.GetContacts(contacts);collisions[collision.collider].AddRange(contacts);
//if(!collisions.ContainsKey(collision.collider)){collisions.Add(collision.collider,new List<ContactPoint>());}else if(){}collision.GetContacts(contacts);collisions[collision.collider].AddRange();
if(DRAW_LEVEL<=-100){
for(int i=0;i<collision.contactCount;i++){var contact=collision.GetContact(i);
Debug.DrawRay(contact.point,contact.normal,Color.white,.5f);
}
}


//if(!collisions.ContainsKey(collision.collider)){collisions.Add(collision.collider,new List<ContactPoint>());}else if(){}collision.GetContacts(contacts);collisions[collision.collider].AddRange();
//if(DRAW_LEVEL<=-100){
//for(int i=0;i<collision.contactCount;i++){var contact=collision.GetContact(i);
//Debug.DrawRay(contact.point,contact.normal,Color.white,.5f);
//}
//}


//var tuple=(collision.gameObject,collision.collider,collision);
//if(!collisions.ContainsKey(tuple)){collisions.Add(tuple,new List<ContactPoint>());}collision.GetContacts(collisions[tuple]);
//if(DRAW_LEVEL<=-100){
//for(int i=0;i<collision.contactCount;i++){var contact=collision.GetContact(i);
//Debug.DrawRay(contact.point,contact.normal,Color.white,.5f);
//}
//}
}
void OnCollisionExit(Collision collision){
if(LOG&&LOG_LEVEL<=0)Debug.Log("collision exit:"+collision.collider.name);
//var tuple=(collision.gameObject,collision.collider,collision);
collisions.Remove(collision.collider);dirtyCollisions.Remove(collision.collider);
}
[NonSerialized]Vector3 pos;
[NonSerialized]Vector3 pos_Pre;
[NonSerialized]Vector2Int coord;[NonSerialized]int idx;[NonSerialized]Chunk chunk;[NonSerialized]protected bool OutOfSight_v=true;public virtual bool OutOfSight{get{return OutOfSight_v;}set{OutOfSight_v=value;}}[NonSerialized]protected bool OutOfSight_disable;[NonSerialized]protected bool OutOfSight_enable;//  Objects need to be disabled and enabled by managers
protected virtual void Update(){


if(DEBUG_TELEPORT){DEBUG_TELEPORT=false;
            Debug.LogWarning("DEBUG_TELEPORT:");


Teleport(transform.rotation,transform.position+new Vector3(0,0,6),false);


}


if(rigidbody!=null){
pos=transform.position;
if(pos!=pos_Pre){
OutOfSight=false;
coord=ChunkManager.PosToCoord(pos);idx=ChunkManager.GetIdx(coord.x,coord.y);
    if(pos.y<-128||(ChunkManager.main!=null&&(!ChunkManager.main.Chunks.TryGetValue(idx,out chunk)||!chunk.Built))){
setOutOfSight();
    }
    pos_Pre=pos;
}
}
}
protected void setOutOfSight(){
        rigidbody.velocity=Vector3.zero;
        rigidbody.angularVelocity=Vector3.zero;
        pos=transform.position=pos_Pre;
OutOfSight=true;
}
[NonSerialized]protected float slowLerpTimeout=0;[NonSerialized]protected float slowLerpTime;[NonSerialized]protected float MoveLerpSpeedReduction=.5f;
public virtual void Teleport(Quaternion rotation,Vector3 position,bool goThroughWalls=false,float slowLerpTimeout=1f){
this.slowLerpTimeout=slowLerpTimeout;slowLerpTime=0;
if(rigidbody!=null){
        rigidbody.velocity=Vector3.zero;
        rigidbody.angularVelocity=Vector3.zero;
}
if(!goThroughWalls){
if(Physics.Raycast(new Ray(transform.position,(position-transform.position).normalized),out RaycastHit hitInfo,Vector3.Distance(position,transform.position))){
                Debug.LogWarning("wall hit while teleporting");


var safePos=hitInfo.point+((transform.position-hitInfo.point).normalized*BodyRadius);position=safePos;
                Debug.LogWarning("safePos:"+safePos);
//if(){
//}


}
}
        transform.rotation=            rotation;
        transform.position=pos=pos_Pre=position;
}
protected virtual void LateUpdate(){}
#if UNITY_EDITOR
protected virtual void OnDrawGizmos(){
}
#endif

    
[SerializeField]internal bool DEBUG_TELEPORT=false;


}