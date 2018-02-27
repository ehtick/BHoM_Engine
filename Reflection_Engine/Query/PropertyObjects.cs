﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Reflection
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<object> PropertyObjects(this object obj, bool goDeep = false)
        {
            List<object> properties = new List<object>();
            foreach (var prop in obj.GetType().GetProperties())
            {
                if (!prop.CanRead || prop.GetMethod.GetParameters().Count() > 0) continue;
                var value = prop.GetValue(obj, null);
                if (value != null && !(value is ValueType))
                {
                    properties.Add(value);
                    if (goDeep)
                        properties.AddRange(value.PropertyObjects(true));
                }
            }
            return properties;
        }

        /***************************************************/

        public static Dictionary<Type, List<object>> PropertyObjects(this IEnumerable<object> objects, Type type)
        {
            Dictionary<Type, List<object>> propByType = new Dictionary<Type, List<object>>();
            foreach (var prop in type.GetProperties())
            {
                if (!prop.CanRead || prop.PropertyType.IsValueType || prop.GetMethod.GetParameters().Count() > 0) continue;
                List<object> properties = new List<object>();
                foreach (object obj in objects)
                {
                    var value = prop.GetValue(obj, null);
                    if (value != null)
                        properties.Add(value);
                }
                if (properties.Count > 0)
                    propByType.Add(prop.PropertyType, properties);
            }
            return propByType;
        }

        /***************************************************/
    }
}
