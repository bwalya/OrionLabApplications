using libxl;
using MathNet.Numerics;
using Prism.Commands;
using Prism.Mvvm;
using ScottPlot;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Expr = MathNet.Symbolics.SymbolicExpression;

namespace AtterbergLimitsApplication.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private PlottableScatter _dataPlot;
        private PlottableScatter _regressionPlot;

        public ObservableCollection<Tuple<double, double>> Data { get; set; }

        public string Title { get; set; } = "Limites d'Atterberg";
        private int _numberOfBows;

        public int NumberOfBlows
        {
            get { return _numberOfBows; }
            set { SetProperty(ref _numberOfBows, value); }
        }

        private string _moistureContent;

        public string MoistureContent
        {
            get { return _moistureContent; }
            set { SetProperty(ref _moistureContent, value); }
        }

        public WpfPlot Series { get; private set; }

        public ICommand AddDataCommand { get; private set; }
        public ICommand ClearDataCommand { get; private set; }

        public ICommand ExportCommand { get; private set; }

        public MainWindowViewModel()
        {
            MoistureContent = "0";
            Data = new ObservableCollection<Tuple<double, double>>()
            {
            };

            Series = new WpfPlot();
            Series.plt.Title("Liquid limit test", fontSize: 20);
            Series.plt.XLabel("Number of blows (N)", bold: true);
            Series.plt.YLabel("Moisture content (%)", bold: true);
            Series.plt.Ticks(logScaleY: true);
            Series.Configure(lockHorizontalAxis: true, lockVerticalAxis: true);

            Series.plt.AxisAuto(horizontalMargin: 0, verticalMargin: 0.9);

            //_dataPlot = Series.plt.PlotScatter(_data.OrderBy(d => d.Item1).Select(d => d.Item1).ToArray(), _data.OrderBy(d => d.Item1).Select(d => d.Item2).ToArray());

            ClearDataCommand = new DelegateCommand(() =>
            {
                NumberOfBlows = 0;
                MoistureContent = "";
                Data.Clear();
                Series.plt.Clear();

                if (_dataPlot == null)
                {
                    _dataPlot = Series.plt.PlotScatter(Data.OrderBy(d => d.Item1).Select(d => d.Item1).ToArray(), Data.OrderBy(d => d.Item1).Select(d => d.Item2).ToArray());
                }
            });

            AddDataCommand = new DelegateCommand(() =>
            {
                Data.Add(new Tuple<double, double>(NumberOfBlows, Double.Parse(MoistureContent)));
                if (Data.Count() >= 2)
                {
                    Series.plt.Clear();
                    _dataPlot = Series.plt.PlotScatter(Data.OrderBy(d => d.Item1).Select(d => d.Item1).ToArray(), Data.OrderBy(d => d.Item1).Select(d => d.Item2).ToArray(), color: System.Drawing.Color.Gray, lineStyle: LineStyle.Dash);

                    DrawRegressionLine();
                }

                NumberOfBlows = 0;
                MoistureContent = "0";
            });

            ExportCommand = new DelegateCommand(() =>
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
        }

        public void DrawRegressionLine()
        {
            var theta = Fit.Line(Data.OrderBy(d => d.Item1).Select(d => d.Item1).ToArray(), Data.OrderBy(d => d.Item1).Select(d => d.Item2).ToArray());

            var x = Expr.Variable("x");
            var a = Expr.Variable("a");
            var b = Expr.Variable("b");

            a = System.Math.Round(theta.Item2, 2);
            b = System.Math.Round(theta.Item1, 2);

            Func<double, double> fx = (a * x + b).Compile("x");

            if (_regressionPlot != null)
            {
                Series.plt.Remove(_regressionPlot);
            }

            var x1 = Data.OrderBy(d => d.Item1).First().Item1;
            var y1 = fx(Data.OrderBy(d => d.Item1).First().Item1);
            var x2 = Data.OrderBy(d => d.Item1).Last().Item1;
            var y2 = fx(Data.OrderBy(d => d.Item1).Last().Item1);

            _regressionPlot = Series.plt.PlotLine(x1, y1, x2, y2, color: System.Drawing.Color.Red);

            Series.plt.PlotText($"{System.Math.Round(fx(25), 2)}", 17, fx(25), System.Drawing.Color.Red, fontSize: 16);
            Series.Render();

            ;
        }
    }
}