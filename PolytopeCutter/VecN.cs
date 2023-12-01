using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

class VecN : IEquatable<VecN>
{
    public double[] coords;
    public int dim => coords.Length;

    public VecN(double[] c)
    {
        coords = c;
    }
    public VecN(int n)
    {
        coords = new double[n];
    }

    public double this[int i]
    {
        get { return coords[i]; }
        set { coords[i] = (double)value; }
    }
    //op * does the dot product when both inputs are VecN
    public static double operator *(VecN a, VecN b)
    {
        if (a.dim != b.dim) throw new InvalidOperationException();
        double s = 0;
        for (int i = 0; i < a.dim; i++)
        {
            s += a.coords[i] * b.coords[i];
        }
        return s;
    }
    //op * does scalar multiplication when formatted as double * VecN; has to be specifically doubles becasue the overloading is funky
    public static VecN operator *(double a, VecN b)
    {
        double[] cL = new double[b.dim];
        for (int i = 0; i < b.dim; i++)
        {
            cL[i] = a * b.coords[i];
        }
        return new VecN(cL);
    }
    public static VecN operator *(VecN a, double b)
    {
        return b * a;
    }
    //op unary ! does Euclidean magnitude
    public static double operator !(VecN a)
    {
        return Math.Sqrt(a * a);
    }
    //additive structure works as expected
    public static VecN operator +(VecN a, VecN b)
    {
        if (a.dim != b.dim) throw new InvalidOperationException();
        double[] cL = new double[a.dim];
        for (int i = 0; i < a.dim; i++)
        {
            cL[i] = a.coords[i] + b.coords[i];
        }
        return new VecN(cL);
    }
    public static VecN operator -(VecN a)
    {
        double[] cL = new double[a.dim];
        for (int i = 0; i < a.dim; i++)
        {
            cL[i] = -a.coords[i];
        }
        return new VecN(cL);
    }
    public static VecN operator -(VecN a, VecN b)
    {
        return a + (-b);
    }

    public static bool operator ==(VecN a, VecN b)
    {
        if (a.dim != b.dim) throw new InvalidOperationException();
        for (int i = 0; i < a.dim; i++)
            if (a.coords[i] != b.coords[i])
                return false;
        return true;
    }
    public static bool operator !=(VecN a, VecN b)
    {
        return !(a == b);
    }

    public VecN unit()
    {
        return 1 / (!this) * this;
    }
    public VecN proj(VecN onto)
    {
        VecN b = onto.unit();
        return (this * b) * b;
    }
    public VecN trans(double[,] mat)
    {
        if (mat.GetLength(0) != dim) throw new ArgumentException();
        VecN o = new VecN(mat.GetLength(1));
        for (int i = 0; i < mat.GetLength(0); i++)
            for (int j = 0; j < mat.GetLength(1); j++)
                o[j] += mat[i, j] * coords[i];
        return o;
    }

    public static VecN Wintersect(VecN a, VecN b)
    {
        if (a.dim != b.dim) throw new InvalidOperationException();
        double[] o = new double[a.dim - 1];
        for (int i = 1; i < a.dim; i++)
        {
            o[i - 1] = (a[i] * Math.Abs(b[0] / (a[0] - b[0]))) + (b[i] * Math.Abs(a[0] / (a[0] - b[0])));
        }
        return new VecN(o);
    }
    public static VecN basis(int n, int k)//returns a unit vector along the kth dim in an n-dim space
    {
        if (k >= n) throw new ArgumentOutOfRangeException();
        double[] c = new double[n];
        for (int i = 0; i < n; i++) c[i] = i == k ? 1 : 0;
        return new VecN(c);
    }


    public override bool Equals(object obj)
    {
        return Equals(obj as VecN);
    }
    public bool Equals(VecN other)
    {
        return this == other;
    }
    public override int GetHashCode()
    {
        int hashCode = -1551897352;
        hashCode = hashCode * -1521134295 + EqualityComparer<double[]>.Default.GetHashCode(coords);
        hashCode = hashCode * -1521134295 + dim.GetHashCode();
        return hashCode;
    }
}
