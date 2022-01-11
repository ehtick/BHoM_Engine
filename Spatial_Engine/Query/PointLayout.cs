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

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Spatial.Layouts;
using BH.Engine.Base;
using BH.Engine.Geometry;

namespace BH.Engine.Spatial
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Queries the points from the Layout based on the layout and a single region curve associated with it.")]
        [Input("layout2D", "The layout object to query the points from.")]
        [Input("hostRegionCurve", "The region curve of the objects associated with the layout.")]
        [Input("openingCurves", "Optional opening curves in the region.")]
        [Output("points", "Point layout generated by the layout objects and region curve.")]
        public static List<Point> PointLayout(this ILayout2D layout2D, ICurve hostRegionCurve, IEnumerable<ICurve> openingCurves = null)
        {
            return IPointLayout(layout2D, new List<ICurve> { hostRegionCurve }, openingCurves);
        }

        /***************************************************/

        [Description("Returns the points in the layout. For ExplicitLayouts, the host geometry will be ignored.")]
        [Input("layout2D", "The layout object to query the points from.")]
        [Input("hostRegionCurves", "The region curves of the objects associated with the layout. Unused for Explicit layouts.")]
        [Input("openingCurves", "Optional opening curves in the region. Unused for ExplicitLayout.")]
        [Output("points", "The Points stored in the ExplicitLayout object.")]
        public static List<Point> PointLayout(this ExplicitLayout layout2D, IEnumerable<ICurve> hostRegionCurves = null, IEnumerable<ICurve> openingCurves = null)
        {
            return layout2D.Points.ToList();
        }

        /***************************************************/

        [Description("Returns a series of points along the host region curves, based on the parameters in the PerimeterLayout.")]
        [Input("layout2D", "The layout object to query the points from.")]
        [Input("hostRegionCurves", "The region curves of the objects associated with the layout.")]
        [Input("openingCurves", "Optional opening curves in the region. Unused for PerimeterLayout.")]
        [Output("points", "Point layout generated by the layout objects and region curve.")]
        public static List<Point> PointLayout(this PerimeterLayout layout2D, IEnumerable<ICurve> hostRegionCurves, IEnumerable<ICurve> openingCurves = null)
        {
            List<ICurve> regionCurveList = hostRegionCurves.ToList();
            List<int> divs = new List<int>();
            if (regionCurveList.Count == 1)
                divs.Add(layout2D.NumberOfPoints);
            else
                divs = DistributeDivisions(regionCurveList, layout2D.NumberOfPoints);

            List<Point> result = new List<Point>();

            for (int j = 0; j < regionCurveList.Count; j++)
            {
                ICurve regionCurve = regionCurveList[j];
                int curveDivs = divs[j];

                List<ICurve> subCurves = regionCurve.ISubParts().ToList();
                if (layout2D.EnforceDiscontinuityPoints && subCurves.Count != 1)
                {
                    List<Point> pts = regionCurve.IDiscontinuityPoints();
                    pts.Add(regionCurve.IStartPoint());
                    pts.Add(regionCurve.IEndPoint());
                    pts = pts.CullDuplicates();
                    int remainingPoints = curveDivs - pts.Count;
                    if (remainingPoints > 0)
                    {
                        List<int> divisions = DistributeDivisions(subCurves, remainingPoints);
                        for (int i = 0; i < subCurves.Count; i++)
                        {
                            if (divisions[i] == 0)
                                continue;

                            //Sample points always includes start and end (which are removed later) and gives back 1 more point than divisions.
                            //To make sure point(s) on the middle of the are extracted, division is increased by 1, which should return 2 more points than divisions asked for.
                            int div = divisions[i] + 1;

                            List<Point> subPts = subCurves[i].SamplePoints(div);

                            //Remove points at start/end
                            subPts.RemoveAt(subPts.Count - 1);
                            subPts.RemoveAt(0);
                            pts.AddRange(subPts);
                        }

                    }
                    int count = pts.Count;
                    pts = pts.ISortAlongCurve(regionCurve);

                    //SortAlongCurve might add extra points, generally duplicate of start and end point for closed curves.
                    if (pts.Count == count + 1 && pts[0].Distance(pts[pts.Count - 1]) < Tolerance.Distance)
                    {
                        pts.RemoveAt(pts.Count - 1);
                    }
                    else if (pts.Count > count)
                    {
                        pts = pts.CullDuplicates();
                    }

                    result.AddRange(pts);
                }
                else
                {

                    if (curveDivs == 0)
                        continue;
                    if (curveDivs == 1)
                    {
                        result.Add(regionCurve.IPointAtParameter(0.5));
                    }
                    else
                    {
                        int divisions = curveDivs;
                        bool closed = regionCurve.IIsClosed();

                        if (!closed)
                            divisions--;

                        List<Point> pts = regionCurve.SamplePoints(divisions);

                        //Remove duplicate endpoint
                        if (closed)
                            pts.RemoveAt(pts.Count - 1);

                        result.AddRange(pts);
                    }
                }
            }

            return result;
        }

        /***************************************************/

        [Description("Returns Points along a straight line through the region curve. If the line is discontinuous because of openings or concave host region, Points will be distributed to the different segments based on their length.")]
        [Input("layout2D", "The layout object to query the points from.")]
        [Input("hostRegionCurves", "The region curves of the objects associated with the layout.")]
        [Input("openingCurves", "Optional opening curves in the region.")]
        [Output("points", "Point layout generated by the layout objects and region curve.")]
        public static List<Point> PointLayout(this LinearLayout layout2D, IEnumerable<ICurve> hostRegionCurves, IEnumerable<ICurve> openingCurves = null)
        {
            Point refPoint = ReferencePoint(hostRegionCurves, openingCurves, layout2D.ReferencePoint);
            BoundingBox bounds = Geometry.Query.Bounds(hostRegionCurves.Select(x => x.IBounds()).ToList());
            refPoint = refPoint + layout2D.Offset * AlignOffsetVector(Vector.ZAxis.CrossProduct(layout2D.Direction), refPoint, bounds.Centre());
            Line axis = new Line { Start = refPoint, End = refPoint + layout2D.Direction, Infinite = true };

            List<Line> distributionLines = IntersectionLines(hostRegionCurves, openingCurves, axis);

            if (distributionLines.Count == 0)
            {
                Engine.Base.Compute.RecordError("Count not find extents of distribution for the layout.");
                return new List<Point>();
            }

            return DivisionPoints(distributionLines, layout2D.NumberOfPoints);
        }

        /***************************************************/


        [Description("Returns Points along one or more straight lines through the region curves. If the line is discontinuous because of openings, a concave host region, or multiple host regions, Points will be distributed to the different segments based on their length.\n" +
                     "The method fits as many points as it can onto each Line along the Direction vector before populating the next.")]
        [Input("layout2D", "The layout object to query the points from.")]
        [Input("hostRegionCurves", "The region curves of the objects associated with the layout.")]
        [Input("openingCurves", "Optional opening curves in the region.")]
        [Output("points", "Point layout generated by the layout objects and region curve.")]
        public static List<Point> PointLayout(this MultiLinearLayout layout2D, IEnumerable<ICurve> hostRegionCurves, IEnumerable<ICurve> openingCurves = null)
        {
            Point refPoint = ReferencePoint(hostRegionCurves, openingCurves, layout2D.ReferencePoint);
            BoundingBox bounds = Geometry.Query.Bounds(hostRegionCurves.Select(x => x.IBounds()).ToList());
            Vector offsetDir = AlignOffsetVector(Vector.ZAxis.CrossProduct(layout2D.Direction), refPoint, bounds.Centre());
            refPoint = refPoint + layout2D.Offset * offsetDir;
            int remainingPoints = layout2D.NumberOfPoints;

            List<Point> result = new List<Point>();


            offsetDir = offsetDir.Normalise() * layout2D.PerpendicularMinimumSpacing;

            while (remainingPoints > 0)
            {
                Line axis = new Line { Start = refPoint, End = refPoint + layout2D.Direction, Infinite = true };
                List<Line> distributionLines = IntersectionLines(hostRegionCurves, openingCurves, axis, layout2D.ParallelMinimumSpacing);

                if (distributionLines.Count == 0)
                {
                    //No lines found and axis is within boundingbox of the curve, try next layer
                    if (axis.IsInRange(bounds) && offsetDir.SquareLength() != 0)
                    {
                        Engine.Base.Compute.RecordNote("Could not find distribution lines for one or more layer.");
                        refPoint += offsetDir;
                        continue;
                    }
                    else
                    {
                        //No more lines can be found
                        Engine.Base.Compute.RecordError("Could not generate distribution lines for the reinforcement. The number of points might not fit in the region curve. The resulting number of points might be different from the number requested.");
                        remainingPoints = 0;
                    }
                }

                int layerDivs = 0;
                for (int i = 0; i < distributionLines.Count; i++)
                {
                    int divs = (int)Math.Ceiling(distributionLines[i].ILength() / layout2D.ParallelMinimumSpacing);
                    layerDivs += divs;
                }

                layerDivs = Math.Min(layerDivs, remainingPoints);

                //SamplePoints adds extra point at start/ end.Hence subtracting count here to make sure correct number is extracted.
                result.AddRange(DivisionPoints(distributionLines, layerDivs));
                remainingPoints -= layerDivs;

                //Find next layer by moving the centre of the axis
                refPoint += offsetDir;
            }

            return result;
        }
        

        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        [Description("Queries the points from the Layout based on the layout and a set of region curves and opening curves associated with it.")]
        [Input("layout2D", "The layout object to query the points from.")]
        [Input("hostRegionCurves", "The region curves of the objects associated with the layout.")]
        [Input("openingCurves", "Optional opening curves in the region.")]
        [Output("points", "Point layout generated by the layout objects and region curve.")]
        public static List<Point> IPointLayout(this ILayout2D layout2D, IEnumerable<ICurve> hostRegionCurves, IEnumerable<ICurve> openingCurves = null)
        {
            return PointLayout(layout2D as dynamic, hostRegionCurves, openingCurves);
        }

        /***************************************************/
        /**** Private Methods - Fallback                ****/
        /***************************************************/

        private static List<Point> PointLayout(this ILayout2D layout2D, IEnumerable<ICurve> hostRegionCurves, IEnumerable<ICurve> openingCurves = null)
        {
            Base.Compute.RecordError("PointLayout for " + layout2D.GetType().Name + " is not implemented.");
            return new List<Point>();
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static List<int> DistributeDivisions(IEnumerable<ICurve> curves, int nbPoints)
        {
            List<double> lengths = curves.Select(x => x.ILength()).ToList();
            double fullLength = lengths.Sum();

            List<int> divs = new List<int>();
            List<double> spacings = new List<double>();

            //Check how many division points to extract from each curve, based on length ratio of the curve in relation to the length of all curves
            for (int i = 0; i < lengths.Count; i++)
            {
                int fullCurveDivs = (int)Math.Ceiling(lengths[i] / fullLength * nbPoints);

                //What is the current spacing of the points
                double currentSpacing = lengths[i] / Math.Max((double)(fullCurveDivs - 1), 1);

                divs.Add(fullCurveDivs);
                spacings.Add(currentSpacing);
            }

            int overshot = divs.Sum() - nbPoints;

            //While to many points, remove one from the curve with smallest spacing
            while (overshot > 0)
            {
                int minIndex = spacings.IndexOf(spacings.Min());
                divs[minIndex]--;
                spacings[minIndex] = double.MaxValue;
                overshot--;
            }

            return divs;

        }

        /***************************************************/

        private static List<Point> DivisionPoints(List<Line> lines, int nbPoints)
        {
            List<int> divs = DistributeDivisions(lines, nbPoints);

            List<Point> pts = new List<Point>();
            for (int i = 0; i < divs.Count; i++)
            {
                if (divs[i] == 0)
                    continue;
                else if (divs[i] == 1)
                    pts.Add(lines[i].PointAtParameter(0.5));
                else
                    pts.AddRange(lines[i].SamplePoints(divs[i] - 1));
            }

            return pts;

        }

        /***************************************************/

        private static Point ReferencePoint(IEnumerable<ICurve> hostRegionCurves, IEnumerable<ICurve> openingCurves, ReferencePoint referencePoint)
        {
            if (referencePoint == oM.Spatial.Layouts.ReferencePoint.Centroid)
            {
                return CentroidWithOpeings(hostRegionCurves.ToList(), openingCurves.ToList());
            }
            BoundingBox bounds = Geometry.Query.Bounds(hostRegionCurves.Select(x => x.IBounds()).ToList());

            switch (referencePoint)
            {
                case oM.Spatial.Layouts.ReferencePoint.BottomLeft:
                    return new Point { X = bounds.Min.X, Y = bounds.Min.Y };
                case oM.Spatial.Layouts.ReferencePoint.BottomCenter:
                    return new Point { X = (bounds.Min.X + bounds.Max.X) / 2, Y = bounds.Min.Y };
                case oM.Spatial.Layouts.ReferencePoint.BottomRight:
                    return new Point { X = bounds.Max.X, Y = bounds.Min.Y };
                case oM.Spatial.Layouts.ReferencePoint.MiddleLeft:
                    return new Point { X = bounds.Min.X, Y = bounds.Min.Y };
                case oM.Spatial.Layouts.ReferencePoint.MiddleCenter:
                    return bounds.Centre();
                case oM.Spatial.Layouts.ReferencePoint.MiddleRight:
                    return new Point { X = bounds.Max.X, Y = (bounds.Min.Y + bounds.Max.Y) / 2 };
                case oM.Spatial.Layouts.ReferencePoint.TopLeft:
                    return new Point { X = bounds.Min.X, Y = bounds.Max.Y };
                case oM.Spatial.Layouts.ReferencePoint.TopCenter:
                    return new Point { X = (bounds.Min.X + bounds.Max.X) / 2, Y = bounds.Max.Y };
                case oM.Spatial.Layouts.ReferencePoint.TopRight:
                    return new Point { X = bounds.Max.X, Y = bounds.Max.Y };
                default:
                    return CentroidWithOpeings(hostRegionCurves.ToList(), openingCurves.ToList());
            }
        }

        /***************************************************/

        private static Point CentroidWithOpeings(List<ICurve> hostRegionCurves, List<ICurve> openingCurves)
        {
            if (hostRegionCurves.Count == 1 && openingCurves == null || openingCurves.Count == 0)
                return Geometry.Query.ICentroid(hostRegionCurves[0]);
            else
            {

                double x = 0;
                double y = 0;
                double z = 0;
                double area = 0;
                List<PolyCurve> extents = Geometry.Compute.BooleanUnion(hostRegionCurves);

                foreach (ICurve c in extents)
                {
                    Point tmp = Geometry.Query.ICentroid(c);
                    double eArea = Geometry.Query.IArea(c);
                    x += tmp.X * eArea;
                    y += tmp.Y * eArea;
                    z += tmp.Z * eArea;
                    area += eArea;
                }

                List<PolyCurve> openings = Geometry.Compute.BooleanUnion(openingCurves);

                foreach (ICurve o in openings)
                {
                    Point oTmp = Geometry.Query.ICentroid(o);
                    double oArea = Geometry.Query.IArea(o);
                    x -= oTmp.X * oArea;
                    y -= oTmp.Y * oArea;
                    z -= oTmp.Z * oArea;
                    area -= oArea;
                }

                return new Point { X = x / area, Y = y / area, Z = z / area };
            }
        }

        /***************************************************/

        private static Vector AlignOffsetVector(Vector offsetVector, Point referencePoint, Point centre)
        {
            //Make the offset vector always point towards the centre when reference point is on the boundary
            Vector clone = offsetVector.ShallowClone();

            Vector refVector = centre - referencePoint;

            if (clone.Angle(refVector) > Math.PI / 2)
                clone = clone.Reverse();

            return clone;
        }

        /***************************************************/

        private static List<Line> IntersectionLines(this IEnumerable<ICurve> hostRegionCurves, IEnumerable<ICurve> openingCurves, Line axis, double minimumEndDistance = -1, double tolerance = Tolerance.Distance)
        {
            openingCurves = openingCurves ?? new List<ICurve>();
            List<Point> intPts = hostRegionCurves.SelectMany(x => x.ILineIntersections(axis, true)).ToList();
            intPts.AddRange(openingCurves.SelectMany(x => x.ILineIntersections(axis, true)));
            intPts = intPts.CullDuplicates(tolerance);
            intPts = intPts.SortCollinear(tolerance);

            List<Line> result = new List<Line>();
            for (int i = 0; i < intPts.Count - 1; i++)
            {
                List<Point> midPt = new List<Point> { intPts.Skip(i).Take(2).Average() };
                if (hostRegionCurves.Any(c => c.IIsContaining(midPt, true, tolerance)) &&
                    !openingCurves.Any(x => x.IIsContaining(midPt, false, tolerance)))
                    result.Add(new Line { Start = intPts[i], End = intPts[i + 1] });
            }

            //Ensure a continous line is merged into one.
            result = Engine.Geometry.Compute.Join(result).Select(x => new Line { Start = x.IStartPoint(), End = x.IEndPoint(), Infinite = false }).ToList();

            //Check if lines are too close together
            if (minimumEndDistance > 0 && result.Count > 1)
            {
                Vector tan = (axis.End - axis.Start).Normalise();
                for (int i = 0; i < result.Count -1; i++)
                {
                    double dist = result[i].End.Distance(result[i + 1].Start);
                    if (dist < minimumEndDistance)
                    {
                        double diff = minimumEndDistance - dist;
                        //If half the difference can be fit on both adjoining lines, then separate them by this distance
                        if (result[i].Length() > diff / 2 && result[i + 1].Length() > diff / 2)
                        {
                            result[i].End -= tan * diff / 2;
                            result[i + 1].Start += tan * diff / 2;
                        }
                        else
                        {
                            //For now, if the above does not work, leave a warning and continue.
                            //TODO: make this work for these cases as well.
                            Engine.Base.Compute.RecordWarning("The ends of some distribution lines are closer together than the minimum spacing, but could not be separated automatically. Please check the result of the PointLayout distribution.");
                        }
                    }
                }
            }

            return result;
        }

        /***************************************************/
    }
}


