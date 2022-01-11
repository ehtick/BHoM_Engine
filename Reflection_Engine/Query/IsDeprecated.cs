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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.oM.Base.Attributes;

namespace BH.Engine.Reflection
{
    public static partial class Query
    {
        /***************************************************/
        /**** Interface Methods                         ****/
        /***************************************************/

        [ToBeRemoved("5.1", "The IsDeprecated function will be removed as the Deprecated Attribute no longer exists. All uses of this function will return false - as no objects or methods can have the Deprecated Attribute going forward so no method or object can be deprecated.")]
        public static bool IIsDeprecated(this object obj)
        {
            if (obj == null)
                return false;

            return IsDeprecated(obj as dynamic);
        }


        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [ToBeRemoved("5.1", "The IsDeprecated function will be removed as the Deprecated Attribute no longer exists. All uses of this function will return false - as no objects or methods can have the Deprecated Attribute going forward so no method or object can be deprecated.")]
        public static bool IsDeprecated(this MethodBase method)
        {
            return false;
        }

        /***************************************************/

        [ToBeRemoved("5.1", "The IsDeprecated function will be removed as the Deprecated Attribute no longer exists. All uses of this function will return false - as no objects or methods can have the Deprecated Attribute going forward so no method or object can be deprecated.")]
        public static bool IsDeprecated(this Type type)
        {
            return false;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static bool IsDeprecated(object obj)
        {
            // Fallback case, if the Interface method is called and no overload is found at runtime, it will end up here
            return false;
        }

        /***************************************************/

    }
}



