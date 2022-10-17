/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Geometry.CoordinateSystem;
using System;
using System.Linq;
using System.Collections.Generic;
using BH.oM.Base.Attributes;
using BH.Engine.Base;
using System.ComponentModel;
using BH.oM.Quantities.Attributes;

namespace BH.Engine.Geometry
{
    public static partial class Modify
    {
        /***************************************************/
        /****          Split curve at points            ****/
        /***************************************************/

        [Description("Splits the curve at the provided Points and returns a list of curves corresponding to the segments. Points with a distance to the curve larger than the tolerance will be ignored.")]
        [Input("arc", "The curve to split.")]
        [Input("points", "The set of points to split the curve at. Method will ignore points that have a distance to the curve that is larger than the provided tolerance.")]
        [Input("tolerance", "Distance tolerance to be used in the method.", typeof(Length))]
        [Output("split", "The segments of the curve corresponding to the original curve split at the position of the provided points.")]
        public static List<Arc> SplitAtPoints(this Arc arc, List<Point> points, double tolerance = Tolerance.Distance)
        {
            if (!points.Any())
                return new List<Arc> { arc.DeepClone() };

            List<Arc> result = new List<Arc>();
            List<Point> cPts = new List<Point>();
            Vector normal = arc.Normal();

            foreach (Point point in points)
            {
                if (point.IsOnCurve(arc, tolerance))
                    cPts.Add(point);
            }

            cPts.Add(arc.StartPoint());
            cPts.Add(arc.EndPoint());
            cPts = cPts.CullDuplicates(tolerance);
            cPts = cPts.SortAlongCurve(arc, tolerance);

            if (arc.EndAngle - 2 * Math.PI < tolerance && arc.EndAngle - 2 * Math.PI > -tolerance)
                cPts.Add(arc.EndPoint());

            if (cPts.Count > 2)
            {
                Double startAng = arc.StartAngle;
                Double endAng = arc.EndAngle;
                Double tmpAng = 0;
                Arc tmpArc;

                for (int i = 1; i < cPts.Count; i++)
                {
                    tmpArc = arc.DeepClone();

                    tmpArc.StartAngle = startAng;
                    tmpAng = (2 * Math.PI + (cPts[i - 1] - arc.Centre()).SignedAngle(cPts[i] - arc.Centre(), normal)) % (2 * Math.PI);
                    endAng = startAng + tmpAng;
                    tmpArc.EndAngle = endAng;
                    result.Add(tmpArc);
                    startAng = endAng;
                }

            }
            else
                result.Add(arc.DeepClone());
            return result;
        }

        /***************************************************/

        [Description("Splits the curve at the provided Points and returns a list of curves corresponding to the segments. Points with a distance to the curve larger than the tolerance will be ignored.")]
        [Input("circle", "The curve to split.")]
        [Input("points", "The set of points to split the curve at. Method will ignore points that have a distance to the curve that is larger than the provided tolerance.")]
        [Input("tolerance", "Distance tolerance to be used in the method.", typeof(Length))]
        [Output("split", "The segments of the curve corresponding to the original curve split at the position of the provided points.")]
        public static List<ICurve> SplitAtPoints(this Circle circle, List<Point> points, double tolerance = Tolerance.Distance)
        {

            List<ICurve> result = new List<ICurve>();
            List<Point> cPts = new List<Point>();

            foreach (Point point in points)
            {
                if (point.IsOnCurve(circle, tolerance))
                    cPts.Add(point);
            }

            cPts = cPts.CullDuplicates(tolerance);
            cPts = cPts.SortAlongCurve(circle);

            Point cirCen = circle.Centre;
            Vector startVector = circle.StartPoint() - circle.Centre;
            Vector normal = circle.Normal;
            Double rotAng;
            Cartesian system = new Cartesian(circle.Centre, startVector.Normalise(), (circle.PointAtParameter(0.25) - cirCen).Normalise(), normal);
            Arc mainArc = new Arc();
            Arc tmpArc = new Arc();

            mainArc.CoordinateSystem = system;
            mainArc.Radius = circle.Radius;

            Vector stVec;
            Vector enVec;
            Double enAng;

            if (cPts.Count == 0)
            {
                result.Add(circle);
                return result;
            }

            if (cPts.Count == 1)
            {
                tmpArc = mainArc;
                stVec = cPts[0] - circle.Centre;
                rotAng = ((2 * Math.PI + startVector.SignedAngle(stVec, normal)) % (2 * Math.PI));
                enAng = 2 * Math.PI;

                tmpArc = tmpArc.Rotate(cirCen, normal, rotAng);
                tmpArc.StartAngle = 0;
                tmpArc.EndAngle = enAng;
                result.Add(tmpArc.DeepClone());
            }
            else
            {
                for (int i = 0; i < cPts.Count; i++)
                {
                    tmpArc = mainArc;
                    stVec = cPts[i] - circle.Centre;
                    enVec = cPts[(i + 1) % cPts.Count] - circle.Centre;
                    rotAng = ((2 * Math.PI + startVector.SignedAngle(stVec, normal)) % (2 * Math.PI));
                    enAng = ((2 * Math.PI + stVec.SignedAngle(enVec, normal)) % (2 * Math.PI));

                    tmpArc = tmpArc.Rotate(cirCen, normal, rotAng);
                    tmpArc.StartAngle = 0;
                    tmpArc.EndAngle = enAng;
                    result.Add(tmpArc.DeepClone());
                }
            }
            return result;
        }

        /***************************************************/

        [Description("Splits the curve at the provided Points and returns a list of curves corresponding to the segments. Points with a distance to the curve larger than the tolerance will be ignored.")]
        [Input("line", "The curve to split.")]
        [Input("points", "The set of points to split the curve at. Method will ignore points that have a distance to the curve that is larger than the provided tolerance.")]
        [Input("tolerance", "Distance tolerance to be used in the method.", typeof(Length))]
        [Output("split", "The segments of the curve corresponding to the original curve split at the position of the provided points.")]
        public static List<Line> SplitAtPoints(this Line line, List<Point> points, double tolerance = Tolerance.Distance)
        {
            List<Line> result = new List<Line>();
            List<Point> cPts = new List<Point> { line.Start, line.End };
            double sqTol = tolerance * tolerance;

            foreach (Point point in points)
            {
                if (point.SquareDistance(line.Start) > sqTol && point.SquareDistance(line.End) > sqTol && point.SquareDistance(line) <= sqTol)
                    cPts.Add(point);
            }

            if (cPts.Count > 2)
            {
                cPts = cPts.CullDuplicates(tolerance);
                cPts = cPts.SortAlongCurve(line, tolerance);
                for (int i = 0; i < cPts.Count - 1; i++)
                {
                    result.Add(new Line { Start = cPts[i], End = cPts[i + 1] });
                }
            }
            else
                result.Add(line.DeepClone());

            return result;
        }

        /***************************************************/

        [Description("Splits the curve at the provided Points and returns a list of curves corresponding to the segments. Points with a distance to the curve larger than the tolerance will be ignored.")]
        [Input("curve", "The curve to split.")]
        [Input("points", "The set of points to split the curve at. Method will ignore points that have a distance to the curve that is larger than the provided tolerance.")]
        [Input("tolerance", "Distance tolerance to be used in the method.", typeof(Length))]
        [Output("split", "The segments of the curve corresponding to the original curve split at the position of the provided points.")]
        public static List<PolyCurve> SplitAtPoints(this PolyCurve curve, List<Point> points, double tolerance = Tolerance.Distance)
        {
            if (points.Count == 0)
                return new List<PolyCurve> { curve.DeepClone() };

            List<PolyCurve> result = new List<PolyCurve>();

            List<ICurve> subParts = curve.ISubParts().ToList(); //Subparts of PolyCurve
            List<Point> nonDuplicatePoints = points.CullDuplicates(tolerance);

            PolyCurve prev = null;  //Polycurve to collect parts across segments
            double sqTol = tolerance * tolerance;
            for (int k = 0; k < subParts.Count; k++)
            {
                ICurve crv = subParts[k];

                List<ICurve> split = crv.ISplitAtPoints(nonDuplicatePoints, tolerance); //Split with all point. Methods called filters out points on the particular curve

                if (split.Count == 0)   //Should never happen
                {
                    Base.Compute.RecordWarning($"Unable to split a segment of type {crv.GetType()} in a Polycurve. Segment ignored in returned split curve.");
                    continue;
                }

                //Check if split at start or end of current segment by checking if the start and/or end point of the curve is within tolerance of the split points.
                Point stPt = split.First().IStartPoint();
                Point enPt = split.Last().IEndPoint();
                bool splitAtStart = nonDuplicatePoints.Any(x => x.SquareDistance(stPt) < sqTol);
                bool splitAtEnd = nonDuplicatePoints.Any(x => x.SquareDistance(enPt) < sqTol);

                for (int i = 0; i < split.Count; i++)
                {
                    if (i == 0 && splitAtStart) //If first split segment and split at start
                    {
                        if (prev != null)   //Check if previous exist and if so, add it
                            result.Add(prev);   //Add prev to return list

                        prev = null;    //Prev handled -> set to null
                    }

                    if (i != split.Count - 1 || splitAtEnd)   //If not last split or if split at end. Only need special case for last segment if _not_ splitting at endpoint
                    {
                        if (prev != null)   //Prev set - only true if this is the first split and not splitting at start
                        {
                            prev.Curves.Add(split[i]);  //Add full split segment to prev
                            result.Add(prev);   //Add prev to output list
                            prev = null;    //Prev handled, set to null. If splitting at end, nothing to bring over
                        }
                        else    //Prev not set
                        {
                            result.Add(new PolyCurve { Curves = new List<ICurve> { split[i] } });   //Add full split segment to output list
                        }
                    }
                    else    //Last split and not splitting at end
                    {
                        if (prev != null)   //If prev is set - should only be true for single split segment not split at start
                            prev.Curves.Add(split[i]);   //Add current split segment to prev
                        else
                            prev = new PolyCurve { Curves = new List<ICurve> { split[i] } }; //If prev is not set, set it with current split segment added
                    }
                }

            }

            if (prev != null)   //If prev is set
            {
                if (curve.StartPoint().SquareDistance(curve.EndPoint()) < sqTol && result.Count != 0) //If curve is closed
                    result[0].Curves.InsertRange(0, prev.Curves);   //Insert the curves in prev to the start of the first curve, ensuring a segment looping around the start/end is added
                else
                    result.Add(prev);   //If not, simply add the prev to the return list
            }

            return result;
        }

        /***************************************************/

        [Description("Splits the curve at the provided Points and returns a list of curves corresponding to the segments. Points with a distance to the curve larger than the tolerance will be ignored.")]
        [Input("curve", "The curve to split.")]
        [Input("points", "The set of points to split the curve at. Method will ignore points that have a distance to the curve that is larger than the provided tolerance.")]
        [Input("tolerance", "Distance tolerance to be used in the method.", typeof(Length))]
        [Output("split", "The segments of the curve corresponding to the original curve split at the position of the provided points.")]
        public static List<Polyline> SplitAtPoints(this Polyline curve, List<Point> points, double tolerance = Tolerance.Distance)
        {
            if (points.Count == 0)
                return new List<Polyline> { curve.DeepClone() };

            List<Polyline> result = new List<Polyline>();
            List<Line> segments = curve.SubParts();
            Polyline section = new Polyline { ControlPoints = new List<Point>() };
            bool closed = curve.IsClosed(tolerance);
            double sqTol = tolerance * tolerance;

            for (int i = 0; i < segments.Count; i++)
            {
                Line l = segments[i];
                bool intStart = false;
                List<Point> iPts = new List<Point>();

                foreach (Point point in points)
                {
                    if (point.SquareDistance(l.Start) <= sqTol)
                    {
                        intStart = true;
                        if (i == 0)
                            closed = false;
                    }
                    else if (point.SquareDistance(l.End) > sqTol && point.SquareDistance(l) <= sqTol)
                        iPts.Add(point);
                }

                section.ControlPoints.Add(l.Start);
                if (intStart && section.ControlPoints.Count > 1)
                {
                    result.Add(section);
                    section = new Polyline { ControlPoints = new List<Point> { l.Start.DeepClone() } };
                }

                if (iPts.Count > 0)
                {
                    iPts = iPts.CullDuplicates(tolerance);
                    iPts = iPts.SortAlongCurve(l, tolerance);
                    foreach (Point iPt in iPts)
                    {
                        section.ControlPoints.Add(iPt);
                        result.Add(section);
                        section = new Polyline { ControlPoints = new List<Point> { iPt } };
                    }
                }
            }
            section.ControlPoints.Add(curve.ControlPoints.Last());
            result.Add(section);

            if (result.Count > 1 && closed)
            {
                result[0].ControlPoints.RemoveAt(0);
                result[0].ControlPoints.InsertRange(0, result.Last().ControlPoints);
                result.RemoveAt(result.Count - 1);
            }

            return result;
        }


        /***************************************************/
        /**** Public Methods - Interfaces               ****/
        /***************************************************/

        [Description("Splits the curve at the provided Points and returns a list of curves corresponding to the segments. Points with a distance to the curve larger than the tolerance will be ignored.")]
        [Input("curve", "The curve to split.")]
        [Input("points", "The set of points to split the curve at. Method will ignore points that have a distance to the curve that is larger than the provided tolerance.")]
        [Input("tolerance", "Distance tolerance to be used in the method.", typeof(Length))]
        [Output("split", "The segments of the curve corresponding to the original curve split at the position of the provided points.")]
        public static List<ICurve> ISplitAtPoints(this ICurve curve, List<Point> points, double tolerance = Tolerance.Distance)
        {
            List<ICurve> result = new List<ICurve>();
            System.Collections.IList splitCurves = SplitAtPoints(curve as dynamic, points, tolerance);
            for (int i = 0; i < splitCurves.Count; i++)
            {
                result.Add(splitCurves[i] as ICurve);
            }

            return result;
        }

        /***************************************************/
        /**** Private Fallback Methods                  ****/
        /***************************************************/

        private static List<ICurve> SplitAtPoints(this ICurve curve, List<Point> points, double tolerance = Tolerance.Distance)
        {
            Base.Compute.RecordError($"SplitAtPoints is not implemented for ICurves of type: {curve.GetType().Name}.");
            return null;
        }

        /***************************************************/
    }
}


