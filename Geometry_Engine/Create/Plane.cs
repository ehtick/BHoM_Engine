﻿using BH.oM.Geometry;

namespace BH.Engine.Geometry
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Plane Plane(Point p1, Point p2, Point p3)
        {
            Vector normal = Query.CrossProduct(p2 - p1, p3 - p1).Normalise();
            return new Plane(p1.Clone(), normal);
        }

        /***************************************************/
    }
}
