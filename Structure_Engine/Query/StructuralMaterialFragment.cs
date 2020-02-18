/*
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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Physical.Materials;

namespace BH.Engine.Structure
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets a structural material fragment, containing all relevant structural material data, from a physical Material class.")]
        [Input("material", "The physical Material to extract a structural material fragment from.")]
        [Output("strMat", "The structural material fragment.")]
        public static IMaterialFragment StructuralMaterialFragment(this Material material)
        {
            if (!material.IsValidStructural())
            {
                Engine.Reflection.Compute.RecordWarning("Material with name " + material.Name + " does not contain an structural material fragment");
                return null;
            }
            
            return material.Properties.Where(x => x is IMaterialFragment).Cast<IMaterialFragment>().FirstOrDefault();
        }

        /***************************************************/

    }
}

