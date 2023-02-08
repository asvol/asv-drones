﻿using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightShellMenuItem : DisposableViewModelBase, IShellMenuItem
    {
        public Uri Id { get; } = new("asv:shell.menu.flight");
        public string Name => RS.FlightShellMenuItem_Name;
        public Uri NavigateTo => FlightViewModel.BaseUri;
        public string Icon => MaterialIconDataProvider.GetData(MaterialIconKind.Map);
        public ShellMenuPosition Position => ShellMenuPosition.Top;
        public ShellMenuItemType Type => ShellMenuItemType.PageNavigation;
        public int Order => 0;
        public ReadOnlyObservableCollection<IShellMenuItem>? Items => null;
    }

}