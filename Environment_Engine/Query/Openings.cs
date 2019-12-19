﻿/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using BH.oM.Environment;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Fragments;
using BH.oM.Base;

using BH.oM.Reflection.Attributes;
using System.ComponentModel;

using BH.Engine.Base;

using BH.oM.Physical.Elements;
using BH.Engine.Geometry;

using BH.oM.Geometry.SettingOut;

namespace BH.Engine.Environment
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns a collection of Environment Openings from a list of generic BHoM objects")]
        [Input("bhomObjects", "A collection of generic BHoM objects")]
        [Output("openings", "A collection of Environment Opening objects")]
        public static List<Opening> Openings(this List<IBHoMObject> bhomObjects)
        {
            bhomObjects = bhomObjects.ObjectsByType(typeof(Opening));
            List<Opening> Openings = new List<Opening>();
            foreach (IBHoMObject o in bhomObjects)
                Openings.Add(o as Opening);

            return Openings;
        }

        [Description("Returns a collection of Environment Openings that match the given element ID")]
        [Input("openings", "A collection of Environment Openings")]
        [Input("elementID", "The Element ID to filter by")]
        [Output("openings", "A collection of Environment Opening objects that match the element ID")]
        public static List<Opening> OpeningsByElementID(this List<Opening> openings, string elementID)
        {
            List<IEnvironmentObject> envObjects = new List<IEnvironmentObject>();
            foreach (Opening o in openings)
                envObjects.Add(o as IEnvironmentObject);

            envObjects = envObjects.ObjectsByFragment(typeof(OriginContextFragment));

            envObjects = envObjects.Where(x => (x.Fragments.Where(y => y.GetType() == typeof(OriginContextFragment)).FirstOrDefault() as OriginContextFragment).ElementID == elementID).ToList();

            List<Opening> rtnOpenings = new List<Opening>();
            foreach (IEnvironmentObject o in envObjects)
                rtnOpenings.Add(o as Opening);

            return rtnOpenings;            
        }

        [Description("Returns a collection of Environment Openings that match the given element ID")]
        [Input("panels", "A collection of Environment Panels to query for openings")]
        [Input("elementID", "The Element ID to filter by")]
        [Output("openings", "A collection of Environment Opening objects that match the element ID")]
        public static List<Opening> OpeningsByElementID(this List<Panel> panels, string elementID)
        {
            List<Opening> allOpenings = new List<Opening>();
            foreach (Panel p in panels)
                allOpenings.AddRange(p.Openings);

            return allOpenings.OpeningsByElementID(elementID);
        }

        [Description("Returns a collection of Environment Openings from a collection of Environment Panels")]
        [Input("panels", "A collection of Environment Panels to query for openings")]
        [Output("openings", "A collection of Environment Opening objects that match the element ID")]
        public static List<Opening> OpeningsFromElements(this List<Panel> panels)
        {
            List<Opening> openings = new List<Opening>();

            foreach (Panel p in panels)
                openings.AddRange(p.Openings);

            return openings;
        }

        [Description("BH.Engine.Environment Query, Returns a collection of Environment Openings that are unique by their instance data from their origin context fragment")]
        [Input("openings", "A collection of Environment Opening to filter")]
        [Output("openings", "A collection of Environment Opening objects with one per instance")]
        public static List<Opening> UniqueOpeningInstances(this List<Opening> openings)
        {
            List<Opening> returnOpenings = new List<Opening>();

            foreach (Opening p in openings)
            {
                OriginContextFragment o = p.FindFragment<OriginContextFragment>(typeof(OriginContextFragment));
                if (o != null)
                {
                    Opening testCheck = returnOpenings.Where(x => x.FindFragment<OriginContextFragment>(typeof(OriginContextFragment)) != null && x.FindFragment<OriginContextFragment>(typeof(OriginContextFragment)).TypeName == o.TypeName).FirstOrDefault();
                    if (testCheck == null)
                        returnOpenings.Add(p);
                }
            }

            return returnOpenings;
        }

        [Description("Returns a collection of Environment Openings queried from a collection of Physical Objects (windows, doors, etc.)")]
        [Input("physicalOpenings", "A collection of Physical Openings to query Environment Openings from")]
        [Output("openings", "A collection of Environment Openings from Physical Objects")]
        public static List<Opening> OpeningsFromPhysical(this List<IOpening> physicalOpenings)
        {
            List<Opening> openings = new List<Opening>();

            foreach(IOpening o in physicalOpenings)
            {
                Opening opening = new Opening();
                opening.Name = o.Name;
                opening.Edges = o.Location.IExternalEdges().ToEdges();
                opening.InnerEdges = o.Location.IInternalEdges().ToEdges();
                opening.Type = (o is Door ? OpeningType.Door : OpeningType.Window);
                openings.Add(opening);
            }

            return openings;
        }

        [Description("Returns a collection of Environment Openings that match the given opening name")]
        [Input("openings", "A collection of Environment Openings")]
        [Input("openingName", "The Opening Name to filter by")]
        [Output("openings", "A collection of Environment Opening objects that match the name")]
        public static List<Opening> OpeningsByName(this List<Opening> openings, string openingName)
        {
            return openings.Where(x => x.Name == openingName).ToList();
        }

        [Description("Returns a collection of Environment Openings where the maximum level of the opening matches the elevation of the given search level")]
        [Input("openings", "A collection of Environment Openings to filter")]
        [Input("searchLevel", "The Setting Level to search by")]
        [Output("openings", "A collection of Environment Openings where the maximum level meets the search level")]
        public static List<Opening> OpeningsByMaximumLevel(this List<Opening> openings, Level searchLevel)
        {
            return openings.OpeningsByMaximumLevel(searchLevel.Elevation);
        }

        [Description("Returns a collection of Environment Openings where the maximum level of the panel matches the elevation of the given search level")]
        [Input("openings", "A collection of Environment Openings to filter")]
        [Input("searchLevel", "The level to search by")]
        [Output("openings", "A collection of Environment Openings where the maximum level meets the search level")]
        public static List<Opening> OpeningsByMaximumLevel(this List<Opening> openings, double searchLevel)
        {
            return openings.Where(x => x.MaximumLevel() == searchLevel).ToList();
        }

        [Description("Returns a collection of Environment Openings where the minimum level of the Opening matches the elevation of the given search level")]
        [Input("openings", "A collection of Environment Openings to filter")]
        [Input("searchLevel", "The Setting Out Level to search by")]
        [Output("openings", "A collection of Environment Openings where the minimum level meets the search level")]
        public static List<Opening> OpeningsByMinimumLevel(this List<Opening> openings, Level searchLevel)
        {
            return openings.OpeningsByMinimumLevel(searchLevel.Elevation);
        }

        [Description("Returns a collection of Environment Openings where the minimum level of the Opening matches the elevation of the given search level")]
        [Input("openings", "A collection of Environment Opening to filter")]
        [Input("searchLevel", "The level to search by")]
        [Output("openings", "A collection of Environment Opening where the minimum level meets the search level")]
        public static List<Opening> OpeningsByMinimumLevel(this List<Opening> openings, double searchLevel)
        {
            return openings.Where(x => x.MinimumLevel() == searchLevel).ToList();
        }

        [Description("Returns a collection of Environment Opening that sit entirely on a given levels elevation")]
        [Input("openings", "A collection of Environment Openings to filter")]
        [Input("searchLevel", "The Setting Out Level to search by")]
        [Output("openings", "A collection of Environment Openings which match the given level")]
        public static List<Opening> OpeningsByLevel(this List<Opening> openings, Level searchLevel)
        {
            return openings.OpeningsByLevel(searchLevel.Elevation);
        }

        [Description("Returns a collection of Environment Openings that sit entirely on a given levels elevation")]
        [Input("openings", "A collection of Environment Openings to filter")]
        [Input("searchLevel", "The level to search by")]
        [Output("openings", "A collection of Environment Openings which match the given level")]
        public static List<Opening> OpeningsByLevel(this List<Opening> openings, double searchLevel)
        {
            return openings.Where(x => x.MinimumLevel() == searchLevel && x.MaximumLevel() == searchLevel).ToList();
        }
    }
}