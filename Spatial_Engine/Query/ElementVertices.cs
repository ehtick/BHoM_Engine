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

using BH.Engine.Geometry;
using BH.oM.Base;
using BH.oM.Dimensional;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Spatial
{
    public static partial class Query
    {
        /******************************************/
        /****            IElement1D            ****/
        /******************************************/

        public static List<Point> ElementVertices(this IElement1D element1D)
        {
            ICurve curve = element1D.IGeometry();
            List<Point> vertices = curve.IDiscontinuityPoints();

            if (curve.IIsClosed())
                vertices.RemoveAt(vertices.Count - 1);

            return vertices;
        }


        /******************************************/
        /****            IElement2D            ****/
        /******************************************/

        public static List<Point> ElementVertices(this IElement2D element2D)
        {
            List<Point> result = new List<Point>();
            result.AddRange(element2D.OutlineCurve().ElementVertices());
            foreach (IElement2D e in element2D.IInternalElements2D())
            {
                result.AddRange(e.ElementVertices());
            }

            return result;
        }

        /******************************************/

        public static List<Point> ElementVertices(this IEnumerable<IElement> elements)
        {
            List<Point> result = new List<Point>();
            foreach (IElement element in elements)
            {
                result.AddRange(element.IElementVertices());
            }
            return result;
        }


        /******************************************/
        /**** Public Methods - Interfaces      ****/
        /******************************************/

        public static List<Point> IElementVertices(this IElement element)
        {
            return ElementVertices(element as dynamic);
        }

        /******************************************/

    }
}

