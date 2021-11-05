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

using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BH.Engine.Reflection
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static bool LoadAssembly(string assemblyPath)
        {
            lock (m_LoadAssembliesLock)
            {
                // On the 1st run, load all assemblies
                if (Global.AllAssemblies == null)
                {
                    Global.AllAssemblies = AppDomain.CurrentDomain.GetAssemblies().GroupBy(x => x.FullName).Select(g => g.First()).ToList();
                    Global.BHoMAssemblies = Global.AllAssemblies.Where(x => x.IsBHoM()).ToList();

                    InitialiseGlobalCollections();

                    foreach (Assembly asm in Global.BHoMAssemblies)
                    {
                        ExtractTypesAndMethods(asm);
                    }
                }

                try
                {
                    Assembly loaded = Assembly.LoadFrom(assemblyPath);
                    ExtractTypesAndMethods(loaded);
                    return true;
                }
                catch
                {
                    RecordWarning("Failed to load assembly " + assemblyPath);
                    return false;
                }
            }
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/


        private static void ExtractTypesAndMethods(Assembly asm)
        {
            ExtractTypes(asm);
            ExtractMethods(asm);
        }

        /***************************************************/

        private static void ExtractTypes(Assembly asm)
        {
            try
            {
                // Save BHoM objects only
                if (asm.IsOmAssembly())
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
                else if (asm.IsAdapterAssembly())
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
                else if (asm.IsEngineAssembly())
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
                            MethodInfo[] typeMethods = type.GetMethods(bindingBHoM);
                            Global.BHoMMethodList.AddRange(typeMethods.Where(x => x.IsLegal()));
                        }

                        if (type.Name == "External")
                        {
                            MethodInfo getExternalMethods = type.GetMethod("Methods");
                            if (getExternalMethods != null)
                                Global.ExternalMethodList.AddRange((List<MethodInfo>)getExternalMethods.Invoke(null, null));
                            MethodInfo getExternalCtor = type.GetMethod("Constructors");
                            if (getExternalCtor != null)
                                Global.ExternalMethodList.AddRange((List<ConstructorInfo>)getExternalCtor.Invoke(null, null));
                        }
                        // Get everything
                        StoreAllMethods(type);
                    }
                }
                else if (asm.IsOmAssembly() || asm.IsAdapterAssembly() || asm.IsUiAssembly())
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
                Global.AllMethodList.AddRange(type.GetConstructors(bindingAll).Where(x => x.GetMethodBody() != null && !x.IsAutoGenerated()));

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
            return name.IsOmAssembly() || name.IsEngineAssembly() || name.IsAdapterAssembly() || name.IsUiAssembly();
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

        private static void InitialiseGlobalCollections()
        {
            Global.BHoMTypeList = new List<Type>();
            Global.AdapterTypeList = new List<Type>();
            Global.AllTypeList = new List<Type>();
            Global.InterfaceList = new List<Type>();
            Global.EngineTypeList = new List<Type>();
            Global.BHoMTypeDictionary = new Dictionary<string, List<Type>>();
            Global.BHoMMethodList = new List<MethodInfo>();
            Global.AllMethodList = new List<MethodBase>();
            Global.ExternalMethodList = new List<MethodBase>();
        }


        /***************************************************/
        /****          Private fields - locks           ****/
        /***************************************************/

        private static readonly object m_LoadAssembliesLock = new object();


        /***************************************************/
        /****          Private fields - regex           ****/
        /***************************************************/

        private static Regex m_RegexOmNamespace = new Regex(@"BH.*.oM.");
        private static Regex m_RegexEngineNamespace = new Regex(@"BH.*.Engine.");

        /***************************************************/
    }
}
