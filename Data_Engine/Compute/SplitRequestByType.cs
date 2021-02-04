﻿/*
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

using BH.Engine.Base;
using BH.oM.Base;
using BH.oM.Data.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Data
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<IRequest> SplitRequestTreeByType(IRequest request, Type splittingType)
        {
            if (!typeof(IRequest).IsAssignableFrom(splittingType))
            {
                BH.Engine.Reflection.Compute.RecordError($"Type {splittingType} does not implement {nameof(IRequest)} interface.");
                return null;
            }

            if (typeof(ILogicalRequest).IsAssignableFrom(splittingType))
            {
                BH.Engine.Reflection.Compute.RecordError($"It is not allowed to split by types that implement {nameof(ILogicalRequest)} interface.");
                return null;
            }

            if (request.IsPotentialOverlap(splittingType))
            {
                BH.Engine.Reflection.Compute.RecordError($"The request could not be split by type {splittingType} because there is a potential logical AND overlap between two requests of the given type.");
                return null;
            }

            IRequest flattened = request.FlattenRequestTree();
            List<IRequest> extracted = new List<IRequest>();
            flattened.ExtractTrees(splittingType, extracted, new List<IRequest>());

            extracted = extracted.Select(x => x.FlattenRequestTree()).ToList();
            extracted.Add(flattened.FlattenRequestTree());

            return extracted.Where(x => x != null).ToList();
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void ExtractTrees(this IRequest request, Type typeToExtract, List<IRequest> extracted, List<IRequest> history)
        {
            List<IRequest> newHistory = new List<IRequest>(history);

            Type type = request.GetType();
            if (type == typeToExtract)
            {
                extracted.Add(request.Extract(newHistory));

                if (history.Count != 0)
                    ((ILogicalRequest)history.Last()).IRequests().Remove(request);

                return;
            }

            newHistory.Add(request);

            if (request is LogicalAndRequest)
            {
                List<IRequest> subRequests = ((LogicalAndRequest)request).Requests;
                IRequest found = subRequests.FirstOrDefault(x => x.GetType() == typeToExtract);
                if (found != null)
                {
                    extracted.Add(found.Extract(newHistory));
                    ((ILogicalRequest)history.Last()).IRequests().Remove(request);
                }
                else
                {
                    for (int i = subRequests.Count - 1; i >= 0; i--)
                    {
                        subRequests[i].ExtractTrees(typeToExtract, extracted, newHistory);
                    }
                }
            }
            else if (request is LogicalOrRequest)
            {
                List<IRequest> subRequests = ((LogicalOrRequest)request).Requests;
                for (int i = subRequests.Count - 1; i >= 0; i--)
                {
                    subRequests[i].ExtractTrees(typeToExtract, extracted, newHistory);
                }

            }
            else if (request is LogicalNotRequest)
                ((LogicalNotRequest)request).Request.ExtractTrees(typeToExtract, extracted, newHistory);
        }

        /***************************************************/

        private static IRequest Extract(this IRequest request, List<IRequest> history)
        {
            history.Add(request);
            List<int> nexts = new List<int>();
            for (int i = 0; i < history.Count - 1; i++)
            {
                nexts.Add(((ILogicalRequest)history[i]).IRequests().IndexOf(history[i + 1]));
            }

            IRequest result = history[0].DeepClone();
            IRequest current = result;
            foreach (int n in nexts)
            {
                List<IRequest> subRequests = ((ILogicalRequest)current).IRequests();
                IRequest next = subRequests[n];
                if (current is LogicalOrRequest)
                {
                    subRequests.Clear();
                    subRequests.Add(next);
                }

                current = next;
            }

            return result;
        }

        /***************************************************/
    }
}

