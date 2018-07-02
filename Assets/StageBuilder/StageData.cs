using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/StageData")]
public class StageData : ScriptableObject
{
    [HideInInspector]
    public List<Block> blocks;

    [Space]
    // Mesh
    public Mesh renderMesh;
    public Mesh colliderMesh;

    [Space]
    // CSV
    public TextAsset data;
}

[System.Serializable]
public class Block
{
    public Vector3Int Tile;
    public Face[] Faces = new Face[6];
}

// Face of a Block
[System.Serializable]
public struct Face
{
    public Vector2Int Tile;
    public int Rotation;
    public bool FlipX;
    public bool FlipY;
    public bool Hidden;

    public static bool operator ==(Face f1, Face f2) { return f1.Equals(f2); }
    public static bool operator !=(Face f1, Face f2) { return !f1.Equals(f2); }

    public override bool Equals(object obj)
    {
        if (obj is Face)
        {
            var other = (Face)obj;
            return Tile == other.Tile && Rotation == other.Rotation && FlipX == other.FlipX && FlipY == other.FlipY && Hidden == other.Hidden;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}