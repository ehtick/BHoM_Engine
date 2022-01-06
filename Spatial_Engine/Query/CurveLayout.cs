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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Spatial.Layouts;
using BH.Engine.Geometry;

namespace BH.Engine.Spatial
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Queries the curves from the Layout based on the layout and a set of region curves and opening curves associated with it.")]
        [Input("offsetCurveLayout", "The layout object to query the curves from.")]
        [Input("hostRegionCurves", "The region curves of the objects associated with the layout. Unused for Explicit layouts.")]
        [Input("openingCurves", "Optional opening curves in the region. Unused for ExplicitLayout.")]
        [Output("curves", "The Curves stored in the ExplicitLayout object.")]
        public static List<ICurve> CurveLayout(this OffsetCurveLayout offsetCurveLayout, IEnumerable<ICurve> hostRegionCurves, IEnumerable<ICurve> openingCurves = null, double minOffset = 0)
        {
            List<ICurve> curveLayout = new List<ICurve>();

            double offset = Math.Max(offsetCurveLayout.Offset, minOffset);

            foreach (ICurve c in hostRegionCurves)
                curveLayout.Add(c.IOffset(-offset));

            foreach (ICurve c in openingCurves)
                curveLayout.Add(c.IOffset(offset));

            return curveLayout;
        }

        /***************************************************/

        [Description("Returns the curves in the layout. For ExplicitLayouts, the host geometry will be ignored.")]
        [Input("explicitCurveLayout", "The layout object to query the curves from.")]
        [Input("hostRegionCurves", "The region curves of the objects associated with the layout. Unused for Explicit layouts.")]
        [Input("openingCurves", "Optional opening curves in the region. Unused for ExplicitLayout.")]
        [Output("curves", "The Curves stored in the ExplicitLayout object.")]
        public static List<ICurve> CurveLayout(this ExplicitCurveLayout explicitCurveLayout, IEnumerable<ICurve> hostRegionCurves, IEnumerable<ICurve> openingCurves = null, double minOffset = 0)
        {
            return explicitCurveLayout.Curves.ToList();
        }

        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        [Description("Queries the curves from the Layout based on the layout and a set of region curves and opening curves associated with it.")]
        [Input("curveLayout", "The layout object to query the curves from.")]
        [Input("hostRegionCurves", "The region curves of the objects associated with the layout.")]
        [Input("openingCurves", "Optional opening curves in the region.")]
        [Output("curves", "Curve layout generated by the layout objects and region curve.")]
        public static List<ICurve> ICurveLayout(this ICurveLayout curveLayout, IEnumerable<ICurve> hostRegionCurves, IEnumerable<ICurve> openingCurves = null, double minOffset = 0)
        {
            return CurveLayout(curveLayout as dynamic, hostRegionCurves, openingCurves, minOffset);
        }
    }
}


