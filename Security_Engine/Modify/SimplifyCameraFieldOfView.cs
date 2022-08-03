﻿/*
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

using System.Collections.Generic;
using BH.oM.Dimensional;
using BH.oM.Geometry;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.Engine.Geometry;
using BH.oM.Security.Elements;
using System.Linq;
using BH.Engine.Base;
using System;

namespace BH.Engine.Security
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Returns the simplify PolyCurve generated by Camera Field of View method.")]
        [Input("cameraFieldOfView", "Camera Field of View PolyCyrve to simplify.")]
        [Input("radius", "Radius of the camera cone.")]
        [Input("distanceTolerance", "Distance tolerance for the method.")]
        [Input("angleTolerance", "Angular tolerance for the method.")]
        [Output("simplifyPolyCurve", "Simplify PolyCurve object.")]
        public static PolyCurve SimplifyCameraFieldOfView(this PolyCurve cameraFieldOfView, double radius, double distanceTolerance = Tolerance.Distance, double angleTolerance = Tolerance.Angle)
        {
            List<Line> cameraLines = new List<Line>();
            foreach (ICurve curve in cameraFieldOfView.SubParts())
            {
                if (curve is Line)
                    cameraLines.Add(curve as Line);
                else
                {
                    Line line = Geometry.Create.Line(curve.IStartPoint(), curve.IEndPoint());
                    cameraLines.Add(line);
                }
            }
            Polyline cameraPolyline = Geometry.Create.Polyline(cameraLines);
            cameraPolyline = cameraPolyline.Simplify(distanceTolerance, angleTolerance);

            Point cameraLocation = cameraFieldOfView.SubParts()[0].IStartPoint();
            PolyCurve simplifyPolyCurve = new PolyCurve();
            foreach (Line line in cameraPolyline.SubParts())
            {
                Point startPoint = line.Start;
                Point endPoint = line.End;

                if ((Math.Abs(startPoint.Distance(cameraLocation) - radius) < distanceTolerance) && (Math.Abs(endPoint.Distance(cameraLocation) - radius) < distanceTolerance))
                {
                    Arc newArc = Geometry.Create.ArcByCentre(cameraLocation, startPoint, endPoint, distanceTolerance);
                    simplifyPolyCurve.Curves.Add(newArc);
                }
                else
                {
                    simplifyPolyCurve.Curves.Add(line);
                }
            }

            return simplifyPolyCurve;
        }
    }
}