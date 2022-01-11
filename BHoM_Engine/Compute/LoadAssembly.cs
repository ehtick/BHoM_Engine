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

using BH.oM.Base.Attributes;
using System.ComponentModel;
using System.Reflection;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using BH.oM.Base;
using System.Linq;

namespace BH.Engine.Base
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Attempts to load an assembly under the given path.")]
        [Input("assemblyPath", "Path from which the assembly is meant to be loaded.")]
        [Output("assembly", "The assembly under the given path, if it exists and has been loaded to BHoM (at any point in time), otherwise null.")]
        [PreviousVersion("5.1", "BH.Engine.Reflection.Compute.LoadAssembly(System.String)")]
        public static Assembly LoadAssembly(string assemblyPath)
        {
            try
            {
                string name = AssemblyName.GetAssemblyName(assemblyPath).FullName;
                if (!Global.AllAssemblies.ContainsKey(name))
                    return Assembly.LoadFrom(assemblyPath);
                else
                    return Global.AllAssemblies[name];
            }
            catch
            {
                RecordWarning("Failed to load assembly " + assemblyPath);
                return null;
            }
        }

        /***************************************************/

        [Description("Records the given assembly in the Global collection of loaded assemblies, then extracts types and methods from it and adds them to relevant collections in the Global class.")]
        [Input("assembly", "Assembly to be reflected.")]
        [PreviousVersion("5.1", "BH.Engine.Reflection.Compute.ReflectAssembly(System.Reflection.Assembly)")]
        public static void LoadAssembly(Assembly assembly)
        {
            if (assembly == null || assembly.ReflectionOnly)
                return;

            lock (Global.AssemblyReflectionLock)
            {
                if (Global.AllAssemblies.ContainsKey(assembly.FullName))
                    return;

                Global.AllAssemblies[assembly.FullName] = assembly;
                if (assembly.IsBHoM())
                {
                    Global.BHoMAssemblies[assembly.FullName] = assembly;
                    ExtractTypes(assembly);
                    ExtractMethods(assembly);
                }
            }
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void ExtractTypes(Assembly asm)
        {
            try
            {
                string name = asm.GetName().Name;
                // Save BHoM objects only
                if (name.IsOmAssembly())
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        if (type.Namespace != null && m_RegexOmNamespace.IsMatch(type.Namespace))
                        {
                            AddBHoMTypeToDictionary(type.FullName, type);
                            if (type.IsInterface)
                                Global.InterfaceList.Add(type);
                            else if (!(type.IsAbstract && type.IsSealed) && (type.IsEnum || typeof(IObject).IsAssignableFrom(type)))
                                Global.BHoMTypeList.Add(type);
                        }
                        if (type.Namespace != null && !type.IsAutoGenerated())
                            Global.AllTypeList.Add(type);
                    }
                }
                // Save adapters
                else if (name.IsAdapterAssembly())
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        if (!type.IsAutoGenerated())
                        {
                            if (!type.IsInterface && type.IsLegal() && type.IsOfType("BHoMAdapter"))
                                Global.AdapterTypeList.Add(type);

                            if (type.Namespace != null)
                                Global.AllTypeList.Add(type);
                        }

                    }
                }
                // Save engine
                else if (name.IsEngineAssembly())
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        if (!type.IsAutoGenerated())
                        {
                            if (type.Namespace != null && m_RegexEngineNamespace.IsMatch(type.Namespace))
                                Global.EngineTypeList.Add(type);

                            if (type.Namespace != null)
                                Global.AllTypeList.Add(type);
                        }
                    }
                }
                else
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        if (type.Namespace != null && type.Namespace.StartsWith("BH.") && !type.IsAutoGenerated())
                            Global.AllTypeList.Add(type);
                    }
                }
            }
            catch (Exception)
            {
                Compute.RecordWarning("Cannot load types from assembly " + asm.GetName().Name);
            }
        }

        /***************************************************/

        private static void AddBHoMTypeToDictionary(string name, Type type)
        {
            if (Global.BHoMTypeDictionary.ContainsKey(name))
                Global.BHoMTypeDictionary[name].Add(type);
            else
            {
                List<Type> list = new List<Type>();
                list.Add(type);
                Global.BHoMTypeDictionary[name] = list;
            }

            int firstDot = name.IndexOf('.');
            if (firstDot >= 0)
                AddBHoMTypeToDictionary(name.Substring(firstDot + 1), type);
        }

        /***************************************************/

        private static void ExtractMethods(Assembly asm)
        {
            BindingFlags bindingBHoM = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static;
            try
            {
                // Save BHoM objects only
                if (asm.IsEngineAssembly())
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        // Get only the BHoM methods
                        if (!type.IsInterface && type.IsAbstract)
                        {
                            foreach (MethodInfo info in type.GetMethods(bindingBHoM).Where(x => x.IsLegal()))
                            {
                                Global.BHoMMethodList.Add(info);
                            }
                        }

                        if (type.Name == "External")
                        {
                            MethodInfo getExternalMethods = type.GetMethod("Methods");
                            if (getExternalMethods != null)
                            {
                                foreach (MethodInfo info in (List<MethodInfo>)getExternalMethods.Invoke(null, null))
                                {
                                    Global.ExternalMethodList.Add(info);
                                }
                            }

                            MethodInfo getExternalCtor = type.GetMethod("Constructors");
                            if (getExternalCtor != null)
                            {
                                foreach (ConstructorInfo info in (List<ConstructorInfo>)getExternalCtor.Invoke(null, null))
                                {
                                    Global.ExternalMethodList.Add(info);
                                }
                            }
                        }
                        // Get everything
                        StoreAllMethods(type);
                    }
                }
                else
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        StoreAllMethods(type);
                    }
                }
            }
            catch (Exception e)
            {

                string message = "Cannot load types from assembly " + asm.GetName().Name + ". Exception message: " + e.Message;

                if (!string.IsNullOrEmpty(e.InnerException?.Message))
                {
                    message += "\nInnerException: " + e.InnerException.Message;
                }

                Compute.RecordWarning(message);
            }
        }

        /***************************************************/

        private static void StoreAllMethods(Type type)
        {
            BindingFlags bindingAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance;

            if (type.Namespace != null && type.Namespace.StartsWith("BH.") && !type.IsAutoGenerated())
            {
                foreach (ConstructorInfo info in type.GetConstructors(bindingAll).Where(x => x.GetMethodBody() != null && !x.IsAutoGenerated()))
                {
                    Global.AllMethodList.Add(info);
                }

                MethodInfo[] methods = type.GetMethods(bindingAll);
                foreach (var method in methods)
                {
                    try
                    {
                        if (method.GetMethodBody() != null && !method.IsAutoGenerated())
                            Global.AllMethodList.Add(method);
                    }
                    catch (Exception e)
                    {
                        string message = "Cannot load method " + method.Name + " from type  " + type.Name + ". Exception message: " + e.Message;

                        if (!string.IsNullOrEmpty(e.InnerException?.Message))
                        {
                            message += "\nInnerException: " + e.InnerException.Message;
                        }

                        Compute.RecordWarning(message);
                    }
                }
            }
        }

        /***************************************************/

        private static bool IsBHoM(this Assembly assembly)
        {
            string name = assembly.GetName().Name;
            return name.IsOmAssembly() || name.IsEngineAssembly() || name.IsAdapterAssembly() || name.IsUIAssembly();
        }

        /***************************************************/

        private static bool IsOfType(this Type type, string match)
        {
            Type baseType = type.BaseType;
            if (baseType == null || baseType.Name == "Object")
                return false;
            else if (baseType.Name == match)
                return true;
            else
                return baseType.IsOfType(match);
        }


        /***************************************************/
        /****          Private fields - regex           ****/
        /***************************************************/

        private static Regex m_RegexOmNamespace = new Regex(@"BH.(\w+.)?oM.");
        private static Regex m_RegexEngineNamespace = new Regex(@"BH.(\w+.)?Engine.");

        /***************************************************/
    }
}

