//using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using static ActorManagementMentana;
public class Character:AI,iCamFollowable{[NonSerialized]public System.Random dnaRandom=null;//  Não usar DoSave em momento algum se não for um NPC aleatório; a não ser que seja um jogador customizando o personagem
[NonSerialized]public static string[]saveSubfolder=new string[1];
public string ObjName{get{return gameObject.name;}}
public LinkedListNode<iCamFollowable>CamFollowableNode{get;set;}
public bool BeingCamFollowed{get;set;}
public Vector3 CamLookAtUp{get;set;}
public Vector3 CamLookAtForward{get;set;}
public Vector3 CamPosition{get;set;}
public Vector3 CamOffset;
[NonSerialized]public string prefabName;[NonSerialized]public int idForPrefabName=-1;
[NonSerialized]public DynamicCharacterAvatar avatar;[NonSerialized]ThirdPersonAnimatorParamChanger animatorParams;[NonSerialized]public Dictionary<string,DnaSetter>dna;
protected override void Awake(){
                   base.Awake();
CamFollowableNode=MainCamera.CamFollowables.AddLast(this);


if(avatar==null){
avatar=GetComponentInChildren<DynamicCharacterAvatar>();animatorParams=GetComponentInChildren<ThirdPersonAnimatorParamChanger>();
if(avatar!=null){
//dna=avatar.GetDNA();
}}


//Debug.LogWarning(saveFolder+this.name.Replace("(Clone)","").ToString());


//if(avatar==null){
//avatar=GetComponentInChildren<DynamicCharacterAvatar>();
//if(avatar!=null){
//Debug.LogWarning("avatar:"+avatar);
//avatar.savePathType=DynamicCharacterAvatar.savePathTypes.FileSystem;
//Directory.CreateDirectory(avatar.savePath=saveSubfolder[0]=saveFolder+this.name.Replace("(Clone)","").ToString()+"/");saveSubfolder[0]+=(avatar.saveFilename="recipe")+".txt";
//Debug.LogWarning("saveSubfolder[0]:"+saveSubfolder[0]);


////file exists


//if(File.Exists(saveSubfolder[0])){avatar.SetLoadFilename(saveSubfolder[0],DynamicCharacterAvatar.loadPathTypes.FileSystem);}

    

////DoSave on disable
////;
            



////avatar.SetLoadFilename(saveSubfolder[0],DynamicCharacterAvatar.loadPathTypes.FileSystem);


////SetLoadFilename string replace (Clone)
////avatar.loadPathType=DynamicCharacterAvatar.loadPathTypes.FileSystem;
//}}


}
protected override void OnDisable(){
                   base.OnDisable();
if(loaded){
if(avatar!=null){
avatar.DoSave();
}
}
}
[NonSerialized]protected float curAnimTime=-1;
[NonSerialized]protected GameObject leftHand;[NonSerialized]protected Vector3 leftHandHitboxHalfSize=new Vector3(0.125f,0.125f,0.125f);[NonSerialized]protected GameObject rightHand;[NonSerialized]protected Vector3 rightHandHitboxHalfSize=new Vector3(0.125f,0.125f,0.125f);
protected override void Update(){


        //Debug.LogWarning("string.IsNullOrEmpty(prefabName):"+string.IsNullOrEmpty(prefabName));


if(!loaded&&!string.IsNullOrEmpty(prefabName)){


if(avatar==null){
loaded=true;
}else if(IsUMA){
dna=avatar.GetDNA();


RecursiveFinder(transform);void RecursiveFinder(Transform transform){
foreach(Transform child in transform){
if(child.name=="LeftHand")leftHand=child.gameObject;if(child.name=="RightHand")rightHand=child.gameObject;
if(leftHand!=null&&rightHand!=null)break;else RecursiveFinder(child);
}
}
if(leftHand!=null)Debug.LogWarning("leftHand:"+leftHand,leftHand);if(rightHand!=null)Debug.LogWarning("rightHand:"+rightHand,rightHand);


if(dna!=null&&dna.Count>0){


avatar.savePathType=DynamicCharacterAvatar.savePathTypes.FileSystem;
Directory.CreateDirectory(avatar.savePath=saveSubfolder[0]=saveFolder+prefabName+"_"+idForPrefabName.ToString()+"/");saveSubfolder[0]+=(avatar.saveFilename="recipe")+".txt";
avatar.loadPathType=DynamicCharacterAvatar.loadPathTypes.FileSystem;
avatar.loadFilename=saveSubfolder[0];


if(File.Exists(saveSubfolder[0])){
avatar.DoLoad();
}else{


#region random NPC
if(dnaRandom!=null){


                        Debug.LogWarning("dnaRandom:"+dnaRandom);
foreach(var dnaSetter in dna){
                        Debug.LogWarning("dnaSetter.Key:"+dnaSetter.Key+"=>dnaSetter.Value:"+dnaSetter.Value);
//dna[dnaSetter.Key].Set((float)dnaRandom.NextDouble());
}
avatar.BuildCharacter();


}
#endregion


avatar.DoSave();
}


loaded=true;}
}


}
if(loaded){
if(avatar!=null){

                
if(dna!=null){
                //Debug.LogWarning("IsUMA:"+IsUMA+";dna.Count:"+dna.Count);
}


}
}
if(doDamage){
            Debug.LogWarning("do damage");
DoDamageHitbox();    
}


//if(!initialized){initialized=true;
//if(avatar!=null&&!File.Exists(saveSubfolder[0])){
//avatar.DoSave();
//avatar.SetLoadFilename(saveSubfolder[0],DynamicCharacterAvatar.loadPathTypes.FileSystem);
//}
//}
                   base.Update();
}[NonSerialized]protected bool loaded=false;//[NonSerialized]bool initialized=false;
[NonSerialized]Vector3 _camPos=new Vector3();[NonSerialized]Vector3 _camRotatedOffset=new Vector3(1,0,1);
protected override void ProcessMovementInput(){
if(!(bool)Enabled.PAUSE[0]){
if(BeingCamFollowed){
if(IsGrounded||!HittingWall){
#region FORWARD BACKWARD
    if((bool)Enabled.FORWARD [0]){inputMoveSpeed.z+=InputMoveAcceleration.z*Time.deltaTime;Autonomous=AutonomyDelayAfterControl;} 
    if((bool)Enabled.BACKWARD[0]){inputMoveSpeed.z-=InputMoveAcceleration.z*Time.deltaTime;Autonomous=AutonomyDelayAfterControl;}
        if(!(bool)Enabled.FORWARD[0]&&!(bool)Enabled.BACKWARD[0]){inputMoveSpeed.z=0;}
            if( inputMoveSpeed.z>MaxMoveSpeed.z                               ){inputMoveSpeed.z= MaxMoveSpeed.z                               ;}
            if(-inputMoveSpeed.z>MaxMoveSpeed.z*MaxMoveSpeedBackwardMultiplier){inputMoveSpeed.z=-MaxMoveSpeed.z*MaxMoveSpeedBackwardMultiplier;}
#endregion
#region RIGHT LEFT
    if((bool)Enabled.RIGHT   [0]){inputMoveSpeed.x+=InputMoveAcceleration.x*Time.deltaTime;Autonomous=AutonomyDelayAfterControl;} 
    if((bool)Enabled.LEFT    [0]){inputMoveSpeed.x-=InputMoveAcceleration.x*Time.deltaTime;Autonomous=AutonomyDelayAfterControl;}
        if(!(bool)Enabled.RIGHT[0]&&!(bool)Enabled.LEFT[0]){inputMoveSpeed.x=0;}
            if( inputMoveSpeed.x>MaxMoveSpeed.x){inputMoveSpeed.x= MaxMoveSpeed.x;}
            if(-inputMoveSpeed.x>MaxMoveSpeed.x){inputMoveSpeed.x=-MaxMoveSpeed.x;}
#endregion
}else{
inputMoveSpeed.x=0;
inputMoveSpeed.z=0;
}


Debug.LogWarning((bool)Enabled.JUMP[0]+" "+IsGrounded);
if(IsGrounded&&(Jump||(Jump=(bool)Enabled.JUMP[0]!=(bool)Enabled.JUMP[1]))){
inputMoveSpeed.y=MaxMoveSpeed.y;Autonomous=AutonomyDelayAfterControl;


//Debug.LogWarning(inputMoveSpeed.y);


}else{
inputMoveSpeed.y=0;
}


if((bool)Enabled.CROUCH[0]&&(bool)Enabled.CROUCH[0]!=(bool)Enabled.CROUCH[1]){
Crouching=!Crouching;
}


#region ROTATE
inputViewRotationEuler.x+=-Enabled.MOUSE_ROTATION_DELTA_Y[0]*InputViewRotationIncreaseSpeed;
inputViewRotationEuler.y+= Enabled.MOUSE_ROTATION_DELTA_X[0]*InputViewRotationIncreaseSpeed;
inputViewRotationEuler.x=inputViewRotationEuler.x%360;
inputViewRotationEuler.y=inputViewRotationEuler.y%360;
if(Enabled.MOUSE_ROTATION_DELTA_Y[0]!=0||Enabled.MOUSE_ROTATION_DELTA_X[0]!=0)Autonomous=AutonomyDelayAfterControl;
#endregion
}
}
                   base.ProcessMovementInput();
}
protected override void LateUpdate(){
                   base.LateUpdate();
if(BeingCamFollowed){
_camPos=renderer!=null?renderer.transform.position:drawPos;
_camPos.y+=CamOffset.y;
_camPos+=headDrawRotation*Vector3.Scale(_camRotatedOffset,CamOffset);
    //CamPosition=drawPos+(headDrawRotation*CamOffset);
    CamPosition=_camPos;
    CamLookAtForward=((headDrawRotation*(Vector3.forward*1000))-CamPosition).normalized;
    //CamLookAtUp=transform.up;
    CamLookAtUp=Vector3.Cross(CamLookAtForward,headDrawRotation*Vector3.right).normalized;
}
if(attackStance!=-1){
BlockMovement=true;
}
if(deadStance==-1&&hitStance==-1&&attackStance==-1){
BlockMovement=false;
}
}
void InterruptCurrentStance(){
if(attackStance!=-1){
doDamage=false;


            Debug.LogWarning("attackStance interrupted");
}
}
protected override bool Attack(AI enemy){
if(nextAttackTimer>0)return false;
               if(!base.Attack(enemy))return false;// temp commented, uncomment asap
if(deadStance!=-1||hitStance!=-1)return false;if(attackStance==-1){InterruptCurrentStance();attackStance=mathrandom.Next(0,6);curAnimTime=0;


    Debug.LogWarning("Attack(AI enemy):"+attackStance);
animatorParams.OnAttack(attackStance);


if(sfx!=null){sfx.Play((int)ActorSounds._ATTACK,true);}
}
return true;}
public override void OnAttackAnimationEnd(){
attackStance=-1;
}
protected override void OverlappedCollidersOnAttack(){
    Debug.LogWarning("OverlappedCollidersOnAttack");
}
[NonSerialized]protected readonly new List<Collider>attackHitboxColliders=new List<Collider>();[NonSerialized]protected readonly new List<AI>didDamage=new List<AI>();
protected override void DoDamageHitbox(){
attackHitboxColliders.Clear();
Collider[]leftHandColliders;if(leftHand!=null&&(leftHandColliders=Physics.OverlapBox(leftHand.transform.position,leftHandHitboxHalfSize,transform.rotation))!=null){
attackHitboxColliders.AddRange(leftHandColliders);}
Collider[]rightHandColliders;if(rightHand!=null&&(rightHandColliders=Physics.OverlapBox(rightHand.transform.position,rightHandHitboxHalfSize,transform.rotation))!=null){
attackHitboxColliders.AddRange(rightHandColliders);}
            Debug.LogWarning("attackHitboxColliders.Count:"+attackHitboxColliders.Count);


for(int i=0;i<attackHitboxColliders.Count;i++){Collider collider=attackHitboxColliders[i];


    Debug.LogWarning(collider.name);

            
if(collider.isTrigger)continue;
AI enemy;
if(collider.CompareTag("Player")&&(enemy=collider.GetComponent<AI>())!=this&&enemy!=null){
    Debug.LogWarning("collider hit:"+collider.name+"; tag:"+collider.tag,this);


if(!didDamage.Contains(enemy)){didDamage.Add(enemy);                
    enemy.TakeDamage(this);
}


}
}


}
[NonSerialized]protected bool doDamage;
public override void OnAttackAnimationStartDoDamage(){
didDamage.Clear();
doDamage=true;
    Debug.LogWarning("OnAttackAnimationStartDoDamage");
}
public override void OnAttackAnimationStopDoDamage(){
doDamage=false;
    Debug.LogWarning("OnAttackAnimationStopDoDamage");


        Debug.LogWarning("didDamage.Count:"+didDamage.Count);


}
public override void TakeDamage(AI fromEnemy){
                base.TakeDamage(fromEnemy);

        
    Debug.LogWarning("TakeDamage");
if(damage<=0)return;
if(deadStance!=-1)return;InterruptCurrentStance();attackStance=-1;hitStance=0;curAnimTime=0;
if(sfx!=null){sfx.Play((int)ActorSounds._HIT,true);}


//InterruptCurrentStance()
}
#if UNITY_EDITOR
protected override void OnDrawGizmos(){
                   base.OnDrawGizmos();
}
#endif
}