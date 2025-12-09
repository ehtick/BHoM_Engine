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
using System.ComponentModel;

namespace BH.Engine.Geometry
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [PreviousVersion("9.0", "BH.Engine.Tagging.Query.SignedDistanceAlongVector(BH.oM.Geometry.Point, BH.oM.Geometry.Point, BH.oM.Geometry.Vector)")]
        [Description("Calculates a signed distance between two points along a given direction.")]
        [Input("point1", "First point to measure the distance.")]
        [Input("point2", "Second point to measure the distance.")]
        [Input("vector", "Vector, along which the distance will be measured.")]
        [Output("distance", "Signed distance between two points along the input direction vector.")]
        public static double SignedDistanceAlongVector(this Point point1, Point point2, Vector vector)
        {
            Vector pointVector = point2 - point1;
            return pointVector.DotProduct(vector.Normalise());
        }

        /***************************************************/
    }
}