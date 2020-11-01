using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Buttons:MonoBehaviour{
[NonSerialized]Button button;[NonSerialized]Image image;
private void Awake(){   
button=GetComponent<Button>();image=button.image;image.alphaHitTestMinimumThreshold=1f;
}
}