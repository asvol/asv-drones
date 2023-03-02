﻿using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class RoiAnchorActionViewModel : UavActionActionBase
    {
        private readonly ILogService _log;

        public RoiAnchorActionViewModel(IVehicle vehicle, IMap map,ILogService log) : base(vehicle, map,log)
        {
            _log = log;
            Title = "Set ROI";
            Icon = MaterialIconKind.ImageFilterCenterFocus;
            Vehicle.IsArmed.ObserveOn(RxApp.MainThreadScheduler).Select(_ => _).Subscribe(CanExecute).DisposeWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            var target = await Map.ShowTargetDialog("Select target for region of interests (ROI)", CancellationToken.None);
            var point = new GeoPoint(target.Latitude, target.Longitude, (double)Vehicle.GlobalPosition.Value.Altitude);
            _log.Info(LogName, $"User set ROI '{point}' for {Vehicle.Name.Value}");
            await Vehicle.SetRoi(point, CancellationToken.None);
        }
    }
}