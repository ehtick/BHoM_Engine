﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Reflection
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static bool IsReleased(this MethodBase method)
        {
            ReleasedAttribute attribute = method.GetCustomAttribute<ReleasedAttribute>();
            if (attribute != null)
            {
                try
                {
                    Version version = new Version(attribute.FromVersion);
                    return (version.CompareTo(method.DeclaringType.Assembly.GetName().Version) <= 0);
                }
                catch
                {
                    return true;
                }
            }
            else
                return false;
        }


        /***************************************************/

        public static bool IsReleased(this Type type) 
        {
            ReleasedAttribute attribute = type.GetCustomAttribute<ReleasedAttribute>();
            if (attribute != null)
            {
                try
                {
                    Version version = new Version(attribute.FromVersion);
                    return (version.CompareTo(type.Assembly.GetName().Version) > 0);
                }
                catch
                {
                    return true;
                }
            }
            else
                return false;
        }


        /***************************************************/



    }
}
