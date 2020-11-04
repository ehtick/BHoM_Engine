﻿using BH.Engine.Geometry;
using BH.oM.Analytical.Elements;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Analytical
{
    public static partial class Query 
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns arrow geometry used to represent a Relation.")]
        [Input("IRelation", "The IRelation to represent with geometry.")]
        [Input("headLength", "Optional length of the arrow head. Default is 1.")]
        [Input("baseWidth", "Optional width of the arrow head. Default is 0.3.")]
        [Input("closed", "Optional boolean to set if the arrow head is closed or open. Default is false.")]
        [Output("composite geometry", "CompositeGeometry of the arrow.")]
        public static CompositeGeometry RelationArrow(this IRelation relation, double headLength = 1, double baseWidth = 0.3, bool closed = false)
        {
            List<IGeometry> geometries = new List<IGeometry>();

            if (headLength == 0)
            {
                Reflection.Compute.RecordWarning("Unable to create arrow with headLength of zero.");
                return BH.Engine.Geometry.Create.CompositeGeometry(geometries);
            }
                

            ICurve curve = relation.Curve;
            geometries.Add(curve);
            if (curve is NurbsCurve)
            {
                NurbsCurve nurbsCurve = curve as NurbsCurve;
                curve = Engine.Geometry.Create.Polyline(nurbsCurve.ControlPoints);
            }
            List<ICurve> nonNurbs = new List<ICurve>();

            foreach (ICurve sub in curve.ISubParts())
            {
                if (sub is NurbsCurve)
                {
                    NurbsCurve nurbsCurve = sub as NurbsCurve;
                    nonNurbs.Add(Engine.Geometry.Create.Polyline(nurbsCurve.ControlPoints));
                }
                else
                {
                    nonNurbs.Add(sub);
                }
            }

            foreach (ICurve c in nonNurbs)
            {
                double length = c.ILength() - headLength;
                geometries.Add(ArrowHead(c.IPointAtLength(length), c.IEndPoint(), baseWidth, closed));
            }
                
            return BH.Engine.Geometry.Create.CompositeGeometry(geometries);
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Polyline ArrowHead(Point start, Point end, double width = 0.3, bool closed = false)
        {
            Vector back = start - end;
            Vector perp = back.CrossProduct(Vector.ZAxis);
            if (perp.Length() == 0)
                perp = back.CrossProduct(Vector.YAxis);
            perp = perp.Normalise();

            perp = perp * width;

            Point p1 = end + (back + perp);
            Point p2 = end + (back - perp);

            Polyline head = new Polyline();
            head.ControlPoints.Add(p1);
            head.ControlPoints.Add(end);
            head.ControlPoints.Add(p2);
            
            if (closed)
                head.ControlPoints.Add(p1);

            return head;
        }

        /***************************************************/
    }
}
