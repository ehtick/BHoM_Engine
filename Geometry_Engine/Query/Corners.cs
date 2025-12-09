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

using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Geometry
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns a list of 8 corner points of a bounding box.")]
        [Input("bbox", "Bounding box to query.")]
        [Output("corners", "Corner points of the input bounding box.")]
        public static List<Point> Corners(this BoundingBox bbox)
        {
            return new List<Point>
            {
                bbox.Min,
                new Point{ X = bbox.Max.X, Y = bbox.Min.Y, Z = bbox.Min.Z },
                new Point{ X = bbox.Max.X, Y = bbox.Max.Y, Z = bbox.Min.Z },
                new Point{ X = bbox.Min.X, Y = bbox.Max.Y, Z = bbox.Min.Z },
                new Point{ X = bbox.Min.X, Y = bbox.Min.Y, Z = bbox.Max.Z },
                new Point{ X = bbox.Max.X, Y = bbox.Min.Y, Z = bbox.Max.Z },
                bbox.Max,
                new Point{ X = bbox.Min.X, Y = bbox.Max.Y, Z = bbox.Max.Z },
            };
        }

        /***************************************************/
    }
}