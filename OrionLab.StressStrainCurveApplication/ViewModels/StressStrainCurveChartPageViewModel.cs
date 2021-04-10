using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;

namespace OrionLab.StressStrainCurveApplication.ViewModels
{
    public class StressStrainCurveChartPageViewModel : BindableBase
    {
        private string _pageTitle;

        public string PageTitle
        {
            get { return _pageTitle; }
            set { SetProperty(ref _pageTitle, value); }
        }

       

        public StressStrainCurveChartPageViewModel()
        {
            PageTitle = "Stress / Strain Curve";

           
        }
    }
}