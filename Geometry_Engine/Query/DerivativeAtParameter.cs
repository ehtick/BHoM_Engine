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
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets the vector which is the k'th derivative of the curve at the point of t, where t is a normalised parameter.")]
        [Input("curve", "Curve to evaluate.")]
        [Input("t", "Parameter to evaluate at. Should be between 0 and 1. For values outside the range, the closest value will be used.")]
        [Input("k", "Degree of the derivation.")]
        [Output("Vector which is the k'th derivative of the curve at the point of t.")]
        public static Vector DerivativeAtParameter(this NurbsCurve curve, double t, int k, bool normalisedParameter = true)
        {
            return curve.DerivativesAtParameter(k + 1, t, normalisedParameter)[k];
        }

        /***************************************************/

        [Description("Gets the vector which is the k'th derivative of the surface at the point of u, v, where u and v are normalised parameters.")]
        [Input("surface", "Surface to evaluate.")]
        [Input("u", "Parameter to evaluate at. Should be between 0 and 1. For values outside the range, the closest value will be used.")]
        [Input("v", "Parameter to evaluate at. Should be between 0 and 1. For values outside the range, the closest value will be used.")]
        [Input("k", "Degree of derivative for u.")]
        [Input("l", "Degree of derivative for v.")]
        [Output("Vector which is the k'th derivative of the surface at the point of t.")]
        public static Vector DerivativeAtParameter(this NurbsSurface surface, double u, double v, int k, int l, bool normalisedParameter = true)
        {
            return surface.DerivativesAtParameter(k + l, u, v, normalisedParameter)[k][l];
        }

        /***************************************************/

    }
}