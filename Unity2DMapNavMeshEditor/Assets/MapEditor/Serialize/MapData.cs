using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapEditor
{
    public class MapData
    {
        public List<MapMaskGrid> mapMaskGrids = new List<MapMaskGrid>();
        public List<MapPosGrid> mapPosGrids = new List<MapPosGrid>();
        public Vector2Int mapSize = Vector2Int.zero;
        public Vector2Int position = Vector2Int.zero;
        public int MapId;
    }
}