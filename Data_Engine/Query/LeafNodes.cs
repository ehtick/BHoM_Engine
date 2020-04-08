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

using System.Collections.Generic;
using BH.oM.Data.Collections;
using System.Linq;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Data
{
    public static partial class Query
    {
        /***************************************************/
        /****           Public Methods                  ****/
        /***************************************************/
        [Description("Extracts a list of leaf nodes from a graph, leaf nodes have no incoming links")]
        [Input("graph", "The graph to extract the leaf nodes from")]
        public static List<GraphNode<T>> LeafNodes<T>(this Graph<T> graph, GraphNode<T> startNode)
        {
            Dictionary<GraphNode<T>, int> depthDictionary = graph.DepthDictionary(startNode);
            List<GraphNode<T>> leafnodes = new List<GraphNode<T>>();
            foreach (GraphNode<T> node in graph.Nodes)
            {
                List<GraphNode<T>> neighbours = Neighbours(graph, node, true);
                //its a leaf if no neighbour is deeper in the graph
                int nodeDepth = depthDictionary[node];
                bool leaf = true;
                foreach (GraphNode<T> n in neighbours)
                    if (depthDictionary[n] > nodeDepth)
                        leaf = false;
                if(leaf) leafnodes.Add(node);
            }
            return leafnodes;
        }
    }
}
