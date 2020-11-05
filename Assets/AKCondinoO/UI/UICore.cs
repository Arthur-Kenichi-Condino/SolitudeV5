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
public void OnF1SimClick(){
    Debug.LogWarning("OnF1SimClick");
     SetActiveBottonUIPanel(0);
}
public void OnF2BuyClick(){
    Debug.LogWarning("OnF2BuyClick");
     SetActiveBottonUIPanel(1);
}
public void OnF3BuildClick(){
    Debug.LogWarning("OnF3BuildClick");
     SetActiveBottonUIPanel(2);
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
    Debug.LogWarning("OnF3BuildTerrainClick");
     SetActiveF3BuildPanel(0);
}
public void OnF3BuildGardenClick(){
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
}