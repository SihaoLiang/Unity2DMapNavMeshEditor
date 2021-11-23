using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MapEditor
{
    public class MapEditorWindow : EditorWindow
    {
        MapTabType mapTabType = MapTabType.OPERRATION;
        MapEditorType mapEditorType = MapEditorType.MASK;

        //当前数据
        EditorMapData curMapData = null;
        //当前场景
        SceneAsset curSceneAsset;
        //当前名字
        string activeSceneName = string.Empty;

        //笔刷
        MapEditorBrush m_Brush;
        //笔刷位置
        Vector2Int BrushPos;
        //层级
        List<EditorMapMaskLayer> m_EditorMapMaskLayers;

        string maskLayerName = string.Empty;

        //所有层级
        private string[] m_LayerMaskOption;

        //当前选择的层级
        private int m_LayerSelectIndex;
        private int m_LastLayerSelectIndex;

        //Ctrl是否按下
        bool CtrlKeyDown = false;


        string m_TempGroupName = string.Empty;

        //定点组
        private List<string> m_PosGroupOptions = new List<string>();
        private int m_MapPosGroupIndex;
        private int m_LastMapPosGroupIndex;

        //索引格子
        Dictionary<int, EditorMapMaskGrid> m_EditorMapMaskDic = new Dictionary<int, EditorMapMaskGrid>();
        Dictionary<int, EditorMapPosGrid> m_EditorMapPosDic = new Dictionary<int, EditorMapPosGrid>();


        Vector2 scrollVec = Vector2.zero;
        public enum MapTabType
        {
            OPERRATION,
            LAYER,
        }

        public enum MapEditorType
        {
            MASK,
            POSITION,
        }

        int m_ShapeSelectIndex = 0;
        private string[] m_ShapeOptions = new string[] { "默认", "十字", "菱形", "方形" };


        private void Awake()
        {
            mapTabType = MapTabType.OPERRATION;
            mapEditorType = MapEditorType.MASK;
            LookAt(Vector3.zero);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            LoadMapData();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void LoadMapData()
        {
            Scene scene = SceneManager.GetActiveScene();
            activeSceneName = scene.name;
            if (scene != null)
            {
                curSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                curMapData = MapEditorManager.GetMapDataByScene(curSceneAsset);

                UpdateEditorMapMaskDic();
                UpdateEditorPosDic();
            }
            UpdateLayer();
        }

        /// <summary>
        /// 重置笔刷
        /// </summary>
        public void ResetBrush()
        {
            if (m_Brush != null)
            {
                m_Brush.id = -1;
                m_Brush.brushType = BrushType.NONE;
                m_Brush.grid = null;
                m_Brush.groupKey = string.Empty;
            }
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        public void UpdateEditorMapMaskDic()
        {

            if (curMapData == null)
                return;

            m_EditorMapMaskDic.Clear();

            if (curMapData.m_MapMaskGridHashSet.Count > 0)
            {
                for (int i = 0; i < curMapData.m_MapMaskGridHashSet.Count; i++)
                {
                    EditorMapMaskGrid grid = curMapData.m_MapMaskGridHashSet[i];
                    m_EditorMapMaskDic.Add(grid.x << 16 | grid.y, grid);
                }
            }
        }

        /// <summary>
        /// 更新布点
        /// </summary>
        public void UpdateEditorPosDic()
        {

            if (curMapData == null)
                return;

            m_MapPosGroupIndex = 0;
            m_LastMapPosGroupIndex = 0;
            m_PosGroupOptions.Clear();
            if (curMapData.m_MapPosGrids.Count > 0)
            {
                List<string> groups = new List<string>();

                for (int i = 0; i < curMapData.m_MapPosGrids.Count; i++)
                {
                    EditorMapPosGrid grid = curMapData.m_MapPosGrids[i];
                    m_EditorMapPosDic.Add(grid.x << 16 | grid.y, grid);
                    if (!m_PosGroupOptions.Contains(grid.group))
                        m_PosGroupOptions.Add(grid.group);
                }
            }
        }

        /// <summary>
        /// 更新图层
        /// </summary>
        private void UpdateLayer()
        {
            m_EditorMapMaskLayers = MapEditorManager.GetEditorMapMaskLayers();
            m_LayerMaskOption = new string[m_EditorMapMaskLayers.Count];

            if (curMapData != null)
            {
                m_LayerSelectIndex = 0;
                bool bEverything = true;
                for (int i = 0; i < m_EditorMapMaskLayers.Count; i++)
                {
                    EditorMapMaskLayer maskLayer = m_EditorMapMaskLayers[i];
                    if ((curMapData.usingLayer & maskLayer.id) > 0)
                    {
                        m_LayerSelectIndex |= (1 << i);
                    }
                    else
                    {
                        bEverything = false;
                    }
                }

                if (bEverything)
                    m_LayerSelectIndex = -1;
            }

            for (int i = 0; i < m_EditorMapMaskLayers.Count; i++)
            {
                m_LayerMaskOption[i] = m_EditorMapMaskLayers[i].key;
            }
        }

        void OnDidOpenScene()
        {
            LoadMapData();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("操作", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                mapTabType = MapTabType.OPERRATION;
                ResetBrush();
            }

            if (GUILayout.Button("层级", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                mapTabType = MapTabType.LAYER;
                ResetBrush();
            }

            if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                MapEditorManager.Save();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUI.changed = false;
            {
                switch (mapTabType)
                {
                    case MapTabType.OPERRATION:
                        OperationGUI();
                        break;

                    case MapTabType.LAYER:
                        LayerGUI();
                        break;
                }
            }



            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 操作GUI
        /// </summary>
        void OperationGUI()
        {
            if (curMapData == null)
            {
                EditorGUILayout.HelpBox("该场景还没有地图数据，创建一个吧", MessageType.Info);
                GUILayout.BeginHorizontal();
                GUILayout.Label("地图标识:", GUILayout.Width(60));
                activeSceneName = EditorGUILayout.TextField(activeSceneName);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("创建"))
                {
                    curMapData = MapEditorManager.Create(activeSceneName, curSceneAsset);
                }
                return;
            }


            GUILayout.BeginVertical("box");
            GUILayout.Label("地图信息", "ShurikenModuleTitle");

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("地图标识:", GUILayout.Width(60));
            EditorGUILayout.TextField("", curMapData.mapId);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("地图大小:", GUILayout.Width(60));
            curMapData.mapSize = EditorGUILayout.Vector2IntField("", curMapData.mapSize);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("地图位置:", GUILayout.Width(60));
            curMapData.position = EditorGUILayout.Vector2IntField("", curMapData.position);

            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("底线颜色:", GUILayout.Width(60));
            curMapData.lineColor = EditorGUILayout.ColorField("", curMapData.lineColor);
            GUILayout.Label("隐藏:", GUILayout.Width(30));
            curMapData.hideLine = EditorGUILayout.Toggle(curMapData.hideLine);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("格子尺寸:", GUILayout.Width(60));
            curMapData.gridSize = EditorGUILayout.FloatField("", curMapData.gridSize);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("隐藏图层:", GUILayout.Width(60));
            curMapData.hideMask = EditorGUILayout.Toggle(curMapData.hideMask);
            GUILayout.Label("隐藏定点:", GUILayout.Width(60));
            curMapData.hidePosition = EditorGUILayout.Toggle(curMapData.hidePosition);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (GUILayout.Button("清空多余的格子"))
            {
                curMapData.ClearSurplusGrid();
                UpdateEditorMapMaskDic();
                SceneView.RepaintAll();
            }
            GUILayout.Space(10);


            GUILayout.EndVertical();
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("图层编辑", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                mapEditorType = MapEditorType.MASK;
                ResetBrush();
            }

            if (GUILayout.Button("定点编辑", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                mapEditorType = MapEditorType.POSITION;
                ResetBrush();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (mapEditorType == MapEditorType.MASK)
                BrushGUI();
            else if (mapEditorType == MapEditorType.POSITION)
                BrushPosGUI();
            GUILayout.EndVertical();

        }

        /// <summary>
        /// 笔刷
        /// </summary>
        void BrushGUI()
        {
            GUILayout.Label("图层编辑", "ShurikenModuleTitle");
            GUILayout.Space(5);


            if (m_LayerMaskOption == null || m_LayerMaskOption.Length == 0)
            {
                EditorGUILayout.HelpBox("暂时没有图层,创建一个吧", MessageType.Info);
                return;
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("可视层级:", GUILayout.Width(60));
                m_LayerSelectIndex = EditorGUILayout.MaskField(m_LayerSelectIndex, m_LayerMaskOption);
                if (m_LastLayerSelectIndex != m_LayerSelectIndex)
                {
                    curMapData.usingLayer = 0;
                    if (m_LayerSelectIndex == 0)
                    {
                        curMapData.usingLayer = 0;
                    }
                    else if (m_LayerSelectIndex > 0)
                    {
                        for (int i = 0; i < m_EditorMapMaskLayers.Count; i++)
                        {
                            if ((m_LayerSelectIndex & (1 << i)) > 0)
                                curMapData.usingLayer |= m_EditorMapMaskLayers[i].id;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < m_EditorMapMaskLayers.Count; i++)
                        {
                            curMapData.usingLayer |= m_EditorMapMaskLayers[i].id;
                        }
                    }
                    m_LastLayerSelectIndex = m_LayerSelectIndex;
                    SceneView.RepaintAll();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }



            GUILayout.BeginHorizontal();
            GUILayout.Label("笔刷", GUILayout.Width(60));
            m_ShapeSelectIndex = EditorGUILayout.Popup(m_ShapeSelectIndex, m_ShapeOptions);
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            for (int i = 0; i < m_EditorMapMaskLayers.Count; i++)
            {
                EditorMapMaskLayer maskLayer = m_EditorMapMaskLayers[i];

                if ((curMapData.usingLayer & maskLayer.id) > 0)
                {
                    GUILayout.BeginHorizontal();

                    bool toggle = EditorGUILayout.Toggle(m_Brush != null && m_Brush.id == maskLayer.id, GUILayout.Width(30));
                    if (toggle)
                    {
                        if (m_Brush == null)
                            m_Brush = new MapEditorBrush();
                        m_Brush.id = maskLayer.id;
                        m_Brush.color = maskLayer.color;
                        m_Brush.brushType = BrushType.MASK;
                    }
                    else
                    {
                        if (m_Brush != null && m_Brush.brushType == BrushType.MASK && m_Brush.id == maskLayer.id)
                            ResetBrush();
                    }
                    GUILayout.Label(maskLayer.key, GUILayout.Width(60));
                    EditorGUILayout.ColorField(maskLayer.color);
                    if (GUILayout.Button("清空", GUILayout.Width(50)))
                    {

                        if (m_Brush != null && m_Brush.brushType == BrushType.MASK && m_Brush.id == maskLayer.id)
                            ResetBrush();

                        curMapData.ClearLayer(maskLayer.id);
                        UpdateEditorMapMaskDic();
                        SceneView.RepaintAll();
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.HelpBox("擦除：Ctrl + 笔刷", MessageType.None);
        }

        void BrushPosGUI()
        {
            GUILayout.Label("分组编辑", "ShurikenModuleTitle");
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("组名:", GUILayout.Width(60));
            m_TempGroupName = EditorGUILayout.TextField("", m_TempGroupName);

            if (GUILayout.Button("新增"))
            {
                if (!string.IsNullOrEmpty(m_TempGroupName))
                {

                    if (m_PosGroupOptions == null)
                        m_PosGroupOptions = new List<string>();

                    m_PosGroupOptions.Add(m_TempGroupName);
                }

            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);


            if (m_PosGroupOptions == null || m_PosGroupOptions.Count <= 0)
            {
                EditorGUILayout.HelpBox("没有布点分组，创建一个吧", MessageType.None);
                return;
            }


            GUILayout.Label("定点编辑", "ShurikenModuleTitle");
            GUILayout.Space(5);


            GUILayout.BeginHorizontal();
            GUILayout.Label("分组:", GUILayout.Width(60));
            m_MapPosGroupIndex = EditorGUILayout.Popup(m_MapPosGroupIndex, m_PosGroupOptions.ToArray());
            if (m_MapPosGroupIndex != m_LastMapPosGroupIndex)
            {
                m_LastMapPosGroupIndex = m_MapPosGroupIndex;
                ResetBrush();
                SceneView.RepaintAll();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);


            GUILayout.BeginHorizontal();
            GUILayout.Label("新增编辑:", GUILayout.Width(60));
            bool editorToggle = EditorGUILayout.Toggle(m_Brush != null && m_Brush.brushType == BrushType.POSITION && m_Brush.groupKey == m_PosGroupOptions[m_MapPosGroupIndex]);
            if (editorToggle)
            {
                if (m_Brush == null)
                    m_Brush = new MapEditorBrush();

                ResetBrush();
                m_Brush.color = Color.red;
                m_Brush.brushType = BrushType.POSITION;
                m_Brush.groupKey = m_PosGroupOptions[m_MapPosGroupIndex];
            }
            else
            {
                if (m_Brush != null && m_Brush.brushType == BrushType.POSITION && m_Brush.groupKey == m_PosGroupOptions[m_MapPosGroupIndex])
                    ResetBrush();
            }

            GUILayout.EndHorizontal();




            scrollVec = EditorGUILayout.BeginScrollView(scrollVec);
            for (int i = 0; i < curMapData.m_MapPosGrids.Count; i++)
            {
                EditorMapPosGrid grid = curMapData.m_MapPosGrids[i];
                if (grid.group != m_PosGroupOptions[m_MapPosGroupIndex])
                    continue;


                GUILayout.BeginVertical("GroupBox");
                GUILayout.BeginHorizontal();
                GUILayout.Label("编辑", GUILayout.Width(60));
                bool toggle = EditorGUILayout.Toggle(m_Brush != null && m_Brush.brushType == BrushType.POSITION && m_Brush.grid == grid);
                GUILayout.EndHorizontal();

                if (toggle)
                {
                    if (m_Brush == null)
                        m_Brush = new MapEditorBrush();
                    m_Brush.color = grid.color;
                    m_Brush.brushType = BrushType.POSITION;
                    m_Brush.grid = grid;
                    m_Brush.groupKey = string.Empty;
                }
                else
                {
                    if (m_Brush != null && m_Brush.brushType == BrushType.POSITION && m_Brush.grid == grid)
                        ResetBrush();
                }

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Label("键值", GUILayout.Width(60));
                grid.key = EditorGUILayout.TextField("", grid.key);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Label("位置", GUILayout.Width(60));

                GUILayout.Label("x", GUILayout.Width(15));
                grid.x = EditorGUILayout.IntField("", grid.x, GUILayout.Width(30));
                GUILayout.Label("y", GUILayout.Width(15));
                grid.y = EditorGUILayout.IntField("", grid.y, GUILayout.Width(30));
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Label("颜色", GUILayout.Width(60));
                grid.color = EditorGUILayout.ColorField("", grid.color);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                if (GUILayout.Button("删除"))
                {
                    if (m_Brush != null && m_Brush.brushType == BrushType.POSITION && m_Brush.grid == grid)
                        ResetBrush();

                    curMapData.m_MapPosGrids.RemoveAt(i);
                    SceneView.RepaintAll();
                    break;
                }
              
                if (!curMapData.CheckPosKeyEnable(grid.group, grid.key))
                    EditorGUILayout.HelpBox("键值错误或者重复", MessageType.Error);
                GUILayout.EndVertical();
            }
            GUILayout.Space(5);

            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("清空分组"))
            {
                if (EditorUtility.DisplayDialog("清空分组", "是否清空分组？", "确定", "取消"))
                {
                    curMapData.ClearGroup(m_PosGroupOptions[m_MapPosGroupIndex]);
                    ResetBrush();
                    UpdateEditorPosDic();
                }
            }
        }


        void LayerGUI()
        {
            if (m_EditorMapMaskLayers == null)
                return;


            GUILayout.Label("地图层级", "ShurikenModuleTitle");
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("层级数量:", GUILayout.Width(60));
            EditorGUILayout.IntField("", m_EditorMapMaskLayers.Count);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);


            GUILayout.BeginHorizontal();
            maskLayerName = EditorGUILayout.TextField("", maskLayerName);
            if (GUILayout.Button("添加层级", GUILayout.Width(60)))
            {
                MapEditorManager.AddMaskLayer(maskLayerName);
                UpdateLayer();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (m_EditorMapMaskLayers.Count <= 0)
                return;

            GUILayout.Label("层级", "ShurikenModuleTitle");
            GUILayout.Space(5);

            for (int i = 0; i < m_EditorMapMaskLayers.Count; i++)
            {
                EditorMapMaskLayer mapMaskLayer = m_EditorMapMaskLayers[i];
                GUILayout.BeginHorizontal();
                mapMaskLayer.key = EditorGUILayout.TextField("", mapMaskLayer.key, GUILayout.Width(60));
                EditorGUILayout.IntField("", mapMaskLayer.id, GUILayout.Width(60));
                mapMaskLayer.color = EditorGUILayout.ColorField("", mapMaskLayer.color);
                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    MapEditorManager.RemoveLayer(mapMaskLayer.key);
                    UpdateEditorMapMaskDic();
                    UpdateLayer();
                    SceneView.RepaintAll();
                    break;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.Layout:
                    int id = GUIUtility.GetControlID(FocusType.Passive);
                    HandleUtility.AddDefaultControl(id);
                    UpdateMousePos(e.mousePosition);
                    break;
                case EventType.Repaint:
                    DrawLine();
                    DrawLayer();
                    DrawPosition();
                    break;
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.LeftControl)
                        CtrlKeyDown = true;
                    break;
                case EventType.KeyUp:
                    CtrlKeyDown = e.keyCode == KeyCode.LeftControl ? false : CtrlKeyDown;
                    break;
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        BrushLayer();
                    }
                    break;

                case EventType.MouseUp:
                    if (e.button == 0)
                    {
                        BrushPosition();
                    }
                    break;
            }
        }

        /// <summary>
        /// 锁定视觉
        /// </summary>
        /// <param name="pos"></param>
        private void LookAt(Vector3 pos)
        {
            SceneView view = SceneView.sceneViews[0] as SceneView;
            if (view != null)
            {
                view.LookAt(pos, Quaternion.Euler(Vector3.right * 90), 40);
                view.orthographic = true;
                view.isRotationLocked = true;
            }
        }

        /// <summary>
        /// 更新鼠标位置
        /// </summary>
        /// <param name="mousePos"></param>
        private void UpdateMousePos(Vector2 mousePos)
        {
            if (curMapData == null)
                return;

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            Vector2Int fromVec = curMapData.position;
            Vector2Int toVec = curMapData.position + curMapData.mapSize;

            int x = (int)(ray.origin.x / curMapData.gridSize);
            int y = (int)(ray.origin.z / curMapData.gridSize);
            if (x >= fromVec.x && x < toVec.x &&
                y >= fromVec.y && y < toVec.y &&
                (x != BrushPos.x || y != BrushPos.y))
            {
                BrushPos = new Vector2Int(x, y);
            }
        }

        void DrawPosition()
        {
            if (curMapData == null)
                return;

            if (curMapData.hidePosition)
                return;

            Handles.color = Color.white;
            GUIStyle style = new GUIStyle();
            for (int i = 0; i < curMapData.m_MapPosGrids.Count; ++i)
            {
                EditorMapPosGrid grid = curMapData.m_MapPosGrids[i];
                if (grid.group == m_PosGroupOptions[m_MapPosGroupIndex])
                {
                    float x = (curMapData.position.x + grid.x) * curMapData.gridSize;
                    float y = (curMapData.position.y + grid.y) * curMapData.gridSize;

                    Vector3 p = new Vector3(x, 0, y);
                    Handles.color = grid.color;
                    style.normal.textColor = grid.color;
                    Handles.DrawWireDisc(p, Vector3.up, 1f);
                    Handles.DrawSolidDisc(p, Vector3.up, 0.2f);
                    Handles.Label(p, grid.key, style);
                }
            }
        }

        void BrushPosition()
        {
            if (m_Brush == null || m_Brush.brushType != BrushType.POSITION)
                return;

            Vector2Int vec = BrushPos - curMapData.position;
            if (!CheckPositionLegal(vec))
                return;

            if (m_Brush.grid != null)
            {
                m_Brush.grid.x = vec.x;
                m_Brush.grid.y = vec.y;
            }
            else if (!string.IsNullOrEmpty(m_Brush.groupKey))
            {
                EditorMapPosGrid grid = new EditorMapPosGrid();
                grid.x = vec.x;
                grid.y = vec.y;
                grid.group = m_PosGroupOptions[m_MapPosGroupIndex];
                grid.color = Color.red;
                curMapData.m_MapPosGrids.Add(grid);
            }
            Repaint();
            SceneView.RepaintAll();
        }
        /// <summary>
        /// 刷图层
        /// </summary>
        void BrushLayer()
        {
            if (m_Brush == null || m_Brush.id < 0 || m_Brush.brushType != BrushType.MASK)
                return;

            if (Tools.current != Tool.Move)
                return;

            Vector2Int[] brushShape = MapEditorBrush.BrushShape[m_ShapeSelectIndex];

            for (int i = 0; i < brushShape.Length; i++)
            {
                Vector2Int vec = new Vector2Int(brushShape[i].x + BrushPos.x, brushShape[i].y + BrushPos.y);

                vec -= curMapData.position;
                if (CheckPositionLegal(vec))
                {
                    int key = vec.x << 16 | vec.y;
                    if (CtrlKeyDown)
                    {
                        if (m_EditorMapMaskDic.ContainsKey(key))
                        {
                            if ((m_EditorMapMaskDic[key].mask & m_Brush.id) > 0)
                                m_EditorMapMaskDic[key].mask -= m_Brush.id;
                        }
                    }
                    else
                    {
                        if (m_EditorMapMaskDic.ContainsKey(key))
                        {
                            m_EditorMapMaskDic[key].mask |= m_Brush.id;
                        }
                        else
                        {
                            EditorMapMaskGrid grid = new EditorMapMaskGrid();
                            grid.x = vec.x;
                            grid.y = vec.y;
                            grid.mask |= m_Brush.id;
                            curMapData.m_MapMaskGridHashSet.Add(grid);
                            m_EditorMapMaskDic.Add(key, grid);
                        }
                    }
                }
            }

            SceneView.RepaintAll();
        }

        /// <summary>
        /// 绘制图层
        /// </summary>
        private void DrawLayer()
        {
            if (curMapData == null)
                return;

            if (curMapData.hideMask)
                return;

            Handles.color = Color.white;

            if (curMapData.usingLayer == 0)
                return;


            for (int i = 0; i < curMapData.m_MapMaskGridHashSet.Count; i++)
            {
                EditorMapMaskGrid grid = curMapData.m_MapMaskGridHashSet[i];

                float x = (curMapData.position.x + grid.x) * curMapData.gridSize;
                float y = (curMapData.position.y + grid.y) * curMapData.gridSize;

                if ((curMapData.usingLayer & grid.mask) <= 0)
                    continue;

                Handles.DrawSolidRectangleWithOutline(new Vector3[]
                {
                    new Vector3(x, 0, y),
                    new Vector3(x + curMapData.gridSize, 0, y),
                    new Vector3(x + curMapData.gridSize, 0, y + curMapData.gridSize),
                    new Vector3(x, 0, y + curMapData.gridSize),
                }, GetLayerColor(grid.mask), curMapData.lineColor);
            }
        }

        /// <summary>
        /// 绘制底线
        /// </summary>
        private void DrawLine()
        {
            if (curMapData == null)
                return;

            if (curMapData.hideLine)
                return;

            Vector2Int fromVec = curMapData.position;
            Vector2Int toVec = curMapData.position + curMapData.mapSize;

            float gridSize = curMapData.gridSize;
            Vector2Int mapSize = curMapData.mapSize;
            Handles.color = curMapData.lineColor;

            for (int x = fromVec.x; x <= toVec.x; ++x)
            {
                Handles.DrawLine(new Vector3(x * gridSize, 0, fromVec.y * gridSize), new Vector3(x * gridSize, 0, toVec.y * gridSize));
            }
            for (int y = fromVec.y; y <= toVec.y; ++y)
            {
                Handles.DrawLine(new Vector3(fromVec.x * gridSize, 0, y * gridSize), new Vector3(toVec.x * gridSize, 0, y * gridSize));
            }
        }

        /// <summary>
        /// 检测越界
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool CheckPositionLegal(Vector2Int pos)
        {
            if (curMapData == null)
                return false;

            Vector2Int fromVec = Vector2Int.zero;
            Vector2Int toVec = curMapData.mapSize;
            if (pos.x >= fromVec.x && pos.x < toVec.x && pos.y >= fromVec.y && pos.y < toVec.y)
            {
                return true;
            }
            return false;
        }
        public Color GetLayerColor(int layer)
        {
            if (m_EditorMapMaskLayers == null || m_EditorMapMaskLayers.Count <= 0)
                return Color.white;

            Color color = Color.black;
            float r = 0, g = 0, b = 0;
            int count = 0;

            for (int i = 0; i < m_EditorMapMaskLayers.Count; i++)
            {
                EditorMapMaskLayer mapMaskLayer = m_EditorMapMaskLayers[i];
                if ((layer & mapMaskLayer.id) > 0 && (curMapData.usingLayer & mapMaskLayer.id) > 0)
                {

                    r += mapMaskLayer.color.r;
                    g += mapMaskLayer.color.g;
                    b += mapMaskLayer.color.b;
                    count++;
                }
            }

            if (count > 0)
                color = new Color(r / count, g / count, b / count, 1);

            return color;
        }
    }
}
