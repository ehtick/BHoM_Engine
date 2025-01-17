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
using System.Reflection;

namespace BH.Engine.Serialiser
{
    public static partial class Query
    {
        /*******************************************/
        /**** Public Methods                    ****/
        /*******************************************/

        public static ParameterInfo[] ParametersWithConstraints(this MethodBase method)
        {
            if(method == null)
            {
                Base.Compute.RecordWarning("Cannot query the parameter information from a null method. An empty array of parameters will be returned instead.");
                return new ParameterInfo[0];
            }

            ParameterInfo[] parameters = method.GetParameters();

            if (method.ContainsGenericParameters)
            {
                Type[] generics = method.GetGenericArguments().Select(x => GenericTypeConstraint(x)).ToArray();
                MethodInfo methodinfo = method as MethodInfo;
                if (methodinfo != null)
                    parameters = methodinfo.MakeGenericMethod(generics).GetParameters();
            }
            return parameters;
        }

        /*******************************************/
    }
}



