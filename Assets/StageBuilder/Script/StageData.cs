using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimension;

[CreateAssetMenu(menuName = "Custom/StageData")]
public class StageData : ScriptableObject
{
    [HideInInspector]
    public Tile3D.Block[] blocks;

    [Space]
    // Mesh
    public Mesh renderMesh;
    public Mesh colliderMesh;

    [Space]
    // Stage情報
    public Vector3    stageForward;
    public Vector3    startPoint;
    public Vector3    goalPoint;
    public Vector3[]  respawnPoints;
    public BackLine[] backLinesRight;
    public BackLine[] backLinesLeft;

    // BestTime
    public float bestTime;
}