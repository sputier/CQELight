using CQELight.MVVM.Interfaces;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CQELight.MVVM.MahApps
{
    /// <summary>
    /// Implementation of MahApps.MetroWindow to be fully compatible with
    /// CQELight MVVM implementation.
    /// </summary>
    public class CQEMetroWindow : MetroWindow, IView
    {

        #region Members

        private ProgressDialogController _progressAwaiter;

        #endregion

        #region IView methods



        public async Task HideLoadingPanelAsync()
        {
            if (_progressAwaiter != null)
            {
                await Application.Current.Dispatcher.Invoke(async () => await _progressAwaiter.CloseAsync().ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        public void PerformOnUIThread(Action act)
            => Application.Current.Dispatcher.Invoke(act);

        public Task ShowAlertAsync(string title, string message, MessageDialogServiceOptions options = null)
            => Application.Current.Dispatcher.Invoke(async () =>
                {
                    await this.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, new MetroDialogSettings
                    {
                        AffirmativeButtonText = "Ok"
                    }).ConfigureAwait(false);
                });

        public Task ShowLoadingPanelAsync(string waitMessage)
        =>
            Application.Current.Dispatcher.Invoke(async () =>
            {
                _progressAwaiter = await this.ShowProgressAsync("Please wait...", waitMessage).ConfigureAwait(false);
            });

        public void ShowPopup(IView popupWindow)
        {
            if (popupWindow is Window baseW)
            {
                Application.Current.Invoke(() => baseW.ShowDialog());
            }
            else
            {
                throw new InvalidOperationException("Impossible to show popup that doesn't inherits from WPF base Window class.");
            }
        }

        public async Task<bool> ShowYesNoDialogAsync(string title, string message, MessageDialogServiceOptions options = null)
        {
            var settings = new MetroDialogSettings();
            if (options?.ShowCancel == true)
            {
                settings.FirstAuxiliaryButtonText = "Cancel";
            }
            bool result = false;
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                var msgBoxResult = await this.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, null).ConfigureAwait(false);
                if (msgBoxResult == MessageDialogResult.Affirmative)
                {
                    result = true;
                }
                else if (msgBoxResult == MessageDialogResult.FirstAuxiliary && options.ShowCancel && options.CancelCallback != null)
                {
                    options.CancelCallback();
                }
            }).ConfigureAwait(false);
            return result;
        }

        #endregion

    }
}
