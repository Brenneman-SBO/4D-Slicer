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
            List<Tet> hyperCube = GenerateHypercube(1.0);
            VecN shift = new VecN(new double[] { -0.5, -0.5, -0.5, -0.5 });
            Mat cumulation = Mat.translation(shift) * Mat.rotation(1, 0, Math.Atan(1)) * Mat.rotation(2, 0, Math.Atan(1/Math.Sqrt(2))) * Mat.rotation(3, 0, Math.Atan(1/Math.Sqrt(3)));
            foreach (Tet tetra in hyperCube)
                tetra.trans(cumulation);

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
            string filePath = "C:/Users/necro/Documents/GitHub/4D-Slicer/PolytopeCutter/hypercube1.obj";


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
            return new List<Tet>();
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
            return TetList;
        }
    }

}
