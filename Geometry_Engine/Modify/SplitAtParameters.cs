/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.oM.Base;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Geometry
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<NurbsCurve> SplitAtParameters(this NurbsCurve curve, List<double> ts, double tolerance = Tolerance.Distance)
        {
            if (ts.Count == 0)
                return new List<NurbsCurve> { curve };

            if (ts.Count > 1)
                ts = ts.OrderBy(x => x).ToList();

            bool isClosed = false;
            //If curve is closed, first t should change the seam
            if (isClosed = curve.IsClosed())
            {
                curve = ChangeSeam(curve, ts[0], tolerance);
                if (ts.Count == 1)
                {
                    BH.Engine.Base.Compute.RecordWarning("Input curve is closed. No split performed, seam has been changed to the input parameter instead.");
                    return new List<NurbsCurve> { curve };
                }
                else
                    ts.RemoveAt(0);
            }

            var ctrlPts = Convert.ToDoubleArray(curve.ControlPoints, curve.Weights);    //Convert to control points
            bool isRational = ctrlPts.Item2;
            List<double[]> cw = ctrlPts.Item1;
            List<double> knots = curve.Knots;
            int degree = curve.Degree();

            //Ensure the curve is clamped
            var clamped = EnsureClamped(cw, knots, degree);
            cw = clamped.Item1;
            knots = clamped.Item2;

            for (int i = 0; i < ts.Count; i++)
            {
                int r = degree - Query.KnotMultiplicity(knots, ts[i]);
                Output<List<double>, List<double[]>> inserted = InsertKnot(degree, knots, cw, ts[i], r);    //insert each knot degree amount of times
                cw = inserted.Item2;
                knots = inserted.Item1;
            }

            List<NurbsCurve> splitCurves = new List<NurbsCurve>();

            int startSpan = 0;
            int startPtIndex = 0;
            List<double> tmpKnots;

            //add from left to right segments up to last split point
            for (int i = 0; i < ts.Count; i++)
            {
                double t = ts[i];

                if (t == 0 || t == 1)
                    continue;

                int endSpan = endSpan = Geometry.Query.KnotSpan(knots, degree, t) + 1;

                tmpKnots = new List<double>();

                for (int j = startSpan; j < endSpan; j++)
                {
                    tmpKnots.Add(knots[j]);
                }

                int endIndex = endSpan - degree + 1;

                var ptsAndWeight = Convert.ToPointAndWeight(cw.GetRange(startPtIndex, endIndex-startPtIndex), isRational);

                splitCurves.Add(new NurbsCurve { Knots = tmpKnots, ControlPoints = ptsAndWeight.Item1, Weights = ptsAndWeight.Item2 });

                startSpan = endSpan - degree;
                startPtIndex = endIndex - 1;
            }

            //Add last segment
            tmpKnots = new List<double>();

            for (int j = startSpan; j < knots.Count; j++)
            {
                tmpKnots.Add(knots[j]);
            }

            var ptsAndWeightEnd = Convert.ToPointAndWeight(cw.GetRange(startPtIndex, cw.Count - startPtIndex), isRational);

            splitCurves.Add(new NurbsCurve { Knots = tmpKnots, ControlPoints = ptsAndWeightEnd.Item1, Weights = ptsAndWeightEnd.Item2 });

            return splitCurves;
        }


        /***************************************************/
    }
}
