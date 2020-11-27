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

using BH.Engine.Base;
using BH.Engine.Spatial;
using BH.oM.Analytical.Elements;
using BH.oM.Analytical.Fragments;
using BH.oM.Base;
using BH.oM.Dimensional;
using BH.oM.Geometry;
using BH.oM.Graphics.Views;
using BH.oM.Graphics.Scales;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Graphics;
using BH.oM.Graphics.Data;
using BH.Engine.Reflection;
using BH.oM.Data.Collections;
using BH.oM.Graphics;

namespace BH.Engine.Analytical
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns a Graph projection defined by the projection provided.")]
        [Input("graph", "The Graph to query.")]
        [Input("projection", "The required IView.")]
        [Output("graph", "The projection of the original Graph.")]
        public static Graph IProjectGraph(this Graph graph, IProjection projection)
        {
            Graph graphProjected = ProjectGraph(graph, projection as dynamic);

            return graphProjected;
        }
        /***************************************************/

        [Description("Returns a Graph projection that contains only geometric entities. Spatial entities are those implementing IElement0D.")]
        [Input("graph", "The Graph to query.")]
        [Input("projection", "The SpatialView.")]
        [Output("graph", "The spatial Graph.")]
        private static Graph ProjectGraph(this Graph graph, GeometricProjection projection)
        {
            Graph geometricGraph = graph.DeepClone();
            foreach (IBHoMObject entity in geometricGraph.Entities.Values.ToList())
            {
                if (!typeof(IElement0D).IsAssignableFrom(entity.GetType()))
                    geometricGraph.RemoveEntity(entity.BHoM_Guid);
            }

            return geometricGraph;
        }

        /***************************************************/

        [Description("Returns a Graph projection that contains only spatial entities. Spatial entities are those implementing IElement0D.")]
        [Input("graph", "The Graph to query.")]
        [Input("projection", "The SpatialView.")]
        [Output("graph", "The spatial Graph.")]
        private static Graph ProjectGraph(this Graph graph, SpatialProjection projection)
        {
            Graph spatialGraph = graph.DeepClone();
            //set representation based on projection
            
            return spatialGraph;
        }

        /***************************************************/

        [Description("Returns a process projection of the Graph.")]
        [Input("graph", "The Graph to query.")]
        [Input("projection", "The ProcessView.")]
        [Output("graph", "The process Graph.")]

        private static Graph ProjectGraph(this Graph graph,  GraphicalProjection projection)
        {
            Graph processGraph = graph.DeepClone();
            //foreach (IBHoMObject entity in processGraph.Entities.Values.ToList())
            //{
            //    ProjectionFragment projectionFragment = entity.FindFragment<ProjectionFragment>();
            //    //if no process projection fragment, remove entity
            //    if (projectionFragment == null)
            //        processGraph.RemoveEntity(entity.BHoM_Guid);
            //}
            
                View(projection.View as DependencyChart, processGraph);
            
            return processGraph;
        }

        /***************************************************/
        /**** Fallback Methods                          ****/
        /***************************************************/

        private static Graph ProjectGraph(this Graph graph, IProjection projection)
        {
            Reflection.Compute.RecordError("IProjection provided does not have a corresponding GraphView method implemented.");
            return new Graph();
        }

        /***************************************************/
        /**** View Methods                              ****/
        /***************************************************/
        //private static void IView(this CustomView view, Graph graph)
        //{
        //    foreach(BH.oM.Graphics.Components.IComponent component in view.IComponents())
        //    {
        //        object data = graph.PropertyValue(component.Dataset.Collection);
        //        component.IRepresentationFragment(data, view.ViewConfig);
        //    }
        //}

        /***************************************************/
        private static void View(this DependencyChart chart, Graph graph)
        {
            //set scales
            List<object> viewX = new List<object>() { 0, chart.ViewConfig.Width};
            List<object> viewY = new List<object>() { 0, chart.ViewConfig.Height};
            //object dataX = graph.Entities().PropertyValue(chart.Boxes.X);
            //object dataY = graph.Entities().PropertyValue(chart.Boxes.Y);
            object allGroupNames = graph.Entities().PropertyValue(chart.Boxes.Group);
            var groups = DataList(allGroupNames).GroupBy(n => n);
            int maxGroup = groups.Max(g => g.Count());
            IScale xScale = null;
            IScale yScale = null;
            if (chart.Boxes.IsHorizontal)
            {
                //group for x scale
                xScale = new ScaleLinear()
                {
                    Domain = new Domain(0,maxGroup),
                    Range = new Domain(0, chart.ViewConfig.Width)
                };
                yScale = Graphics.Create.IScale(groups.Select(g => g.Key).ToList(), viewY);
            }
            else
            {
                //group for y scale
                xScale = Graphics.Create.IScale(groups.Select(g => g.Key).ToList(), viewX);
                yScale = new ScaleLinear()
                {
                    Domain = new Domain(0, maxGroup),
                    Range = new Domain(0, chart.ViewConfig.Height)
                };
            }
            
            xScale.Name = "xScale";
            
            yScale.Name = "yScale";
            Gradient gradient = Graphics.Create.Gradient();
            List<IScale> scales = new List<IScale>() { xScale, yScale};
            chart.Boxes.IRepresentationFragment(graph.Entities(), chart.ViewConfig, scales);            
            
        }

        
        private static List<object> DataList(object obj)
        {
            List<object> list = new List<object>();
            if (obj is IEnumerable<object>)
            {
                var enumerator = ((IEnumerable<object>)obj).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if(enumerator.Current!=null)
                        list.Add(enumerator.Current);
                }
            }
            return list;
        }
    }
}
