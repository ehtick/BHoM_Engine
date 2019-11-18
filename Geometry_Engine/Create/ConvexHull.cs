/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.oM.Geometry;
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Geometry
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Deprecated("3.0", "Deprecated, method moved to compute file", null, "BH.Engine.Geometry.Compute.ConvexHull(List<Point>")]
        //TODO: Only works for points in the XY plane - add plane as input?
        [Description("Creates a Convex Hull from a list of points. Currently only works for points in the XY plane")]
        public static Polyline ConvexHull(List<Point> points)
        {
            return BH.Engine.Geometry.Compute.ConvexHull(points);
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Point NextHullPoint(List<Point> points, Point currentPt)
        {
            int right = -1;
            int none = 0;

            Point nextPt = currentPt;
            int t;
            foreach (Point pt in points)
            {
                t = ((nextPt.X - currentPt.X) * (pt.Y - currentPt.Y) - (pt.X - currentPt.X) * (nextPt.Y - currentPt.Y)).CompareTo(0);
                if (t == right || t == none && Geometry.Query.Distance(currentPt, pt) > Geometry.Query.Distance(currentPt, nextPt))
                    nextPt = pt;
            }

            return nextPt;
        }

        /***************************************************/
    }
}