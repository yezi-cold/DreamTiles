using UnityEngine;
//����������ű�
// ����һ����Ϊ HexDirection ��ö�����͡�ö����������ʾһ�鳣������������
[System.Serializable]//������������ö�ٿ�����unity��inspector�������ʾ
public enum HexDirection
{
    //��������������ĳ���������ֵ0��5�������������ѭ����ʹ��
    Right = 0,       // �ҷ� (Q+1, R+0)
    UpRight = 1,     // ���Ϸ� (Q+1, R-1)
    UpLeft = 2,      // ���Ϸ� (Q+0, R-1)
    Left = 3,        // �� (Q-1, R+0)
    DownLeft = 4,    // ���·� (Q-1, R+1)
    DownRight = 5    // ���·� (Q+0, R+1)
}
//����һ����ΪHexCoord�Ľṹ�壨struct�����ṹ�����������������������ʺ��������������������ļ����ݡ�
public struct HexCoord
{
    //--�ֶ�--
    //�ֶ�������߽ṹ����ֱ�������ı�����
    [SerializeField] private int q;//˽���ֶΣ�����������q�����
    [SerializeField] private int r;//˽���ֶΣ�����������r�����
    public static HexCoord zero => new HexCoord(0, 0); 
    //--����--
    //����������߽ṹ���ж���ĺ��������ڻ�ȡ�������ֶε�ֵ��������ֻ������
    public int Q => q;//����ֻ�����ԣ��ⲿ�������ͨ��Q����ȡq��ֵ���������޸�
    public int R => r;//����ֻ�����ԣ��ⲿ�������ͨ��R����ȡr��ֵ���������޸�
    public static HexCoord Zero =>new HexCoord(0, 0);//������ֻ̬�����ԣ�����ֵΪHexCoord(0, 0)

    //--���캯��--
    //���캯�����ڴ����ṹ��ʵ��ʱ���õ����ⷽ�������ڳ�ʼ���ֶ�
    public HexCoord(int q, int r)//�������캯��������q��r��������
    {
        this.q = q;//ʹ��this �ؼ��������ֲ���q���ֶ�q��������ֵ�����ֶ�
        this.r = r;//������r��ֵ�����ֶ�
        
    }
    //--��̬�ֶ�--
    //��̬�ֶ����༶��ı��������Ա�����ʵ������
    //���ﶨ����һ��˽�еľ�ֻ̬�����飬���ڴ�����������ĵ�λ������
    //static��ʾ����ֶ����� HexCoord �࣬������ĳ��ʵ����
    // readonly ��ʾ��������ڳ�ʼ�������ٱ���ֵ
    private static readonly HexCoord[] directions = new HexCoord[]
    {
        new HexCoord(1, 0), new HexCoord(1, -1), new HexCoord(0, -1),
        new HexCoord(-1, 0), new HexCoord(-1, 1), new HexCoord(0, 1)
    };
    //--����--
    //����������߽ṹ���ж���ĺ���������ʵ��һЩ���ܡ�
    // ��ȡ�ض�������ھ�����
    public HexCoord GetNeighbor(int direction)//��������������һ������������0-5��
    {
        //���ص�ǰ�������Ӧ����������ӵĽ��
        return this + directions[direction];
    }
    //--���������--
    //��������Ϊ�ṹ���Զ������������Ϊ��
    // ���ؼӺ��������+��������������ֱ��
    public static HexCoord operator +(HexCoord a, HexCoord b)
    {
        //�����������������ӵ������ꡣ
        return new HexCoord(a.q + b.q, a.r + b.r);
    }
    //���ؼ����������-��
    public static HexCoord operator -(HexCoord a, HexCoord b)
    {
        //���������������������������
        return new HexCoord(a.q - b.q, a.r - b.r);
    }
    //--�ֵ���ݷ���--
    // �������� HexCoord ����Ϊ�ֵ� Dictionary �ļ���key��
    //�ж����������Ƿ����
    public override bool Equals(object obj)
    {
        //�ж�obj�Ƿ�ΪHexCoord���ͣ�����q��r�ֶ�ֵ��ȣ�����Ϊ�������
        return obj is HexCoord coord && q == coord.q && r == coord.r;
    }
    //Ϊ��������һ��Ψһ�Ĺ�ϣֵ
    public override int GetHashCode()
    {
        //ʹ��q��rԪ���gethashcode���������ɹ�ϣֵ
        return (q, r).GetHashCode();
    }
    //���ص����������==��
    public static bool operator ==(HexCoord a, HexCoord b)
    {
        return a.q == b.q && a.r == b.r;
    }
    //���ز��������������=��
    public static bool operator !=(HexCoord a, HexCoord b)
    {
        return !(a == b);
    }
}