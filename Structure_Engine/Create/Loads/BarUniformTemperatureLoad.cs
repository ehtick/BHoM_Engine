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
using BH.oM.Structure.Loads;
using BH.oM.Base;
using BH.oM.Structure.Elements;
using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Structure
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates an uniform temprature load to be applied to Bars.")]
        [InputFromProperty("loadcase")]
        [InputFromProperty("temperatureChange")]
        [Input("objects", "The collection of Bars the load should be applied to.")]
        [InputFromProperty("axis")]
        [InputFromProperty("projected")]
        [Input("name", "The name of the created load.")]
        [Output("barUniformTempLoad", "The created BarUniformTemperatureLoad.")]
        public static BarUniformTemperatureLoad BarUniformTemperatureLoad(Loadcase loadcase, double temperatureChange, IEnumerable<Bar> objects, LoadAxis axis = LoadAxis.Global, bool projected = false, string name = "")
        {
            BHoMGroup<Bar> group = new BHoMGroup<Bar>();
            if (objects == null)
                group = null;
            else
                group.Elements = objects.ToList();

            return loadcase.IsNull() ? null : new BarUniformTemperatureLoad
            {
                Loadcase = loadcase,
                TemperatureChange = temperatureChange,
                Objects = group,
                Axis = axis,
                Projected = projected,
                Name = name
            };
        }

        /***************************************************/
        /**** Public Methods - Deprecated               ****/
        /***************************************************/

        [Deprecated("3.1", "Replaced with autogenerated Property assignement method.")]
        public static BarUniformTemperatureLoad BarUniformTemperatureLoad(Loadcase loadcase, double temperatureChange, BHoMGroup<Bar> group, LoadAxis axis = LoadAxis.Global, bool projected = false, string name = "")
        {
            return new BarUniformTemperatureLoad
            {
                Loadcase = loadcase,
                TemperatureChange = temperatureChange,
                Objects = group,
                Axis = axis,
                Projected = projected,
                Name = name
            };
        }

        /***************************************************/

    }
}


