using Prism.Mvvm;

namespace OrionLab.StressStrainCurveApplication.ViewModels
{
    public class PoissonsRatioStressCurveChartPageViewModel : BindableBase
    {

        public string PageTitle { get; set; } = "Poisson's Ratio (v) / Stress Curve";
        public PoissonsRatioStressCurveChartPageViewModel()
        {
        }
    }
}