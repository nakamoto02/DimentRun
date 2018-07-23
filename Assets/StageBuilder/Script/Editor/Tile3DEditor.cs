using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Dimension;

[CustomEditor(typeof(Tile3D))]
public class Tile3DEditor : Editor
{
    public Tile3D tiler { get { return (Tile3D)target; } }
    public Vector3 origin { get { return tiler.transform.position; } }

    #region Tool
    
    public enum ToolMode
    {
        Transform,
        Mesh,
        Stage
    }
    private ToolMode toolMode = ToolMode.Transform;
    //-----------------------------------------------------
    //  MeshTool
    //-----------------------------------------------------
    public enum MeshTool
    {
        Building,
        Painting
    }
    private MeshTool meshTool = MeshTool.Building;
    // painting modes
    public enum PaintModes
    {
        Brush,
        Fill
    }
    private PaintModes paintMode = PaintModes.Brush;
    //-----------------------------------------------------
    //  StageTool
    //-----------------------------------------------------
    public enum StageTool
    {
        Data,
        Gimick,
        BackPoint
    }
    private StageTool stageTool = StageTool.Data;
    public enum BackModes
    {
        Right,
        Left,
        ReSpawrn
    }
    private BackModes backMode = BackModes.Right;

    #endregion

    #region EdiorData

    // 単体選択しているときの選択しているブロックの保存するクラス
    private class SingleSelection
    {
        public Vector3Int Tile;
        public Vector3 Face;
    }

    // 複数選択しているときの選択しているブロックの保存するクラス
    private class MultiSelection
    {
        public List<Vector3Int> Tiles = new List<Vector3Int>();
        public Vector3 Face;
        public MultiSelection() { }
        public MultiSelection(SingleSelection from)
        {
            Tiles.Add(from.Tile);
            Face = from.Face;
        }
    }

    // active selections
    private SingleSelection hover = null;
    private MultiSelection selected = null;
    private Tile3D.Face brush = new Tile3D.Face() { Hidden = true };
    // 選択状況のデータ
    private Event e;    // 処理中のcurrentイベント
    private bool invokeRepaint = false; // 塗り替えるかどうか
    private bool draggingBlock = false; // ブロックを複数選択しているかどうか
    private bool interacting = false; // 

    #endregion

    #region CreateData

    bool isExecution = false;

    Vector3 stageForward;   // ステージ正面方向
    Vector3 startPosition;  // スタート位置
    Vector3 goalPosition;   // ゴール位置

    List<Vector3> respawnPoints     = new List<Vector3>();
    List<BackLine> backLinesRight   = new List<BackLine>();
    List<BackLine> backLinesLeft    = new List<BackLine>();

    // PATH
    const string DATA_PATH = "Assets/StageBuilder/StageData/Data/";
    const string MESH_PATH = "Assets/StageBuilder/StageData/Mesh/";

    //-----------------------------------------------------
    // 新しく作成
    //-----------------------------------------------------
    public void NewData()
    {
        isExecution = true;
        Debug.Log("Create Start");

        StageData data = CreateData();  // データの作成
        UpdateData(ref data);           // データ更新
        tiler.stageData = data;         // 変数に登録

        Debug.Log("Create Finish");
        isExecution = false;
    }
    //-----------------------------------------------------
    // 保存
    //-----------------------------------------------------
    public void SaveStage()
    {
        isExecution = true;
        Debug.Log("Save Start");

        UpdateData(ref tiler.stageData);  // データ更新

        Debug.Log("Save Finish");
        isExecution = false;
    }
    //-----------------------------------------------------
    //  読み込み
    //-----------------------------------------------------
    public void LoadStage()
    {
        isExecution = true;
        Debug.Log("Load Start");

        ReadData(tiler.stageData);        // データ読み込み

        Debug.Log("Load Finish");
        isExecution = false;
    }

    //-----------------------------------------------------
    //  StageData作成
    //-----------------------------------------------------
    StageData CreateData()
    {
        // ScriptableObject準備
        StageData data = new StageData
        {
            name = tiler.stageName,
        };
        // Meshの作成
        Mesh renderMesh     = tiler.RenderMesh;
        Mesh colliderMesh   = tiler.ColliderMesh;
        renderMesh.name     = data.name + " Render Mesh";
        colliderMesh.name   = data.name + " Collider Mesh";
        AssetDatabase.CreateAsset(renderMesh,   MESH_PATH + renderMesh.name   + ".asset");
        AssetDatabase.CreateAsset(colliderMesh, MESH_PATH + colliderMesh.name + ".asset");
        data.renderMesh     = renderMesh;
        data.colliderMesh   = colliderMesh;

        // StageDataを保存
        AssetDatabase.CreateAsset(data, DATA_PATH + data.name + ".asset");

        return data;
    }
    //-----------------------------------------------------
    // StageData更新
    //-----------------------------------------------------
    void UpdateData(ref StageData data)
    {
        // Mesh準備
        Mesh renderMesh   = tiler.RenderMesh;
        Mesh colliderMesh = tiler.ColliderMesh;

        // 変更開始
        AssetDatabase.StartAssetEditing();

        // Data
        data.blocks                 = tiler.Blocks.ToArray();
        data.renderMesh.vertices    = renderMesh.vertices;
        data.renderMesh.triangles   = renderMesh.triangles;
        data.renderMesh.uv          = renderMesh.uv;
        data.colliderMesh.vertices  = colliderMesh.vertices;
        data.colliderMesh.triangles = colliderMesh.triangles;
        data.colliderMesh.uv        = colliderMesh.uv;
        data.stageForward           = stageForward;             // ステージの正面方向
        data.startPoint             = startPosition;            // スタートの位置
        data.goalPoint              = goalPosition;             // ゴールの位置
        data.respawnPoints          = respawnPoints.ToArray();  // 復帰地点
        data.backLinesRight         = backLinesRight.ToArray(); // 2Dから3Dへ切り替える際の戻る位置(右)
        data.backLinesLeft          = backLinesLeft.ToArray();  // 2Dから3Dへ切り替える際の戻る位置(左)

        AssetDatabase.StopAssetEditing();   // 変更完了
        EditorUtility.SetDirty(data);       // 変更をUnityEditorに伝える
        AssetDatabase.SaveAssets();         // すべてのアセットを保存
    }
    //-----------------------------------------------------
    //  StageDataの読み込み
    //-----------------------------------------------------
    void ReadData(StageData data)
    {
        // データの読み込み
        tiler.Blocks.Clear();
        tiler.Blocks.AddRange(data.blocks);

        stageForward    = data.stageForward;            // ステージ正面方向
        startPosition   = data.startPoint;              // スタートの位置
        goalPosition    = data.goalPoint;               // ゴールの位置
        respawnPoints.Clear();                          // 復帰地点リセット
        backLinesRight.Clear();                         // 2Dから3Dへ切り替える際の戻る位置(右)リセット
        backLinesLeft.Clear();                          // 2Dから3Dへ切り替える際の戻る位置(左)リセット
        respawnPoints.AddRange(data.respawnPoints);     // 復帰地点
        backLinesRight.AddRange(data.backLinesRight);   // 2Dから3Dへ切り替える際の戻る位置(右)
        backLinesLeft.AddRange(data.backLinesLeft);     // 2Dから3Dへ切り替える際の戻る位置(左)

        // ステージ情報の更新
        tiler.RebuildBlockMap();
        tiler.Rebuild();
    }
    //-----------------------------------------------------
    //  一時的データの保存
    //-----------------------------------------------------
    void OneSaveData()
    {
        StageData data = new StageData {
            stageForward    = stageForward,
            startPoint      = startPosition,
            goalPoint       = goalPosition,
            respawnPoints   = respawnPoints.ToArray(),
            backLinesRight  = backLinesRight.ToArray(),
            backLinesLeft   = backLinesLeft.ToArray()
        };

        tiler.oneTimeSave = data;
    }
    void OneReadData()
    {
        StageData data = tiler.oneTimeSave;

        stageForward = data.stageForward;
        startPosition = data.startPoint;
        goalPosition = data.goalPoint;
        respawnPoints.Clear();
        backLinesRight.Clear();
        backLinesLeft.Clear();
        respawnPoints.AddRange(data.respawnPoints);
        backLinesRight.AddRange(data.backLinesRight);
        backLinesLeft.AddRange(data.backLinesLeft);
    }
        
        
    #endregion

    #region DefaultEvent

    private void OnEnable()
    {
        isExecution = false;
        //OneReadData();
        InitStageVector();

        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnDisable()
    {
        //OneSaveData();

        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Rebuild Mesh"))
            tiler.Rebuild();

        // StageData
        GUILayout.Space(16);

        if (tiler.stageData == null)
            tiler.stageName = EditorGUILayout.TextField(tiler.stageName);
        else
            EditorGUILayout.LabelField(tiler.stageData.name);

        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stageData"));
        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void OnSceneGUI()
    {
        // 選択状況の保存データ
        SelectInitialize();

        // overlay gui
        Handles.BeginGUI();
        {
            toolMode = (ToolMode)GUI.Toolbar(new Rect(10, 10, 200, 20), (int)toolMode, new[] { "Move", "Mesh", "Data" });

            // meshTool
            if (toolMode == ToolMode.Mesh)
            {
                meshTool = (MeshTool)GUI.Toolbar(new Rect(10, 35, 200, 20), (int)meshTool, new[] { "Build", "Paint" });
                if (meshTool == MeshTool.Painting)
                    selected = null;

                // tileset
                if (meshTool == MeshTool.Painting)
                    GUI.Window(0, new Rect(10, 80, 200, 300), PaintingWindow, "Tiles");
            }
            else
            if (toolMode == ToolMode.Stage)
            {
                stageTool = (StageTool)GUI.Toolbar(new Rect(10, 35, 200, 20), (int)stageTool, new[] { "Data", "Gimick", "Back" });

                // Data
                if (stageTool == StageTool.Data)
                    GUI.Window(0, new Rect(10, 80, 200, 300), DataWindow, "Data");

                // Gimick
                if (stageTool == StageTool.Gimick)
                    GUI.Window(0, new Rect(10, 80, 200, 300), GimickWindow, "Gimick");

                // Back
                if (stageTool == StageTool.BackPoint)
                    GUI.Window(0, new Rect(10, 80, 200, 300), BackWindow, "BackPoint");
            }
        }
        Handles.EndGUI();

        if (toolMode == ToolMode.Transform)
        {
            TransformMove();
            return;
        }

        DefaultControll();

        if (toolMode == ToolMode.Mesh)
        {   // Mesh作成
            switch (meshTool)
            {
                case MeshTool.Building: MeshToolBuild(); break;
                case MeshTool.Painting: MeshToolPaint(); break;
            }
        }
        else
        if (toolMode == ToolMode.Stage)
        {   // 情報の登録
            //ActiveHover();

            switch (stageTool)
            {
                case StageTool.Data: StageToolData(); break;
                case StageTool.Gimick: StageToolGimick(); break;
                case StageTool.BackPoint: StageToolBackPoint(); break;
            }
        }
        // 描画
        Drawing();

        // 塗りなおし
        if (invokeRepaint) Repaint();

        // always keep the tiler selected for now
        // later should detect if something is being grabbed or hovered
        Selection.activeGameObject = tiler.transform.gameObject;
    }

    #endregion

    #region Tile3D Editor

    //-----------------------------------------------------
    //  選択状況の初期化
    //-----------------------------------------------------
    void SelectInitialize()
    {
        e = Event.current;
        invokeRepaint = false;
        draggingBlock = false;
        // ControlもAltキーも押されてなくて、左クリックが押されているか
        // Unityのシーンビューの操作を邪魔しないためだと思われる
        interacting = (!e.control && !e.alt && e.button == 0);
    }
    //-----------------------------------------------------
    // シーンビューで使用する通常のツールの状態
    //-----------------------------------------------------
    void DefaultControll()
    {
        Tools.current = Tool.None;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }
    //-----------------------------------------------------
    //  マウスの位置にあるタイルを取得
    //-----------------------------------------------------
    void ActiveHover()
    {
        if (!(e.type == EventType.MouseMove || e.type == EventType.MouseDrag)) return;
        if (!interacting) return;
        if (draggingBlock) return;

        var next = GetSelectionAt(e.mousePosition);
        // 前回のタイルと違うならGizmoを表示し直す
        if ((hover == null && next != null) || (hover != null && next == null) || (hover != null && next != null && (hover.Tile != next.Tile || hover.Face != next.Face)))
            invokeRepaint = true;
        hover = next;
    }
    //-----------------------------------------------------
    //  描画
    //-----------------------------------------------------
    void Drawing()
    {
        if (hover != null) DrawSelection(hover, Color.magenta);
        if (selected != null) DrawSelection(selected, Color.blue);
    }

    #endregion

    #region MoveTool
    //-----------------------------------------------------
    // Transform
    //-----------------------------------------------------
    void TransformMove()
    {
        if (Tools.current == Tool.None)
            Tools.current = Tool.Move;
    }
    #endregion

    #region MeshTool
    //=========================================================================
    //
    //  MeshTool
    //
    //=========================================================================
    //-----------------------------------------------------
    // Building
    //-----------------------------------------------------
    void MeshToolBuild()
    {
        // 複数選択されている場合
        if (selected != null)
        {
            Handles.color = Color.blue;
            EditorGUI.BeginChangeCheck();

            var start = CenterOfSelection(selected) + selected.Face * 0.5f;
            var pulled = Handles.Slider(start, selected.Face);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "EditMesh");

                draggingBlock = true;
                if (hover != null)
                {
                    hover = null;
                    invokeRepaint = true;
                }

                // get distance and direction
                var distance = (pulled - start).magnitude;
                var outwards = (int)Mathf.Sign(Vector3.Dot(pulled - start, selected.Face));

                // create or destroy a block (depending on direction)
                if (distance > 1f)
                {
                    var newTiles = new List<Vector3Int>();
                    foreach (var tile in selected.Tiles)
                    {
                        var was = tile;
                        var next = tile + selected.Face.Int() * outwards;

                        if (outwards > 0)
                            tiler.Create(next, was);
                        else
                            tiler.Destroy(was);
                        tiler.Rebuild();

                        newTiles.Add(next);
                    }

                    selected.Tiles = newTiles;
                    tiler.Rebuild();
                }
            }
        }
        // select tiles
        if (!draggingBlock && interacting)
        {
            if (e.type == EventType.MouseDown && !e.shift)
            {
                if (hover == null)
                    selected = null;
                else
                    selected = new MultiSelection(hover);
                invokeRepaint = true;
            }
            else if (e.type == EventType.MouseDrag && selected != null && hover != null && !selected.Tiles.Contains(hover.Tile))
            {
                selected.Tiles.Add(hover.Tile);
                invokeRepaint = true;
            }
        }

        ActiveHover();
    }
    //-----------------------------------------------------
    // Painting
    //-----------------------------------------------------
    void MeshToolPaint()
    {
        ActiveHover();

        // Paint
        if (IsTilePaint()) TilePaint();
        if (IsRotationPaint()) RotaionPaint();
    }
    // タイルを塗るかどうか
    bool IsTilePaint()
    {
        if (!interacting) return false;
        if (hover == null) return false;
        if (tiler.At(hover.Tile) == null) return false;
        if (!(e.type == EventType.MouseDown || e.type == EventType.MouseDrag)) return false;
        return true;
    }
    // タイルを塗る
    void TilePaint()
    {
        var block = tiler.At(hover.Tile);

        if (paintMode == PaintModes.Brush)
        {   // 一枚ずつ
            if (SetBlockFace(block, hover.Face, brush))
                tiler.Rebuild();
        }
        else
        if (paintMode == PaintModes.Fill)
        {   // 塗りつぶし
            var face = GetBlockFace(block, hover.Face);
            if (FillBlockFace(block, face))
                tiler.Rebuild();
        }
    }
    // タイルを回転しながら塗るかどうか
    bool IsRotationPaint()
    {
        if (e.button != 1) return false;
        if (e.control) return false;
        if (e.alt) return false;
        if (hover == null) return false;
        if (!(e.type == EventType.MouseDown || e.type == EventType.MouseDrag)) return false;
        return true;
    }
    // 回転しながら塗る
    void RotaionPaint()
    {
        brush.Rotation = (brush.Rotation + 1) % 4;

        var cell = tiler.At(hover.Tile);
        if (cell != null && SetBlockFace(cell, hover.Face, brush))
            tiler.Rebuild();
    }
    #endregion

    #region StageTool
    //=========================================================================
    //
    //  StageTool
    //
    //=========================================================================
    Vector3 stageRight;     // ステージ右方向
    // ステージの座標をワールド座標に
    Vector3 StageToWorld(Vector3 stage)
    {
        return stageRight * stage.x + Vector3.up * stage.y + stageForward * stage.z;
    }
    // ワールド座標をステージの座標に
    Vector3 WorldToStage(Vector3 world)
    {
        return stageRight * world.x + Vector3.up * world.y + stageForward * world.z;
    }
    // ステージの軸を計算
    void InitStageVector()
    {
        stageRight = Quaternion.Euler(new Vector3(0, 90, 0)) * stageForward;
        stageRight = new Vector3(Mathf.Round(stageRight.x), 0, Mathf.Round(stageRight.z));
    }
    //-----------------------------------------------------
    //  Data
    //-----------------------------------------------------
    void StageToolData()
    {
        // StageForward
        Handles.color = Color.blue;
        Handles.Slider(tiler.transform.position + Vector3.up * 5, stageForward);
    }
    //-----------------------------------------------------
    //  Gimick
    //-----------------------------------------------------
    void StageToolGimick()
    {
        ActiveHover();
    }
    //-----------------------------------------------------
    //  Back3D
    //-----------------------------------------------------
    void StageToolBackPoint()
    {
        if (backMode == BackModes.Right)
        {
            DrawBackLineList(ref backLinesRight);
        }
        else
        if (backMode == BackModes.Left)
        {
            DrawBackLineList(ref backLinesLeft);
        }
        else
        if (backMode == BackModes.ReSpawrn)
        {
            DrawReSpawnPoint(ref respawnPoints);
        }
    }
    // 2Dから3Dに戻る際の位置を表示
    void DrawBackLineList(ref List<Dimension.BackLine> backLines)
    {
        int lineCnt = 0, highCnt = 0;
        foreach (var line in backLines)
        {
            // 奥行き
            Vector3 depthCenter = stageForward * line.endDepth + Vector3.up * 3.0f;
            Handles.color = Color.blue;
            Vector3 depth = Handles.Slider(depthCenter, stageForward, 3.0f, Handles.ArrowCap, EditorPrefs.GetFloat("MoveSnapZ", 1f));
            if (interacting) backLines[lineCnt].endDepth = Conversion.Vector3ToFloat(Vector3.Scale(depth, stageForward));

            Vector3[] solidPoints = new Vector3[] {
                    depth + Vector3.up * 3.0f + stageRight * 5.0f, depth + Vector3.up * 3.0f - stageRight * 5.0f,
                    depth - Vector3.up * 3.0f - stageRight * 5.0f, depth - Vector3.up * 3.0f + stageRight * 5.0f
                };
            Handles.color = Color.yellow;
            Handles.DrawSolidRectangleWithOutline(solidPoints, new Color(1, 1, 0, 0.3f), new Color(1, 1, 0, 1));

            // 高さ
            if (lineCnt != 0)
            {
                highCnt = 0;
                Handles.color = Color.red;
                foreach (Vector2 highSide in line.highSide)
                {
                    Handles.DrawLine(
                        stageRight * highSide.x + Vector3.up * highSide.y + stageForward * backLines[lineCnt - 1].endDepth,
                        stageRight * highSide.x + Vector3.up * highSide.y + stageForward * line.endDepth
                        );
                    ++highCnt;
                }
            }
            ++lineCnt;
        }
    }
    // リスポーン地点を表示
    void DrawReSpawnPoint(ref List<Vector3> respawnPoints)
    {
        int cnt = 0;
        foreach (Vector3 point in respawnPoints)
        {
            // 位置
            Vector3 movePos = StageToWorld(point);
            EditorGUI.BeginChangeCheck();
            {   // 表示
                Handles.color = Handles.xAxisColor;
                movePos = Handles.Slider(movePos, stageRight);  // 右
                Handles.color = Handles.zAxisColor;
                movePos = Handles.Slider(movePos, stageForward);// 正面
                Handles.color = Color.yellow;
                Handles.DrawLine(movePos + Vector3.up * 3, movePos - Vector3.up * 1);
            }
            if (EditorGUI.EndChangeCheck() && interacting)
            {   // 保存
                respawnPoints[cnt] = WorldToStage(movePos);
            }
            ++cnt;
        }
    }

    #endregion

    #region Tile3D CreateBlock

    private bool SetBlockFace(Tile3D.Block block, Vector3 normal, Tile3D.Face brush)
    {
        Undo.RecordObject(target, "SetBlockFaces");

        for (int i = 0; i < Tile3D.Faces.Length; i++)
        {
            if (Vector3.Dot(normal, Tile3D.Faces[i]) > 0.8f)
            {
                if (!brush.Hidden)
                {
                    if (brush != block.Faces[i])
                    {
                        block.Faces[i] = brush;
                        return true;
                    }
                }
                else if (!block.Faces[i].Hidden)
                {
                    block.Faces[i].Hidden = true;
                    return true;
                }
            }
        }

        return false;
    }

    private Tile3D.Face GetBlockFace(Tile3D.Block block, Vector3 face)
    {
        for (int i = 0; i < Tile3D.Faces.Length; i++)
        {
            if (Vector3.Dot(face, Tile3D.Faces[i]) > 0.8f)
                return block.Faces[i];
        }

        return block.Faces[0];
    }

    private bool FillBlockFace(Tile3D.Block block, Tile3D.Face face)
    {
        Vector3Int perp1, perp2;
        GetPerpendiculars(hover.Face, out perp1, out perp2);

        var active = new List<Tile3D.Block>();
        var filled = new HashSet<Tile3D.Block>();
        var directions = new Vector3Int[4] { perp1, perp1 * -1, perp2, perp2 * -1 };
        var outwards = hover.Face.Int();
        var changed = false;

        filled.Add(block);
        active.Add(block);
        SetBlockFace(block, hover.Face, brush);

        while (active.Count > 0)
        {
            var from = active[0];
            active.RemoveAt(0);

            for (int i = 0; i < 4; i++)
            {
                var next = tiler.At(from.Tile + directions[i]);
                if (next != null && !filled.Contains(next) && tiler.At(from.Tile + directions[i] + outwards) == null && GetBlockFace(next, hover.Face).Tile == face.Tile)
                {
                    filled.Add(next);
                    active.Add(next);
                    if (SetBlockFace(next, hover.Face, brush))
                        changed = true;
                }
            }
        }

        return changed;
    }

    private Vector3 CenterOfSelection(Vector3Int tile)
    {
        return origin + new Vector3(tile.x + 0.5f, tile.y + 0.5f, tile.z + 0.5f);
    }

    private Vector3 CenterOfSelection(SingleSelection selection)
    {
        return CenterOfSelection(selection.Tile);
    }

    private Vector3 CenterOfSelection(MultiSelection selection)
    {
        var tile = Vector3.zero;
        foreach (var t in selection.Tiles)
            tile += new Vector3(t.x + 0.5f, t.y + 0.5f, t.z + 0.5f);
        tile /= selection.Tiles.Count;
        tile += origin;

        return tile;
    }

    private void DrawSelection(SingleSelection selection, Color color)
    {
        var center = CenterOfSelection(selection);
        DrawSelection(center, selection.Face, color);
    }

    private void DrawSelection(MultiSelection selection, Color color)
    {
        foreach (var tile in selection.Tiles)
            DrawSelection(CenterOfSelection(tile), selection.Face, color);
    }

    private void DrawSelection(Vector3 center, Vector3 face, Color color)
    {
        var front = center + face * 0.5f;
        Vector3 perp1, perp2;
        GetPerpendiculars(face, out perp1, out perp2);

        var a = front + (-perp1 + perp2) * 0.5f;
        var b = front + (perp1 + perp2) * 0.5f;
        var c = front + (perp1 + -perp2) * 0.5f;
        var d = front + (-perp1 + -perp2) * 0.5f;

        Handles.color = color;
        Handles.DrawDottedLine(a, b, 2f);
        Handles.DrawDottedLine(b, c, 2f);
        Handles.DrawDottedLine(c, d, 2f);
        Handles.DrawDottedLine(d, a, 2f);
    }

    private void GetPerpendiculars(Vector3 face, out Vector3 updown, out Vector3 leftright)
    {
        var up = (face.y == 0 ? Vector3.up : Vector3.right);
        updown = Vector3.Cross(face, up);
        leftright = Vector3.Cross(updown, face);
    }

    private void GetPerpendiculars(Vector3 face, out Vector3Int updown, out Vector3Int leftright)
    {
        Vector3 perp1, perp2;
        GetPerpendiculars(face, out perp1, out perp2);
        updown = perp1.Int();
        leftright = perp2.Int();
    }

    //-----------------------------------------------------
    //  引数(Screen座標)の位置にあるタイルを取得
    //-----------------------------------------------------
    private SingleSelection GetSelectionAt(Vector2 mousePosition)
    {
        var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        var hits = Physics.RaycastAll(ray);

        foreach (var hit in hits)
        {
            var other = hit.collider.gameObject.GetComponent<Tile3D>();
            if (other == tiler)
            {
                var center = hit.point - hit.normal * 0.5f;

                return new SingleSelection()
                {
                    Tile = (center - origin).Floor(),
                    Face = hit.normal
                };
            }
        }

        return null;
    }

    #endregion

    #region Window
    //=========================================================================
    //
    //  Window
    //
    //=========================================================================
    const int WINDOW_UP     = 20;   // ウィンドウの上の位置
    const int WINDOW_LEFT   = 10;   // ウィンドウの左の位置
    const int WINDOW_WIDTH  = 180;  // ウィンドウの幅
    const int WINDOW_HEIGHT = 280;  // ウィンドウの高さ
    //-----------------------------------------------------
    //  PaintWindow
    //-----------------------------------------------------
    void PaintingWindow(int id)
    {
        // paint mode
        paintMode = (PaintModes)GUI.Toolbar(new Rect(WINDOW_LEFT, WINDOW_UP, WINDOW_WIDTH, 30), (int)paintMode, new[] { "Brush", "Fill" });
        brush.Rotation = GUI.Toolbar(new Rect(WINDOW_LEFT + 50, WINDOW_UP + 40, 130, 20), brush.Rotation, new[] { "0", "90", "180", "270" });
        brush.FlipX = GUI.Toggle(new Rect(WINDOW_LEFT + 50, WINDOW_UP + 65, 90, 20), brush.FlipX, "FLIP X");
        brush.FlipY = GUI.Toggle(new Rect(WINDOW_LEFT + 115, WINDOW_UP + 65, 90, 20), brush.FlipY, "FLIP Y");

        // empty tile
        if (DrawPaletteTile(new Rect(WINDOW_LEFT, WINDOW_UP + 40, 40, 40), null, brush.Hidden))
            brush.Hidden = true;

        // tiles
        if (tiler.MeshTexture == null)
        {
            GUI.Label(new Rect(WINDOW_LEFT, WINDOW_UP + 95, WINDOW_WIDTH, 80), "Requires a Material\nwith a Texture");
        }
        else
        {
            SelectPanel(0, 95, tiler.MeshTexture);
        }

        // repaint
        var e = Event.current;
        if (e.type == EventType.MouseMove || e.type == EventType.MouseDown)
            Repaint();
    }
    //-----------------------------------------------------
    //  DataWindow
    //-----------------------------------------------------
    void DataWindow(int id)
    {
        // StageForward
        EditorGUILayout.LabelField("StageForward");

        Vector3 forward = new Vector3(0, 0, 0);
        EditorGUI.BeginChangeCheck();
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("X"))
                    forward = new Vector3(1, 0, 0);
                if (GUILayout.Button("-X"))
                    forward = new Vector3(-1, 0, 0);
                if (GUILayout.Button("Z"))
                    forward = new Vector3(0, 0, 1);
                if (GUILayout.Button("-Z"))
                    forward = new Vector3(0, 0, -1);
            }
            EditorGUILayout.EndHorizontal();
        }
        if (EditorGUI.EndChangeCheck())
        {
            stageForward = forward;
            InitStageVector();
            //tiler.playerObj.forward  = forward;
            //tiler.playerObj.position = StageToWorld(startPosition);
            //tiler.goalObj.position   = StageToWorld(goalPosition);
        }

        // Start
        EditorGUILayout.LabelField("StartPosition");
        //EditorGUI.BeginChangeCheck();
        //{
        //    startPosition = EditorGUILayout.Vector3Field("", startPosition);
        //}
        //if(EditorGUI.EndChangeCheck() && tiler.playerObj != null)
        //{
        //    //tiler.playerObj.position = StageToWorld(startPosition);
        //}

        // Goal
        EditorGUILayout.LabelField("GoalPosition");
        //EditorGUI.BeginChangeCheck();
        //{
        //    goalPosition = EditorGUILayout.Vector3Field("", goalPosition);
        //}
        //if(EditorGUI.EndChangeCheck() && tiler.goalObj != null)
        //{
        //    tiler.goalObj.position = StageToWorld(goalPosition);
        //}
    }
    //-----------------------------------------------------
    //  GimickWindow
    //-----------------------------------------------------
    void GimickWindow(int id)
    {
        if (tiler.GimickTexture == null)
        {
            GUI.Label(new Rect(WINDOW_LEFT, 25, WINDOW_WIDTH, 80), "Requires a Resource\nGimick Texture");
        }
        else
        {
            SelectPanel(0, 0, tiler.GimickTexture);
        }

        // repaint
        var e = Event.current;
        if (e.type == EventType.MouseMove || e.type == EventType.MouseDown)
            Repaint();
    }
    //-----------------------------------------------------
    //  BackWindow
    //-----------------------------------------------------
    Vector2 scrollPos = Vector2.zero;
    void BackWindow(int id)
    {
        backMode = (BackModes)GUI.Toolbar(new Rect(WINDOW_LEFT, WINDOW_UP, WINDOW_WIDTH, 30), (int)backMode, new[] { "Right", "Left", "Spawn" });
        GUILayout.Space(32);

        if (backMode == BackModes.Right)
        {
            backLinesRight = BackLineWindow(backLinesRight);
        }

        if (backMode == BackModes.Left)
        {
            backLinesLeft = BackLineWindow(backLinesLeft);
        }

        if (backMode == BackModes.ReSpawrn)
        {   // リスポーン地点
            respawnPoints = ReSpawnPointWindow(respawnPoints);
        }
    }
    // 2Dから3Dに戻す際の位置を指定するウィンドウ
    List<Dimension.BackLine> BackLineWindow(List<Dimension.BackLine> backLines)
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box);
        {
            int lineCnt = 0, highCnt = 0, select = 0;
            bool isLineAdd = false, isLineDeleate = false, isHighAdd = false, isHighDeleate = false;

            foreach (var line in backLines)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    // Depth
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(lineCnt.ToString() + ":", GUILayout.Width(70));
                        EditorGUILayout.LabelField("Depth", GUILayout.Width(40));
                        backLines[lineCnt].endDepth = EditorGUILayout.FloatField(line.endDepth, GUILayout.Width(50));
                    }
                    EditorGUILayout.EndHorizontal();

                    // HighSide
                    if (lineCnt != 0)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        {
                            EditorGUILayout.LabelField("HighSide", GUILayout.Width(60));

                            highCnt = 0;
                            isHighAdd = isHighDeleate = false;
                            foreach (var highSide in line.highSide)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.LabelField(highCnt.ToString() + ":", GUILayout.Width(15));
                                    backLines[lineCnt].highSide[highCnt] =
                                        EditorGUILayout.Vector2Field("", line.highSide[highCnt], GUILayout.Width(140));
                                }
                                EditorGUILayout.EndHorizontal();
                                // Button
                                EditorGUILayout.BeginHorizontal();
                                {
                                    if (GUILayout.Button("Add", GUILayout.Width(70)))
                                    {
                                        isHighAdd = true;
                                        select = highCnt;
                                    }
                                    if (backLines[lineCnt].highSide.Count > 1 && GUILayout.Button("Deleate", GUILayout.Width(70)))
                                    {
                                        isHighDeleate = true;
                                        select = highCnt;
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                ++highCnt;
                            }
                            if (isHighAdd)
                                backLines[lineCnt].highSide.Insert(select + 1, new Vector2(0, 0));
                            if (isHighDeleate)
                                backLines[lineCnt].highSide.RemoveAt(select);
                        }
                        EditorGUILayout.EndVertical();
                    }

                    // Button
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Add", GUILayout.Width(70)))
                        {
                            isLineAdd = true;
                            select = lineCnt;
                        }
                        if (backLines.Count > 1 && GUILayout.Button("Deleate", GUILayout.Width(70)))
                        {
                            isLineDeleate = true;
                            select = lineCnt;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                ++lineCnt;
            }

            if (isLineAdd)
                backLines.Insert(select + 1, new Dimension.BackLine());
            if (isLineDeleate)
                backLines.RemoveAt(select);
        }
        EditorGUILayout.EndScrollView();

        return backLines;
    }
    // リスポーン地点の位置を指定するウィンドウ
    List<Vector3> ReSpawnPointWindow(List<Vector3> respawnPoints)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.LabelField("ReSpawnPoint");
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box);

            int cnt = 0, select = 0;
            bool isDeleate = false, isAdd = false;

            foreach (Vector3 point in respawnPoints)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        Vector3 movePos = point;

                        EditorGUILayout.LabelField(cnt.ToString() + ":", GUILayout.Width(20));
                        EditorGUILayout.LabelField("X", GUILayout.Width(15));
                        movePos.x = EditorGUILayout.FloatField(movePos.x, GUILayout.Width(40));
                        EditorGUILayout.LabelField("Z", GUILayout.Width(15));
                        movePos.z = EditorGUILayout.FloatField(movePos.z, GUILayout.Width(40));

                        respawnPoints[cnt] = movePos;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Add", GUILayout.Width(70)))
                        {
                            isAdd = true;
                            select = cnt;
                        }
                        if (respawnPoints.Count > 1 && GUILayout.Button("Deleate", GUILayout.Width(70)))
                        {
                            isDeleate = true;
                            select = cnt;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                ++cnt;
            }
            EditorGUILayout.EndScrollView();

            if (isAdd)
                respawnPoints.Insert(select + 1, respawnPoints[select]);
            if (isDeleate)
                respawnPoints.RemoveAt(select);
        }
        EditorGUILayout.EndVertical();

        return respawnPoints;
    }
    //-----------------------------------------------------
    // Textureからパネル作成
    void SelectPanel(int left, int up, Texture texture)
    {
        var columns = texture.width / tiler.TileWidth;
        var rows = texture.height / tiler.TileHeight;
        var tileWidth = WINDOW_WIDTH / columns;
        var tileHeight = (tiler.TileHeight / (float)tiler.TileWidth) * tileWidth;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                var rect = new Rect(
                    WINDOW_LEFT + left + x * tileWidth,
                    WINDOW_UP + up + y * tileHeight,
                    tileWidth,
                    tileHeight
                    );
                var tile = new Vector2Int(x, rows - 1 - y);
                if (DrawPaletteTile(rect, tile, brush.Tile == tile && !brush.Hidden))
                {
                    brush.Tile = tile;
                    brush.Hidden = false;
                }
            }
        }
    }
    //
    bool DrawPaletteTile(Rect rect, Vector2Int? tile, bool selected)
    {
        // 処理中のイベント
        var e = Event.current;
        // 選択済みではなく、マウスがrectの範囲内かどうか
        var hover = !selected && e.mousePosition.x > rect.x && e.mousePosition.y > rect.y && e.mousePosition.x < rect.xMax && e.mousePosition.y < rect.yMax;
        // 
        var pressed = hover && e.type == EventType.MouseDown && e.button == 0;

        // マウスとrectが重なっている
        if (hover)
            EditorGUI.DrawRect(rect, Color.yellow);
        // 選択済み
        else if (selected)
            EditorGUI.DrawRect(rect, Color.blue);

        // tile
        if (tile.HasValue)
        {
            var coords = new Rect(tile.Value.x * tiler.UVMeshTileSize.x, tile.Value.y * tiler.UVMeshTileSize.y, tiler.UVMeshTileSize.x, tiler.UVMeshTileSize.y);
            GUI.DrawTextureWithTexCoords(new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4), tiler.MeshTexture, coords);
        }
        else
        {
            EditorGUI.DrawRect(new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4), Color.white);
            EditorGUI.DrawRect(new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) / 2, (rect.height - 4) / 2), Color.gray);
            EditorGUI.DrawRect(new Rect(rect.x + 2 + (rect.width - 4) / 2, rect.y + 2 + (rect.height - 4) / 2, (rect.width - 4) / 2, (rect.height - 4) / 2), Color.gray);
        }

        if (pressed)
            e.Use();

        return pressed;
    }
    #endregion

    void OnUndoRedo()
    {
        var tar = target as Tile3D;
        selected = null;
        hover = null;
        // After an undo the underlying block dictionary is out of sync with
        // the blocks list. Blocks have been removed, dictionary hasn't been
        // updated yet which causes artifacts during rebuild. So - update it.
        tar.RebuildBlockMap();
        tar.Rebuild();
    }
}