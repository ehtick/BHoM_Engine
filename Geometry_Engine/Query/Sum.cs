using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.Engine.Geometry
{
    public static partial class Query
    {
        public static Vector Sum(this IEnumerable<Vector> vectors)
        {
            Vector result = new Vector();
            foreach (Vector item in vectors)
            {
                result += item;
            }

            return result;
        }
    }
}
