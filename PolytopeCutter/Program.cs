using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PolytopeCutter
{
    class Program
    {
        static void Main(string[] args)
        {
            var hyperCube = GenerateHypercube(1.0);

            // Exports 4 dimensional shape to .obj file for visualization (does not merge things, deal with it)

            // Stores vertexes that have been encountered so far
            List<VecN> vertexList = new List<VecN>();
            List<List<int>> faceList = new List<List<int>>();
            // iterates over all of the tets in the hypercube
            foreach (Tet Tetra in hyperCube)
            {
                List<VecN> vertexListTetra = new List<VecN>();
                // Unwraps each tet into triangles
                var tetTris = Tetra.slice();
                // Iterates over each triangle of the tet
                foreach (Tri triangle in tetTris)
                {
                    List<int> verts = new List<int>();
                    foreach (VecN point in triangle.pts)
                    {
                        int index = vertexListTetra.IndexOf(vertexListTetra.Find(obj => obj == point));
                        
                        if (index != -1)
                        {
                            // Item is already in vertexList, so use its index as the index for one of this faces vertex
                            verts.Add(index + 1 + vertexList.Count);
                        }
                        else
                        {
                            // Item is not in the vertexList, so add it and use the new index as the point
                            verts.Add(vertexListTetra.Count+1+vertexList.Count);
                            vertexListTetra.Add(point);
                        }
                    }
                    faceList.Add(verts);
                }
                vertexList.AddRange(vertexListTetra);
            }
            // Specify the file path
            string filePath = "C:/Users/necro/Documents/C# stuff/PolytopeCutter/hypercube1.obj";


            // Use a StreamWriter to write to the file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Converts the 4D vertexes into 3D vertexes, multiplying the magnitude by W
                // Exports vertexes
                foreach (VecN point in vertexList)
                {
                    List<double> doubleList = point.coords.ToList();
                    string formattedValues = "v " + string.Join(" ", doubleList.Select(d => d.ToString("F5")));
                    writer.WriteLine(formattedValues);
                }
                
                foreach (List<int> facePoint in faceList) 
                {
                    string faceLine = "f " + facePoint[0].ToString() + " " + facePoint[1].ToString() + " " + facePoint[2].ToString();
                    writer.WriteLine(faceLine);
                }
            }

        }

        public static List<Tet> toTetFromFace(List<VecN> facePoints,VecN center) 
        {
            List<Tet> TetList = new List<Tet>();
            // Connects all face points to the 0th index face point and then all of the new triangles to the center vertex to form a list of "Tet"s
            for (int i = 1; i < facePoints.Count()-1; i++) 
            {
                TetList.Add(new Tet(new VecN[] { facePoints[i + 1], facePoints[i], facePoints[0], center }));
            }
            return TetList;
        }
        
        // Cases:
        // 1X1X1X1 (can only generate hyperrectangular prisms, represented as start and end points),
        // 1X1X2 (2D always forms loops so it can be represented as a cycle?)
        // 1X3 (How should 3D be represented?)
        // 2X2 (2 loops?)
        public static List<Tet> GenerateHyperCrossProduct()
        {
            

        }

        public static List<Tet> GenerateHypercube(double sideLength)
        {

            List<Tet> TetList = new List<Tet>();
            int[] HELPME = new int[] { 0, 1, 3, 2 };
            for (int dim1 = 0; dim1 < 4; dim1++)
            {
                for (int dim2 = dim1 + 1; dim2 < 4; dim2++) // dim1&2 are the fixed dimensions
                {
                    List<int> offDims = new List<int> { 0, 1, 2, 3 };
                    List<int> onDims = new List<int>{dim1, dim2};
                    offDims = offDims.Except(onDims).ToList();
                    foreach (int p in HELPME) // choose the values of the two static ones
                    {
                        List<VecN> verts = new List<VecN>();
                        foreach (int q in HELPME) // and here's the two dynamic ones
                        {
                            VecN x = new VecN(new double[] { 0, 0, 0, 0 });
                            x[ onDims[0]] =  q & 1;
                            x[ onDims[1]] = (q & 2) >> 1;
                            x[offDims[0]] =  p & 1;
                            x[offDims[1]] = (p & 2) >> 1;
                            verts.Add(sideLength * x);
                        }

                        // Center tet vertex generation from dim1&2
                        // will be sideLength/2 on 3 axis and sideLength or 0 on the remaining axis
                        VecN center = new VecN(new double[] { 0.5, 0.5, 0.5, 0.5 });
                        center[offDims[0]] = p & 1;
                        TetList.AddRange(toTetFromFace(verts, sideLength * center)); // Case dim1
                        center = new VecN(new double[] { 0.5, 0.5, 0.5, 0.5 });
                        center[offDims[1]] = (p & 2)>>1;
                        TetList.AddRange(toTetFromFace(verts, sideLength * center)); // Case dim2

                    }
                }
            }
            VecN shift = new VecN(new double[] { -0.9, -0.0, -0.0, -0.0 });
            Func<VecN, VecN> Fshift = x => x + shift;
            foreach (Tet tetra in TetList)
                tetra.trans(Fshift);
            return TetList;
        }
    }

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

        public double this[int x, int y]
        {
            get { return val[x, y]; }
            set { val[x,y] = (double)value; }
        }
        public static Mat operator +(Mat a, Mat b)
        {
            if (a.d0 != b.d0 || a.d1 != b.d1) throw new InvalidOperationException();
            Mat o = new Mat(a.d0, a.d1);
            for (int x=0; x<a.d0; x++)
                for (int y=0; y<a.d1; y++)
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
            for (int i=0; i<a.dim; i++)
            {
                s+= a.coords[i] * b.coords[i];
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
            return 1/(!this) * this;
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
            for (int i=1; i<a.dim; i++)
            {
                o[i-1] = (a[i] * Math.Abs(b[0] / (a[0] - b[0]))) + (b[i] * Math.Abs(a[0] / (a[0] - b[0])));
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
            for (int i=0; i<4; i++)
            {
                o.Add(new Tri(pts.Except(new VecN[] { pts[i]}).ToArray()));
                //o.Add(new Tri(new VecN[] { pts[i], pts[(i + 1) % 4], pts[(i + 2) % 4] }));
            }
            return o;
        }
        public void trans(double[,] mat)
        {
            if (mat.GetLength(0) != mat.GetLength(1)) throw new ArgumentException(); //square matrices only!!
            for (int i=0; i<4; i++)
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
    }
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

}
