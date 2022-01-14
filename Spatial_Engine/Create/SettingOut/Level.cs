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

using BH.oM.Spatial.SettingOut;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Engine.Spatial
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates a Level object based on a provided elevation.")]
        [PreviousVersion("5.1", "BH.Engine.Architecture.Elements.Create.Level(System.Double)")]
        [PreviousVersion("5.1", "BH.Engine.Geometry.Create.Level(System.Double)")]
        public static Level Level(double elevation)
        {
            return new Level
            {
                Elevation = elevation
            };
        }

        /***************************************************/

        [Description("Creates a Level object based on a provided elevation and name.")]
        [PreviousVersion("5.1", "BH.Engine.Architecture.Elements.Create.Level(System.Double, System.String)")]
        [PreviousVersion("5.1", "BH.Engine.Geometry.Create.Level(System.Double, System.String)")]
        public static Level Level(double elevation, string name)
        {
            return new Level
            {
                Elevation = elevation,
                Name = name,
            };
        }

        /***************************************************/
    }
}



