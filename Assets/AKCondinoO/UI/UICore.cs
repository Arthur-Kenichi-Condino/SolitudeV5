using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UICore:MonoBehaviour{
[NonSerialized]MainCamera gameState;
void OnEnable(){
gameState=Camera.main.GetComponent<MainCamera>();
     SetActiveBottonUIPanel(-1);
}
[SerializeField]RectTransform[]elementsOnScreen;
public static bool UIReceivedInput{get;private set;}
public static void DetectMouseOnUI(){


//RectTransformUtility.


}
public static void ResetMouseCheck(){
UIReceivedInput=false;
}
public void OnF1SimClick(){
UIReceivedInput=true;
    Debug.LogWarning("OnF1SimClick");
     SetF1Sim();
}
void SetF1Sim(){
     SetActiveBottonUIPanel(0);SetTool(0);
}
public void OnF2BuyClick(){
UIReceivedInput=true;
    Debug.LogWarning("OnF2BuyClick");
     SetF2Buy();
}
void SetF2Buy(){
     SetActiveBottonUIPanel(1);SetTool(3);
}
public void OnF3BuildClick(){
UIReceivedInput=true;
    Debug.LogWarning("OnF3BuildClick");
     SetF3Build();
}
void SetF3Build(){
     SetActiveBottonUIPanel(2);SetTool(3);
}
public GameObject[]BottonUIPanels;public GameObject[]BottonUISubpanels;
void SetActiveBottonUIPanel(int active){if(BottonUIPanels==null||BottonUISubpanels==null)return;
for(int i=0;i<BottonUIPanels.Length&&i<BottonUISubpanels.Length;i++){
                          var panel=BottonUIPanels[i];                var subpanels=BottonUISubpanels[i];
if(i==active){if(panel!=null){panel.SetActive( true);}if(subpanels!=null){subpanels.SetActive( true);}
}else{        if(panel!=null){panel.SetActive(false);}if(subpanels!=null){subpanels.SetActive(false);}
}
}
}
public void OnF3BuildTerrainClick(){
UIReceivedInput=true;
    Debug.LogWarning("OnF3BuildTerrainClick");
     SetActiveF3BuildPanel(0);
}
public void OnF3BuildGardenClick(){
UIReceivedInput=true;
    Debug.LogWarning("OnF3BuildGardenClick");
     SetActiveF3BuildPanel(1);
}
public GameObject[]F3BuildPanels;
void SetActiveF3BuildPanel(int active){if(F3BuildPanels==null)return;
for(int i=0;i<F3BuildPanels.Length;i++){
                             var subpanel=F3BuildPanels[i];
if(i==active){if(subpanel!=null){subpanel.SetActive( true);}
}else{        if(subpanel!=null){subpanel.SetActive(false);}
}
}
}
public void OnSetCurrentToolClick(int tool){
UIReceivedInput=true;
     SetTool(tool);
}
void SetTool(int tool){
gameState.CurrentTool=(MainCamera.SelectedGameModeTool)tool;
    Debug.LogWarning(gameState.CurrentTool);
}
}