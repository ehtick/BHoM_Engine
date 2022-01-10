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

using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BH.Engine.Reflection
{
    public static partial class Compute
    {
        /***************************************************/
        /****               Public methods              ****/
        /***************************************************/

        [Description("Extracts all types from an already loaded non-BHoM assembly and adds them to the Global collection of all reflected types.")]
        [Input("assembly", "Non-BHoM assembly with the types to be reflected.")]
        public static void ReflectNonBHoMTypes(Assembly assembly)
        {
            if (assembly == null || !Global.AllAssemblies.ContainsKey(assembly.FullName) || assembly.ReflectionOnly)
            {
                RecordError($"{nameof(ReflectNonBHoMTypes)} method works only with assemblies loaded to BHoM.");
                return;
            }

            lock (Global.AssemblyReflectionLock)
            {
                if (m_NonBHoMTypesReflected.Contains(assembly.FullName) || assembly.IsBHoM())
                    return;

                m_NonBHoMTypesReflected.Add(assembly.FullName);
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Namespace != null && !type.IsAutoGenerated())
                        Global.AllTypeList.Add(type);
                }
            }
        }

        /***************************************************/

        [Description("Extracts all types from an already loaded non-BHoM assembly under the given name and adds them to the Global collection of all reflected types.")]
        [Input("assemblyName", "Name of the non-BHoM assembly with the types to be reflected.")]
        public static void ReflectNonBHoMTypes(string assemblyName)
        {
            Assembly asm = Global.AllAssemblies.Values.FirstOrDefault(x => x.GetName().Name == assemblyName);
            if (asm != null)
                ReflectNonBHoMTypes(asm);
            else
                RecordError($"No assembly named {assemblyName} has been loaded to BHoM.");
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static HashSet<string> m_NonBHoMTypesReflected = new HashSet<string>();

        /***************************************************/
    }
}

