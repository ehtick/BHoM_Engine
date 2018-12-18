﻿using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;

namespace BH.Engine.Geometry
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public  Methods - Curves                  ****/
        /***************************************************/

        public static Polyline ToPolyline(this PolyCurve curve)
        {
            if (curve.Curves.Count == 0)
                return new Polyline();

            List<Point> controlPoints = new List<Point> { curve.Curves[0].IStartPoint() };
            foreach (ICurve c in curve.SubParts())
            {
                if (c is Line)
                    controlPoints.Add((c as Line).End);
                else
                    return null;
            }

            return new Polyline { ControlPoints = controlPoints };
        }

        /***************************************************/

        [NotImplemented]
        public static Polyline ToPolyline(NurbsCurve curve)
        {
            throw new NotImplementedException();
        }

        /***************************************************/

        [NotImplemented]
        public static Polyline ToPolyline(Arc curve)
        {
            throw new NotImplementedException();
        }

        /***************************************************/

        [NotImplemented]
        public static Polyline ToPolyline(Circle curve)
        {
            throw new NotImplementedException();
        }

        /***************************************************/

        public static Polyline ToPolyline(Line curve)
        {
            return new Polyline { ControlPoints = new List<Point> { curve.Start, curve.End } };
        }

        /***************************************************/

        public static Polyline ToPolyline(Polyline curve)
        {
            return curve;
        }


        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        public static Polyline IToPolyline(ICurve curve)
        {
            return ToPolyline(curve as dynamic);
        }

        /***************************************************/
    }
}