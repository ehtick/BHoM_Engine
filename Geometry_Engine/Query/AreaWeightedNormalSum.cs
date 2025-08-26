using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.Engine.Geometry
{
    public static partial class Query
    {
        public static double AreaWeightedNormalSum(this Mesh mesh)
        {
            if (mesh == null)
            {
                Engine.Base.Compute.RecordError("Cannot calculate area weighted normal sum of a null mesh.");
                return double.NaN;
            }

            if (mesh.Vertices == null || mesh.Faces == null)
            {
                Engine.Base.Compute.RecordError("Mesh has to have valid vertices and faces to calculate its area weighted normal sum.");
                return double.NaN;
            }

            Vector totalNormal = new Vector();
            double totalArea = 0.0;

            List<Point> vertices = mesh.Vertices;
            List<Face> faces = mesh.Faces;
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                Face face = faces[i];
                Point pA = vertices[face.A];
                Point pB = vertices[face.B];
                Point pC = vertices[face.C];

                if (face.D == -1) // Triangular face
                {
                    Vector normal = (pB - pA).CrossProduct(pC - pA).Normalise();

                    Vector AB = new Vector { X = pB.X - pA.X, Y = pB.Y - pA.Y, Z = pB.Z - pA.Z };
                    Vector AC = new Vector { X = pC.X - pA.X, Y = pC.Y - pA.Y, Z = pC.Z - pA.Z };
                    double area = 0.5 * AB.CrossProduct(AC).Length();

                    totalNormal += normal * area;
                    totalArea += area;
                }
                else // Quadrilateral face - split into two triangles
                {
                    Point pD = vertices[face.D];

                    // First triangle: A, B, C
                    Vector normal1 = (pB - pA).CrossProduct(pC - pA).Normalise();
                    Vector AB = new Vector { X = pB.X - pA.X, Y = pB.Y - pA.Y, Z = pB.Z - pA.Z };
                    Vector AC = new Vector { X = pC.X - pA.X, Y = pC.Y - pA.Y, Z = pC.Z - pA.Z };
                    double area1 = 0.5 * AB.CrossProduct(AC).Length();

                    // Second triangle: A, C, D
                    Vector normal2 = (pC - pA).CrossProduct(pD - pA).Normalise();
                    Vector AD = new Vector { X = pD.X - pA.X, Y = pD.Y - pA.Y, Z = pD.Z - pA.Z };
                    double area2 = 0.5 * AC.CrossProduct(AD).Length();

                    totalNormal += normal1 * area1 + normal2 * area2;
                    totalArea += area1 + area2;
                }
            }

            return totalNormal.Length() / totalArea;
        }
    }
}
