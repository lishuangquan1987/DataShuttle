using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataShuttle.Core.Models;
using DataShuttle.WpfSample.Configs;
using DataShuttle.WpfSample.Views;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.ViewModels
{
    public class MainWindowViewModel:ObservableObject
    {
        public MainWindowViewModel()
        {
            AddItemConfig=new AsyncRelayCommand(AddItem);
        }

        private async Task AddItem()
        {
            var result= await Dialog.Show(new SetupView())
                .GetResultAsync<OperationResult<ItemConfig>>();
            if (!result.IsSuccess)
            {
                return;
            }

            
        }

        public AsyncRelayCommand AddItemConfig { get; }
    }
}
