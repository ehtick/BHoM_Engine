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
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Base
{
    public static partial class Query
    {
        [Description("Determine whether a type is a numeric type.")]
        [Input("type", "Type that we want to check if it is numeric type or not.")]
        [Output("isNumeric", "True if the object is a numeric Type, false if not.")]
        [PreviousVersion("5.1", "BH.Engine.Reflection.Query.IsNumeric(System.Type)")]
        public static bool IsNumeric(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /***************************************************/

        [Description("Determine whether a type is a floating-point numeric type." +
            "\nSee https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types for more information.")]
        [Input("type", "Type that we want to check if it is numeric type or not.")]
        [Output("isNumeric", "True if the object is a numeric Type, false if not.")]
        [PreviousVersion("5.1", "BH.Engine.Reflection.Query.IsFloatingPointNumericType(System.Type)")]
        public static bool IsNumericFloatingPointType(this Type type)
        {
            // As per https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /***************************************************/

        [Description("Determine whether a type is a integral numeric type (an integer)." +
            "\nSee https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types for more information.")]
        [Input("type", "Type that we want to check if it is numeric type or not.")]
        [Output("isNumeric", "True if the object is a numeric Type, false if not.")]
        [PreviousVersion("5.1", "BH.Engine.Reflection.Query.IsIntegralNumericType(System.Type)")]
        public static bool IsNumericIntegralType(this Type type)
        {
            // As per https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }
    }
}
