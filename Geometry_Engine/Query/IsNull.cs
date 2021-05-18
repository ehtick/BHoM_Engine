﻿/*
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

using BH.Engine.Geometry;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using BH.oM.Quantities.Attributes;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BH.Engine.Geometry
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Checks if an Geometry is null and outputs relevant error message.")]
        [Input("geometry", "The Geometry to test for null.")]
        [Input("methodName", "The name of the method to reference in the error message.")]
        [Input("errorOverride", "Optional error message to override the default error message. Only the contents of this string will be returned as an error.")]
        [Output("isNull", "True if the Geometry is null.")]
        public static bool IsNull(this IGeometry geometry, string methodName = "", string errorOverride = "")
        {
            if (geometry == null)
            {
                //If the methodName is not provided, use StackTrace to get it, if the method was called indepedently use "Method".
                if (!string.IsNullOrEmpty(errorOverride))
                {
                    Reflection.Compute.RecordError(errorOverride);
                }
                else
                {
                    if (string.IsNullOrEmpty(methodName))
                    {
                        StackTrace st = new StackTrace();
                        if (st.FrameCount > 0)
                        {
                            methodName = st.GetFrame(1).GetMethod().Name;
                            methodName.Substring(methodName.IndexOf("<") + 1, methodName.IndexOf("<") + 1 - methodName.IndexOf(">"));
                        }
                        else
                            methodName = "Method";
                    }
                    Reflection.Compute.RecordError($"Cannot evaluate {methodName} because the Geometry failed a null check.");
                }

                return true;
            }

            return false;
        }

    }
}
