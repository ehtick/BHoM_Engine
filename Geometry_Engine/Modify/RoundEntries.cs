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
using System;
using System.ComponentModel;

namespace BH.Engine.Geometry
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Rounds all entries in a TransformMatrix to the specified number of decimal places.")]
        [Input("matrix", "The TransformMatrix whose entries should be rounded.")]
        [Input("decimalPlaces", "The number of decimal places to round to.")]
        [Output("matrix", "A new TransformMatrix with all entries rounded to the specified decimal places.")]
        public static TransformMatrix RoundEntries(this TransformMatrix matrix, int decimalPlaces)
        {
            double[,] rounded = new double[4, 4];
            for (int m = 0; m < matrix.Matrix.GetLength(0); m++)
            {
                for (int n = 0; n < matrix.Matrix.GetLength(1); n++)
                {
                    rounded[m, n] = Math.Round(matrix.Matrix[m, n], decimalPlaces);
                }
            }

            return new TransformMatrix { Matrix = rounded };
        }

        /***************************************************/
    }
}
