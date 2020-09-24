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

using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Reflection;
using BH.Engine.Serialiser;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;
using BH.Engine.Base;
using System.Collections;
using System.Data;

namespace BH.Engine.Base
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/ 

        [Description("Computes a Hash code for the object which depends solely on its properties.")]
        [Input("obj", "Objects the hash code should be calculated for.")]
        [Input("propertyNameExceptions", "(Optional) e.g. `Fragments`. If you want to exclude a property that many objects have.")]
        [Input("propertyFullNameExceptions", "(Optional) e.g. `BH.oM.Structure.Elements.Bar.Fragments`. If you want to exclude a specific property of an object.")]
        [Input("namespaceExceptions", "(Optional) e.g. `BH.oM.Structure`. Any corresponding namespace is ignored.")]
        [Input("typeExceptions", "(Optional) e.g. `typeof(Guid)`. Any corresponding type is ignored.")]
        [Input("maxNesting", "(Optional) e.g. `100`. If any property is nested into the object over that level, it is ignored.")]
        public static string Hash(
            this IObject iObj,
            List<string> propertyNameExceptions = null, //e.g. `Fragments`
            List<string> propertyFullNameExceptions = null, //e.g. `BH.oM.Structure.Elements.Bar.Fragments`
            List<string> namespaceExceptions = null, //e.g. `BH.oM.Structure`
            List<Type> typeExceptions = null, //e.g. `typeof(Guid)`
            int maxNesting = 100)
        {
            // If null, add "BHoM_Guid" to the propertyNameExceptions.
            propertyNameExceptions = propertyNameExceptions ?? new List<string>() { nameof(BHoMObject.BHoM_Guid) };

            string hashString = DefiningString(iObj, 0, maxNesting, propertyNameExceptions, propertyFullNameExceptions, namespaceExceptions, typeExceptions);

            return SHA256Hash(hashString);
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        // Computes a SHA 256 hash from the given string.
        private static string SHA256Hash(string str)
        {
            byte[] strBytes = ASCIIEncoding.Default.GetBytes(str);

            HashAlgorithm SHA256Algorithm = SHA256.Create();
            byte[] byteHash = SHA256Algorithm.ComputeHash(strBytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteHash)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        /***************************************************/

        [Description("Generates a string representing the whole structure of the object with its assigned values.")]
        [Input("obj", "Objects the string should be calculated for.")]
        [Input("propertyNameExceptions", "(Optional) e.g. `Fragments`. If you want to exclude a property that many objects have.")]
        [Input("propertyFullNameExceptions", "(Optional) e.g. `BH.oM.Structure.Elements.Bar.Fragments`. If you want to exclude a specific property of an object.")]
        [Input("namespaceExceptions", "(Optional) e.g. `BH.oM.Structure`. Any corresponding namespace is ignored.")]
        [Input("typeExceptions", "(Optional) e.g. `typeof(Guid)`. Any corresponding type is ignored.")]
        [Input("maxNesting", "(Optional) e.g. `100`. If any property is nested into the object over that level, it is ignored.")]
        private static string DefiningString(
            object obj,
            int nestingLevel,
            int maxNesting = 100,
            List<string> propertyNameExceptions = null, //e.g. "Fragments"
            List<string> propertyFullNameExceptions = null, //e.g. "BH.oM.Structure.Elements.Bar.Fragments"
            List<string> namespaceExceptions = null, //e.g. "BH.oM.Structure"
            List<Type> typeExceptions = null) //e.g. typeof(Guid)
        {
            string composedString = "";
            string tabs = new String('\t', nestingLevel);

            Type type = obj?.GetType();

            if (type == null
                || (typeExceptions != null && typeExceptions.Contains(type))
                || (namespaceExceptions != null && namespaceExceptions.Where(ex => type.Namespace.Contains(ex)).Any())
                || nestingLevel >= maxNesting)
            {
                return composedString;
            }
            else if (type.IsPrimitive || type == typeof(Decimal) || type == typeof(String))
            {
                composedString += $"\n{tabs}" + obj?.ToString() ?? "";
            }
            else if (type.IsArray)
            {
                foreach (var element in (obj as dynamic))
                    composedString += $"\n{tabs}" + DefiningString(element, nestingLevel + 1, maxNesting, propertyNameExceptions, propertyFullNameExceptions, namespaceExceptions, typeExceptions);
            }
            else if (typeof(IDictionary).IsAssignableFrom(type))
            {
                IDictionary dic = obj as IDictionary;
                foreach (DictionaryEntry entry in dic)
                {
                    composedString += $"\n{tabs}" + $"[{entry.Key.GetType().FullName}]\n{tabs}{entry.Key}:\n { DefiningString(entry.Value, nestingLevel + 1, maxNesting, propertyNameExceptions, propertyFullNameExceptions, namespaceExceptions, typeExceptions)}";
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type) || typeof(IList).IsAssignableFrom(type) || typeof(ICollection).IsAssignableFrom(type))
            {
                foreach (var element in (obj as dynamic))
                    composedString += $"\n{tabs}" + DefiningString(element, nestingLevel + 1, maxNesting, propertyNameExceptions, propertyFullNameExceptions, namespaceExceptions, typeExceptions);
            }
            else if (type.FullName.Contains("System.Collections.Generic.ObjectEqualityComparer`1"))
            {
                composedString = "";
            }
            else if (type == typeof(System.Data.DataTable))
            {
                DataTable dt = obj as DataTable;
                return composedString += $"{type.FullName} {string.Join(", ", dt.Columns.OfType<DataColumn>().Select(c => c.ColumnName))}\n{tabs}" + DefiningString(dt.AsEnumerable(), nestingLevel + 1, maxNesting, propertyNameExceptions, propertyFullNameExceptions, namespaceExceptions, typeExceptions);
            }
            else if (typeof(IObject).IsAssignableFrom(type))
            {
                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo prop in properties)
                {
                    bool isInPropertyNameExceptions = (propertyNameExceptions != null && propertyNameExceptions.Where(ex => prop.Name.Contains(ex)).Any());
                    bool isInPropertyFullNameExceptions = isInPropertyNameExceptions || (propertyNameExceptions != null && propertyNameExceptions.Where(ex => prop.Name.Contains(ex)).Any());

                    if (isInPropertyNameExceptions || isInPropertyFullNameExceptions)
                    {
                        continue;
                    }

                    object propValue = prop.GetValue(obj);
                    if (propValue != null)
                    {
                        string outString = DefiningString(propValue, nestingLevel + 1, maxNesting, propertyNameExceptions, propertyFullNameExceptions, namespaceExceptions, typeExceptions) ?? "";
                        if (!string.IsNullOrWhiteSpace(outString))
                            composedString += $"\n{tabs}" + $"{type.FullName}.{prop.Name}:\n{tabs}{outString} ";
                    }
                }
            }
            else
            {
                composedString = $"\n{tabs}" + obj?.ToString() ?? "";
            }

            if (!string.IsNullOrWhiteSpace(composedString))
                composedString = (nestingLevel > 0 ? "\t" : "") + $"[{type.FullName}]" + composedString;

            return composedString;
        }

        /***************************************************/
    }
}
