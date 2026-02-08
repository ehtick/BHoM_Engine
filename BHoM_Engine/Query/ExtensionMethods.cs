/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BH.Engine.Base.Objects;
using BH.oM.Base.Attributes;

namespace BH.Engine.Base
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Finds all extension methods with the specified name that can be applied to the given type. \n" +
            "Automatically loads required assemblies before searching to ensure all relevant extension methods are discovered.")]
        [Input("type", "The Type to find extension methods for. The method will search for extensions where this type is assignable to the first parameter.")]
        [Input("methodName", "The name of the extension method to search for.")]
        [Output("methods", "A list of MethodInfo objects representing all extension methods with the specified name that can be applied to the given type.")]
        public static List<MethodInfo> ExtensionMethods(this Type type, string methodName)
        {
            // Make sure to load all assemblies that might contain this extension method
            IAssemblyResolver resolver = Query.AssemblyResolver();
            if (resolver != null)
                resolver.MakeSureAssemblyIsLoadedForExtensionMethod(methodName, type);

            // Search for the method itself
            List<MethodInfo> methods = new List<MethodInfo>();

            foreach (MethodInfo method in BHoMMethodList().Where(x => x.Name == methodName))
            {
                ParameterInfo[] param = method.GetParameters();

                if (param.Length > 0)
                {
                    if (param[0].ParameterType.IsAssignableFromIncludeGenerics(type))
                        methods.Add(method);
                }
            }

            return methods;
        }

        /***************************************************/
    }
}







