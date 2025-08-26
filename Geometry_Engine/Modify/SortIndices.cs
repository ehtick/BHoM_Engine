using BH.Engine.Base;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Geometry
{
    public static partial class Modify
    {
        public static void SortIndices(this Mesh mesh)
        {
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                Face f = mesh.Faces[i];

                List<int> indices = new List<int> { f.A, f.B, f.C };
                if (f.D != -1)
                    indices.Add(f.D);

                int min = indices.Min();
                int j = indices.IndexOf(min);

                indices = indices.ShiftList(j);
                f.A = indices[0];
                f.B = indices[1];
                f.C = indices[2];

                if (f.D != -1)
                    f.D = indices[3];
            }

            mesh.Faces = mesh.Faces.OrderBy(x => x.A).ThenBy(x => x.B).ThenBy(x => x.C).ThenBy(x => x.D).ToList();
        }
    }
}
