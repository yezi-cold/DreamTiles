using UnityEngine;

// ʹ�� struct ��Ϊ�����������ʺ�����������ֵ��������
[System.Serializable]
public enum HexDirection
{
    Right = 0,       // �ҷ� (Q+1, R+0)
    UpRight = 1,     // ���Ϸ� (Q+1, R-1)
    UpLeft = 2,      // ���Ϸ� (Q+0, R-1)
    Left = 3,        // �� (Q-1, R+0)
    DownLeft = 4,    // ���·� (Q-1, R+1)
    DownRight = 5    // ���·� (Q+0, R+1)
}
public struct HexCoord
{
    [SerializeField] private int q;
    [SerializeField] private int r;

    public int Q => q;
    public int R => r;
    public static HexCoord zero =>new HexCoord(0, 0);

    // ���캯��
    public HexCoord(int q, int r)
    {
        this.q = q;
        this.r = r;
        
    }

    // ������������ĵ�λ�������ǳ����ã�
    private static readonly HexCoord[] directions = new HexCoord[]
    {
        new HexCoord(1, 0), new HexCoord(1, -1), new HexCoord(0, -1),
        new HexCoord(-1, 0), new HexCoord(-1, 1), new HexCoord(0, 1)
    };

    // ��ȡ�ض�������ھ�����
    public HexCoord GetNeighbor(int direction)
    {
        return this + directions[direction];
    }

    // ���������������������ֱ��
    public static HexCoord operator +(HexCoord a, HexCoord b)
    {
        return new HexCoord(a.q + b.q, a.r + b.r);
    }

    public static HexCoord operator -(HexCoord a, HexCoord b)
    {
        return new HexCoord(a.q - b.q, a.r - b.r);
    }

    // �������� HexCoord ����Ϊ Dictionary Key �Ĺؼ�
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