using BH.oM.Geometry;
using System;

namespace BH.Engine.Geometr
{
    public static partial class Modify
    {
        public static TransformMatrix RoundEntries(this TransformMatrix matrix, int decimalPlaces)
        {
            double[,] rounded = new double[4, 4];
            for (int m = 0; m < matrix.Matrix.GetLength(0); m++)
            {
                for (int n = 0; n < matrix.Matrix.GetLength(1); n++)
                {
                    rounded[m, n] = Math.Round(matrix.Matrix[m, n], decimalPlaces);
                }
            }

            return new TransformMatrix { Matrix = rounded };
        }
    }
}
