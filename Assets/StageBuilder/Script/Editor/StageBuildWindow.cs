using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class StageBuildWindow : EditorWindow
{
    Tile3D      tiler;
    StageBuilder builder;
    StageData    stageData;
    
    public enum ToolType
    {
        Tile3D,
        Stage
    }
    public enum Tile3DTool
    {
        Mesh,
        Paint
    }
    public enum StageTool
    {
        Data,
        Gimick,
        Respawn,
        Right,
        Left
    }

    ToolType   toolType  = 0;
    Tile3DTool tileTool  = 0;
    StageTool  stageTool = 0;

    //-----------------------------------------------------
    //  
    //-----------------------------------------------------
    [MenuItem("Window/StageBuilder")]
    static void Open()
    {
        System.Type projectType = System.Reflection.Assembly.Load("UnityEditor").GetType("UnityEditor.ProjectBrowser");
        GetWindow<StageBuildWindow>("StageBuilder", projectType);
    }
    //-----------------------------------------------------
    //  
    //-----------------------------------------------------
    void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            stageData = (StageData)EditorGUILayout.ObjectField("StageData", stageData, typeof(StageData), true);
            if(GUILayout.Button("New", GUILayout.Width(50)))
            {
                Debug.Log("ここから過程大事に");
            }
        }

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        {
            toolType = (ToolType)GUILayout.Toolbar((int)toolType, new string[] { "Tile3D", "Stage" }, (GUIStyle)"OL Titlemid");
        }
        if (EditorGUI.EndChangeCheck())
        {
            if(toolType == ToolType.Tile3D)
            {
                if (tiler == null) tiler = FindObjectOfType<Tile3D>();
                Selection.activeGameObject = tiler.gameObject;

                Debug.Log("AB");
            } else
            if(toolType == ToolType.Stage)
            {
                if (builder == null) builder = FindObjectOfType<StageBuilder>();
                Selection.activeGameObject = builder.gameObject;

                Debug.Log("CD");
            }
        }

        if(toolType == ToolType.Tile3D)
        {
            tileTool = (Tile3DTool)GUILayout.Toolbar((int)tileTool, new string[] { "Mesh", "Paint" }, (GUIStyle)"OL Titlemid");
        } else 
        if(toolType == ToolType.Stage)
        {

            stageTool = (StageTool)GUILayout.Toolbar((int)stageTool, new string[] { "Data", "Gimick", "Respawn", "Right", "Left" }, (GUIStyle)"OL Titlemid");
        }
    }
}
