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

using BH.oM.Dimensional;
using BH.oM.Geometry;
using BH.oM.Physical.Materials;
using BH.oM.Quantities.Attributes;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Matter
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets the unique Materials along with their volumes defining an object's make-up.")]
        [Input("elementM", "The element to get the MaterialTakeoff from.")]
        [Output("materialTakeoff", "The kind of matter the element is composed of and in which volumes.")]
        public static MaterialTakeoff IMaterialTakeoff(this IElementM elementM)
        {
            if (elementM == null)
            {
                Base.Compute.RecordError("Cannot query the MaterialTakeoff from a null element.");
                return null;
            }
            //IElementMs should implement one of the following:
            // -SolidVolume and MaterialComposition or
            // -MaterialTakeoff
            //This method first checks if the MaterialTakeoff method can be found and run, and if so uses it.
            //If not, it falls back to running the MaterialComposition and SolidVolume methods and gets the MaterialTakeoff from them.

            MaterialTakeoff matTakeoff;
            if (TryGetMaterialTakeoff(elementM, out matTakeoff))
                return matTakeoff;
            else
            {
                MaterialComposition matComp;
                double volume;
                if (TryGetMaterialComposition(elementM, out matComp) && TryGetSolidVolume(elementM, out volume))
                    return Create.MaterialTakeoff(matComp, volume);
                else
                {
                    Base.Compute.RecordError($"The provided element of type {elementM.GetType()} does not implement MaterialTakeoff or MaterialComposition and SolidVolume methods. The MaterialTakeoff could not be extracted.");
                    return null;
                }
            }
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        [Description("Tries running the MaterialTakeoff method on the IElementM. Returns true if the method successfully can be found.")]
        private static bool TryGetMaterialTakeoff(this IElementM elementM, out MaterialTakeoff materialTakeoff)
        {
            object result;
            bool success = Base.Compute.TryRunExtensionMethod(elementM, "MaterialTakeoff", out result);
            materialTakeoff = result as MaterialTakeoff;
            return success;
        }

        /***************************************************/
    }
}



