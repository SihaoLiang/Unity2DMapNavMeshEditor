using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MapEditor
{
    public class MapEditorManager
    {
        const string MAP_INFO_DATA_PATH = "Assets/Editor/MapEditor/Data/";
        const string MAP_INFO_DATA_NAME = "MapData.asset";

        static MapEditorData mapData = null;

        [MenuItem("Tools/地图编辑器")]
        public static void OpenMapEditorWindow()
        {
            GetMapInfo();
            MapEditorWindow window = EditorWindow.GetWindow<MapEditorWindow>("地图编辑器");
            window.Show();
        }

        public static MapEditorData GetMapInfo()
        {
            string path = MAP_INFO_DATA_PATH + MAP_INFO_DATA_NAME;

            try
            {
                if (!Directory.Exists(MAP_INFO_DATA_PATH))
                    Directory.CreateDirectory(MAP_INFO_DATA_PATH);

                if (!File.Exists(path))
                {
                    MapEditorData data = ScriptableObject.CreateInstance<MapEditorData>();
                    AssetDatabase.CreateAsset(data, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
            mapData = AssetDatabase.LoadAssetAtPath<MapEditorData>(path);

            return mapData;
        }


        /// <summary>
        /// 通过Id获取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static EditorMapData GetMapDataById(string id)
        {
            if (mapData == null)
            {
                GetMapInfo();
            }

            List<EditorMapData> allMapData = mapData.allMapData;
            if (allMapData == null || allMapData.Count <= 0)
            {
                return null;
            }

            for (int i = 0; i < allMapData.Count; i++)
            {
                EditorMapData data = allMapData[i];
                if (data.mapId == id)
                {
                    return data;
                }
            }

            return null;
        }
        public static EditorMapData Create(string id, SceneAsset sceneAsset)
        {
            EditorMapData temp = GetMapDataById(id);
            if (temp != null)
            {
                Debug.LogError($"创建地图数据失败，地图数据Id {id} 已存在，请使用其他Id");
                return null;
            }

            EditorMapData data = new EditorMapData();
            data.mapId = id;
            data.sceneAsset = sceneAsset;
            data.mapSize = new Vector2Int(10, 10);
            mapData.allMapData.Add(data);
            Save();
            return data;
        }

        public static void Save()
        {
            if (mapData == null)
                return;

            EditorUtility.SetDirty(mapData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 通过场景获取地图数据
        /// </summary>
        /// <param name="sceneAsset"></param>
        /// <returns></returns>
        public static List<EditorMapData> GetMapDatasByScene(SceneAsset sceneAsset)
        {
            if (sceneAsset == null)
                return null;

            if (mapData == null)
            {
                GetMapInfo();
            }

            List<EditorMapData> allMapData = mapData.allMapData;
            if (allMapData == null || allMapData.Count <= 0)
            {
                return null;
            }

            List<EditorMapData> editorMapDatas = new List<EditorMapData>();

            for (int i = 0; i < allMapData.Count; i++)
            {
                EditorMapData data = allMapData[i];
                if (data.sceneAsset == sceneAsset)
                {
                    editorMapDatas.Add(data);
                }
            }

            return editorMapDatas;
        }
        /// <summary>
        /// 通过场景获取地图数据
        /// </summary>
        /// <param name="sceneAsset"></param>
        /// <returns></returns>
        public static EditorMapData GetMapDataByScene(SceneAsset sceneAsset)
        {
            if (mapData == null)
            {
                GetMapInfo();
            }

            List<EditorMapData> allMapData = mapData.allMapData;
            if (allMapData == null || allMapData.Count <= 0)
            {
                return null;
            }

            for (int i = 0; i < allMapData.Count; i++)
            {
                EditorMapData data = allMapData[i];
                if (data.sceneAsset == sceneAsset)
                {
                    return data;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取层级
        /// </summary>
        /// <returns></returns>
        public static List<EditorMapMaskLayer> GetEditorMapMaskLayers()
        {
            if (mapData == null)
            {
                GetMapInfo();
            }

            return mapData.mapMaskLayers;
        }


        /// <summary>
        /// 获取层级
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static EditorMapMaskLayer GetMaskLayerByKey(string layer)
        {
            if (mapData == null)
                return null;
            if (mapData.mapMaskLayers == null || mapData.mapMaskLayers.Count <= 0)
                return null;

            for (int i = 0; i < mapData.mapMaskLayers.Count; i++)
            {
                EditorMapMaskLayer mapMaskLayer = mapData.mapMaskLayers[i];
                if (layer == mapMaskLayer.key)
                {
                    return mapMaskLayer;
                }
            }

            return null;
        }

        public static EditorMapMaskLayer GetMaskLayerByLayerId(int layer)
        {
            if (mapData == null)
                return null;
            if (mapData.mapMaskLayers == null || mapData.mapMaskLayers.Count <= 0)
                return null;

            for (int i = 0; i < mapData.mapMaskLayers.Count; i++)
            {
                EditorMapMaskLayer mapMaskLayer = mapData.mapMaskLayers[i];
                if (layer == mapMaskLayer.id)
                {
                    return mapMaskLayer;
                }
            }

            return null;
        }

        /// <summary>
        /// 添加层级
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static EditorMapMaskLayer AddMaskLayer(string layer)
        {
            if (string.IsNullOrEmpty(layer))
            {
                Debug.LogError("Mask层级名称不能为空");
                return null;
            }

            EditorMapMaskLayer editorMapMaskLayer = GetMaskLayerByKey(layer);
            if (editorMapMaskLayer != null)
            {
                Debug.LogError($"Mask:{layer}层级已存在");
                return null;
            }

            editorMapMaskLayer = new EditorMapMaskLayer();
            editorMapMaskLayer.key = layer;
            int i = 0;
            while (true)
            {
                int layerId = 1 << i;
                EditorMapMaskLayer temp = GetMaskLayerByLayerId(layerId);
                if (temp == null)
                {
                    editorMapMaskLayer.id = layerId;
                    break;
                }
                i++;
            }

            editorMapMaskLayer.color = Color.red;
            mapData.mapMaskLayers.Add(editorMapMaskLayer);

            Debug.Log($"Mask:{layer}层级已添加");
            return editorMapMaskLayer;
        }

        /// <summary>
        /// 删除层级
        /// </summary>
        /// <param name="layer"></param>
        public static void RemoveLayer(string layer)
        {
            if (mapData == null)
                return;

            if (mapData.mapMaskLayers == null || mapData.mapMaskLayers.Count <= 0)
                return;

            for (int i = 0; i < mapData.mapMaskLayers.Count; i++)
            {
                EditorMapMaskLayer mapMaskLayer = mapData.mapMaskLayers[i];
                if (layer == mapMaskLayer.key)
                {

                    OnRemoveLayer(mapMaskLayer.id);
                    mapData.mapMaskLayers.RemoveAt(i);
                    Debug.Log($"删除层级:{layer}成功");
                    return;
                }
            }

            Debug.LogError($"找不到Mask层级:{layer}");
        }

        static void OnRemoveLayer(int layer)
        {
            if (mapData == null)
                return;

            if (mapData.allMapData.Count <= 0)
                return;

            for (int i = 0; i < mapData.allMapData.Count; i++)
            {
                EditorMapData editorMapData = mapData.allMapData[i];
                editorMapData.OnRemoveLayer(layer);
            }
        }
       
    }
}
