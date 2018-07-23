using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StageBuilder))]
public class StageBuildEditor : Editor
{
    public StageBuilder Builder { get { return (StageBuilder)target; } }

    #region Tool

    public enum ToolMode
    {
        Mesh,
        Stage
    }
    private ToolMode toolMode = ToolMode.Mesh;
    //-----------------------------------------------------
    //  MeshTool
    //-----------------------------------------------------
    public enum MeshTool
    {
        Building,
        Painting
    }
    private MeshTool meshTool = MeshTool.Building;
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

    #region Default Event

    private void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedo;
    }
    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    protected virtual void OnSceneGUI()
    {
        Handles.BeginGUI();
        {
            toolMode = (ToolMode)GUI.Toolbar(new Rect(10, 10, 200, 20), (int)toolMode, new[] { "Mesh", "Data" });

            // meshTool
            if (toolMode == ToolMode.Mesh)
            {
                meshTool = (MeshTool)GUI.Toolbar(new Rect(10, 35, 200, 20), (int)meshTool, new[] { "Build", "Paint" });
                //if (meshTool == MeshTool.Painting)
                //    selected = null;

                //// tileset
                //if (meshTool == MeshTool.Painting)
                //    GUI.Window(0, new Rect(10, 80, 200, 300), PaintingWindow, "Tiles");
            }
            else
            if (toolMode == ToolMode.Stage)
            {
                stageTool = (StageTool)GUI.Toolbar(new Rect(10, 35, 200, 20), (int)stageTool, new[] { "Data", "Gimick", "Back" });

                //// Data
                //if (stageTool == StageTool.Data)
                //    GUI.Window(0, new Rect(10, 80, 200, 300), DataWindow, "Data");

                //// Gimick
                //if (stageTool == StageTool.Gimick)
                //    GUI.Window(0, new Rect(10, 80, 200, 300), GimickWindow, "Gimick");

                //// Back
                //if (stageTool == StageTool.BackPoint)
                //    GUI.Window(0, new Rect(10, 80, 200, 300), BackWindow, "BackPoint");
            }
        }
        Handles.EndGUI();
    }

    void OnUndoRedo()
    {
        
    }
    #endregion

    #region Mesh

    #endregion

    #region Window



    #endregion
}
