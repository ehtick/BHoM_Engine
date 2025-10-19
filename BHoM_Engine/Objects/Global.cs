/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BH.Engine.Base
{
    internal static class Global
    {
        /***************************************************/
        /****     Internal properties - collections     ****/
        /***************************************************/

        internal static ConcurrentDictionary<string, Assembly> BHoMAssemblies { get; set; } = new ConcurrentDictionary<string, Assembly>();

        internal static ConcurrentDictionary<string, Assembly> AllAssemblies { get; set; } = new ConcurrentDictionary<string, Assembly>();

        internal static ConcurrentBag<Type> BHoMTypeList { get; set; } = new ConcurrentBag<Type>();

        internal static ConcurrentBag<Type> AdapterTypeList { get; set; } = new ConcurrentBag<Type>();

        internal static ConcurrentBag<Type> AllTypeList { get; set; } = new ConcurrentBag<Type>();

        internal static ConcurrentBag<Type> InterfaceList { get; set; } = new ConcurrentBag<Type>();

        internal static ConcurrentBag<Type> EngineTypeList { get; set; } = new ConcurrentBag<Type>();

        internal static ConcurrentDictionary<string, List<Type>> BHoMTypeDictionary { get; set; } = new ConcurrentDictionary<string, List<Type>>();

        internal static ConcurrentBag<MethodInfo> BHoMMethodList { get; set; } = new ConcurrentBag<MethodInfo>();

        internal static ConcurrentBag<MethodBase> AllMethodList { get; set; } = new ConcurrentBag<MethodBase>();

        internal static ConcurrentBag<MethodBase> ExternalMethodList { get; set; } = new ConcurrentBag<MethodBase>();

        internal static ConcurrentDictionary<string, string> AssemblyNamePerType { get; set; } = new ConcurrentDictionary<string, string>();


        /***************************************************/
        /****        Internal properties - locks        ****/
        /***************************************************/

        internal static object AssemblyReflectionLock { get; } = new object();

        internal static object DebugLogLock { get; } = new object();

        internal static Regex OmNamespacePattern { get; }

        internal static Regex EngineNamespacePattern { get; }

        internal static Regex AdapterNamespacePattern { get; }


        /***************************************************/
        /****            Static constructor             ****/
        /***************************************************/

        static Global()
        {
            BH.Engine.Base.Compute.RecordNote($"Engine Global initialising at {DateTime.UtcNow}");

            OmNamespacePattern = new Regex(@"^BH\.(\w+\.)?oM\.");
            EngineNamespacePattern = new Regex(@"^BH\.(\w+\.)?Engine\.");
            AdapterNamespacePattern = new Regex(@"^BH\.Adapter");

            // Subscribe to the assembly load event.
            AppDomain.CurrentDomain.AssemblyLoad += ReflectAssemblyOnLoad;

            // Dedicated assembly resolution mechanism to minimise issues related with dependency incompatibility
            AppDomain.CurrentDomain.AssemblyResolve += ResolveBHoMAssembly;

            // Collect the relation between types and the assembly they belong to
            CollectTypeAssemblyRelations();


            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Reflect the assemblies that have already been loaded.
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Compute.ExtractAssembly(asm);
            }

            stopwatch.Stop();
            BH.Engine.Base.Compute.RecordNote($"Time to extract all assemblies from current domain: {stopwatch.Elapsed.TotalMilliseconds / 1000} s. Completed at {DateTime.UtcNow}");

            BH.Engine.Base.Compute.RecordNote($"Engine Global initialised at {DateTime.UtcNow}");
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void ReflectAssemblyOnLoad(object sender, AssemblyLoadEventArgs args)
        {
            Compute.ExtractAssembly(args?.LoadedAssembly);
        }

        /***************************************************/

        private static Assembly ResolveBHoMAssembly(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Name))
                return null;

            string asmName = new AssemblyName(args.Name).Name;
            if (asmName.EndsWith(".resources"))  //ignore resource files
                return null;

            Assembly callingAssembly = Assembly.GetCallingAssembly();
            string callingAssemblyLocation = callingAssembly?.Location;

            if(string.IsNullOrEmpty(callingAssemblyLocation)) 
                return null;

            string assemblyLocation = Path.GetDirectoryName(callingAssemblyLocation);

            // Check whether the issue was requsted by a dll in the BHoMFolder
            // If it was, try loading the assembly from BHoM folder
            if (assemblyLocation == Query.BHoMFolder())
            {
                string assemblyPath = Path.Combine(Query.BHoMFolder(), $"{asmName}.dll");
                if (File.Exists(assemblyPath))
                    return Assembly.LoadFrom(assemblyPath);

            }

            return null;
        }

        /***************************************************/

        private static void CollectTypeAssemblyRelations()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string assemblyContentFile = Path.Combine(Query.BHoMFolderResources(), "AssemblyContent.tsv");
            if (File.Exists(assemblyContentFile))
            {
                List<string[]> content = File.ReadAllLines(assemblyContentFile)
                        .Select(x => x.Split('\t'))
                        .Where(x => x.Length == 4)
                        .ToList();

                // Add all the oM types
                Dictionary<string, string> result = content
                    .Where(x => x[1] == "Type")
                    .ToDictionary(x => x[2].Trim(), x => x[0].Trim());

                // All the engine types
                content.Where(x => x[1].StartsWith("Method_") || x[1].EndsWith("Creator"))
                    .Select(x =>
                    {
                        int startIndex = x[3].IndexOf("BH.Engine");
                        int endIndex = x[3].IndexOf(", Version");
                        return new Tuple<string, string>(x[3].Substring(startIndex, endIndex - startIndex).Trim(), x[0].Trim());
                    })
                    .GroupBy(x => x.Item1)
                    .ToList()
                    .ForEach(x => result[x.Key] = x.Select(v => v.Item2).First());

                // Add the adapter types
                foreach (var item in content.Where(x => x[1] == "AdapterConstructor"))
                {
                    string typeName = item[2].Split('(').First().Trim();
                    int lastIndex = typeName.LastIndexOf(".");
                    result[typeName.Substring(0, lastIndex)] = item[0].Trim();
                }
                    

                AssemblyNamePerType = new ConcurrentDictionary<string, string>(result);
            }

            stopwatch.Stop();
            BH.Engine.Base.Compute.RecordNote($"Time to collect all type-assembly relations: {stopwatch.Elapsed.TotalMilliseconds / 1000} s. Completed at {DateTime.UtcNow}");

        }

        /***************************************************/
    }
}




