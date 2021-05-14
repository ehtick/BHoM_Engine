/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Spatial.ShapeProfiles;
using BH.oM.Reflection.Attributes;
using BH.Engine.Spatial;

namespace BH.Engine.Structure
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [NotImplemented]
        public static double WidthAt(this IProfile section, double y)
        {
            if (section.IsNull())
                return 0;

            IntegrationSlice slice = Engine.Geometry.Query.SliceAt(section.Edges, y, 1, oM.Geometry.Plane.XZ);
            //Slice slice = SliceAt(y, 1, Plane.XZ());// new Plane(Point.Origin, Vector.YAxis()));
            return slice.Length;
        }

        /***************************************************/

        [NotImplemented]
        public static double WidthAt(this IProfile section, double y, ref double[] range)
        {
            if (section.IsNull())
                return 0;

            IntegrationSlice slice = Engine.Geometry.Query.SliceAt(section.Edges, y, 1, oM.Geometry.Plane.XZ);
            //Slice slice = SliceAt(y, 1, Plane.XZ());
            range = slice.Placement;
            return slice.Length;
        }

        /***************************************************/
    }
}


