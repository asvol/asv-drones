using Asv.Common;
using Asv.Drones.Gui.Core;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class DGpsRateGbsRttViewModel : GbsRttItem
{
    public DGpsRateGbsRttViewModel()
    {
        DGpsRate = "30Kb/s";
    }

    public DGpsRateGbsRttViewModel(IGbsDevice gbs, ILocalizationService localizationService) : base(gbs, GenerateUri(gbs,"dgpsrate"))
    {
        Order = 2;

        Gbs.MavlinkClient.Gbs.Status
            .Subscribe(_ => DGpsRate = localizationService.ByteRate.ConvertToStringWithUnits(_.DgpsRate))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public string DGpsRate { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}