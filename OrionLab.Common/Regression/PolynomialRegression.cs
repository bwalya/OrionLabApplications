using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrionLab.Common.Regression
{
    public static class PolynomialRegression
    {
        private static Vector<double> Model(Matrix<double> x, Vector<double> theta) => x.Multiply(theta);

        private static Vector<double> Grad(Matrix<double> x, Vector<double> y, Vector<double> theta)
        {
            var m = (double)y.Count();

            return x.TransposeThisAndMultiply(Model(x, theta).Subtract(y)) * 1.0 / m;
        }

        private static Vector<double> GradientDescent(Matrix<double> x, Vector<double> y, Vector<double> theta)
        {
            var tmpTheta = theta.Clone();
            var learningRate = 1e-5;
            for (int i = 0; i < 900000; i++)
            {
                tmpTheta = tmpTheta - learningRate * Grad(x, y, tmpTheta);
            }

            //var t =  Vector<double>.Build.Dense(new double[] { 1, 2, 3 }) * Vector<double>.Build.Dense(new double[] { 3, 4, 5 });

            return tmpTheta;
        }

        private static double CostFunction(Matrix<double> x, Vector<double> y, Vector<double> theta)
        {
            return (Model(x, theta) - y).PointwisePower(2).Sum() / 2.0 * y.Count;
        }

        public static double R2(Vector<double> y, Vector<double> prediction)
        {
            var u = (y - prediction).PointwisePower(2.0).Sum();
            var v = (y - y.Sum() / y.Count).PointwisePower(2.0).Sum();

            return 1.0 - u / v;
        }

        public static (double, double, double[]) Y(double[] x, double[] y)
        {
            var result = new ConcurrentBag<(double, double, double[])>();

            var yVector = Vector<double>.Build.Dense(y);
            Parallel.For(1, 11, polynomOrder =>
            {
                var xMatrix = Matrix<double>.Build.Dense(x.Length, polynomOrder + 1, 1);
                for (int i = 0; i < polynomOrder; i++)
                {
                    xMatrix.SetColumn(i, x.Select(e => System.Math.Pow(e, polynomOrder - i)).ToArray());
                }

                var theta = xMatrix.PseudoInverse().Multiply(yVector);
                var predicted = Model(xMatrix, theta);
                var r2 = R2(yVector, predicted);

                result.Add((polynomOrder, r2, predicted.AsArray()));
            });

            return result.OrderBy(s => s.Item2).Last();
        }

        public static (double, double[]) Y(double[] x, double[] y, int polynomDegree)
        {
            var yVector = Vector<double>.Build.Dense(y);

           
                var xMatrix = Matrix<double>.Build.Dense(x.Length, polynomDegree + 1, 1);
                for (int i = 0; i < polynomDegree; i++)
                {
                    xMatrix.SetColumn(i, x.Select(e => System.Math.Pow(e, polynomDegree - i)).ToArray());
                }

                var theta = xMatrix.PseudoInverse().Multiply(yVector);
                var predicted = Model(xMatrix, theta);
                var r2 = R2(yVector, predicted);


            var thetaTab = theta.AsArray();
            Array.Reverse(thetaTab);

            var p = new MathNet.Numerics.Polynomial(thetaTab);

             var xData = MathNet.Numerics.Generate.LinearRange(3.63, 0.01, 12.55);
             var xDataVector = Vector<double>.Build.Dense(xData).Map(d => System.Math.Round(d,2)).ToHashSet();

            var result = new ConcurrentBag<(double, double)>();
            Parallel.ForEach(xDataVector, data =>
            {


                result.Add((data, p.Evaluate(data)));

            });


                return (r2, predicted.AsArray());
            
        }

            public static double[] LeastSquartPolynonial(double[] x, double[] y, int order)
            {
                var xMatrix = Matrix<double>.Build.Dense(x.Length, order + 1, 1);

                for (int i = 0; i < order; i++)
                {
                    xMatrix.SetColumn(i, x.Select(e => System.Math.Pow(e, order - i)).ToArray());
                }

                var yVector = Vector<double>.Build.Dense(y);

              

                return xMatrix.PseudoInverse().Multiply(yVector).AsArray();
            }

            public static (double[], double[]) Generate(double[] coefs, double step, double max, double noise)
            {

            MathNet.Numerics.Generate.LinearSpaced(11, 0.0, 1.0);

            Array.Reverse(coefs);
                var curve = new MathNet.Numerics.Polynomial(coefs);

                var yData = new List<double>();
                var xData = new List<double>();

                xData.Add(0);
                yData.Add(curve.Evaluate(0));
                for (double x = step; x < max; x = x + step)
                {
                    var y = curve.Evaluate(x);
                    xData.Add(x);
                    yData.Add(new Random((int)x).Next((int)(y - noise), (int)(y + noise)));
                }

                return (xData.ToArray(), yData.ToArray());
            }
        }
    }