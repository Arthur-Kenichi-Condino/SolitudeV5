using System;
public static class AtlasHelper{
}
/// <summary>
///  Lista de tipos de material de terreno.
/// </summary>
[Serializable]public enum MaterialId:short{
Unknown=-1,Air=0,
Bedrock=1,//  Indestrutível
Dirt=2,
Rock=3,
Sand=4,
}