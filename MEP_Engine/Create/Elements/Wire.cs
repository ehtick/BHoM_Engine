﻿/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.MEP.Elements;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using BH.oM.MEP.SectionProperties;

namespace BH.Engine.MEP
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        [Description("Creates a Wire object. Material that flows through this Pipe can be established at the system level.")]
        [Input("line", "A line that determines the Wire's length and direction.")]
        [Input("flowRate", "The volume of fluid being conveyed by the Wire per second (m3/s).")]
        [Input("sectionProperty", "Provide a pipeSectionProperty to prepare a composite Wire section for accurate capacity and spatial quality.")]
        [Output("wire", "Wire object to work within an MEP systems.")]
        public static WireSegment Wire(Line line, double flowRate = 0, WireSectionProperty sectionProperty = null)
        {
            return new WireSegment
            {
                StartNode = (Node)line.Start,
                EndNode = (Node)line.End,
                SectionProperty = sectionProperty,
            };
        }
        /***************************************************/
    }
}