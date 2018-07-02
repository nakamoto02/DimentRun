using System.Collections.Generic;
using UnityEngine;

namespace Dimension
{
    //---------------------------------------------------------
    [System.Serializable]
    public class BackLine
    {
        public float endDepth = 0;
        public List<Vector2> highSide = new List<Vector2>();

        public BackLine()
        {
            highSide.Add(new Vector2(0, 0));
        }
    }
    //---------------------------------------------------------
    public class Plane
    {
        const float SIDE_LENGTH = 1.0f; // 1辺の長さ
        const int   VERTEX_NUM = 4;     // 頂点数

        public Vector3[] vertices;      // 頂点
        public Vector3 normalVector;    // 法線ベクトル

        public Vector3 Center { get { return vertices[0] + (vertices[2] - vertices[0]) * SIDE_LENGTH * 0.5f; } }

        // コンストラクタ
        public Plane()
        {
            vertices = new Vector3[VERTEX_NUM];
        }
        public Plane(Vector3 one, Vector3 two, Vector3 three, Vector3 fowr)
        {
            vertices = new Vector3[] { one, two, three, fowr };
        }

        public static bool operator ==(Plane p1, Plane p2) { return p1.Equals(p2); }
        public static bool operator !=(Plane p1, Plane p2) { return !p1.Equals(p2); }

        public override bool Equals(object obj)
        {
            if (!(obj is Plane)) return false;

            Plane other = (Plane)obj;
            for (int i = 0; i < VERTEX_NUM; ++i)
                if (vertices[i] != other.vertices[i]) return false;

            return true;
        }
        public override int GetHashCode() { return base.GetHashCode(); }

        // 法線ベクトルを計算
        public Vector3 GetNormalVector()
        {
            normalVector = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]);
            return normalVector;
        }
        // 引数の軸の値が同一かどうか
        public bool CheckAxisSame(Vector3 axis)
        {
            return Vector3.Scale(axis, vertices[0]) == Vector3.Scale(axis, vertices[1]) &&
                   Vector3.Scale(axis, vertices[1]) == Vector3.Scale(axis, vertices[2]) &&
                   Vector3.Scale(axis, vertices[2]) == Vector3.Scale(axis, vertices[3]);
        }
    }
    //---------------------------------------------------------
    public class Cube
    {
        const float SIDE_LENGTH = 1.0f; // 1辺の長さ
        const int   VERTEX_NUM = 8;     // 頂点数

        public Vector3[] vertices;      // 頂点
        public int[] triangles;         // 三角形

        public Cube(Plane plane)
        {
            // 頂点
            vertices = new Vector3[] {
                    plane.vertices[0] + plane.normalVector * SIDE_LENGTH * 0.5f,
                    plane.vertices[1] + plane.normalVector * SIDE_LENGTH * 0.5f,
                    plane.vertices[2] + plane.normalVector * SIDE_LENGTH * 0.5f,
                    plane.vertices[3] + plane.normalVector * SIDE_LENGTH * 0.5f,
                    plane.vertices[1] - plane.normalVector * SIDE_LENGTH * 0.5f,
                    plane.vertices[0] - plane.normalVector * SIDE_LENGTH * 0.5f,
                    plane.vertices[3] - plane.normalVector * SIDE_LENGTH * 0.5f,
                    plane.vertices[2] - plane.normalVector * SIDE_LENGTH * 0.5f,
                };
            // 三角形
            triangles = new int[] {
                    0, 1, 2, 0, 2, 3,   // 正面
                    4, 5, 6, 4, 6, 7,   // 背後
                    3, 2, 7, 3, 7, 6,   // 上
                    1, 0, 5, 1, 5, 4,   // 下
                    5, 0, 3, 5, 3, 6,   // 右
                    1, 4, 7, 1, 7, 2    // 左
                };
        }

        public void CreateTriangle(int start)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] += start;
            }
        }
    }
}