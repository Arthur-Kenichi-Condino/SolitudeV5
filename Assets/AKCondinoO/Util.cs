using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Util{
public static double NextDouble(this System.Random random,double min,double max){return random.NextDouble()*(max-min)+min;}
}