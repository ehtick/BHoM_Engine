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

using System;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;

namespace BH.Engine.Geometry
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Line curve1, Line curve2, double tolerance = Tolerance.Distance)
        {
            if (curve1.CurveIntersections(curve2).Count > 0)
                return new BH.oM.Reflection.Output < Point,Point >  { Item1=curve1.CurveIntersections(curve2)[0],Item2=curve1.CurveIntersections(curve2)[0] };
            Point min1 = new Point();
            Point min2 = new Point();
            min1 = curve1.Start;
            min2 = curve2.Start;
            if(curve1.End.Distance(curve2)<min1.Distance(curve2))
            {
                min1 = curve1.End;
            }
            if(curve2.End.Distance(curve1)<min2.Distance(curve1))
            {
                min2 = curve2.End;
            }
            if (min2.Distance(curve1) < min1.Distance(curve2))
            {
                min1 = curve1.ClosestPoint(min2);
            }
            else
                min2 = curve2.ClosestPoint(min1);
            if (curve1.IsCoplanar(curve2))
            {
                return new BH.oM.Reflection.Output<Point, Point> { Item1=min1, Item2=min2 };
            }

            double[] t = curve1.SkewLineProximity(curve2);
            double t1 = Math.Max(Math.Min(t[0], 1), 0);
            double t2 = Math.Max(Math.Min(t[1], 1), 0);
            Vector e1 = curve1.End - curve1.Start;
            Vector e2 = curve2.End - curve2.Start;
            if ((curve1.Start + e1 * t1).Distance(curve2.Start + e2 * t2) < min1.Distance(min2))
                return new BH.oM.Reflection.Output < Point,Point >  { Item1=curve1.Start + e1 * t1, Item2=curve2.Start + e2 * t2 };
            else
                return new BH.oM.Reflection.Output<Point, Point> { Item1=min1, Item2=min2 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Line curve1, Arc curve2, double tolerance = Tolerance.Distance)
        {
            if (curve1.CurveIntersections(curve2).Count > 0)
                return new BH.oM.Reflection.Output<Point, Point> { Item1 = curve1.CurveIntersections(curve2)[0], Item2 = curve1.CurveIntersections(curve2)[0] };
            List<Point> plnPts = new List<Point>();
            plnPts.Add(curve1.Start);
            plnPts.Add(curve1.End);
            plnPts.Add(curve2.Centre());
            Point tmp = new Point();
            Plane ftPln = plnPts.FitPlane();
            if (ftPln != null)
            {
                List<Point> intersecting = curve2.PlaneIntersections(ftPln);
                if (intersecting.Count > 1)
                {
                    if (intersecting[0].Distance(curve1) < intersecting[1].Distance(curve1))
                        tmp = intersecting[0];
                    else
                        tmp = intersecting[1];
                }
                else if (intersecting.Count == 1)
                    tmp = intersecting[0];
                else
                    tmp = curve2.StartPoint();
            }
            else
                tmp = curve2.StartPoint();
            if (curve2.StartPoint().Distance(curve1) < tmp.Distance(curve1))
                tmp = curve2.StartPoint();
            if (curve2.EndPoint().Distance(curve1) < tmp.Distance(curve1))
                tmp = curve2.EndPoint();
            Line prLn = curve1.Project(curve2.FitPlane());
            List<Point> lnInt = prLn.CurveIntersections(curve2);
            if (lnInt.Count>0)
            {
                if (lnInt.Count > 1) 
                {
                    if (lnInt[0].Distance(curve1) > lnInt[0].Distance(curve1))
                        lnInt[0] = lnInt[1];
                }
                if (lnInt[0].Distance(curve1) < tmp.Distance(curve1))
                    tmp = lnInt[0];
            }
            BH.oM.Reflection.Output<Point, Point> result = new BH.oM.Reflection.Output<Point, Point>();
            result.Item1 = curve1.ClosestPoint(tmp);
            result.Item2 = curve2.ClosestPoint(result.Item1);
            result.Item1 = curve1.ClosestPoint(result.Item2);
            BH.oM.Reflection.Output<Point, Point> oldfinal = new BH.oM.Reflection.Output<Point, Point>();

            BH.oM.Reflection.Output<Point, Point> result2 = new BH.oM.Reflection.Output<Point, Point>();
            if (curve1.Start.Distance(curve2) < curve1.End.Distance(curve2))
                result2.Item1 = curve1.Start;
            else
                result2.Item1 = curve1.End;
            result2.Item2 = curve2.ClosestPoint(result2.Item1);
            result2.Item1 = curve1.ClosestPoint(result2.Item2); 
            BH.oM.Reflection.Output<Point, Point> final = new BH.oM.Reflection.Output<Point, Point>();
            if (result.Item1.Distance(result.Item2) < result2.Item1.Distance(result2.Item2))
                final= result;
            else
                final= result2;
            do
            {
                oldfinal.Item1 = final.Item1.Clone();
                oldfinal.Item2 = final.Item2.Clone();
                final.Item2 = curve2.ClosestPoint(final.Item1);
                final.Item1 = curve1.ClosestPoint(final.Item2);
            }
            while (Math.Abs(oldfinal.Item1.Distance(oldfinal.Item2) - final.Item1.Distance(final.Item2)) > tolerance * tolerance);
            return final;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Line curve1, Circle curve2, double tolerance = Tolerance.Distance)
        {
            if (curve1.CurveIntersections(curve2).Count > 0)
                return new BH.oM.Reflection.Output<Point, Point> { Item1 = curve1.CurveIntersections(curve2)[0], Item2 = curve1.CurveIntersections(curve2)[0] };
            List<Point> plnPts = new List<Point>();
            plnPts.Add(curve1.Start);
            plnPts.Add(curve1.End);
            plnPts.Add(curve2.Centre);
            Point tmp = new Point();
            Plane ftPln = plnPts.FitPlane();
            if (ftPln != null)
            {
                List<Point> intersecting = curve2.PlaneIntersections(ftPln);

                if (intersecting[0].Distance(curve1) < intersecting[1].Distance(curve1))
                    tmp = intersecting[0];
                else
                    tmp = intersecting[1];
            }
            else
                tmp = curve1.ClosestPoint(curve2.Centre);
            Line prLn = curve1.Project(curve2.FitPlane());
            List<Point> lnInt = prLn.CurveIntersections(curve2);
            if (lnInt.Count > 0)
            {
                if (lnInt.Count > 1)
                {
                    if (lnInt[0].Distance(curve1) > lnInt[0].Distance(curve1))
                        lnInt[0] = lnInt[1];
                }
                if (lnInt[0].Distance(curve1) < tmp.Distance(curve1))
                    tmp = lnInt[0];
            }
            BH.oM.Reflection.Output<Point, Point> result = new BH.oM.Reflection.Output<Point, Point>();
            result.Item1 = curve1.ClosestPoint(tmp);
            result.Item2 = curve2.ClosestPoint(result.Item1);
            BH.oM.Reflection.Output<Point, Point> oldresult = new BH.oM.Reflection.Output<Point, Point>();
            do
            {
                oldresult.Item1 = result.Item1.Clone();
                oldresult.Item2 = result.Item2.Clone();
                result.Item1 = curve1.ClosestPoint(result.Item2);
                result.Item2 = curve2.ClosestPoint(result.Item1);
            } while (Math.Abs(oldresult.Item1.Distance(oldresult.Item2) - result.Item1.Distance(result.Item2)) > tolerance * tolerance);
            return result;
            //Point[] list = new Point[4];
            //for(double i=0;i<1;i+=0.25)
            //{
            //    list[(int)(4 * i)] = curve2.PointAtParameter(i);
            //}
            //Arc temp1 = Create.Arc(list[0], list[1], list[2]);
            //Arc temp2 = Create.Arc(list[2], list[3], list[0]);
            //BH.oM.Reflection.Output<Point, Point> result1 = curve1.CurveProximity(temp1);
            //BH.oM.Reflection.Output<Point, Point> result2 = curve1.CurveProximity(temp2);
            //if (result1.Item1.Distance(result1.Item2) < result2.Item1.Distance(result2.Item2))
            //    return result1;
            //else
            //    return result2;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Line curve1, PolyCurve curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(curve1);
            return new BH.oM.Reflection.Output<Point, Point> { Item1 = result.Item2, Item2 = result.Item1 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Line curve1, Polyline curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(curve1);
            return new BH.oM.Reflection.Output<Point, Point> { Item1 = result.Item2, Item2 = result.Item1 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Arc curve1, Line curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(curve1);
            return new BH.oM.Reflection.Output<Point, Point> { Item1 = result.Item2, Item2 = result.Item1 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Arc curve1, Arc curve2, double tolerance = Tolerance.Distance)
        {
            if (curve1.CurveIntersections(curve2).Count > 0)
                return new BH.oM.Reflection.Output<Point, Point> { Item1=curve1.CurveIntersections(curve2)[0], Item2=curve1.CurveIntersections(curve2)[1] };
            Point min1, min2;
            double distance1 = Math.Min(curve2.EndPoint().Distance(curve1), curve2.StartPoint().Distance(curve1));
            double distance2 = Math.Min(curve1.EndPoint().Distance(curve2), curve1.StartPoint().Distance(curve2));
            min1 = curve1.StartPoint();
            min2 = curve2.StartPoint();
            if (curve1.EndPoint().Distance(curve2) < min1.Distance(curve2))
                min1 = curve1.EndPoint();
            if (curve2.EndPoint().Distance(curve1) < min2.Distance(curve1))
                min2 = curve2.EndPoint();
            if (min1.Distance(curve2) < min2.Distance(curve1))
                min2 = curve2.ClosestPoint(min1);
            else
                min1 = curve1.ClosestPoint(min2);
            Point start1 = curve1.StartPoint();
            Point start2 = curve2.StartPoint();
            Point end1 = curve1.EndPoint();
            Point end2 = curve2.EndPoint();
            Arc tmp1 = curve1.Clone();
            Arc tmp2 = curve2.Clone();
            Point binSearch = new Point();
            while((start1-end1).Length()>tolerance)
            {
                binSearch = tmp1.PointAtParameter(0.5);
                if (start1.Distance(curve2) > end1.Distance(curve2))
                    start1 = binSearch;
                else
                    end1 = binSearch;
                tmp1=tmp1.Trim(start1, end1);
            }
            while ((start2 - end2).Length() > tolerance)
            {
                binSearch = tmp2.PointAtParameter(0.5);
                if (start2.Distance(curve1) > end2.Distance(curve1))
                    start2 = binSearch;
                else
                    end2 = binSearch;
                tmp2=tmp2.Trim(start2, end2);
            }
            BH.oM.Reflection.Output<Point, Point> result = new oM.Reflection.Output<Point, Point>();
            if (start1.Distance(curve2) < end1.Distance(curve2))
                result.Item1 = start1;
            else
                result.Item1 = end1;
            result.Item2 = curve2.ClosestPoint(result.Item1);
            if(start2.Distance(curve1)<result.Item2.Distance(result.Item1))
            {
                result.Item1 = start2;
                result.Item2 = curve1.ClosestPoint(start2);
            }
            if (end2.Distance(curve1) < result.Item2.Distance(result.Item1))
            {
                result.Item1 = end2;
                result.Item2 = curve1.ClosestPoint(end2);
            }
            if (result.Item1.Distance(result.Item2) > min1.Distance(min2))
                return new BH.oM.Reflection.Output<Point, Point> { Item1 = min1, Item2 = min2 };
            if (result.Item1.IsOnCurve(curve1))
                return result;
            return new BH.oM.Reflection.Output<Point, Point> { Item1 = result.Item2, Item2 = result.Item1 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Arc curve1, Circle curve2, double tolerance = Tolerance.Distance)
        {
            if (curve1.CurveIntersections(curve2).Count > 0)
                return new BH.oM.Reflection.Output<Point, Point> { Item1=curve1.CurveIntersections(curve2)[0], Item2=curve1.CurveIntersections(curve2)[1] };
            Point[] crclpts = new Point[4];
            for (double i = 0; i < 1; i += 0.25)
            {
                crclpts[(int)(4 * i)] = curve2.PointAtParameter(i);
            }
            Arc tmp = Create.Arc(crclpts[0], crclpts[1], crclpts[2]);
            Arc tmp2 = Create.Arc(crclpts[2], crclpts[3], crclpts[0]);
            BH.oM.Reflection.Output<Point, Point> result1 = curve1.CurveProximity(tmp);
            BH.oM.Reflection.Output<Point, Point> result2 = curve1.CurveProximity(tmp2);
            if (result1.Item1.Distance(result1.Item2) < result2.Item1.Distance(result2.Item2))
                return result1;
            else
                return result2;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Arc curve1, PolyCurve curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(curve1);
            return new BH.oM.Reflection.Output<Point, Point> { Item1 = result.Item2, Item2 = result.Item1 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Arc curve1, Polyline curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(curve1);
            return new BH.oM.Reflection.Output<Point, Point> { Item1 = result.Item2, Item2 = result.Item1 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Circle curve1, Line curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(curve1);
            return new BH.oM.Reflection.Output<Point, Point> { Item1 = result.Item2, Item2 = result.Item1 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Circle curve1, Arc curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(curve1);
            return new BH.oM.Reflection.Output<Point, Point> { Item1 = result.Item2, Item2 = result.Item1 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Circle curve1, Circle curve2, double tolerance = Tolerance.Distance)
        {
            if (curve1.CurveIntersections(curve2).Count > 0)
                return new BH.oM.Reflection.Output<Point, Point> { Item1=curve1.CurveIntersections(curve2)[0], Item2=curve1.CurveIntersections(curve2)[1] };
            BH.oM.Reflection.Output<Point, Point> result = new BH.oM.Reflection.Output<Point, Point>();
            if (Math.Abs(curve1.Normal.IsParallel(curve2.Normal))==1)
            {
                result.Item1 = curve1.PointAtParameter(0);
                result.Item2 = curve2.ClosestPoint(result.Item1);
                return result;
            }
            Plane ftPln1 = curve1.FitPlane();
            Plane ftPln2 = curve2.FitPlane();
            List<Point> intPts = new List<Point>();
            Point tmp = new Point();
            if ((intPts = curve1.PlaneIntersections(ftPln2)).Count!=0)
            {
                if (intPts.Count == 1)
                    tmp = intPts[0];
                else
                {
                    if (intPts[0].Distance(curve2) < intPts[1].Distance(curve2))
                        tmp = intPts[0];
                    else
                        tmp = intPts[1];
                }
            }
            else if ((intPts = curve2.PlaneIntersections(ftPln1)).Count != 0)
            {
                if (intPts.Count == 1)
                    tmp = intPts[0];
                else
                {
                    if (intPts[0].Distance(curve1) < intPts[1].Distance(curve1))
                        tmp = intPts[0];
                    else
                        tmp = intPts[1];
                }
            }
            else
            {
                Line intersect = Create.Line(curve1.Centre, curve2.Centre);
                Point tmp1 = intersect.CurveProximity(curve1).Item2;
                Point tmp2 = intersect.CurveProximity(curve1).Item2;
                if (tmp1.Distance(curve2) < tmp2.Distance(curve1))
                    tmp = tmp1;
                else
                    tmp = tmp2;
            }
            BH.oM.Reflection.Output<Point, Point> oldresult = new BH.oM.Reflection.Output<Point, Point>();
            if(tmp.IsOnCurve(curve1))
            {
                result.Item1 = tmp;
                result.Item2 = curve2.ClosestPoint(tmp);
            }
            else
            {
                result.Item2 = tmp;
                result.Item1 = curve1.ClosestPoint(tmp);
            }
            do
            {
                oldresult.Item1 = result.Item1.Clone();
                oldresult.Item2 = result.Item2.Clone();
                result.Item1 = curve2.ClosestPoint(result.Item2);
                result.Item2 = curve1.ClosestPoint(result.Item1);
            } while (Math.Abs(oldresult.Item2.Distance(oldresult.Item1) - result.Item2.Distance(result.Item1)) > tolerance*tolerance);
            return result;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Circle curve1, PolyCurve curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(curve1);
            return new BH.oM.Reflection.Output<Point, Point> { Item1 = result.Item2, Item2 = result.Item1 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Circle curve1, Polyline curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(curve1);
            return new BH.oM.Reflection.Output<Point, Point> { Item1 = result.Item2, Item2 = result.Item1 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this PolyCurve curve1, Line curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.ICurveProximity(curve1.Curves[0]);
            BH.oM.Reflection.Output<Point, Point> cp = new BH.oM.Reflection.Output<Point, Point>();
            for (int i = 1; i < curve1.Curves.Count; i++)
            {
                cp = curve1.Curves[i].ICurveProximity(curve2);
                if (cp.Item1.Distance(cp.Item2)< result.Item1.Distance(result.Item2))
                {
                    result = cp;
                }
            }
            return result;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this PolyCurve curve1, Arc curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.ICurveProximity(curve1.Curves[0]);
            BH.oM.Reflection.Output<Point, Point> cp = new BH.oM.Reflection.Output<Point, Point>();
            for (int i = 1; i < curve1.Curves.Count; i++)
            {
                cp = curve1.Curves[i].ICurveProximity(curve2);
                if (cp.Item1.Distance(cp.Item2) < result.Item1.Distance(result.Item2))
                {
                    result = cp;
                }
            }
            return result;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this PolyCurve curve1, Circle curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.ICurveProximity(curve1.Curves[0]);
            BH.oM.Reflection.Output<Point, Point> cp = new BH.oM.Reflection.Output<Point, Point>();
            for (int i = 1; i < curve1.Curves.Count; i++)
            {
                cp = curve1.Curves[i].ICurveProximity(curve2);
                if (cp.Item1.Distance(cp.Item2) < result.Item1.Distance(result.Item2))
                {
                    result = cp;
                }
            }
            return result;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this PolyCurve curve1, Polyline curve2, double tolerance = Tolerance.Distance)
        {
            List<Line> temp = new List<Line>();
            for (int i = 0; i < curve2.ControlPoints.Count - 1; i++)
                temp.Add(Create.Line(curve2.ControlPoints[i], curve2.ControlPoints[i + 1]));
            if (curve2.IsClosed())
                temp.Add(Create.Line(curve2.ControlPoints[curve2.ControlPoints.Count - 1], curve2.ControlPoints[0]));
            BH.oM.Reflection.Output<Point, Point> result = curve2.ICurveProximity(curve1.Curves[0]);
            BH.oM.Reflection.Output<Point, Point> cp = new BH.oM.Reflection.Output<Point, Point>();
            for (int i = 1; i < curve1.Curves.Count; i++)
            {
                cp = curve1.Curves[i].ICurveProximity(curve2);
                if (cp.Item1.Distance(cp.Item2) < result.Item1.Distance(result.Item2))
                {
                    result = cp;
                }
            }
            return result;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this PolyCurve curve1, PolyCurve curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.ICurveProximity(curve1.Curves[0]);
            BH.oM.Reflection.Output<Point, Point> cp = new BH.oM.Reflection.Output<Point, Point>();
            for (int i = 1; i < curve1.Curves.Count; i++)
            {
                cp = curve1.Curves[i].ICurveProximity(curve2);
                if (cp.Item1.Distance(cp.Item2) < result.Item1.Distance(result.Item2))
                {
                    result = cp;
                }
            }
            return result;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Polyline curve1, Line curve2, double tolerance = Tolerance.Distance)
        {
            List<Line> temp = new List<Line>();
            for (int i = 0; i < curve1.ControlPoints.Count - 1; i++)
                temp.Add(Create.Line(curve1.ControlPoints[i], curve1.ControlPoints[i + 1]));
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(temp[0]);
            BH.oM.Reflection.Output<Point, Point> cp = new BH.oM.Reflection.Output<Point, Point>();
            for (int i = 1; i < temp.Count; i++)
            {
                cp = curve2.CurveProximity(temp[i]);
                if (cp.Item1.Distance(cp.Item2)<result.Item1.Distance(result.Item2))
                {
                    result = cp;
                }
            }
            return result;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Polyline curve1, Arc curve2, double tolerance = Tolerance.Distance)
        {
            List<Line> temp = new List<Line>();
            for (int i = 0; i < curve1.ControlPoints.Count - 1; i++)
                temp.Add(Create.Line(curve1.ControlPoints[i], curve1.ControlPoints[i + 1]));
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(temp[0]);
            BH.oM.Reflection.Output<Point, Point> cp = new BH.oM.Reflection.Output<Point, Point>();
            for (int i = 1; i < temp.Count; i++)
            {
                cp = temp[i].CurveProximity(curve2);
                if (cp.Item1.Distance(cp.Item2) < result.Item1.Distance(result.Item2))
                {
                    result = cp;
                }
            }
            return result;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Polyline curve1, Circle curve2, double tolerance = Tolerance.Distance)
        {
            List<Line> temp = new List<Line>();
            for (int i = 0; i < curve1.ControlPoints.Count - 1; i++)
                temp.Add(Create.Line(curve1.ControlPoints[i], curve1.ControlPoints[i + 1]));
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(temp[0]);
            BH.oM.Reflection.Output<Point, Point> cp = new BH.oM.Reflection.Output<Point, Point>();
            for (int i = 1; i < temp.Count; i++)
            {
                cp = temp[i].CurveProximity(curve2);
                if (cp.Item1.Distance(cp.Item2) < result.Item1.Distance(result.Item2))
                {
                    result = cp;
                }
            }
            return result;
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Polyline curve1, PolyCurve curve2, double tolerance = Tolerance.Distance)
        {
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(curve1);
            return new BH.oM.Reflection.Output<Point, Point> { Item1 = result.Item2, Item2 = result.Item1 };
        }

        /***************************************************/

        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this Polyline curve1, Polyline curve2, double tolerance = Tolerance.Distance)
        {
            List<Line> temp = new List<Line>();
            for (int i = 0; i < curve1.ControlPoints.Count - 1; i++)
                temp.Add(Create.Line(curve1.ControlPoints[i], curve1.ControlPoints[i + 1]));
            BH.oM.Reflection.Output<Point, Point> result = curve2.CurveProximity(temp[0]);
            BH.oM.Reflection.Output<Point, Point> cp = new BH.oM.Reflection.Output<Point, Point>();
            for (int i = 1; i < temp.Count; i++)
            {
                cp = temp[i].CurveProximity(curve2);
                if (cp.Item1.Distance(cp.Item2) < result.Item1.Distance(result.Item2))
                {
                    result = cp;
                }
            }
            return result;
        }

        /***************************************************/

        [NotImplemented]
        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this ICurve curve1, Ellipse curve2, double tolerance = Tolerance.Distance)
        {
            throw new NotImplementedException();
        }

        /***************************************************/

        [NotImplemented]
        public static BH.oM.Reflection.Output<Point, Point> CurveProximity(this ICurve curve1, NurbsCurve curve2, double tolerance = Tolerance.Distance)
        {
            throw new NotImplementedException();
        }

        /***************************************************/
        /**** Public Methods - Interfaces               ****/
        /***************************************************/

        public static BH.oM.Reflection.Output<Point,Point> ICurveProximity(this ICurve curve1, ICurve curve2, double tolerance = Tolerance.Distance)
        {
            return CurveProximity(curve1 as dynamic, curve2 as dynamic);
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        internal static double[] SkewLineProximity(this Line line1, Line line2, double angleTolerance = Tolerance.Angle)
        {
            Vector v1 = line1.End - line1.Start;
            Vector v2 = line2.End - line2.Start;
            Vector v1N = v1.Normalise();
            Vector v2N = v2.Normalise();

            if (v1N == null || v2N == null || 1 - Math.Abs(v1N.DotProduct(v2N)) <= angleTolerance)
                return null;

            Point p1 = line1.Start;
            Point p2 = line2.Start;

            Vector cp = v1.CrossProduct(v2);
            Vector n1 = v1.CrossProduct(-cp);
            Vector n2 = v2.CrossProduct(cp);

            double t1 = (p2 - p1) * n2 / (v1 * n2);
            double t2 = (p1 - p2) * n1 / (v2 * n1);

            return new double[] { t1, t2 };
        }

        /***************************************************/
    }
}
