using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UICore:MonoBehaviour{
void OnEnable(){
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
public GameObject[]BottonUIPanels;
void SetActiveBottonUIPanel(int active){if(BottonUIPanels==null)return;
for(int i=0;i<BottonUIPanels.Length;i++){var panel=BottonUIPanels[i];
                               if(i==active){panel.SetActive( true);
                               }else{        panel.SetActive(false);
                               }
}
}
public GameObject[]F3BuildPanels;
public void OnF3BuildTerrainClick(){
    Debug.LogWarning("OnF3BuildTerrainClick");
}
}