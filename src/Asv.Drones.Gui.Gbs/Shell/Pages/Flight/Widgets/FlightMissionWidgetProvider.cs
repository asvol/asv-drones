using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using DynamicData;

namespace Asv.Drones.Gui.Gbs
{
    [Export(FlightPageViewModel.UriString, typeof(IViewModelProvider<IMapWidget>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightMissionWidgetProvider:ViewModelProviderBase<IMapWidget>
    {
        [ImportingConstructor]
        public FlightMissionWidgetProvider(
            IMavlinkGbsService devices,ILogService log,
            ILocalizationService localization,
            IConfiguration configuration,
            [ImportMany]IEnumerable<IGbsRttItemProvider> rttItems)
        {
            devices.BaseStations
                .Transform(_ => (IMapWidget)new FlightGbsViewModel(_,log, localization,configuration,rttItems))
                .ChangeKey( ((_, v) => v.Id) )
                .PopulateInto(Source);
        }
    }
}