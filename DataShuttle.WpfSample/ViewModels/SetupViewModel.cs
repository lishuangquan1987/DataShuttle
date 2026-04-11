using CommunityToolkit.Mvvm.ComponentModel;
using DataShuttle.Core.Models;
using DataShuttle.WpfSample.Configs;
using HandyControl.Tools.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.ViewModels
{
    public class SetupViewModel : ObservableObject, IDialogResultable<OperationResult<ItemConfig>>
    {
        public OperationResult<ItemConfig> Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
