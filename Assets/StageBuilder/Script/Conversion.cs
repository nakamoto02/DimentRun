using UnityEngine;

public static class Conversion
{
    //-----------------------------------------------------
    // Stringへ変換
    //-----------------------------------------------------
    public static string Vector2ToString(Vector2 vec) {
        string[] strAry = { vec.x.ToString(), vec.y.ToString() };
        string str = string.Join(",", strAry);
        return str;
    }
    public static string Vector3ToString(Vector3 vec) {
        string[] strAry = { vec.x.ToString(), vec.y.ToString(), vec.z.ToString() };
        string str = string.Join(",", strAry);
        return str;
    }
    public static string RectToString(Rect rect) {
        string[] strAry = { rect.x.ToString(), rect.y.ToString(), rect.width.ToString(), rect.height.ToString() };
        string str = string.Join(",", strAry);
        return str;
    }
    //-----------------------------------------------------
    // Stringを変換
    //-----------------------------------------------------
    public static Vector2 StringToVector2(string str) {
        string[] strAry = str.Split(',');
        if (strAry.Length != 2) return new Vector2(0, 0);
        return new Vector2(float.Parse(strAry[0]), float.Parse(strAry[1]));
    }
    public static Vector3 StringToVector3(string str) {
        string[] strAry = str.Split(',');
        if (strAry.Length != 3) return new Vector3(0, 0, 0);
        return new Vector3(float.Parse(strAry[0]), float.Parse(strAry[1]), float.Parse(strAry[2]));
    }
    public static Rect StringToRect(string str) {
        string[] strAry = str.Split(',');
        if (strAry.Length != 4) return new Rect(0, 0, 0, 0);
        return new Rect(float.Parse(strAry[0]), float.Parse(strAry[1]), float.Parse(strAry[2]), float.Parse(strAry[3]));
    }
    //-----------------------------------------------------
    //  Vector3を変換
    //-----------------------------------------------------
    public static float Vector3ToFloat(Vector3 vec) {
        return vec.x + vec.y + vec.z;
    }
    public static float Vector3AxisFloat(Vector3 axis, Vector3 vec) {
        return Vector3ToFloat(Vector3.Scale(axis, vec));
    }
    //-----------------------------------------------------
    //  Vector2を変換
    //-----------------------------------------------------
    public static float Vector2ToFloat(Vector2 vec) {
        return vec.x + vec.y;
    }
    public static float Vector2AxisFloat(Vector2 axis, Vector2 vec) {
        return Vector2ToFloat(Vector2.Scale(axis, vec));
    }
}
