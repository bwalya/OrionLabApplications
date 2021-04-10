using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using OrionLab.Common.Math;
using OrionLab.Common.Regression;
using OrionLab.StressStrainCurveApplication.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace OrionLab.StressStrainCurveApplication.ViewModels
{
    public class HomePageViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        private double _currentXData;
        private double _currentYData;

        private readonly LineSeries _axialDataPlot;
        private readonly LineSeries _radialDataPlot;
        private readonly LineSeries _axialCurvePlot;
        private readonly LineSeries _radialCurvePlot;
        private readonly LineSeries _generatedCurveDataPlot;
        private readonly LineSeries _tgPlot;
        private readonly HashSet<ObservablePoint> _axialData;
        private readonly HashSet<ObservablePoint> _radialData;
        public SeriesCollection StressStrainCurveSeries { get; set; }

        private int _polynomDegree;

        public int PolynomDegree
        {
            get { return _polynomDegree; }
            set { SetProperty(ref _polynomDegree, value); SmoothData(); }
        }

        public int GeneratedDataNoise { get; set; } = 1;

        private double _inputMicrostrain;

        public double InputMicrostrain
        {
            get { return _inputMicrostrain; }
            set { SetProperty(ref _inputMicrostrain, value); }
        }

        private double _inputStress;

        public double InputStress
        {
            get { return _inputStress; }
            set { SetProperty(ref _inputStress, value); }
        }

        private double _editMicrostrain;

        public double EditMicrostrain
        {
            get { return _editMicrostrain; }
            set { SetProperty(ref _editMicrostrain, value); }
        }

        private double _editStress;

        public double EditStress
        {
            get { return _editStress; }
            set { SetProperty(ref _editStress, value); }
        }

        private ObservableCollection<ObservablePoint> _data;

        public ObservableCollection<ObservablePoint> Data
        {
            get;
            set;
        }

        public ICommand StressStrainCurveModeCommand { get; private set; }

        public ICommand PoissonsRatioStressCurveModeCommand { get; private set; }

        public ICommand ModulusStressCurveModeCommand { get; set; }
        public ICommand AddDataCommand { get; private set; }
        public ICommand EditDataCommand { get; private set; }

        public ICommand MouseDoubleClickCommand { get; private set; }

        public ICommand SmoothDataCommand { get; private set; }

        public ICommand ResetCommand { get; private set; }

        public ICommand DrawTangentCommand { get; private set; }

        public ICommand GenerateDataCommand { get; private set; }

        public HomePageViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            Data = new ObservableCollection<ObservablePoint>();
            _axialData = new HashSet<ObservablePoint>();
            _axialDataPlot = new LineSeries
            {
                Title = "Axial",
                Fill = Brushes.Transparent,
                Stroke = Brushes.Black,
                LineSmoothness = 0,
                StrokeDashArray = new DoubleCollection { 2 },
                Values = new ChartValues<ObservablePoint>(),
                DataLabels = true

            };

            _radialData = new HashSet<ObservablePoint>();
            _radialDataPlot = new LineSeries
            {
                Title = "Radial Data",
                Fill = Brushes.Transparent,
                Stroke = Brushes.Orange,
                LineSmoothness = 0,
                StrokeDashArray = new DoubleCollection { 2 },
                Values = new ChartValues<ObservablePoint>()
            };

            _radialCurvePlot = new LineSeries
            {
                Title = "Radial Curve"
            };

            _axialCurvePlot = new LineSeries
            {
                Title = "Axial",
                Fill = Brushes.Transparent,
                Stroke = Brushes.Red,
                LineSmoothness = 0.2,
                Values = new ChartValues<ObservablePoint>()
            };

            _tgPlot = new LineSeries
            {
                Title = "Tg",
                Fill = Brushes.Transparent,
                Stroke = Brushes.Green,
                Values = new ChartValues<ObservablePoint>()
            };

            _generatedCurveDataPlot = new LineSeries
            {
                Title = "Generated Data",
                Fill = Brushes.Transparent,
                Stroke = Brushes.Magenta,
                PointGeometrySize = 0.1,
                Values = new ChartValues<ObservablePoint>()
            };

            StressStrainCurveSeries = new SeriesCollection();
            StressStrainCurveSeries.Add(_axialDataPlot);
            StressStrainCurveSeries.Add(_radialDataPlot);
            StressStrainCurveSeries.Add(_axialCurvePlot);
            StressStrainCurveSeries.Add(_tgPlot);
            StressStrainCurveSeries.Add(_generatedCurveDataPlot);

            //Commands

            StressStrainCurveModeCommand = new DelegateCommand(() =>
            {
                _regionManager.RequestNavigate(Configurations.Regions.ChartRegion, nameof(StressStrainCurveChartPage));
            });

            PoissonsRatioStressCurveModeCommand = new DelegateCommand(() =>
            {
                _regionManager.RequestNavigate(Configurations.Regions.ChartRegion, nameof(PoissonsRatioStressCurveChartPage));
            });

            ModulusStressCurveModeCommand = new DelegateCommand(() =>
            {
                _regionManager.RequestNavigate(Configurations.Regions.ChartRegion, nameof(ModulusStressCurveChartPage));
            });

            AddDataCommand = new DelegateCommand(() => AddData(_inputMicrostrain, _inputStress));
            EditDataCommand = new DelegateCommand<ChartPoint>(p =>

            {
                EditMicrostrain = p.X;
                EditStress = p.Y;
            });

            MouseDoubleClickCommand = new DelegateCommand(() =>
            {
                AddData(_currentXData, _currentYData);
            });

            SmoothDataCommand = new DelegateCommand(async () => await SmoothData());

            ResetCommand = new DelegateCommand(() =>
            {
                Data.Clear();
                _axialData.Clear();
                _axialDataPlot.Values.Clear();
                _axialCurvePlot.Values.Clear();
                _tgPlot.Values.Clear();
                _generatedCurveDataPlot.Values.Clear();
                _radialData.Clear();
                _radialDataPlot.Values.Clear();
            });

            DrawTangentCommand = new DelegateCommand<bool?>(v =>
            {
                _tgPlot.Values.Clear();

                _tgPlot.Visibility = (bool)v ? Visibility.Visible : Visibility.Hidden;
                if ((bool)v)
                {
                    var x = _axialData.OrderBy(d => d.X).Select(d => d.X).ToArray();
                    var y = _axialData.OrderBy(d => d.X).Select(d => d.Y).ToArray();
                    var t = PolynomialRegression.LeastSquartPolynonial(x, y, PolynomDegree);

                    var tgY = Tangent.Y(t, _axialData.OrderBy(p => p.X).Last().X);

                    _tgPlot.Values.Add(new ObservablePoint { X = 0, Y = 0 });
                    _tgPlot.Values.Add(new ObservablePoint { X = _axialData.OrderBy(p => p.X).Last().X, Y = tgY });
                }
            });

            GenerateDataCommand = new DelegateCommand(() =>
            {
                _generatedCurveDataPlot.Values.Clear();
                var x = _axialData.OrderBy(d => d.X).Select(d => d.X).ToArray();
                var y = _axialData.OrderBy(d => d.X).Select(d => d.Y).ToArray();
                var t = PolynomialRegression.LeastSquartPolynonial(x, y, PolynomDegree);
                Task.Run(() =>
                {
                    var generatedData = PolynomialRegression.Generate(t, 50, _axialData.OrderBy(d => d.X).Last().X, GeneratedDataNoise);
                    var strain = generatedData.Item1;
                    var stress = generatedData.Item2;
                    for (int i = 0; i < strain.Length; i++)
                    {
                        _generatedCurveDataPlot.Values.Add(new ObservablePoint { X = strain[i], Y = stress[i] });
                    }
                });
            });
        }

        private async Task SmoothData()
        {
            await Task.Run(() =>
            {
                _axialCurvePlot.Values.Clear();
                var x = _axialData.OrderBy(d => d.X).Select(d => d.X).ToArray();
                var y = _axialData.OrderBy(d => d.X).Select(d => d.Y).ToArray();
                var result = PolynomialRegression.Y(x, y, PolynomDegree);

                for (int i = 0; i < result.Item2.Length; i++)
                {
                    _axialCurvePlot.Values.Add(new ObservablePoint { X = x[i], Y = result.Item2[i] });
                }
            });
        }

        private void AddData(double microstrain, double stress)
        {
            var newData = new ObservablePoint { X = microstrain, Y = stress };
            Data.Add(newData);

            if (newData.X == 0)
            {
                _axialData.Add(newData);
                _radialData.Add(newData);
                _radialDataPlot.Values.Clear();
                _radialDataPlot.Values.AddRange(_radialData);
                _axialDataPlot.Values.Clear();
                _axialDataPlot.Values.AddRange(_axialData.OrderBy(d => d.X));
            }
            else if (newData.X < 0)
            {
                _radialData.Add(newData);

                _radialDataPlot.Values.Clear();
                _radialDataPlot.Values.AddRange(_radialData.OrderBy(d => d.X));
            }
            else
            {
                _axialData.Add(newData);
                _axialDataPlot.Values.Clear();
                _axialDataPlot.Values.AddRange(_axialData.OrderBy(d => d.X));
            }
        }

        public void AddCursorData(double x, double y)
        {
            _currentXData = Math.Round(x, 2);
            _currentYData = Math.Round(y, 2);
        }
    }
}