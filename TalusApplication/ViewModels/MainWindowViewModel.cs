using libxl;
using MathNet.Numerics;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using ScottPlot;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Input;

namespace TalusApplication.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public WpfPlot Series { get; private set; }
        public PlottableScatter DataPlot { get; private set; }
        public string Title { get; set; } = "OrionLab - Talus Application";

        public ObservableCollection<Tuple<double, double>> Data { get; private set; }

        private string _pointLabel;

        public string PointLabel
        {
            get { return _pointLabel; }
            set { SetProperty(ref _pointLabel, value); }
        }

        private double _clayFractionOfWholeSample;

        public double ClayFractionOfWholeSample
        {
            get { return _clayFractionOfWholeSample; }
            set { SetProperty(ref _clayFractionOfWholeSample, value); }
        }

        private double _piOfWholeSample;

        public double PiOfWholeSample
        {
            get { return _piOfWholeSample; }
            set { SetProperty(ref _piOfWholeSample, value); }
        }

        private double _xMax = 80;

        public double XMax
        {
            get { return _xMax; }
            set { SetProperty(ref _xMax, value); }
        }

        private double _yMax = 60;

        public double YMax
        {
            get { return _yMax; }
            set { SetProperty(ref _yMax, value); }
        }

        public ICommand AddDataCommand { get; private set; }
        public ICommand ClearDataCommand { get; private set; }

        public ICommand ExportCommand { get; private set; }

        public MainWindowViewModel()
        {
            Data = new ObservableCollection<Tuple<double, double>>();

            Series = new WpfPlot();
            Series.plt.Title("Potential Expansivess", bold: true, fontSize: 22);
            Series.plt.XLabel("Clay fraction of whole sample", bold: true);
            Series.plt.YLabel("PI of whole sample", bold: true);
            

            Series.plt.AxisBounds(0, _xMax, 0, _yMax);
            Series.Configure(lockHorizontalAxis: true, lockVerticalAxis: true);
            Series.plt.Grid(xSpacing: 10, ySpacing: 10, lineWidth: 2);
           // Series.plt.AxisZoom(0, 0, -20, -20);
            //Series.Width = 800;

            AddDataCommand = new DelegateCommand(() =>
            {
                Data.Add(new Tuple<double, double>(ClayFractionOfWholeSample, PiOfWholeSample));

                Series.plt.PlotPoint(ClayFractionOfWholeSample, PiOfWholeSample, GetColor(), 15);
                Series.plt.PlotText(_pointLabel, ClayFractionOfWholeSample - 0.5, PiOfWholeSample + 1, System.Drawing.Color.Black);

                Series.Render();
            });

            ClearDataCommand = new DelegateCommand(() =>
            {
                ClayFractionOfWholeSample = 0;
                PiOfWholeSample = 0;

                Data.Clear();
                Series.plt.Clear();
                DrawAbaque();
                Series.plt.AxisBounds(0, _xMax, 0, _yMax);
                Series.Render();
            });

            ExportCommand = new DelegateCommand(()=>
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "Excel file (*.xls)|*.xls";
                dialog.ShowDialog();
                ;

                var fileName = dialog.FileName;

                if (String.IsNullOrEmpty(fileName))
                {
                    return;
                }

                Series.plt.SaveFig($"{fileName}.png");

                var book = new BinBook();

                var id = book.addPicture(fileName + ".png");

                var sheet = book.addSheet("Chart");
                sheet.setPicture(10, 3, id);

                book.save(fileName);
            });

            DrawAbaque();
        }




        private void DrawAbaque()
        {
            var plt = Series.plt;
            plt.Clear();

            plt.PlotLine(12, _yMax, 12, 10, color: System.Drawing.Color.Red, lineWidth: 2);
            plt.PlotLine(18, _yMax, 18, 20, color: System.Drawing.Color.Red, lineWidth: 2);
            plt.PlotLine(38, YMax, 38, 30, color: System.Drawing.Color.Red, lineWidth: 2);

            plt.PlotLine(12, 10, 25, 10, color: System.Drawing.Color.Red, lineWidth: 2);
            plt.PlotLine(18, 20, 38, 20, color: System.Drawing.Color.Red, lineWidth: 2);
            plt.PlotLine(38, 30, 46, 30, color: System.Drawing.Color.Red, lineWidth: 2);

            plt.PlotLine(25, 10, _xMax, 38, color: System.Drawing.Color.Red, lineWidth: 2);
            plt.PlotLine(38, 20, _xMax, 45, color: System.Drawing.Color.Red, lineWidth: 2);
            plt.PlotLine(46, 30, _xMax, 55, color: System.Drawing.Color.Red, lineWidth: 2);

            //

            plt.PlotText("LOW", 7, 15, System.Drawing.Color.Black, fontSize: 16, bold: true, rotation: 90);
            plt.PlotText("MEDIUM", 16, _yMax / 2 + 10, System.Drawing.Color.Black, fontSize: 16, bold: true, rotation: 90);
            plt.PlotText("HIGH", 26, _yMax / 2 + 10, System.Drawing.Color.Black, fontSize: 16, bold: true, rotation: 90);
            plt.PlotText("VERY HIGH", 56, _yMax / 2 + 15, System.Drawing.Color.Black, fontSize: 16, bold: true, rotation: 90);

           
        }

        private System.Drawing.Color GetColor()
        {
            bool IsLow()
            {

                var coefs = Fit.Line(new double[] {25,_xMax }, new double[] { 10,38});
                var p = new Polynomial(new double[] { coefs.Item1, coefs.Item2 });



                var ok = false;
                if (ClayFractionOfWholeSample<12 || (ClayFractionOfWholeSample>12 && PiOfWholeSample < 10) || PiOfWholeSample < p.Evaluate(ClayFractionOfWholeSample))
                {
                    ok = true;
                }

                return ok;
            }

            bool IsMedium()
            {
                var coefs = Fit.Line(new double[] { 38, _xMax }, new double[] { 20, 45 });
                var p = new Polynomial(new double[] { coefs.Item1, coefs.Item2 });
                var ok = false;
                if ((ClayFractionOfWholeSample > 12 && ClayFractionOfWholeSample < 18) || (ClayFractionOfWholeSample > 18 && PiOfWholeSample < 20) || PiOfWholeSample < p.Evaluate(ClayFractionOfWholeSample))
                {
                    ok = true;
                }
                return ok;
            }

            bool IsHigh()
            {
                var ok = false;
                var coefs = Fit.Line(new double[] { 46, _xMax }, new double[] { 30, 55 });
                var p = new Polynomial(new double[] { coefs.Item1, coefs.Item2 });

                if ((ClayFractionOfWholeSample > 18 && ClayFractionOfWholeSample < 38) || (ClayFractionOfWholeSample > 38 && PiOfWholeSample <30) || PiOfWholeSample < p.Evaluate(ClayFractionOfWholeSample))
                {
                    ok = true;
                }

                return ok;
            }

            if (IsLow())
            {
                return System.Drawing.Color.Green;
            }
            else if (IsMedium())
            {
                return System.Drawing.Color.Blue;
            }
            else if (IsHigh())
            {
                return System.Drawing.Color.Yellow;
            }

            return System.Drawing.Color.Red;
        }
    }
}