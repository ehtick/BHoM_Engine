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
using System.Management.Automation;

namespace BH.Engine.Base
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Computes a Hash code for the iObject. The hash uniquely represents an object's state based on its combined properties and their values.")]
        [Input("iObj", "iObject the hash code should be calculated for.")]
        public static string Hash(this IObject iObj, HashConfig hashConfig = null)
        {
            // Make sure to clone for immutability, and always have a HashConfig.
            HashConfig hc = hashConfig == null ? new HashConfig() : hashConfig.DeepClone();

            // Make sure that "BHoM_Guid" is added to the propertyNameExceptions of the HashConfig.
            hc.PropertyNameExceptions = hc.PropertyNameExceptions ?? new List<string>();
            if (!hc.PropertyNameExceptions.Contains(nameof(BHoMObject.BHoM_Guid)))
                hc.PropertyNameExceptions.Add(nameof(BHoMObject.BHoM_Guid));

            int fractionalDigits = Math.Abs(Convert.ToInt32(Math.Log10(hc.NumericTolerance)));

            string hashString = DefiningString(iObj, hc, fractionalDigits, 0);

            if (string.IsNullOrWhiteSpace(hashString))
                throw new Exception("Error computing the defining string of the object.");

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
        [Input("hc", "HashConfig, options for the hash calculation.")]
        [Input("nestingLevel", "Nesting level of the property.")]
        [Input("propertyPath", "(Optional) Indicates the 'property path' of the current object, e.g. `BH.oM.Structure.Elements.Bar.StartNode.Point.X`")]
        private static string DefiningString(object obj, HashConfig hc, int fractionalDigits, int nestingLevel, string propertyPath = null)
        {
            string composedString = "";
            string tabs = new String('\t', nestingLevel);

            Type type = obj?.GetType();

            if (type == null
                || (hc.TypeExceptions != null && hc.TypeExceptions.Contains(type))
                || (hc.NamespaceExceptions != null && hc.NamespaceExceptions.Where(ex => type.Namespace.Contains(ex)).Any())
                || nestingLevel >= hc.MaxNesting)
            {
                return composedString;
            }
            else if (type.IsPrimitive || type == typeof(decimal) || type == typeof(String))
            {
                if (type == typeof(double) || type == typeof(decimal) || type == typeof(float))
                    obj = Math.Round(obj as dynamic, fractionalDigits);

                composedString += $"\n{tabs}" + obj?.ToString() ?? "";
            }
            else if (type.IsArray)
            {
                foreach (var element in (obj as dynamic))
                    composedString += $"\n{tabs}" + DefiningString(element, hc, fractionalDigits, nestingLevel + 1, propertyPath);
            }
            else if (typeof(IDictionary).IsAssignableFrom(type))
            {
                IDictionary dic = obj as IDictionary;
                foreach (DictionaryEntry entry in dic)
                {
                    composedString += $"\n{tabs}" + $"[{entry.Key.GetType().FullName}]\n{tabs}{entry.Key}:\n { DefiningString(entry.Value, hc, fractionalDigits, nestingLevel + 1, propertyPath)}";
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type) || typeof(IList).IsAssignableFrom(type) || typeof(ICollection).IsAssignableFrom(type))
            {
                foreach (var element in (obj as dynamic))
                    composedString += $"\n{tabs}" + DefiningString(element, hc, fractionalDigits, nestingLevel + 1, propertyPath);
            }
            else if (type.FullName.Contains("System.Collections.Generic.ObjectEqualityComparer`1"))
            {
                composedString = "";
            }
            else if (type == typeof(System.Data.DataTable))
            {
                DataTable dt = obj as DataTable;
                return composedString += $"{type.FullName} {string.Join(", ", dt.Columns.OfType<DataColumn>().Select(c => c.ColumnName))}\n{tabs}" + DefiningString(dt.AsEnumerable(), hc, fractionalDigits, nestingLevel + 1, propertyPath);
            }
            else if (typeof(IObject).IsAssignableFrom(type))
            {
                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo prop in properties)
                {
                    bool isInPropertyNameExceptions = hc.PropertyNameExceptions?.Count > 0 && hc.PropertyNameExceptions.Where(ex => prop.Name.Contains(ex)).Any();
                    bool isInPropertyFullNameExceptions = hc.PropertyFullNameExceptions?.Count > 0 && hc.PropertyFullNameExceptions.Where(ex => new WildcardPattern(ex).IsMatch(prop.Name + "." + prop.DeclaringType.FullName)).Any();

                    if (isInPropertyNameExceptions || isInPropertyFullNameExceptions)
                        continue;

                    if (hc.PropertiesToConsider?.Count() > 0 && !hc.PropertiesToConsider.Contains(prop.Name))
                        continue;

                    object propValue = prop.GetValue(obj);
                    if (propValue != null)
                    {
                        if (string.IsNullOrWhiteSpace(propertyPath))
                            propertyPath = type.FullName + "." + prop.Name;
                        else if (prop.PropertyType.IsClass)
                            propertyPath += "." + prop.Name;

                        string outString = "";

                        if (hc.FractionalDigitsPerProperty != null &&
                            prop.PropertyType == typeof(double) || prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(float))
                        {
                            Dictionary<string, int> matches = new Dictionary<string, int>();

                            string path = propertyPath + "." + prop.Name;

                            foreach (var kv in hc.FractionalDigitsPerProperty)
                            {
                                if (path.Contains(kv.Key) ||
                                new WildcardPattern(kv.Key).IsMatch(path))
                                    matches.Add(kv.Key, kv.Value);
                            }

                            if (matches.Count() > 1)
                                throw new ArgumentException($"Too many matching results obtained with specified {nameof(hc.FractionalDigitsPerProperty)}.");

                            int fracDigits = matches.Count() == 1 ? matches.FirstOrDefault().Value : fractionalDigits;

                            outString = DefiningString(propValue, hc, fracDigits, nestingLevel + 1, path) ?? "";
                        }
                        else
                            outString = DefiningString(propValue, hc, fractionalDigits, nestingLevel + 1, propertyPath) ?? "";

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
