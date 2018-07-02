using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Dimension.Stage
{
    public class StageController : MonoBehaviour
    {
        public StageData stageData;

        [Space]
        // ステージ
        public MeshSetter stageMesh;
        public MeshSetter rightSecond;
        public MeshSetter leftSecond;

        [Space]
        // ギミック
        public StageGoal goalPre;

        Mesh renderMesh;     // レンダリングされるメッシュ
        Mesh colliderMesh;   // コライダー作成用のメッシュ

        // 戻る位置
        BackLine[] backLinesRight;  // ステージ座標
        BackLine[] backLinesLeft;   // ステージ座標
        // リスポーン地点
        Vector3[] ReSpawrnList;     // ワールド
        //-----------------------------------------------------
        //  プロパティ
        //-----------------------------------------------------
        public Vector3 StageForward { get; private set; }   // ステージの正面方向
        public Vector3 StageRight   { get; private set; }   // ステージの右方向
        public Vector3 StageCenter  { get; private set; }   // ステージの中心  ワールド
        public Vector3 StartPoint   { get; private set; }   // スタート地点    ワールド
        public Vector3 GoalPoint    { get; private set; }   // ゴール地点      ワールド  
        public float StageWidth     { get; private set; }   // ステージの幅
        public float StageHeight    { get; private set; }   // ステージの高さ
        public float StageDepth     { get; private set; }   // ステージの奥行き

        //-----------------------------------------------------
        //  ステージ情報の設定
        //-----------------------------------------------------
        public void SetStageData(StageData data)
        {
            stageData = data;
            Initialize();
        }
        //-----------------------------------------------------
        //  リスポーン地点を返す
        //-----------------------------------------------------
        public Vector3 GetReSpawrnPoint(Vector3 playerPos)
        {
            // 移動する位置と一番短い距離
            Vector3 bestPoint = new Vector3(0, 0, 0);
            float lengthMin = 100;

            foreach(Vector3 point in ReSpawrnList) {
                float length = (playerPos - point).magnitude;
                if (length > lengthMin) continue;
                lengthMin = length;
                bestPoint = point;
            }
            return bestPoint;
        }
        //-----------------------------------------------------
        //  2Dから3Dに変換するときの位置
        //-----------------------------------------------------
        public Vector3 GetBackPoint(Vector3 playerPos, bool isRight)
        {
            Vector3 rePoint = playerPos;
            rePoint.y = Mathf.Round(rePoint.y);

            // 使用するほうを選択
            BackLine[] backLines = (isRight) ? backLinesRight : backLinesLeft;
            float playerDepth = Conversion.Vector3AxisFloat(StageForward, playerPos);

            for(int i = 1; i < backLines.Length; ++i) {
                if (playerDepth > backLines[i].endDepth) continue;
                // 指定が一つなら
                if(backLines[i].highSide.Count == 1) {
                    rePoint = StageRight * backLines[i].highSide[0].x + Vector3.up * rePoint.y + StageForward * playerDepth;
                    break;
                }
                // 複数なら
                int cnt = 0;
                foreach (Vector2 highSide in backLines[i].highSide)
                {
                    if (rePoint.y < highSide.y) break;
                    ++cnt;
                }
                cnt = Mathf.Max(cnt - 1, 0);
                rePoint = StageRight * backLines[i].highSide[cnt].x + Vector3.up * rePoint.y + StageForward * playerDepth;
                break;
            }
            // 最奥にいるとき
            if (rePoint == playerPos)
                rePoint = StageRight * 0 + Vector3.up * rePoint.y + StageForward * playerDepth;
            return rePoint;
        }
        //-----------------------------------------------------
        //  初期化
        //-----------------------------------------------------
        void Initialize()
        {
            ReadStageData();    // 読み込み
            DataCalculation();  // 計算
            BackLineCalulation();
            CreateStage();      // 生成
        }
        //-----------------------------------------------------
        //  データの読み込み
        //-----------------------------------------------------
        void ReadStageData()
        {
            renderMesh = stageData.renderMesh;
            colliderMesh = stageData.colliderMesh;

            // 読み込み
            StringReader reader = new StringReader(stageData.data.text);

            StageForward = Conversion.StringToVector3(reader.ReadLine());
            StartPoint   = Conversion.StringToVector3(reader.ReadLine());
            GoalPoint    = Conversion.StringToVector3(reader.ReadLine());

            // 計算
            DataCalculation();

            // 2Dから3Dに変換するときに戻る位置 右
            backLinesRight = ReadBackLines(ref reader);

            // 2Dから3Dに変換するときに戻る位置 左
            backLinesLeft = ReadBackLines(ref reader);

            // リスポーン地点
            int num = int.Parse(reader.ReadLine());
            ReSpawrnList = new Vector3[num];
            for (int i = 0; i < ReSpawrnList.Length; ++i) {
                // 読み込んだ位置をワールド座標に
                ReSpawrnList[i] = StageToWorld(Conversion.StringToVector3(reader.ReadLine()));
            }

            reader.Close();
        }
        //-----------------------------------------------------
        //  2Dから3Dへ変換するときの位置情報の読み込み
        //-----------------------------------------------------
        BackLine[] ReadBackLines(ref StringReader sr)
        {
            int num = int.Parse(sr.ReadLine());
            BackLine[] backLines = new BackLine[num];
            for (int i = 0; i < backLines.Length; ++i)
            {
                backLines[i] = new BackLine {
                    endDepth = float.Parse(sr.ReadLine())
                };
                backLines[i].highSide.Clear();
                int highNum = int.Parse(sr.ReadLine());
                for (int j = 0; j < highNum; ++j) {
                    backLines[i].highSide.Add(Conversion.StringToVector2(sr.ReadLine()));
                }
            }
            return backLines;
        }
        //-----------------------------------------------------
        //  読み込んだデータから計算
        //-----------------------------------------------------
        void DataCalculation()
        {
            // ステージ右方向
            StageRight = Quaternion.Euler(new Vector3(0, 90, 0)) * StageForward;
            StageRight = new Vector3(Mathf.Round(StageRight.x), 0, Mathf.Round(StageRight.y));

            // ステージのサイズ
            Vector3 vecMax = new Vector3(0, 0, 0);
            Vector3 vecMin = new Vector3(0, 0, 0);

            foreach (Vector3 vec in stageData.colliderMesh.vertices) {
                vecMax = Vector3.Max(vec, vecMax);
                vecMin = Vector3.Min(vec, vecMin);
            }

            // ステージの中央
            Vector3 centerVec = (vecMax - vecMin).normalized;
            float dis = (vecMax - vecMin).magnitude * 0.5f;
            StageCenter = vecMin + centerVec * dis;

            StageWidth  = vecMax.x - vecMin.x;
            StageHeight = vecMax.y - vecMin.y;
            StageDepth  = vecMax.z - vecMin.z;
        }
        //-----------------------------------------------------
        //  2Dから3D切り替え時の戻る位置のリストを計算
        //-----------------------------------------------------
        void BackLineCalulation()
        {
            // 上向きの平面
            List<Plane> topPlanes = GetMeshToPlane(colliderMesh, Vector3.up);
            // 配列
            BackLine[] backLines = new BackLine[(int)StageDepth - 1];
            Vector3 basePoint = StageCenter - StageForward * (StageDepth / 2);
            Vector3 AxisForward = new Vector3(Mathf.Abs(StageForward.x), 0, Mathf.Abs(StageForward.z)); // 正面方向の軸
            Vector3 AxisRight   = new Vector3(Mathf.Abs(StageRight.x), 0, Mathf.Abs(StageRight.z)); // 右方向の軸
            for(int i = 0; i < backLines.Length; ++i) {
                backLines[i] = new BackLine();
                backLines[i].highSide.Clear();
                foreach (Plane plane in topPlanes) {
                    // 正面方向への距離
                    Vector3 center = plane.Center - Vector3.Scale(StageForward, basePoint);
                    float depth = Conversion.Vector3AxisFloat(AxisForward, center);
                    if (depth >= i + 1) continue;
                    Vector2 highSide = new Vector2(
                        Conversion.Vector3AxisFloat(AxisRight, plane.Center),
                        plane.Center.y
                        );
                    // 同じ高さの値がないとき追加
                    if(CheckNotHeightSum(backLines[i].highSide, highSide)) backLines[i].highSide.Add(highSide);
                 }
            }

            
            // 右
            //backLinesRight = new BackLine[backLines.Length];
            //for(int i = 0; i < backLinesRight.Length; ++i)
            //{
            //    backLinesRight[i] = new BackLine();
            //    backLinesRight[i].highSide.Clear();
            //    List<Vector2> sides = new List<Vector2>();
            //    foreach(Vector2 highSide in backLines[i].highSide)
            //    {
            //        if (sides.Count == 0) { sides.Add(highSide); continue; }
                    
            //        foreach(Vector2 side in sides)
            //        {

            //        }
            //    }
            //}

            // 左
            
        }
        
        //-----------------------------------------------------
        //  ステージ生成
        //-----------------------------------------------------
        void CreateStage()
        {
            // 三次元用
            stageMesh.SetMesh(renderMesh, colliderMesh);

            // 二次元用
            rightSecond.transform.position = StageRight  * StageWidth * 2;
            leftSecond.transform.position  = -StageRight * StageWidth * 2;
            CreateSecondCollider();

            // ゴールの配置
            StageGoal goalObj = Instantiate(goalPre, GoalPoint, Quaternion.identity);
            goalObj.transform.parent = transform;
        }
        //-----------------------------------------------------
        //  2次元用Collider(3D)を用意
        //-----------------------------------------------------
        void CreateSecondCollider()
        {
            List<Plane>     planes    = new List<Plane>();  // 平面のリスト
            List<Cube>      cubes     = new List<Cube>();       // 立方体のリスト
            List<Vector3>   vertices  = new List<Vector3>();    // 頂点のリスト
            List<int>       triangles = new List<int>();        // 三角形のリスト

            // 必要な平面を取得
            planes = GetMeshToPlane(colliderMesh, StageRight);
            planes = SetNormalVectorPosition(planes);
            planes = DeleateSamePlane(planes);

            // 平面を立方体に
            foreach(Plane plane in planes) {
                cubes.Add(new Cube(plane));
            }

            //頂点と三角形を取得
            foreach(Cube cube in cubes) {
                cube.CreateTriangle(vertices.Count);

                vertices.AddRange(cube.vertices);
                triangles.AddRange(cube.triangles);
            }

            // Mesh作成
            Mesh secondMesh = new Mesh
            {
                name = "Second Mesh",
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };

            secondMesh.RecalculateBounds();
            secondMesh.RecalculateNormals();

            rightSecond.SetMesh(secondMesh, secondMesh);
            leftSecond.SetMesh(secondMesh, secondMesh);

            return;
        }
        //-----------------------------------------------------
        //  指定された軸を向いた平面を取得
        //-----------------------------------------------------
        List<Plane> GetMeshToPlane(Mesh mesh, Vector3 axis)
        {
            List<Plane> planeList = new List<Plane>();

            for (int i = 0; i < colliderMesh.vertexCount / 4; ++i)
            {
                // 平面を取得
                Plane plane = new Plane(
                    colliderMesh.vertices[(i * 4) + 0],
                    colliderMesh.vertices[(i * 4) + 1],
                    colliderMesh.vertices[(i * 4) + 2],
                    colliderMesh.vertices[(i * 4) + 3]
                    );

                // 頂点の順序
                if (plane.GetNormalVector() != axis) continue;

                //リストに追加
                planeList.Add(plane);
            }

            return planeList;
        }
        //-----------------------------------------------------
        //  List内のPlaneの位置を調整する
        //-----------------------------------------------------
        List<Plane> SetNormalVectorPosition(List<Plane> list, float value = 0)
        {
            for(int i = 0; i < list.Count; ++i) {
                Vector3 normalVec = list[i].GetNormalVector();
                for(int j = 0; j < list[i].vertices.Length; ++j) {
                    list[i].vertices[j] -= Vector3.Scale(normalVec, list[i].vertices[j]);
                }
            }
            return list;
        }
        //-----------------------------------------------------
        //  List内の同じ位置のPlaneを削除
        //-----------------------------------------------------
        List<Plane> DeleateSamePlane(List<Plane> list)
        {
            List<Plane> planes = new List<Plane>();
            foreach(Plane plane in list) {
                if (planes.Count == 0) planes.Add(plane);
                else if (CheckNotListSame(planes, plane)) planes.Add(plane);
            }
            return planes;
        }
        //-----------------------------------------------------
        //  List内にvalueと同じ値がないとき、true
        //-----------------------------------------------------
        bool CheckNotListSame(List<Plane> list, Plane value)
        {
            foreach (var item in list)
            {
                if (value == item) return false;
            }
            return true;
        }
        //-----------------------------------------------------
        //  List内に同じ高さの値がない時、true
        //-----------------------------------------------------
        bool CheckNotHeightSum(List<Vector2> list, Vector2 point)
        {
            foreach(var item in list)
            {
                if (point.y == item.y) return false;
            }
            return true;
        }
        //-----------------------------------------------------
        // ステージの座標をワールド座標に
        //-----------------------------------------------------
        Vector3 StageToWorld(Vector3 stage)
        {
            return StageRight * stage.x + Vector3.up * stage.y + StageForward * stage.z;
        }
        //-----------------------------------------------------
        // ワールド座標をステージの座標に
        //-----------------------------------------------------
        Vector3 WorldToStage(Vector3 world)
        {
            return StageRight * world.x + Vector3.up * world.y + StageForward * world.z;
        }
    }
}