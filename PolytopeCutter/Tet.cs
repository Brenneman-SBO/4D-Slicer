using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

class Tet
{
    public VecN[] pts = new VecN[4];
    public Tet(VecN[] p)
    {
        if (p.Length != 4)
        {
            throw new ArgumentException();
        }
        pts = p;
    }
    public List<Tri> slice()
    {
        List<VecN> above = new List<VecN>();
        List<VecN> below = new List<VecN>();
        foreach (VecN p in pts)
        {
            if (p[0] > 0) above.Add(p);
            else below.Add(p);
        }
        if (above.Count % 4 == 0) return new List<Tri>();//%4==0 means there's either 4 above or 4 below, either way there's no intersection.
        List<VecN> flat = new List<VecN>();
        foreach (VecN a in above)
            foreach (VecN b in below)
                flat.Add(VecN.Wintersect(a, b));
        if (flat.Count == 3) return new List<Tri> { new Tri(flat.ToArray()) }; //if there are exactly 3 intersection points we have one triangle and just send it back
        return new List<Tri>//if we get here we're in the 4-intersections regime
            {
                new Tri(new VecN[] {flat[0], flat[1], flat[2]}),
                new Tri(new VecN[] {flat[3], flat[1], flat[2]})
            };
    }
    public List<Tri> unwrap()
    {
        List<Tri> o = new List<Tri>();
        for (int i = 0; i < 4; i++)
        {
            o.Add(new Tri(pts.Except(new VecN[] { pts[i] }).ToArray()));
            //o.Add(new Tri(new VecN[] { pts[i], pts[(i + 1) % 4], pts[(i + 2) % 4] }));
        }
        return o;
    }
    public void trans(double[,] mat)
    {
        if (mat.GetLength(0) != mat.GetLength(1)) throw new ArgumentException(); //square matrices only!!
        for (int i = 0; i < 4; i++)
        {
            pts[i] = pts[i].trans(mat);
        }
    }
    public void trans(Func<VecN, VecN> f)
    {
        for (int i = 0; i < 4; i++)
        {
            pts[i] = f(pts[i]);
        }
    }
    public void trans(Mat m)
    {
        if (m.d0 != 5 || m.d1 != 5) throw new ArgumentException();
        for (int i=0; i<4; i++)
        {
            VecN va = new VecN(new double[] { pts[i][0], pts[i][1], pts[i][2], pts[i][3], 1.0 });
            va = va * m;
            pts[i] = new VecN(new double[] { va[0], va[1], va[2], va[3] });
        }
    }
}