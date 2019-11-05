/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structure.Elements;
using BH.oM.Geometry;
using BH.Engine.Geometry;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Structure
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static double ShearAreaPolyline(this Polyline pLine)
        {
            // the Polyline should have the upper side along the x-axis and the rest of the lines should be defineble as a function of x apart for veritcal segments
            // The last LineSegment should be the upper one
            // use WetBlanketInterpertation()

            double Sy = 0;
            double ShearArea = 0;
            // Sy = 0
            List<Point> controllPoints = new List<Point>(pLine.ControlPoints);  // Should the formatting happen here or earlier??
            // foreach (linesegement) except for the upper one, must begin at the line after the upper one (on the left side)
            for (int i = 0; i < controllPoints.Count - 2; i++)
            {
                ShearArea += ShearAreaLine(controllPoints[i], controllPoints[i + 1], Sy);
                Sy += BH.Engine.Geometry.Compute.IIntegrateRegion(new Line() { Start = controllPoints[i], End = controllPoints[i + 1] }, 1);
                //Sy += BH.Engine.Geometry.Compute.IntSurfLine(controllPoints[i], controllPoints[i + 1], 1);
            }
            // ShearAreaLine()
            // Calculate Sy for the linesegment (by IntSurfLine()) and add to Sy +=

            return ShearArea;
        }

        /***************************************************/

        private static double ShearAreaLine(Point a, Point b, double s)
        {
            //TODO Should do some checks if these are good Tolerances


            // a.Y or b.Y can't be 0.    (due to some log expresssions, which comes from the expression being divided by the width (can't divide by zero))

            double axbx = a.X - b.X;
            if (Math.Abs(axbx) < Tolerance.MicroDistance)  // The solution is zero
                return 0;

            double byay = b.Y - a.Y;
            double ax2 = Math.Pow(a.X, 2);
            double ay2 = Math.Pow(a.Y, 2);

            if (Math.Abs(byay) < Tolerance.MicroDistance)  // The solution for a "constant" integral, i.e. horizontal line
            {
                //$$ \frac{(x-z)(10 y (x-z)^2 (3 x^2 y-2 s)-30 x y (x-z) (x^2 y-2 s)+15 (x^2 y-2 s)^2+3 y^2 (x-z)^4-15 x y^2 (x-z)^3)}{60 y} $$
                return (axbx) * (
                        10 * a.Y * Math.Pow(axbx, 2) * (3 * ax2 * a.Y - 2 * s)
                        - 30 * a.X * a.Y * axbx * (ax2 * a.Y - 2 * s)
                        + 15 * Math.Pow(ax2 * a.Y - 2 * s, 2)
                        + 3 * ay2 * Math.Pow(axbx, 4)
                        - 15 * a.X * ay2 * Math.Pow(axbx, 3)
                    ) / (60 * a.Y);
            }

            if (a.Y > -Tolerance.MicroDistance)  //Everithing should happen below the X-axis
            {
                a.Y = -Tolerance.MicroDistance;
                ay2 = Math.Pow(a.Y, 2);
                byay = b.Y - a.Y;
            }
            if (b.Y > -Tolerance.MicroDistance)  // and these should never be zero
            {
                b.Y = -Tolerance.MicroDistance;
                byay = b.Y - a.Y;
            }
            // if this change made (Math.Abs(byay) < Tolerance.MicroDistance) = true it is problem


            double ax3 = Math.Pow(a.X, 3);

            double bx2 = Math.Pow(b.X, 2);
            double bx3 = Math.Pow(b.X, 3);

            double ay3 = Math.Pow(a.Y, 3);

            double by2 = Math.Pow(b.Y, 2);
            double by3 = Math.Pow(b.Y, 3);

            double byay2 = Math.Pow(byay, 2);


            double A =
                    -(20 * Math.Pow(axbx, 2) * (24 * s * byay2 + a.Y * (-39 * by2 * ax2 + 6 * b.Y * a.X * a.Y * (14 * a.X - b.X) + ay2 * (-44 * ax2 + 4 * a.X * b.X + bx2))))
                    / (byay2);
            double B =
                    -(30 * axbx * (a.Y * (18 * by3 * ax3 + 3 * by2 * ax2 * a.Y * (5 * b.X - 23 * a.X) + 6 * b.Y * a.X * ay2 * (13 * ax2 - 3 * a.X * b.X - bx2) + ay3 * (-28 * ax3 + 6 * ax2 * b.X + 3 * a.X * bx2 + bx3)) - 12 * s * byay2 * (3 * b.Y * a.X + a.Y * (b.X - 4 * a.X))))
                    / (Math.Pow(byay, 3));
            double C =
                    (60 * a.Y * axbx * (a.Y * (2 * a.X + b.X) - 3 * b.Y * a.X) * (a.Y * (6 * by2 * ax2 - 3 * b.Y * a.X * a.Y * (3 * a.X + b.X) + ay2 * (4 * ax2 + a.X * b.X + bx2)) - 12 * s * byay2))
                    / (Math.Pow(byay, 4));
            double D =
                    (60 * Math.Log(b.Y / a.Y) * Math.Pow(a.Y * (3 * by2 * ax2 - 3 * b.Y * a.X * a.Y * (a.X + b.X) + ay2 * (ax2 + a.X * b.X + bx2)) - 6 * s * byay2, 2))
                    / (Math.Pow(byay, 5));
            double E =
                    40 * byay * Math.Pow(axbx, 4) - 48 * Math.Pow(axbx, 3) * (3 * b.Y * a.X - 5 * a.X * a.Y + 2 * a.Y * b.X)

                    + (15 * Math.Pow(axbx, 2) * (9 * by2 * ax2 - 6 * b.Y * a.X * a.Y * (8 * a.X - 5 * b.X) + ay2 * (40 * ax2 - 32 * a.X * b.X + bx2)))
                    / (byay);

            double factor = (axbx / 2160);

            return (A + B + C + D + E) * factor;
        }

        /***************************************************/
    }
}
