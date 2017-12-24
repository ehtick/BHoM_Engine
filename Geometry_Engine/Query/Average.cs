﻿using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Geometry
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Point Average(this IList<Point> points)
        {
            int count = points.Count();
            if (count < 1) return null;

            Point mean = new Point(0, 0, 0);

            foreach (Point pt in points)
                mean += pt;

            return mean /= count;
        }

        /***************************************************/

        public static Vector Average(this List<Vector> vs)
        {
            int count = vs.Count();
            if (count < 1) return null;

            Vector mean = new Vector(0, 0, 0);

            foreach (Vector v in vs)
                mean += v;

            return mean /= count;
        }

        /***************************************************/
    }
}
