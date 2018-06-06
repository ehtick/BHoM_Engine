﻿using BH.oM.Geometry;
using System;

namespace BH.Engine.Geometry
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Vector Vector(double x = 0, double y = 0, double z = 0)
        {
            return new Vector { X = x, Y = y, Z = z };
        }

        /***************************************************/

        public static Vector Vector(Point v)
        {
            return new Vector { X = v.X, Y = v.Y, Z = v.Z };
        }

        /***************************************************/

        public static Vector RandomVector(int seed = -1, BoundingBox box = null)
        {
            if (seed == -1)
                seed = m_Random.Next();
            Random rnd = new Random(seed);
            return RandomVector(new Random(seed), box);
        }

        /***************************************************/

        public static Vector RandomVector(Random rnd, BoundingBox box = null)
        {
            if (box != null)
            {
                return new Vector
                {
                    X = box.Min.X + rnd.NextDouble() * (box.Max.X - box.Min.X),
                    Y = box.Min.Y + rnd.NextDouble() * (box.Max.Y - box.Min.Y),
                    Z = box.Min.Z + rnd.NextDouble() * (box.Max.Z - box.Min.Z)
                };
            }
            else
            {
                return new Vector { X = rnd.NextDouble()*2-1, Y = rnd.NextDouble()*2 - 1, Z = rnd.NextDouble()*2 - 1 };
            }
        }

        /***************************************************/
    }
}
