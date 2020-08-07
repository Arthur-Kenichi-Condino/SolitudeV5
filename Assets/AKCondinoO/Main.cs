using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Main{
public static bool Stop{
    get{bool tmp;lock(Stop_Syn){tmp=Stop_v;      }return tmp;}
    set{         lock(Stop_Syn){    Stop_v=value;}           }
}[NonSerialized]static readonly object Stop_Syn=new object();[NonSerialized]static bool Stop_v;
static void Quit(){
Stop=true;
}
[RuntimeInitializeOnLoadMethod]
#pragma warning disable IDE0051
static void RunOnStart(){
#pragma warning restore IDE0051
Application.quitting+=Quit;
}
}