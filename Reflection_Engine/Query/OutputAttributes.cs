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

using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BH.oM.Quantities.Attributes;

namespace BH.Engine.Reflection
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Return names and descriptions of the multiple outputs of a C# method")]
        public static List<OutputAttribute> OutputAttributes(this MethodBase method)
        {
            if (method.IsMultipleOutputs())
            {
                Dictionary<int, MultiOutputAttribute> outputDefs = method.GetCustomAttributes<MultiOutputAttribute>().ToDictionary(x => x.Index);
                Type[] types = method.OutputType().GetGenericArguments();

                List<OutputAttribute> outputs = new List<OutputAttribute>();
                for (int i = 0; i < types.Length; i++)
                {
                    if (outputDefs.ContainsKey(i))
                    {
                        string desc = outputDefs[i].Description;

                        if (types[i] != null)
                        {
                            desc += Environment.NewLine;
                            QuantityAttribute quantityAttribute = outputDefs[i].Quantity;
                            desc += types[i].UnderlyingType().Type.Description(quantityAttribute);
                        }
                        outputs.Add(new OutputAttribute(outputDefs[i].Name, desc));
                    }
                    else
                    {
                        string name = types[i].UnderlyingType().Type.Name.Substring(0, 1);
                        int nbSame = outputs.Where(x => x.Name.StartsWith(name)).Count();
                        if (nbSame > 0)
                            name += (nbSame + 1).ToString();
                        outputs.Add(new OutputAttribute(name, ""));
                    }
                        
                }
                return outputs;
            }
            else
            {
                OutputAttribute attribute = method.GetCustomAttribute<OutputAttribute>();
                if (attribute != null)
                    return new List<OutputAttribute> { attribute };
                else
                    return new List<OutputAttribute>();
            }
        }

        /***************************************************/
    }
}

