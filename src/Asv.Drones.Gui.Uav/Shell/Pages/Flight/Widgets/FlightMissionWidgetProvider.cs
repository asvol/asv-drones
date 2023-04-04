using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using DynamicData;

namespace Asv.Drones.Gui.Uav
{
    [Export(FlightPageViewModel.UriString, typeof(IViewModelProvider<IMapWidget>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightMissionWidgetProvider:ViewModelProviderBase<IMapWidget>
    {
        [ImportingConstructor]
        public FlightMissionWidgetProvider(
            IMavlinkDevicesService devices,ILogService log,
            ILocalizationService localization,
            [ImportMany]IEnumerable<IUavRttItemProvider> rttItems)
        {
            devices.Vehicles
                .Transform(_ => (IMapWidget)new FlightUavViewModel(_, log, localization, rttItems))
                .ChangeKey( ((_, v) => v.Id) )
                .PopulateInto(Source);
        }
    }
}