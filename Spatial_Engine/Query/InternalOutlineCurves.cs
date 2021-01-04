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

using BH.oM.Dimensional;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Spatial
{
    public static partial class Query
    {
        /******************************************/
        /****            IElement2D            ****/
        /******************************************/

        [PreviousVersion("3.2", "BH.Engine.Common.Query.IInternalOutlineCurves(BH.oM.Dimensional.IElement2D)")]
        [Description("Queries the IElement2Ds internal IElement2Ds outline curves.")]
        [Input("element2D", "The IElement2D to get the internal IElement2Ds outlinecurves from.")]
        [Output("outlines", "A list of the IElement2Ds internal elements outline curves. Each internal element returns a single outline curve.")]
        public static List<PolyCurve> InternalOutlineCurves(this IElement2D element2D)
        {
            return element2D.IInternalElements2D().Select(x => x.OutlineCurve()).ToList();
        }

        /******************************************/
    }
}


