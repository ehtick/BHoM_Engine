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

using BH.Engine.Base;
using BH.oM.Graphics;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Graphics
{
    public static partial class Modify
    {
        /***************************************************/
        /****           Public Methods                  ****/
        /***************************************************/

        [Description("Sets the colour in the middle of a gradient to 0 relative to a pair of boundary values.")]
        [Input("gradient", "The gradient to modify.")]
        [Input("from", "The lower bound that the gradient will be used with.")]
        [Input("to", "The higher bound that the gradient will be used with.")]
        [Output("gradient", "A gradient with its middle colour set to 0 relative to the pair of boundary values.")]
        [PreviousVersion("4.2", "BH.Engine.Graphics.Query.CenterGradientAsymmetric(BH.oM.Graphics.Gradient, System.Double, System.Double)")]
        public static Gradient CenterGradientAsymmetric(this Gradient gradient, double from, double to)
        {
            Gradient result = gradient.ShallowClone();

            // Add a marker to avoid issues when deleting and transforming
            if (!result.Markers.ContainsKey((decimal)0.5))
                result.Markers.Add((decimal)0.5, result.Color(0.5));

            if (to <= 0)
            {
                // Scale marker positions to span 0 to 2 and delete those above 1
                result = Graphics.Create.Gradient(result.Markers.Where(x => x.Key <= (decimal)0.5).Select(x => x.Value),
                                                    result.Markers.Keys.Select(x => x * 2).Where(x => x <= 1));
            }
            else if (from >= 0)
            {
                // Scale marker positions to span -1 to 1 and delete those below 0
                result = Graphics.Create.Gradient(result.Markers.Where(x => x.Key >= (decimal)0.5).Select(x => x.Value),
                                                    result.Markers.Keys.Select(x => (x - 1) * 2 + 1).Where(x => x >= 0));
            }
            else
            {
                // found = the relative position of zero on the gradient
                decimal found = (decimal)(-from / (to - from));
                // scale marker positions below 'found' from: 0 to 0.5 => 0 to found
                // scale marker positions above 'found' from: 0.5 to 1 => found to 1
                result = Graphics.Create.Gradient(result.Markers.Values,
                                                    result.Markers.Keys.Select(x => x > found ?
                                                                                        (x - 1) * 2 * (1 - found) + 1 :
                                                                                         x * 2 * found));
            }

            return result;
        }

        /***************************************************/
    }
}