using OrionLab.StressStrainCurveApplication.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace OrionLab.StressStrainCurveApplication
{
    public class MainModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();

            regionManager.RegisterViewWithRegion("ContentRegion", typeof(HomePage));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation(typeof(StressStrainCurveChartPage), nameof(StressStrainCurveChartPage));
            containerRegistry.RegisterForNavigation(typeof(PoissonsRatioStressCurveChartPage), nameof(PoissonsRatioStressCurveChartPage));
            containerRegistry.RegisterForNavigation(typeof(ModulusStressCurveChartPage), nameof(ModulusStressCurveChartPage));
        }
    }
}