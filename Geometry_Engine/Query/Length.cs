/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Geometry;
using BH.oM.Quantities.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Geometry
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods - Vector                   ****/
        /***************************************************/

        [Description("Calculates the length of a Vector.")]
        [Input("vector", "The vector to calculate the length of.")]
        [Output("length", "The length of the Vector.", typeof(Length))]
        public static double Length(this Vector vector)
        {
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }

        /***************************************************/

        [Description("Calculates the square length of a Vector. Faster to compute than the length and can be usefull for example where only relative lengths between vectors are sought.")]
        [Input("vector", "The vector to calculate the square length of.")]
        [Output("sqLength", "The square length of the Vector.")]
        public static double SquareLength(this Vector vector)
        {
            return vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
        }


        /***************************************************/
        /**** Public Methods - Curves                   ****/
        /***************************************************/

        [Description("Calculates the length of an Arc.")]
        [Input("curve", "The Arc to calculate the length of.")]
        [Output("length", "The length of the Arc.", typeof(Length))]
        public static double Length(this Arc curve)
        {
            if (curve == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot query length as the geometry is null.");
                return double.NaN;
            }
            return curve.Angle() * curve.Radius;
        }

        /***************************************************/

        [Description("Calculates the length of an Circle.")]
        [Input("curve", "The Circle to calculate the length of.")]
        [Output("length", "The length of the Circle.", typeof(Length))]
        public static double Length(this Circle curve)
        {
            if (curve == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot query length as the geometry is null.");
                return double.NaN;
            }
            return 2 * Math.PI * curve.Radius;
        }

        /***************************************************/

        [Description("Calculates the length of an Ellipse. Evaluated as an infinite series, utilising 10 times the ratio of the long and short radius number of terms.\n" +
                     "Gives very close to exact results for ellipses with an ratio of up to 1:20 000 between the long and short radius.")]
        [Input("curve", "The Ellipse to calculate the length of.")]
        [Output("length", "The length of the Ellipse.", typeof(Length))]
        public static double Length(this Ellipse curve)
        {
            if (curve == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot query length as the geometry is null.");
                return double.NaN;
            }

            //Get out a as the long radius and b as the short radius
            double a = Math.Max(curve.Radius1, curve.Radius2);
            double b = Math.Min(curve.Radius1, curve.Radius2);

            double h = (a - b) / (a + b);
            h *= h;

            //When h is equal to 1, the ellipse is a line
            //The algorithm below will not be able to handle to elongated ellipses, hence 
            //pointless to evaluate.
            if (1 - h < 1e-6)
            {
                //Raise a warning when b is not exactly equal to 0
                if (b != 0)
                {
                    Base.Compute.RecordWarning("The aspect ratio of the provided Ellipse is to large to be able to accurately evaluate the length. Approximate value of 4 times length of line between vertex and co-vertex returned.");
                    double hypotenus = Math.Sqrt(a * a + b * b);
                    return 4 * hypotenus;
                }
                else
                    return 4 * a;
            }

            double p = 0;

            //Ratio of ellipse to calculate number of series to evaluate
            int nbSeries = (int)Math.Round(a / b * 10);

            //"Infinite" series evaluated
            List<double> binomialCooefs = SquareBinomialCoefficientsEllipse(nbSeries);

            //Ratio of ellipse to calculate number of series to evaluate
            nbSeries = (int)Math.Round(Math.Min(a / b * 10, binomialCooefs.Count));

            //Evaluated as the "infinite" series listed in here:
            //https://en.wikipedia.org/wiki/Ellipse#Circumference
            //noted on the wikipedia page as Ivory and Bessel.
            for (int i = 0; i < nbSeries; i++)
            {
                p += Math.Pow(h, i) * binomialCooefs[i];
            }

            double length = Math.PI * (a + b) * p;

            //For ellipses with an extremely high ratio (over 1:1000 000) the length from the evaluated series will be to low.
            //The check bellow checks that the returned length is at least that of 4 times the longest radius.
            if (length < 4 * a)
            {
                Base.Compute.RecordWarning("The aspect ratio of the provided Ellipse is to large to be able to accurately evaluate the length. Approximate value of 4 times length of line between vertex and co-vertex. ");
                double hypotenus = Math.Sqrt(a * a + b * b);
                return 4 * hypotenus;
            }

            return length;
        }

        /***************************************************/

        [Description("Calculates the length of an Line.")]
        [Input("curve", "The Line to calculate the length of.")]
        [Output("length", "The length of the Line.", typeof(Length))]
        public static double Length(this Line curve)
        {
            return (curve.Start - curve.End).Length();
        }

        /***************************************************/

        [Description("Calculates the square length of an Line. Faster to calculate than the length.")]
        [Input("curve", "The Line to calculate the square length of.")]
        [Output("sqLength", "The square length of the Line.")]
        public static double SquareLength(this Line curve)
        {
            return (curve.Start - curve.End).SquareLength();
        }

        /***************************************************/

        [Description("Calculates the length of a NurbsCurve using numerical integration.")]
        [Input("curve", "The NurbsCurve to calculate the length of.")]
        [Input("tolerance", "Tolerance for the numerical integration. Smaller values give more accurate results but take longer to compute.", typeof(Length))]
        [Output("length", "The length of the NurbsCurve.", typeof(Length))]
        public static double Length(this NurbsCurve curve, double tolerance = Tolerance.Distance)
        {
            if (curve == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot query length as the geometry is null.");
                return double.NaN;
            }

            // Check derivative smoothness to determine integration method
            bool hasSmootDerivatives = HasSmoothDerivatives(curve, tolerance);

            if (hasSmootDerivatives)
            {
                // Use Gauss-Legendre quadrature for smooth curves
                return GaussLegendreArcLength(curve, 0.0, 1.0, tolerance, 16);
            }
            else
            {
                // Use adaptive Simpson's rule for curves with rapidly changing derivatives
                return AdaptiveSimpsonArcLength(curve, 0.0, 1.0, tolerance);
            }
        }

        /***************************************************/

        [Description("Calculates the length of an PolyCurve. Calculated as the sum of the length of its parts.")]
        [Input("curve", "The PolyCurve to calculate the length of.")]
        [Output("length", "The length of the PolyCurve.", typeof(Length))]
        public static double Length(this PolyCurve curve)
        {
            if (curve == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot query length as the geometry is null.");
                return double.NaN;
            }
            return curve.Curves.Sum(c => c.ILength());
        }

        /***************************************************/

        [Description("Calculates the length of a Polyline.")]
        [Input("curve", "The Polyline to calculate the length of.")]
        [Output("length", "The length of the Polyline.", typeof(Length))]
        public static double Length(this Polyline curve)
        {
            if (curve == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot query length as the geometry is null.");
                return double.NaN;
            }
            double length = 0;
            List<Point> pts = curve.ControlPoints;

            for (int i = 1; i < pts.Count; i++)
                length += (pts[i] - pts[i - 1]).Length();

            return length;
        }

        /***************************************************/
        /**** Public Methods - Interfaces               ****/
        /***************************************************/

        [Description("Calculates the length of a Curve.")]
        [Input("curve", "The ICurve to calculate the length of.")]
        [Output("length", "The length of the Arc.", typeof(Length))]
        public static double ILength(this ICurve curve)
        {
            if (curve == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot query length as the geometry is null.");
                return double.NaN;
            }
            return Length(curve as dynamic);
        }


        /***************************************************/
        /**** Private Fallback Methods                  ****/
        /***************************************************/

        private static double Length(this ICurve curve)
        {
            Base.Compute.RecordError($"Length is not implemented for ICurves of type: {curve.GetType().Name}.");
            return double.NaN;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        [Description("Computes the arc length of a NurbsCurve using adaptive Simpson's rule integration.")]
        private static double AdaptiveSimpsonArcLength(NurbsCurve curve, double a, double b, double tolerance)
        {
            // Initial Simpson's rule approximation
            double c = (a + b) / 2.0;
            double h = b - a;

            double fa = DerivativeMagnitude(curve, a);
            double fb = DerivativeMagnitude(curve, b);
            double fc = DerivativeMagnitude(curve, c);

            // Check for degenerate cases
            if (fa <= 0 && fb <= 0 && fc <= 0)
            {
                Engine.Base.Compute.RecordWarning("All derivative magnitudes are zero or negative. The curve may be degenerate.");
                return 0.0;
            }

            double s = (h / 6.0) * (fa + 4.0 * fc + fb);

            return AdaptiveSimpsonArcLengthRecursive(curve, a, b, tolerance, s, fa, fb, fc, 20);
        }

        /***************************************************/

        [Description("Recursive helper for adaptive Simpson's rule integration.")]
        private static double AdaptiveSimpsonArcLengthRecursive(NurbsCurve curve, double a, double b, double tolerance, double s, double fa, double fb, double fc, int depth)
        {
            if (depth <= 0)
                return s; // Maximum recursion depth reached

            double c = (a + b) / 2.0;
            double h = b - a;
            double d = (a + c) / 2.0;
            double e = (c + b) / 2.0;

            double fd = DerivativeMagnitude(curve, d);
            double fe = DerivativeMagnitude(curve, e);

            double sleft = (h / 12.0) * (fa + 4.0 * fd + fc);
            double sright = (h / 12.0) * (fc + 4.0 * fe + fb);
            double s2 = sleft + sright;

            double sqTolerance = tolerance * tolerance;

            // More conservative error estimate for better accuracy
            double error = Math.Abs(s2 - s);
            if (error <= 15.0 * tolerance || h < sqTolerance) // Also stop if interval is too small
                return s2 + (s2 - s) / 15.0; // Richardson extrapolation

            // Subdivide further
            return AdaptiveSimpsonArcLengthRecursive(curve, a, c, tolerance / 2.0, sleft, fa, fc, fd, depth - 1) +
                   AdaptiveSimpsonArcLengthRecursive(curve, c, b, tolerance / 2.0, sright, fc, fb, fe, depth - 1);
        }

        /***************************************************/

        [Description("Computes the magnitude of the first derivative of a NurbsCurve at parameter t.")]
        private static double DerivativeMagnitude(NurbsCurve curve, double t)
        {
            // Get the unnormalized first derivative vector at parameter t
            List<Vector> derivatives = curve.DerivativesAtParameter(1, t, true);
            if (derivatives == null || derivatives.Count < 2)
                return 0.0;

            Vector firstDerivative = derivatives[1]; // Index 1 is the first derivative
            if (firstDerivative == null)
                return 0.0;

            // Return the magnitude of the derivative vector
            return firstDerivative.Length();
        }

        /***************************************************/

        [Description("Checks if a NURBS curve has smooth derivatives by sampling derivative magnitudes.")]
        private static bool HasSmoothDerivatives(NurbsCurve curve, double tolerance)
        {
            // Sample derivative magnitudes at several points to assess smoothness
            int numSamples = 101; // Sample at 0, 0.125, 0.25, 0.375, 0.5, 0.625, 0.75, 0.875, 1.0
            double[] derivMagnitudes = new double[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double t = i / (double)(numSamples - 1);
                derivMagnitudes[i] = DerivativeMagnitude(curve, t);
            }

            // Check for smoothness criteria:
            // 1. No zero or very small derivatives (indicates potential singularities)
            double minDerivMag = derivMagnitudes.Min();
            if (minDerivMag < tolerance) // Allow some small derivatives but not near-zero
                return false;

            // 2. Relatively consistent derivative magnitudes (smooth variation)
            double maxDerivMag = derivMagnitudes.Max();
            double derivativeRatio = maxDerivMag / minDerivMag;

            // If derivative magnitude varies by more than factor of 10, consider it non-smooth
            if (derivativeRatio > 10.0)
                return false;

            // 3. Check for rapid changes between adjacent samples
            for (int i = 1; i < numSamples; i++)
            {
                double ratio = Math.Max(derivMagnitudes[i], derivMagnitudes[i - 1]) /
                              Math.Min(derivMagnitudes[i], derivMagnitudes[i - 1]);

                // If adjacent derivatives differ by more than factor of 3, consider non-smooth
                if (ratio > 3.0)
                    return false;
            }

            // If all checks pass, the curve has smooth derivatives
            return true;
        }

        /***************************************************/

        [Description("Computes arc length using Gauss-Legendre quadrature with adaptive subdivision.")]
        private static double GaussLegendreArcLength(NurbsCurve curve, double a, double b, double tolerance, int numPoints)
        {
            // Gauss-Legendre nodes and weights for different orders
            double[] nodes, weights;
            GetGaussLegendreNodesAndWeights(numPoints, out nodes, out weights);

            double sum = 0.0;
            double halfRange = (b - a) * 0.5;
            double midPoint = (a + b) * 0.5;

            // Transform integration interval [a,b] to [-1,1] and integrate
            for (int i = 0; i < numPoints; i++)
            {
                double t = midPoint + halfRange * nodes[i];
                double derivMag = DerivativeMagnitude(curve, t);
                sum += weights[i] * derivMag;
            }

            double result = halfRange * sum;

            // Adaptive refinement: compare with lower-order quadrature
            if (numPoints >= 8)
            {
                double coarseResult = GaussLegendreArcLength(curve, a, b, tolerance * 10, numPoints / 2);
                double error = Math.Abs(result - coarseResult);

                if (error > tolerance && (b - a) > tolerance)
                {
                    // Subdivide the interval
                    double mid = (a + b) * 0.5;
                    double leftResult = GaussLegendreArcLength(curve, a, mid, tolerance * 0.5, numPoints);
                    double rightResult = GaussLegendreArcLength(curve, mid, b, tolerance * 0.5, numPoints);
                    return leftResult + rightResult;
                }
            }

            return result;
        }

        /***************************************************/

        [Description("Gets Gauss-Legendre quadrature nodes and weights for numerical integration.")]
        private static void GetGaussLegendreNodesAndWeights(int n, out double[] nodes, out double[] weights)
        {
            // Pre-computed Gauss-Legendre nodes and weights for common orders
            switch (n)
            {
                case 4:
                    nodes = new double[] { -0.8611363115940526, -0.3399810435848563, 0.3399810435848563, 0.8611363115940526 };
                    weights = new double[] { 0.3478548451374538, 0.6521451548625461, 0.6521451548625461, 0.3478548451374538 };
                    break;
                case 8:
                    nodes = new double[] { -0.9602898564975363, -0.7966664774136267, -0.5255324099163290, -0.1834346424956498,
                                          0.1834346424956498, 0.5255324099163290, 0.7966664774136267, 0.9602898564975363 };
                    weights = new double[] { 0.1012285362903763, 0.2223810344533745, 0.3137066458778873, 0.3626837833783620,
                                           0.3626837833783620, 0.3137066458778873, 0.2223810344533745, 0.1012285362903763 };
                    break;
                case 16:
                    nodes = new double[] { -0.9894009349916499, -0.9445750230732326, -0.8656312023878318, -0.7554044083550030,
                                          -0.6178762444026438, -0.4580167776572274, -0.2816035507792589, -0.0950125098376374,
                                          0.0950125098376374, 0.2816035507792589, 0.4580167776572274, 0.6178762444026438,
                                          0.7554044083550030, 0.8656312023878318, 0.9445750230732326, 0.9894009349916499 };
                    weights = new double[] { 0.0271524594117541, 0.0622535239386479, 0.0951585116824928, 0.1246289712555339,
                                            0.1495959888165767, 0.1691565193950025, 0.1826034150449236, 0.1894506104550685,
                                            0.1894506104550685, 0.1826034150449236, 0.1691565193950025, 0.1495959888165767,
                                            0.1246289712555339, 0.0951585116824928, 0.0622535239386479, 0.0271524594117541 };
                    break;
                default:
                    // Fallback to 4-point quadrature
                    nodes = new double[] { -0.8611363115940526, -0.3399810435848563, 0.3399810435848563, 0.8611363115940526 };
                    weights = new double[] { 0.3478548451374538, 0.6521451548625461, 0.6521451548625461, 0.3478548451374538 };
                    break;
            }
        }

        /***************************************************/

        [Description("Calculates the square binomial cooefficients used in the infinite series of the ellipse length calculation.")]
        private static List<double> SquareBinomialCoefficientsEllipse(int requiredCount)
        {
            //Only allow for one thread to modify the cache at once.
            lock (m_CooeficientLock)
            {
                //Calcuation of coefficients only needs to be done once.
                //Coefficients are being stored for next ellipse to be evaluated

                //Check if coefficients up to the count required have already been added
                if (requiredCount > m_EllipseCoeficients.Count)
                {
                    //Limit the count to 100 000 to avoid being locked for to long time.
                    //For common cases (ratios of less than 1:100) 1000 easily enough, but allowing a larger limit.
                    //The set limit of 100 000 should give accurate results for ellipses with a ratio of up to 1:20 000 between the long and short radius.
                    int addCount = Math.Min(requiredCount, 100000);
                    int currentCount = m_EllipseCoeficients.Count;
                    for (double i = currentCount; i < addCount; i++)
                    {
                        double binomialCoef = 1;

                        List<double> js = new List<double>();

                        //Construction a series to evaluate, counting first, last, send first, second last etc.
                        //This is done to avoid numeric overflow of the binomial cooefficient, as it for get to big if this is done
                        //For i over 1000 without this technique.
                        if (i % 2 == 0)
                        {
                            double j = 1;
                            while (js.Count < i)
                            {
                                js.Add(j);
                                js.Add(i - j + 1);
                                j++;
                            }
                        }
                        else
                        {
                            double j = 1;
                            while (js.Count < i - 1)
                            {
                                js.Add(j);
                                js.Add(i - j + 1);
                                j++;
                            }
                            js.Add(i / 2 + 0.5);
                        }

                        //Calculate the cooeficient for the current i
                        foreach (double j in js)
                        {
                            binomialCoef *= (0.5 - (i - j)) / j;
                        }

                        //The square of the cooeficcient is used for the length calculation, hence storing the square cooeficient
                        binomialCoef *= binomialCoef;

                        m_EllipseCoeficients.Add(binomialCoef);
                    }
                }
            }
            return m_EllipseCoeficients;
        }

        /***************************************************/

        private static List<double> m_EllipseCoeficients = new List<double>();
        private static object m_CooeficientLock = new object();

        /***************************************************/
    }
}