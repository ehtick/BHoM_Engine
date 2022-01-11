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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Physical.Constructions;

using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Engine.Physical
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        [Description("Returns the thickness of a generic construction")]
        [Input("construction", "A generic Construction object")]
        [Output("thickness", "The total thickness of the generic construction as a sum of all its layers")]
        public static double IThickness(this IConstruction construction)
        {
            return Thickness(construction as dynamic);
        }

        [Description("Returns the thickness of a construction")]
        [Input("construction", "A Construction object")]
        [Output("thickness", "The total thickness of the construction as a sum of all its layers")]
        public static double Thickness(this Construction construction)
        {
            if(construction == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot query the thickness of a null construction.");
                return 0;
            }

            double thickness = 0;
            foreach (Layer l in construction.Layers)
                thickness += l.Thickness;

            return thickness;
        }

        private static double Thickness(this object construction)
        {
            return 0; //Fallback method
        }
    }
}


