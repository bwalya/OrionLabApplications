using LiveCharts.Wpf;
using OrionLab.StressStrainCurveApplication.ViewModels;
using System.Windows.Controls;

namespace OrionLab.StressStrainCurveApplication.Views
{
    /// <summary>
    /// Interaction logic for HomePage
    /// </summary>
    public partial class HomePage : UserControl
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void Chart_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var point = Chart.ConvertToChartValues(e.GetPosition(Chart));
            var vm = (HomePageViewModel)this.DataContext;
            vm.AddCursorData(point.X, point.Y);
        }
    }
}