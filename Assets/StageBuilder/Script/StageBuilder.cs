using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimension;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class StageBuilder : MonoBehaviour
{
    // 保存先のパス
    const string DATA_PATH = "Assets/StageBuilder/StageData/Data/";

    // ツール
    [SerializeField]
    private Tile3D            thirdMeshBuilder;
    [SerializeField]
    private SecondMeshCreator secondMeshBuilder;
    
    // データ
     public string    stageName;
    [HideInInspector] public StageData stageData;

    public Vector3 StageForward  { get; set; }
    public Vector3 StartPosition { get; set; }
    public Vector3 GoalPosition  { get; set; }

    List<Vector3>  respawnPoints  = new List<Vector3>();
    List<BackLine> backLinesRight = new List<BackLine>();
    List<BackLine> backLinesLeft  = new List<BackLine>();

    //=====================================================
    private void OnEnable()
    {
        foreach(Transform child in transform)
        {
            if (child.GetComponent<Tile3D>() != null)
                thirdMeshBuilder = child.GetComponent<Tile3D>();

            if (child.GetComponent<SecondMeshCreator>() != null)
                secondMeshBuilder = child.GetComponent<SecondMeshCreator>();
        }

        if(thirdMeshBuilder == null) {
            GameObject obj = new GameObject("ThirdMesh");
            thirdMeshBuilder = obj.AddComponent<Tile3D>();
            obj.transform.parent = transform;
        }
        if(secondMeshBuilder == null) {
            GameObject obj = new GameObject("SecondMesh");
            secondMeshBuilder = obj.AddComponent<SecondMeshCreator>();
            obj.transform.parent = transform;
        }
        

        Debug.Log("SB_OnEnable");


    }
    //-----------------------------------------------------
    // 新しく作成
    //-----------------------------------------------------

    //-----------------------------------------------------
    // 保存
    //-----------------------------------------------------

    //-----------------------------------------------------
    // 読み込み
    //-----------------------------------------------------

    //-----------------------------------------------------
    //
    //-----------------------------------------------------

}