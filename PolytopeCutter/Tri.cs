using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

class Tri//TODO refactor Tri and Tet as children of abstract Simplex
{
    public VecN[] pts = new VecN[3];
    public Tri(VecN[] p)
    {
        if (p.Length != 3)
        {
            throw new ArgumentException();
        }
        pts = p;
    }
    public void trans(double[,] mat)
    {
        if (mat.GetLength(0) != mat.GetLength(1)) throw new ArgumentException(); //square matrices only!!
        for (int i = 0; i < 4; i++)
        {
            pts[i] = pts[i].trans(mat);
        }
    }

}
