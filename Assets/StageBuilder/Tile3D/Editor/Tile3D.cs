using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using Dimension;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
[ExecuteInEditMode]
public class Tile3D : MonoBehaviour
{
    // sides of a cube
    public static Vector3[] Faces = new Vector3[6]
    {
        Vector3.up,      Vector3.down,
        Vector3.left,    Vector3.right,
        Vector3.forward, Vector3.back
    };

    // used to build the meshes
    // currently we use 2, one for what is actually being rendered and one for collisions (so the editor can click stuff)
    private class MeshBuilder
    {
        public Mesh Mesh;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> triangles = new List<int>();
        private bool collider;
        private Vector2 uvTileSize;
        private float tilePadding;
        private float vertexNoise;
        private Vector2[] temp = new Vector2[4];

        public MeshBuilder(bool collider)
        {
            Mesh = new Mesh();
            Mesh.name = (collider ? "Tile3D Editor Collision Mesh" : "Tile3D Render Mesh");
            this.collider = collider;
        }

        public void Begin(Vector2 uvTileSize, float tilePadding)
        {
            this.uvTileSize = uvTileSize;
            this.tilePadding = tilePadding;

            vertices.Clear();
            uvs.Clear();
            triangles.Clear();
        }

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Face face)
        {
            if (face.Hidden && !collider)
                return;

            var start = vertices.Count;

            // add Vertices
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);

            // add UVs
            Vector2 uva, uvb, uvc, uvd;
            {
                if (!collider)
                {
                    var center = new Vector2(face.Tile.x + 0.5f, face.Tile.y + 0.5f);
                    var s = 0.5f - tilePadding;
                    var flipx = (face.FlipX ? -1 : 1);
                    var flipy = (face.FlipY ? -1 : 1);

                    temp[0] = new Vector2((center.x + s * flipx) * uvTileSize.x, (center.y - s * flipy) * uvTileSize.y);
                    temp[1] = new Vector2((center.x - s * flipx) * uvTileSize.x, (center.y - s * flipy) * uvTileSize.y);
                    temp[2] = new Vector2((center.x - s * flipx) * uvTileSize.x, (center.y + s * flipy) * uvTileSize.y);
                    temp[3] = new Vector2((center.x + s * flipx) * uvTileSize.x, (center.y + s * flipy) * uvTileSize.y);

                    uva = temp[(face.Rotation + 0) % 4];
                    uvb = temp[(face.Rotation + 1) % 4];
                    uvc = temp[(face.Rotation + 2) % 4];
                    uvd = temp[(face.Rotation + 3) % 4];
                }
                else
                {
                    uva = uvb = uvc = uvd = Vector2.zero;
                }

                uvs.Add(uva);
                uvs.Add(uvb);
                uvs.Add(uvc);
                uvs.Add(uvd);
            }

            // Add Triangles
            triangles.Add(start + 0);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
            triangles.Add(start + 0);
            triangles.Add(start + 2);
            triangles.Add(start + 3);
        }

        public void End()
        {
            Mesh.Clear();
            Mesh.vertices = vertices.ToArray();
            Mesh.uv = uvs.ToArray();
            Mesh.triangles = triangles.ToArray();
            Mesh.RecalculateBounds();
            Mesh.RecalculateNormals();
        }
    }

    [HideInInspector]
    public List<Block> Blocks;
    public int TileWidth = 16;
    public int TileHeight = 16;
    public float TilePadding = 0.05f;

    public Texture MeshTexture
    {
        get
        {
            var material = meshRenderer.sharedMaterials[0];
            if (material != null)
                return material.mainTexture;
            return null;
        }
    }
    public Texture GimickTexture
    {
        get
        {
            if (meshRenderer.sharedMaterials.Length < 2) return null;

            var material = meshRenderer.sharedMaterials[1];
            return material.mainTexture;
        }
    }
    public Vector2 UVMeshTileSize
    {
        get
        {
            if (MeshTexture != null)
                return new Vector2(1f / (MeshTexture.width / TileWidth), 1f / (MeshTexture.height / TileHeight));
            return Vector2.one;
        }
    }

    private Dictionary<Vector3Int, Block> map;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFiler;
    private MeshCollider meshCollider;
    private MeshBuilder renderMeshBuilder;
    private MeshBuilder colliderMeshBuilder;

    // シーンが開かれた際に呼ばれる
    private void OnEnable()
    {
        meshFiler = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        // create initial mesh
        if (renderMeshBuilder == null)
        {
            renderMeshBuilder = new MeshBuilder(false);
            colliderMeshBuilder = new MeshBuilder(true);

            meshFiler.sharedMesh = renderMeshBuilder.Mesh;
            meshCollider.sharedMesh = colliderMeshBuilder.Mesh;
        }

        if (stageData != null)
        {
            LoadStage();
            return;
        }

        // reconstruct map
        if (map == null)
        {
            RebuildBlockMap();
        }

        // make initial cells
        if (Blocks == null)
        {
            Blocks = new List<Block>();
            for (int x = -4; x < 4; x++)
                for (int z = -4; z < 4; z++)
                    Create(new Vector3Int(x, 0, z));
        }

        // Data
        StageForward = new Vector3(0, 0, -1);

        if (backLinesRight.Count == 0) backLinesRight.Add(new BackLine());
        if (backLinesLeft.Count == 0) backLinesLeft.Add(new BackLine());
        if (respawnPoints.Count == 0) respawnPoints.Add(new Vector3(0, 2, 0));

        Rebuild();
    }

    public Block Create(Vector3Int at, Vector3Int? from = null)
    {
        Block block;
        if (!map.TryGetValue(at, out block))
        {
            block = new Block();
            block.Tile = at;
            Blocks.Add(block);
            map.Add(at, block);

            if (from != null)
            {
                var before = At(from.Value);
                if (before != null)
                    for (int i = 0; i < Faces.Length; i++)
                        block.Faces[i] = before.Faces[i];
            }
        }

        return block;
    }

    public void Destroy(Vector3Int at)
    {
        Block block;
        if (map.TryGetValue(at, out block))
        {
            map.Remove(at);
            Blocks.Remove(block);
        }
    }

    public Block At(Vector3Int at)
    {
        Block block;
        if (map.TryGetValue(at, out block))
            return block;
        return null;
    }

    public void RebuildBlockMap()
    {
        map = new Dictionary<Vector3Int, Block>();
        if (Blocks != null)
            foreach (var cell in Blocks)
                map.Add(cell.Tile, cell);
    }

    public void Rebuild()
    {
        renderMeshBuilder.Begin(UVMeshTileSize, TilePadding);
        colliderMeshBuilder.Begin(UVMeshTileSize, TilePadding);

        // generate each block
        foreach (var block in Blocks)
        {
            var origin = new Vector3(block.Tile.x + 0.5f, block.Tile.y + 0.5f, block.Tile.z + 0.5f);

            for (int i = 0; i < Faces.Length; i++)
            {
                var normal = new Vector3Int((int)Faces[i].x, (int)Faces[i].y, (int)Faces[i].z);
                if (At(block.Tile + normal) == null)
                    BuildFace(origin, block, normal, block.Faces[i]);
            }
        }

        renderMeshBuilder.End();
        colliderMeshBuilder.End();

        meshFiler.sharedMesh = renderMeshBuilder.Mesh;
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = colliderMeshBuilder.Mesh;
    }

    private void BuildFace(Vector3 center, Block block, Vector3 normal, Face face)
    {
        var up = Vector3.down;
        if (normal.y != 0)
            up = Vector2.left;

        var front = center + normal * 0.5f;
        var perp1 = Vector3.Cross(normal, up);
        var perp2 = Vector3.Cross(perp1, normal);

        var a = front + (-perp1 + perp2) * 0.5f;
        var b = front + (perp1 + perp2) * 0.5f;
        var c = front + (perp1 + -perp2) * 0.5f;
        var d = front + (-perp1 + -perp2) * 0.5f;

        renderMeshBuilder.Quad(a, b, c, d, face);
        colliderMeshBuilder.Quad(a, b, c, d, face);
    }
    //-----------------------------------------------------
    //  CSV作成
    //-----------------------------------------------------


    public Transform playerObj;
    public Transform goalObj;

    [HideInInspector] public List<Vector3> respawnPoints = new List<Vector3>();
    [HideInInspector] public List<BackLine> backLinesRight = new List<BackLine>();
    [HideInInspector] public List<BackLine> backLinesLeft = new List<BackLine>();

    [HideInInspector] public bool isSetStart = false;
    [HideInInspector] public bool isSetGoal = false;
    [HideInInspector] public bool isExecution = false;
    public Vector3 StageForward { get; set; }
    public Vector3 StartPosition { get; set; }
    public Vector3 GoalPosition { get; set; }

    // 条件
    public bool IsCreate { get { return !isExecution && stageData == null && stageName != "" && isSetStart && isSetGoal; } }
    public bool IsSave { get { return !isExecution && stageData != null && isSetStart && isSetGoal; } }
    public bool IsLoad { get { return !isExecution && stageData != null && stageData.blocks.Count != 0; } }

    [HideInInspector] public string stageName;
    [HideInInspector] public StageData stageData;

    // PATH
    const string DATA_PATH = "Assets/StageBuilder/StageData/Data/";
    const string MESH_PATH = "Assets/StageBuilder/StageData/Mesh/";
    const string TEXT_PATH = "/StageBuilder/StageData/Text/";

    //-----------------------------------------------------
    // CreateData
    //-----------------------------------------------------
    public void CreateData()
    {
        isExecution = true;
        Debug.Log("Create Start");

        // ScriptableObject準備
        StageData data = new StageData
        {
            name = stageName
        };
        AssetDatabase.CreateAsset(data, DATA_PATH + data.name + ".asset");

        UpdateData(ref data);
        stageData = data;

        Debug.Log("Create Finish");
        isExecution = false;
    }
    //-----------------------------------------------------
    // Save
    //-----------------------------------------------------
    public void SaveStage()
    {
        isExecution = true;
        Debug.Log("Save Start");

        UpdateData(ref stageData);

        Debug.Log("Save Finish");
        isExecution = false;
    }
    //-----------------------------------------------------
    //  Load
    //-----------------------------------------------------
    public void LoadStage()
    {
        // 実行開始
        isExecution = true;
        Debug.Log("Load Start");

        string CSV_PATH = TEXT_PATH + stageData.name + ".csv";
        ReadCSV(stageData.data);

        Blocks = stageData.blocks;
        RebuildBlockMap();
        Rebuild();

        // 実行終了
        Debug.Log("Load Finish");
        isExecution = false;
    }
    //-----------------------------------------------------
    //  Dataを計算
    //-----------------------------------------------------
    void DataCalculation()
    {

    }
    //-----------------------------------------------------
    // Data更新
    //-----------------------------------------------------
    void UpdateData(ref StageData data)
    {
        // Mesh準備

        Mesh renderMesh = renderMeshBuilder.Mesh;
        Mesh colliderMesh = colliderMeshBuilder.Mesh;
        renderMesh.name = data.name + " Render Mesh";
        colliderMesh.name = data.name + " Collider Mesh";

        if (data.renderMesh != null)
            AssetDatabase.DeleteAsset(MESH_PATH + renderMesh.name + ".asset");
        if (data.colliderMesh != null)
            AssetDatabase.DeleteAsset(MESH_PATH + renderMesh.name + ".asset");

        AssetDatabase.CreateAsset(renderMesh, MESH_PATH + renderMesh.name + ".asset");
        AssetDatabase.CreateAsset(colliderMesh, MESH_PATH + colliderMesh.name + ".asset");

        // CSV
        string CSV_PATH = TEXT_PATH + data.name + ".csv";
        WriteCSV(CSV_PATH);

        TextAsset csv = AssetDatabase.LoadAssetAtPath("Assets" + CSV_PATH, typeof(TextAsset)) as TextAsset;

        // 変更
        AssetDatabase.StartAssetEditing();

        // Data
        data.blocks = Blocks;
        data.renderMesh = renderMesh;
        data.colliderMesh = colliderMesh;
        data.data = csv;
        AssetDatabase.StopAssetEditing();

        // 変更をUnityEditorに伝える
        EditorUtility.SetDirty(renderMesh);
        EditorUtility.SetDirty(colliderMesh);
        EditorUtility.SetDirty(data);

        // 保存
        AssetDatabase.SaveAssets();
    }
    //-----------------------------------------------------
    //  CSV書き込み
    //-----------------------------------------------------
    void WriteCSV(string path)
    {
        StreamWriter sw = new StreamWriter(
            Application.dataPath + path,    // 保存先(絶対パス)
            false,                              // 追記(true) or 上書き(false)
            Encoding.GetEncoding("Shift_JIS")   // エンコード
            );

        // データ
        sw.WriteLine(Conversion.Vector3ToString(StageForward)); // ステージ正面方向
        sw.WriteLine(Conversion.Vector3ToString(StartPosition));// スタートの位置
        sw.WriteLine(Conversion.Vector3ToString(GoalPosition)); // ゴールの位置

        // 2Dから3Dに変換するときに戻る位置 右
        // 書かれ方
        //   BackLineの数
        //   奥行きの位置
        //   高さの数
        //   高さと横
        sw.WriteLine(backLinesRight.Count);
        foreach (BackLine line in backLinesRight)
        {
            sw.WriteLine(line.endDepth.ToString());
            sw.WriteLine(line.highSide.Count);
            for (int i = 0; i < line.highSide.Count; i++)
            {
                sw.WriteLine(Conversion.Vector2ToString(line.highSide[i]));
            }
        }

        // 2Dから3Dに変換するときに戻る位置 左
        sw.WriteLine(backLinesLeft.Count);
        foreach (BackLine line in backLinesLeft)
        {
            sw.WriteLine(line.endDepth.ToString());
            sw.WriteLine(line.highSide.Count);
            for (int i = 0; i < line.highSide.Count; i++)
            {
                sw.WriteLine(Conversion.Vector2ToString(line.highSide[i]));
            }
        }

        // リスポーン地点
        sw.WriteLine(respawnPoints.Count);
        foreach (Vector3 point in respawnPoints)
        {
            sw.WriteLine(Conversion.Vector3ToString(point));
        }

        // ギミック

        sw.WriteLine("End");
        sw.Close();
    }
    //-----------------------------------------------------
    //  CSV読み込み
    //-----------------------------------------------------
    void ReadCSV(TextAsset textAsset)
    {
        StringReader sr = new StringReader(textAsset.text);

        StageForward = Conversion.StringToVector3(sr.ReadLine());  // ステージ正面方向
        StartPosition = Conversion.StringToVector3(sr.ReadLine());  // スタートの位置
        GoalPosition = Conversion.StringToVector3(sr.ReadLine());  // ゴールの位置

        // 2Dから3Dに変換するときに戻る位置 右
        int num = int.Parse(sr.ReadLine());
        BackLine[] backLines = new BackLine[num];
        for (int i = 0; i < backLines.Length; ++i)
        {
            backLines[i] = new BackLine
            {
                endDepth = float.Parse(sr.ReadLine())
            };
            backLines[i].highSide.Clear();
            int highNum = int.Parse(sr.ReadLine());
            for (int j = 0; j < highNum; ++j)
            {
                backLines[i].highSide.Add(Conversion.StringToVector2(sr.ReadLine()));
            }
        }
        backLinesRight.Clear();
        backLinesRight.AddRange(backLines);

        // 2Dから3Dに変換するときに戻る位置 左
        num = int.Parse(sr.ReadLine());
        backLines = new BackLine[num];
        for (int i = 0; i < backLines.Length; ++i)
        {
            backLines[i] = new BackLine
            {
                endDepth = float.Parse(sr.ReadLine()),
            };
            backLines[i].highSide.Clear();
            int highNum = int.Parse(sr.ReadLine());
            for (int j = 0; j < highNum; ++j)
            {
                backLines[i].highSide.Add(Conversion.StringToVector2(sr.ReadLine()));
            }
        }
        backLinesLeft.Clear();
        backLinesLeft.AddRange(backLines);

        // リスポーン地点
        num = int.Parse(sr.ReadLine());
        respawnPoints.Clear();
        for (int i = 0; i < num; ++i)
        {
            respawnPoints.Add(Conversion.StringToVector3(sr.ReadLine()));
        }

        // ギミック

        sr.Close();
    }
}