using Prism.Mvvm;

namespace OrionLab.StressStrainCurveApplication.ViewModels
{
    public class ModulusStressCurveChartPageViewModel : BindableBase
    {
        public string PageTitle { get; set; } = "Modulus (E ) / Stress Curve";
        public ModulusStressCurveChartPageViewModel()
        {
        }
    }
}