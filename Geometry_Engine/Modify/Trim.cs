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

using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.Engine.Geometry
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static NurbsCurve Trim(this NurbsCurve curve, double t0, double t1, double tolerance = Tolerance.Distance)
        {
            if (curve == null)
            {
                BH.Engine.Base.Compute.RecordError("Can't trim a null curve.");
                return null;
            }

            double[] domain = curve.Domain();
            double tMin = domain[0];
            double tMax = domain[1];

            if (t0 < tMin - tolerance || t1 > tMax + tolerance)
            {
                BH.Engine.Base.Compute.RecordError("Trim parameters must be in the range [0, 1].");
                return null;
            }

            if (t0 - tMin <= tolerance)
                t0 = tMin;

            if (tMax - t1 <= tolerance)
                t1 = tMax;

            if (t0 >= t1 - tolerance)
            {
                BH.Engine.Base.Compute.RecordError("First trim parameter must be smaller than the second.");
                return null;
            }

            if (t0 == tMin && t1 == tMax)
                return curve;

            List<NurbsCurve> splitCurves = SplitAtParameters(curve, new List<double> { t0, t1 }, tolerance);

            bool isClosed = curve.IsClosed(tolerance);
            int splitCount = isClosed ? 2 : 3;
            int resultIndex = isClosed ? 0 : 1;

            if (splitCurves.Count == splitCount)
                return splitCurves[resultIndex];

            BH.Engine.Base.Compute.RecordError("Curve could not be trimmed due to an error in splitting.");
            return null;

            /***************************************************/
        }
    }
}
