using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

class Mat : IEquatable<Mat>
{
    public double[,] val;
    public int d0 => val.GetLength(0);
    public int d1 => val.GetLength(1);

    public Mat(double[,] v)
    {
        val = v;
    }
    public Mat(int x, int y)
    {
        val = new double[x, y];
    }

    public Mat T()
    {
        Mat o = new Mat(d1, d0);
        for (int i = 0; i < d0; i++)
            for (int j = 0; j < d1; j++)
                o[j, i] = val[i, j];
        return o;
    }
    public static Mat rotation(int s, int f, double Θ)
    {
        if (s < 0 || f < 0 || s > 3 || f > 3 || s == f) throw new ArgumentException();
        Mat o = new Mat(5, 5);
        o[s, s] = o[f, f] = Math.Cos(Θ);
        o[s, f] = -Math.Sin(Θ);
        o[f, s] =  Math.Sin(Θ);
        return o;
    }
    public static Mat translation(VecN v)
    {
        if (v.dim != 4) throw new ArgumentException();
        Mat o = new Mat(5, 5);
        for (int i = 0; i < 4; i++)
            o[i, i] = v[i];
        return o;
    }

    public double this[int x, int y]
    {
        get { return val[x, y]; }
        set { val[x, y] = (double)value; }
    }
    public static Mat operator +(Mat a, Mat b)
    {
        if (a.d0 != b.d0 || a.d1 != b.d1) throw new InvalidOperationException();
        Mat o = new Mat(a.d0, a.d1);
        for (int x = 0; x < a.d0; x++)
            for (int y = 0; y < a.d1; y++)
                o[x, y] = a[x, y] + b[x, y];
        return o;
    }
    public static Mat operator *(Mat a, Mat b)
    {
        if (a.d1 != b.d0) throw new InvalidOperationException();
        Mat o = new Mat(a.d0, b.d1);
        for (int i = 0; i < a.d0; i++)
            for (int j = 0; j < a.d1; j++)
                for (int k = 0; k < b.d1; k++)
                    o[i, k] += a[i, j] * b[j, k];
        return o;
    }
    public static Mat operator *(double c, Mat m)
    {
        Mat o = new Mat(m.d0, m.d1);
        for (int i = 0; i < m.d0; i++)
            for (int j = 0; j < m.d1; j++)
                o[i, j] = c * m[i, j];
        return o;
    }
    public static Mat operator *(Mat m, double c)
    {
        return c * m;
    }
    public static VecN operator *(VecN v, Mat m)
    {
        if (v.dim != m.d0) throw new InvalidOperationException();
        VecN o = new VecN(m.d1);
        for (int i = 0; i < m.d0; i++)
            for (int j = 0; j < m.d1; j++)
                o[j] += v[i] * m[i, j];
        return o;
    }
    public static VecN operator *(Mat m, VecN v)
    {
        if (v.dim != m.d1) throw new InvalidOperationException();
        VecN o = new VecN(m.d0);
        for (int i = 0; i < m.d0; i++)
            for (int j = 0; j < m.d1; j++)
                o[i] += v[j] * m[i, j];
        return o;
    }
    public static Mat operator -(Mat a)
    {
        return -1 * a;
    }
    public static Mat operator -(Mat a, Mat b)
    {
        return a + (-b);
    }

    public static bool operator ==(Mat a, Mat b)
    {
        if (a.d0 != b.d0 || a.d1 != b.d1) return false;
        for (int x = 0; x < a.d0; x++)
            for (int y = 0; y < a.d1; y++)
                if (a[x, y] != b[x, y])
                    return false;
        return true;
    }
    public static bool operator !=(Mat a, Mat b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Mat);
    }
    public bool Equals(Mat other)
    {
        return this == other;
    }
    public override int GetHashCode()
    {
        int hashCode = -1551897352;
        hashCode = hashCode * -1521134295 + EqualityComparer<double[,]>.Default.GetHashCode(val);
        return hashCode;
    }
}
