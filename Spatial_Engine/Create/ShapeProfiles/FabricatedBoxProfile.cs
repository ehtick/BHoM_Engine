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

using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using BH.oM.Spatial.ShapeProfiles;
using BH.oM.Geometry;
using System;
using BH.Engine.Reflection;
using BH.oM.Reflection.Attributes;
using BH.Engine.Geometry;
using System.ComponentModel;

namespace BH.Engine.Spatial
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates a rectangular hollow profile based on input dimensions. Method generates edgecurves based on the inputs.")]
        [InputFromProperty("height")]
        [InputFromProperty("width")]
        [InputFromProperty("webThickness")]
        [InputFromProperty("topFlangeThickness")]
        [InputFromProperty("botFlangeThickness")]
        [InputFromProperty("weldSize")]
        [Output("fabBox", "The created BoxProfile.")]
        public static FabricatedBoxProfile FabricatedBoxProfile(double height, double width, double webThickness, double topFlangeThickness, double botFlangeThickness, double weldSize = 0)
        {
            if (height < topFlangeThickness + botFlangeThickness + 2 * Math.Sqrt(2) * weldSize || height <= topFlangeThickness + botFlangeThickness)
            {
                InvalidRatioError("height", "topFlangeThickness, botFlangeThickness and weldSize");
                return null;
            }

            if (width < webThickness * 2 + 2 * Math.Sqrt(2) * weldSize || width <= webThickness * 2)
            {
                InvalidRatioError("width", "webThickness and weldSize");
                return null;
            }

            if (height <= 0 || width <= 0 || webThickness <= 0 || topFlangeThickness <= 0 || botFlangeThickness <= 0 || weldSize < 0)
            {
                Engine.Reflection.Compute.RecordError("Input length less or equal to 0");
                return null;
            }

            List<ICurve> curves = FabricatedBoxProfileCurves(width, height, webThickness, topFlangeThickness, botFlangeThickness, weldSize);
            return new FabricatedBoxProfile(height, width, webThickness, topFlangeThickness, botFlangeThickness, weldSize, curves);
        }

        /***************************************************/

    }
}