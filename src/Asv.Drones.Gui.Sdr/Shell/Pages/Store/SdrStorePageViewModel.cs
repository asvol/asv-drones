using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrStorePageViewModel:ShellPage
{
    private readonly ILogService _log;
    private readonly ILocalizationService _loc;
    private readonly ISdrStoreService _store;
    public const string UriString = "asv:shell.page.sdr-store";
    public static readonly Uri Uri = new (UriString);
    private readonly ReadOnlyObservableCollection<SdrDeviceViewModel> _devices;

    public SdrStorePageViewModel() : base(UriString)
    {
        Title = "SDR Records";
        if (Design.IsDesignMode)
        {
            Store = new SdrStoreBrowserViewModel();
            Device = new SdrPayloadBrowserViewModel();
           
        }
    }

    [ImportingConstructor]
    public SdrStorePageViewModel(ILogService log, ILocalizationService loc, ISdrStoreService store, IMavlinkDevicesService mavlink):this()
    {
        _log = log;
        _loc = loc;
        _store = store;
        Store = new SdrStoreBrowserViewModel(store, log);
        Device = new SdrPayloadBrowserViewModel(mavlink, loc, log);
        DownloadRecord = new CancellableCommandWithProgress<Unit, Unit>(DownloadRecordImpl,"Download record",log).DisposeItWith(Disposable);
        Store.WhenValueChanged(_ => _.SelectedItem).Subscribe(TrySelectDeviceItem).DisposeItWith(Disposable);
        Device.WhenValueChanged(_ => _.SelectedDevice!.SelectedRecord).Subscribe(TrySelectStoreItem).DisposeItWith(Disposable);
    }

    private void TrySelectDeviceItem(SdrStoreEntityViewModel? sdrStoreEntityViewModel)
    {
        if (sdrStoreEntityViewModel == null) return;
        Store.TrySelect(sdrStoreEntityViewModel.EntityId);
    }

    private void TrySelectStoreItem(SdrPayloadRecordViewModel? sdrPayloadRecordViewModel)
    {
        if (sdrPayloadRecordViewModel == null) return;
        Device.TrySelect(sdrPayloadRecordViewModel.Record.Id);
    }

    public CancellableCommandWithProgress<Unit,Unit> DownloadRecord { get; private set; }

    private async Task<Unit> DownloadRecordImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        var ifc = Device.SelectedDevice.Client.Sdr.Base;
        var rec = Device.SelectedDevice.SelectedRecord.Record;
        var recId = rec.Id;
        var count = rec.DataCount.Value;
        var recType = rec.DataType.Value;
        var recTypeAsUInt = (uint)recType;
        var take = 10U;
        await rec.DownloadTagList(new CallbackProgress<double>(_ => { }),cancel);

        var parent = Store.SelectedItem == null ? _store.Store.RootFolderId :
            Store.SelectedItem.IsRecord ? Store.SelectedItem.ParentId : Store.SelectedItem.EntityId;
        
        using var writer = _store.Store.ExistFile(rec.Id) 
            ? _store.Store.Open(rec.Id) 
            : _store.Store.Create(recId, parent , rec.CopyTo);
        
        rec.CopyMetadataTo(writer.File);
        
        var remoteCount = rec.DataCount.Value;
        Debug.WriteLine($"Begin read {remoteCount} items from device");
        for (uint i = 0; i < remoteCount; i+=take)
        {
            var chunk = new ListDataFileHelper.Chunk { Skip = i , Take = take };
            using var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, DisposeCancel);
            linkedCancel.CancelAfter(10*1000); // TODO: change timeout
            var tcs = new TaskCompletionSource();
            await using var c1 = linkedCancel.Token.Register(() => tcs.TrySetCanceled());
            var readCount = 0;
            using var subscribe = ifc.OnRecordData.Where(packetV2=>packetV2.MessageId == recTypeAsUInt).Subscribe(x =>
            {
                Interlocked.Increment(ref readCount);
                Debug.WriteLine($"Save record {x.Name}");
                SaveRecord(writer.File,x);
            });
            Debug.WriteLine($"Request skip:{chunk.Skip} , take:{chunk.Take}");
            var result = await ifc.GetRecordDataList(recId, chunk.Skip, chunk.Take, cancel);
            if (result.ItemsCount == 0) break;

            Observable.Timer(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
                .Subscribe(_ =>
                {
                    if (readCount >= (int)result.ItemsCount)
                    {
                        Debug.WriteLine($"Complete");
                        tcs.TrySetResult();
                    }
                },linkedCancel.Token);
            await tcs.Task;
            
        }
        await Store.Refresh.Command.Execute();
        return Unit.Default;
    }


    private void SaveRecord(IListDataFile<AsvSdrRecordFileMetadata> writer, IPacketV2<IPayload> payload)
    {
        writer.Write(payload);
    }
    public SdrStoreBrowserViewModel Store { get; }
    
    public SdrPayloadBrowserViewModel Device { get; set; }
    
}

