using UnityEngine;

// 这个[System.Serializable]特性让这个类的实例可以在Unity的Inspector中显示。
// 它不是一个MonoBehaviour，只是一个纯粹的数据容器。
[System.Serializable]
public class ScoreModifier
{
    public int prosperity; // 繁荣度变化值
    public int population;   // 人口变化值
    public int happiness;    // 幸福度变化值
}
