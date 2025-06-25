using UnityEngine;

// 使用 struct 因为它更轻量，适合做坐标这种值类型数据
[System.Serializable]
public enum HexDirection
{
    Right = 0,       // 右方 (Q+1, R+0)
    UpRight = 1,     // 右上方 (Q+1, R-1)
    UpLeft = 2,      // 左上方 (Q+0, R-1)
    Left = 3,        // 左方 (Q-1, R+0)
    DownLeft = 4,    // 左下方 (Q-1, R+1)
    DownRight = 5    // 右下方 (Q+0, R+1)
}
public struct HexCoord
{
    [SerializeField] private int q;
    [SerializeField] private int r;

    public int Q => q;
    public int R => r;
    public static HexCoord zero =>new HexCoord(0, 0);

    // 构造函数
    public HexCoord(int q, int r)
    {
        this.q = q;
        this.r = r;
        
    }

    // 定义六个方向的单位向量，非常有用！
    private static readonly HexCoord[] directions = new HexCoord[]
    {
        new HexCoord(1, 0), new HexCoord(1, -1), new HexCoord(0, -1),
        new HexCoord(-1, 0), new HexCoord(-1, 1), new HexCoord(0, 1)
    };

    // 获取特定方向的邻居坐标
    public HexCoord GetNeighbor(int direction)
    {
        return this + directions[direction];
    }

    // 重载运算符，让坐标计算更直观
    public static HexCoord operator +(HexCoord a, HexCoord b)
    {
        return new HexCoord(a.q + b.q, a.r + b.r);
    }

    public static HexCoord operator -(HexCoord a, HexCoord b)
    {
        return new HexCoord(a.q - b.q, a.r - b.r);
    }

    // 以下是让 HexCoord 能作为 Dictionary Key 的关键
    public override bool Equals(object obj)
    {
        return obj is HexCoord coord && q == coord.q && r == coord.r;
    }

    public override int GetHashCode()
    {
        return (q, r).GetHashCode();
    }

    public static bool operator ==(HexCoord a, HexCoord b)
    {
        return a.q == b.q && a.r == b.r;
    }

    public static bool operator !=(HexCoord a, HexCoord b)
    {
        return !(a == b);
    }
}