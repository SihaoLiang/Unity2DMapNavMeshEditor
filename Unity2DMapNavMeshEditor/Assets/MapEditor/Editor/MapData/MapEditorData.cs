using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapEditor
{
    [System.Serializable]
    public class MapEditorData : ScriptableObject
    {
        //所有地图数据
        [SerializeField]
        public List<EditorMapData> allMapData;
        //所有层级数据
        [SerializeField]
        public List<EditorMapMaskLayer> mapMaskLayers;

        public MapEditorData()
        {
            allMapData = new List<EditorMapData>();
            mapMaskLayers = new List<EditorMapMaskLayer>();
        }
    }
}