using UnityEngine;
//六边形坐标脚本
// 定义一个名为 HexDirection 的枚举类型。枚举是用来表示一组常量的数据类型
[System.Serializable]//这个特性让这个枚举可以在unity的inspector面板中显示
public enum HexDirection
{
    //定义了六个方向的常量，并赋值0到5，方便在数组和循环中使用
    Right = 0,       // 右方 (Q+1, R+0)
    UpRight = 1,     // 右上方 (Q+1, R-1)
    UpLeft = 2,      // 左上方 (Q+0, R-1)
    Left = 3,        // 左方 (Q-1, R+0)
    DownLeft = 4,    // 左下方 (Q-1, R+1)
    DownRight = 5    // 右下方 (Q+0, R+1)
}
//定义一个名为HexCoord的结构体（struct）。结构体是轻量级的数据容器，适合用来储存像坐标这样的简单数据。
public struct HexCoord
{
    //--字段--
    //字段是类或者结构体中直接声明的变量。
    [SerializeField] private int q;//私有字段，储存六边形q轴分量
    [SerializeField] private int r;//私有字段，储存六边形r轴分量
    public static HexCoord zero => new HexCoord(0, 0); 
    //--属性--
    //属性是类或者结构体中定义的函数，用于获取或设置字段的值。这里是只读属性
    public int Q => q;//公共只读属性，外部代码可以通过Q来读取q的值，但不能修改
    public int R => r;//公共只读属性，外部代码可以通过R来读取r的值，但不能修改
    public static HexCoord Zero =>new HexCoord(0, 0);//公共静态只读属性，返回值为HexCoord(0, 0)

    //--构造函数--
    //构造函数是在创建结构体实例时调用的特殊方法，用于初始化字段
    public HexCoord(int q, int r)//公共构造函数，接受q和r两个参数
    {
        this.q = q;//使用this 关键字来区分参数q和字段q，将参数值赋给字段
        this.r = r;//将参数r的值赋予字段
        
    }
    //--静态字段--
    //静态字段是类级别的变量，可以被所有实例共享。
    //这里定义了一个私有的静态只读数组，用于储存六个方向的单位向量。
    //static表示这个字段属于 HexCoord 类，而不是某个实例。
    // readonly 表示这个数组在初始化后不能再被赋值
    private static readonly HexCoord[] directions = new HexCoord[]
    {
        new HexCoord(1, 0), new HexCoord(1, -1), new HexCoord(0, -1),
        new HexCoord(-1, 0), new HexCoord(-1, 1), new HexCoord(0, 1)
    };
    //--方法--
    //方法是类或者结构体中定义的函数，用于实现一些功能。
    // 获取特定方向的邻居坐标
    public HexCoord GetNeighbor(int direction)//公共方法，接受一个方向索引（0-5）
    {
        //返回当前坐标与对应方向向量相加的结果
        return this + directions[direction];
    }
    //--运算符重载--
    //允许我们为结构体自定义运算符的行为。
    // 重载加号运算符（+），让坐标计算更直观
    public static HexCoord operator +(HexCoord a, HexCoord b)
    {
        //返回两个坐标分量相加的新坐标。
        return new HexCoord(a.q + b.q, a.r + b.r);
    }
    //重载减号运算符（-）
    public static HexCoord operator -(HexCoord a, HexCoord b)
    {
        //返回两个坐标分量相减后的新坐标
        return new HexCoord(a.q - b.q, a.r - b.r);
    }
    //--字典兼容方法--
    // 以下是让 HexCoord 能作为字典 Dictionary 的键（key）
    //判断两个对象是否相等
    public override bool Equals(object obj)
    {
        //判断obj是否为HexCoord类型，并且q和r字段值相等，则认为它们相等
        return obj is HexCoord coord && q == coord.q && r == coord.r;
    }
    //为对象生成一个唯一的哈希值
    public override int GetHashCode()
    {
        //使用q和r元组的gethashcode方法来生成哈希值
        return (q, r).GetHashCode();
    }
    //重载等于运算符（==）
    public static bool operator ==(HexCoord a, HexCoord b)
    {
        return a.q == b.q && a.r == b.r;
    }
    //重载不等于运算符（！=）
    public static bool operator !=(HexCoord a, HexCoord b)
    {
        return !(a == b);
    }
}