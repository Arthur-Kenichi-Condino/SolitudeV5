using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CharacterDataUI:MonoBehaviour{
[NonSerialized]AI actor;
[NonSerialized]RectTransform SPBar;[NonSerialized]Vector3 SPBarScale;
[NonSerialized]RectTransform FPBar;
void Start(){
actor=GetComponentInParent<AI>();
SPBar=transform.Find("CurSPBar")as RectTransform;
FPBar=transform.Find("CurFPBar")as RectTransform;
}
void Update(){
SPBarScale=SPBar.localScale;SPBarScale.x=(actor.Attributes.CurStamina/(actor.Attributes.BaseMaxStamina<=0?1f:actor.Attributes.BaseMaxStamina))*.5f;SPBar.localScale=SPBarScale;
}
}