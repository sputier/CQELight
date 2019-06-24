using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GeneaoMobile.ViewModels
{
    public class BaseViewModel : CQELight.MVVM.BaseViewModel
    {

        bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => Set(ref isBusy, value);
        }

        string title = string.Empty;
        public string Title
        {
            get => title;
            set => Set(ref title, value);
        }

    }
}
