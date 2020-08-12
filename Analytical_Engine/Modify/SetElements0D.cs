﻿/*
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

using BH.oM.Dimensional;
using BH.oM.Geometry;
using BH.oM.Analytical.Elements;
using System.Collections.Generic;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;
using BH.Engine.Base;

namespace BH.Engine.Structure
{
    public static partial class Modify
    {
        /******************************************/
        /****            IElement1D            ****/
        /******************************************/

        [Description("Sets the IElement0Ds of the ILink, i.e. its two end Nodes or Points. Method required for IElement1Ds.")]
        [Input("bar", "The Bar to set the IElement0Ds to.")]
        [Input("newElements0D", "The new IElement0Ds of the ILink. Should be a list of length two, containing exactly two analytical Nodes mathcing the type of the ILink or Geometrical Points. \n" +
                                "Points will assigin default end properties to the ILink.")]
        [Output("bar", "The ILink with updated Nodes.")]
        public static ILink<TNode> SetElements0D<TNode>(this ILink<TNode> link, List<IElement0D> newElements0D)
            where TNode : class, INode, new()
        {
            if (newElements0D.Count != 2)
            {
                Reflection.Compute.RecordError("A ILink is defined by 2 nodes.");
                return null;
            }

            ILink<TNode> clone = link.DeepClone();

            // Default the ILink end if the input is an Point
            if (newElements0D[0] is Point)
            {
                clone.StartNode = new TNode() { Position = newElements0D[0] as Point };
            }
            else
                clone.StartNode = newElements0D[0] as TNode;

            // Default the ILink end if the input is an Point
            if (newElements0D[1] is Point)
            {
                clone.EndNode = new TNode() { Position = newElements0D[1] as Point };
            }
            else
                clone.EndNode = newElements0D[1] as TNode;

            return clone;
        }

        /******************************************/
    }
}

