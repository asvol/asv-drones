using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class PluginInfoViewModel : DisposableReactiveObject
{
    private readonly IPluginSearchInfo _pluginInfo;
    private readonly IPluginManager _manager;
    private ILocalPluginInfo? _localInfo;

    public PluginInfoViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Install = CoreDesignTime.CancellableCommand<Unit, Unit>();
        Uninstall = CoreDesignTime.CancellableCommand<Unit, Unit>();
    }

    public PluginInfoViewModel(IPluginSearchInfo pluginInfo, IPluginManager manager, ILogService log)
    {
        _pluginInfo = pluginInfo;
        _manager = manager;
        Id = pluginInfo.Id;
        Install = new CancellableCommandWithProgress<Unit, Unit>(InstallImpl, "Install", log, RxApp.TaskpoolScheduler)
            .DisposeItWith(Disposable);
        Uninstall = new CancellableCommandWithProgress<Unit, Unit>(UninstallImpl, "Uninstall", log,
            RxApp.TaskpoolScheduler).DisposeItWith(Disposable);
        IsInstalled = _manager.IsInstalled(pluginInfo.PackageId, out _localInfo);
        Name = pluginInfo.Title;
        Author = pluginInfo.Authors;
        Description = pluginInfo.Description;
        SourceName = pluginInfo.Source.Name;
        SourceUri = pluginInfo.Source.SourceUri;
        LastVersion = $"{pluginInfo.LastVersion} (API: {pluginInfo.ApiVersion})";
        IsApiCompatible = pluginInfo.ApiVersion == manager.ApiVersion;
        LocalVersion = (_localInfo != null) ? $"{_localInfo?.Version} (API: {_localInfo?.ApiVersion})" : null;
        DownloadCount = pluginInfo.DownloadCount.ToString();
        Tags = pluginInfo.Tags;
        Dependencies = new List<string>();
        foreach (var dependency in pluginInfo.Dependencies)
        {
            if (dependency.VersionRange.MinVersion != null)
                Dependencies.Add($"{dependency.Id} ( \u2265 {dependency.VersionRange.MinVersion.ToString()})");
        }
        if (Author != null) IsVerified = Author.Contains("https://github.com/asv-soft") && SourceUri.Contains("https://nuget.pkg.github.com/asv-soft/index.json");
    }
    
    public bool IsApiCompatible { get; set; }
    public string Id { get; set; }
    public string? Author { get; set; }
    public string? SourceUri { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string SourceName { get; set; }
    public string LastVersion { get; set; }
    public string? LocalVersion { get; set; }
    public string? DownloadCount { get; set; }
    public string? Tags { get; set; }
    public List<string> Dependencies { get; set; }
    public bool IsInstalled { get; set; }
    [Reactive] public bool IsVerified { get; set; }
    

    private async Task<Unit> UninstallImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        if (_localInfo == null)
        {
            throw new Exception("Plugin not installed");
        }

        _manager.Uninstall(_localInfo);
        return Unit.Default;
    }

    private async Task<Unit> InstallImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        await _manager.Install(_pluginInfo.Source, _pluginInfo.PackageId, _pluginInfo.LastVersion,
            new Progress<ProgressMessage>(
                _ => { progress.Report(_.Progress); }), cancel);
        return Unit.Default;
    }


    public CancellableCommandWithProgress<Unit, Unit> Uninstall { get; }
    public CancellableCommandWithProgress<Unit, Unit> Install { get; }
}