/*
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
using System.Reflection;
using System.Text.RegularExpressions;

namespace BH.Engine.Reflection
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        [PreviousVersion("5.0", "BH.Engine.Reflection.Query.BHoMInterfaceList()")]
        [Description("Returns all BHoM interface types loaded in the current domain.")]
        [Output("types", "List of BHoM interface types loaded in the current domain.")]
        public static List<Type> BHoMInterfaceTypeList()
        {
            lock (m_GetTypesLock)
            {
                if (m_InterfaceList == null)
                    ExtractAllTypes();

                return m_InterfaceList;
            }
        }

        /***************************************************/

        [Description("Returns all BHoM types loaded in the current domain.")]
        [Output("types", "List of BHoM types loaded in the current domain.")]
        public static List<Type> BHoMTypeList()
        {
            lock (m_GetTypesLock)
            {
                if (m_BHoMTypeList == null)
                    ExtractAllTypes();

                return m_BHoMTypeList;
            }
        }

        /***************************************************/

        [Description("Returns all BHoM adapter types loaded in the current domain.")]
        [Output("types", "List of BHoM adapter types loaded in the current domain.")]
        public static List<Type> AdapterTypeList()
        {
            lock (m_GetTypesLock)
            {
                if (m_AdapterTypeList == null)
                    ExtractAllTypes();

                return m_AdapterTypeList;
            }
        }

        /***************************************************/

        [Description("Returns all types loaded in the current domain.")]
        [Output("types", "List of all types loaded in the current domain.")]
        public static List<Type> AllTypeList()
        {
            lock (m_GetTypesLock)
            {
                if (m_AllTypeList == null)
                    ExtractAllTypes();

                return m_AllTypeList;
            }
        }

        /***************************************************/

        [Description("Returns all BHoM engine types loaded in the current domain.")]
        [Output("types", "List of BHoM engine types loaded in the current domain.")]
        public static List<Type> EngineTypeList()
        {
            lock (m_GetTypesLock)
            {
                if (m_EngineTypeList == null)
                    ExtractAllTypes();

                return m_EngineTypeList;
            }
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static void ExtractAllTypes()
        {
            m_BHoMTypeList = new List<Type>();
            m_AdapterTypeList = new List<Type>();
            m_AllTypeList = new List<Type>();
            m_InterfaceList = new List<Type>();
            m_EngineTypeList = new List<Type>();
            m_BHoMTypeDictionary = new Dictionary<string, List<Type>>();

            foreach (Assembly asm in BHoMAssemblyList())
            {
                try
                {
                    // Save BHoM objects only
                    if (asm.IsOmAssembly())
                    {
                        foreach (Type type in asm.GetTypes())
                        {
                            if (type.Namespace != null && regexOmNamespace.IsMatch(type.Namespace))
                            {
                                AddBHoMTypeToDictionary(type.FullName, type);
                                if (type.IsInterface)
                                    m_InterfaceList.Add(type);
                                else if (!(type.IsAbstract && type.IsSealed) && (type.IsEnum || typeof(IObject).IsAssignableFrom(type)))
                                    m_BHoMTypeList.Add(type);
                            }
                            if (type.Namespace != null && !type.IsAutoGenerated())
                                m_AllTypeList.Add(type);
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
                                    m_AdapterTypeList.Add(type);

                                if (type.Namespace != null)
                                    m_AllTypeList.Add(type);
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
                                if (type.Namespace != null && regexEngineNamespace.IsMatch(type.Namespace))
                                    m_EngineTypeList.Add(type);

                                if (type.Namespace != null)
                                    m_AllTypeList.Add(type);
                            }
                        }
                    }
                    else
                    {
                        foreach (Type type in asm.GetTypes())
                        {
                            if (type.Namespace != null && type.Namespace.StartsWith("BH.") && !type.IsAutoGenerated())
                                m_AllTypeList.Add(type);
                        }
                    }
                }
                catch (Exception)
                {
                    Compute.RecordWarning("Cannot load types from assembly " + asm.GetName().Name);
                }
            }
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
        /**** Private Fields                            ****/
        /***************************************************/

        private static List<Type> m_BHoMTypeList = null;
        private static List<Type> m_AdapterTypeList = null;
        private static List<Type> m_AllTypeList = null;
        private static List<Type> m_InterfaceList = null;
        private static List<Type> m_EngineTypeList = null;
        private static readonly object m_GetTypesLock = new object();
        private static Regex regexOmNamespace = new Regex(@"BH.*.oM.");
        private static Regex regexEngineNamespace = new Regex(@"BH.*.Engine.");

        /***************************************************/
    }
}


