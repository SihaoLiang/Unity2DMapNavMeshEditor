using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MapEditor
{
    [System.Serializable]
    public class EditorMapData
    {
        [SerializeField]
        public List<EditorMapPosGrid> m_MapPosGrids = new List<EditorMapPosGrid>();
        public int usingLayer = 0;

        [SerializeField]
        public List<EditorMapMaskGrid> m_MapMaskGridHashSet = new List<EditorMapMaskGrid>();
        public Vector2Int mapSize = Vector2Int.zero;
        public Vector2Int position = Vector2Int.zero;

        public string mapId;
        public SceneAsset sceneAsset;

        //画线颜色
        public Color lineColor = Color.white;
        public bool hideLine = false;

        public bool hideMask = false;
        public bool hidePosition = false;

        /// <summary>
        /// 格子大小
        /// </summary>
        public float gridSize = 0.5f;

        /// <summary>
        /// 清空图层
        /// </summary>
        /// <param name="layer"></param>
        public void ClearLayer(int layer)
        {
            if ((usingLayer & layer) <= 0)
                return;

            for (int i = 0; i < m_MapMaskGridHashSet.Count; i++)
            {
                EditorMapMaskGrid grid = m_MapMaskGridHashSet[i];
                if ((grid.mask & layer) > 0)
                {
                    grid.mask -= layer;
                }

                if (grid.mask <= 0)
                {
                    m_MapMaskGridHashSet.RemoveAt(i);
                    i--;
                }
            }
        }

        public void OnRemoveLayer(int layer)
        {
            if ((usingLayer & layer) <= 0)
                return;

            ClearLayer(layer);
            usingLayer -= layer;
        }

        public void ClearSurplusGrid()
        {
            for (int i = 0; i < m_MapMaskGridHashSet.Count; i++)
            {
                EditorMapMaskGrid grid = m_MapMaskGridHashSet[i];
                if (grid.x < 0 || grid.y < 0 || grid.x >= mapSize.x || grid.y >= mapSize.y || grid.mask == 0)
                {
                    m_MapMaskGridHashSet.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < m_MapPosGrids.Count; i++)
            {
                EditorMapPosGrid grid = m_MapPosGrids[i];
                if (grid.x < 0 || grid.y < 0 || grid.x >= mapSize.x || grid.y >= mapSize.y || string.IsNullOrEmpty(grid.key))
                {
                    m_MapPosGrids.RemoveAt(i);
                    i--;
                }
            }
        }

        public void OnMapSizeChange()
        {
            Vector2Int toVec = position + mapSize;

            for (int i = 0; i < m_MapMaskGridHashSet.Count; i++)
            {
                EditorMapMaskGrid grid = m_MapMaskGridHashSet[i];
                if (grid.x < position.x || grid.y < position.y || grid.x >= toVec.x || grid.y >= toVec.y)
                {
                    m_MapMaskGridHashSet.RemoveAt(i);
                    i--;
                }
            }
        }

        public bool CheckPosKeyEnable(string group, string key)
        {
            if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(key))
                return false;
            int count = 0;
            for (int i = 0; i < m_MapPosGrids.Count; i++)
            {
                EditorMapPosGrid grid = m_MapPosGrids[i];
                if (grid.group == group && grid.key == key)
                    count++;

                if (count > 1) //重复了
                    return false;
            }


            return true;
        }


        public void ClearGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
                return;
            for (int i = 0; i < m_MapPosGrids.Count; i++)
            {
                EditorMapPosGrid grid = m_MapPosGrids[i];
                if (grid.group == group)
                {
                    m_MapPosGrids.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
