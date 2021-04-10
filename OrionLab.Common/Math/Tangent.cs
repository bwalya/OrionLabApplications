using MathNet.Numerics;

using System;
using System.Collections.Generic;
using Expr = MathNet.Symbolics.SymbolicExpression;

namespace OrionLab.Common.Math
{
    public static class Tangent
    {
        public static double Y(double[] coefs, double pX)
        {
            Array.Reverse(coefs);

            var x = Expr.Variable("x");

            var curve = Expr.Parse(new Polynomial(coefs).ToString().Replace(",", ".").Replace("x", "*x"));
            var tang = curve.Differentiate(x) * x;

            Func<double, double> fx = (curve).Compile("x");

            var xResult = fx(5);

            var xx = FindRoots.OfFunction(fx,-5,5);

            return tang.Evaluate(new Dictionary<string, MathNet.Symbolics.FloatingPoint> { { "x", pX } }).RealValue;
        }
    }
}