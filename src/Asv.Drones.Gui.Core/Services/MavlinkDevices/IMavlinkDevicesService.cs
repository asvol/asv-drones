﻿using Asv.Common;
using Asv.Mavlink;
using DynamicData;

namespace Asv.Drones.Gui.Uav
{
    public interface IMavlinkDevicesService
    {
        /// <summary>
        /// Collection with all devices in network
        /// </summary>
        IObservable<IChangeSet<IMavlinkDevice, ushort>> Devices { get; }
        /// <summary>
        /// Timeout for device connection. If device not response in this time, device will be removed from collection
        /// </summary>
        IRxEditableValue<TimeSpan> DeviceTimeout { get; }
        /// <summary>
        /// Mavlink router
        /// </summary>
        IMavlinkRouter Router { get; }
        /// <summary>
        /// Specify that need reload app to apply new config
        /// </summary>
        IRxValue<bool> NeedReloadToApplyConfig { get; }
        /// <summary>
        /// ComponentId identifier of this app in mavlink network
        /// </summary>
        IRxEditableValue<byte> ComponentId { get; }
        /// <summary>
        /// SystemId identifier of this app in mavlink network
        /// </summary>
        IRxEditableValue<byte> SystemId { get; }
        /// <summary>
        /// Rate of heartbeat packets for sending to network
        /// </summary>
        IRxEditableValue<TimeSpan> HeartbeatRate { get; }
        /// <summary>
        /// List of all founded vehicles in network
        /// </summary>
        IObservable<IChangeSet<IVehicleClient, ushort>> Vehicles { get; }
        /// <summary>
        /// Gets vehicle by it's id
        /// </summary>
        /// <param name="id">Id of searched vehicle</param>
        /// <returns>Vehicle object</returns>
        public IVehicleClient? GetVehicleByFullId(ushort id);
        IObservable<IChangeSet<IGbsClientDevice, ushort>> BaseStations { get; }
        public IGbsClientDevice? GetGbsByFullId(ushort id);
        IObservable<IChangeSet<ISdrClientDevice, ushort>> Payloads { get; }
        public ISdrClientDevice? GetPayloadsByFullId(ushort id);
        public IObservable<IChangeSet<IAdsbClientDevice, ushort>> AdsbDevices { get; }
        public IAdsbClientDevice? GetAdsbVehicleByFullId(ushort id);
    }

    
}