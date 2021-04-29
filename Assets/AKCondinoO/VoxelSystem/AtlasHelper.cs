using System;
using UnityEngine;
public static class AtlasHelper{
public static Material Material{get;private set;}
public static void GetAtlasData(Material material){Material=material;
float _U,_V;var texture=material.GetTexture("_MainTex");material.SetTexture("_MainTex1",texture);var w=texture.width;var h=texture.height;var tilesResolution=material.GetFloat("_TilesResolution"); 
var TileWidth=(w/tilesResolution);
var TileHeight=(h/tilesResolution);
_U=(TileWidth/w);//  X
_V=(TileHeight/h);//  Y
_UVs=new Vector2[Enum.GetNames(typeof(MaterialId)).Length-1];
_UVs[(int)MaterialId.Sand   ]=new Vector2(2*_U,0*_V);
_UVs[(int)MaterialId.Rock   ]=new Vector2(1*_U,0*_V);
_UVs[(int)MaterialId.Dirt   ]=new Vector2(0*_U,1*_V);
_UVs[(int)MaterialId.Bedrock]=new Vector2(1*_U,1*_V);
_UVs[(int)MaterialId.Air    ]=new Vector2(0*_U,0*_V);
}
public static Vector2 GetUV(MaterialId type){switch(type){
case(MaterialId.Sand      ):{return _UVs[(int)MaterialId.Sand   ];}
case(MaterialId.Rock      ):{return _UVs[(int)MaterialId.Rock   ];}
case(MaterialId.Dirt      ):{return _UVs[(int)MaterialId.Dirt   ];}
case(MaterialId.Bedrock   ):{return _UVs[(int)MaterialId.Bedrock];}
default                    :{return _UVs[(int)MaterialId.Air    ];}
}}
public static MaterialId GetMaterial(Vector2 uv){
return(MaterialId)Array.IndexOf(_UVs,uv);
}
public static Vector2[]_UVs;
public static readonly string[]_Shader_Input=new string[]{"_CameraPosition",
                                                            "_FogIntensity"        ,
                                                            "_FogColor"            ,
                                                            "_FogQuadrangularStart",
                                                            "_FogQuadrangularEnd"  ,
                                                                "_FadeQuadrangularStart",
                                                                "_FadeQuadrangularEnd"  ,
};
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