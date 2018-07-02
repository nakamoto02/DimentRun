using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //シングルトン
    static public GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    //=====================================================

    //-----------------------------------------------------
    //  プロパティ
    //-----------------------------------------------------
    public StageData NextStage;    // 次のステージのデータ
}
