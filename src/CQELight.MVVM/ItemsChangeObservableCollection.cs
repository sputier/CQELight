using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CQELight.MVVM
{
    /// <summary>
    /// A specific collection that watch for changed on items that it manages.
    /// </summary>
    /// <typeparam name="T">Items to manage, that should be INotifyPropertyChanged.</typeparam>
    public class ItemsChangeObservableCollection<T> : ObservableCollection<T>
        where T : INotifyPropertyChanged
    {

        #region Ctor

        /// <summary>
        /// Default ctor without any items.
        /// </summary>
        public ItemsChangeObservableCollection()
            : base(Enumerable.Empty<T>())
        {

        }
        /// <summary>
        /// Default ctor with a specific collection of items.
        /// </summary>
        /// <param name="items">Collection of items to manage.</param>
        public ItemsChangeObservableCollection(IEnumerable<T> items)
            : base(items)
        {
            RegisterPropertyChanged(items);
        }

        #endregion

        #region ObservableCollection

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is IEnumerable<T> newItems)
            {
                RegisterPropertyChanged(newItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is IEnumerable<T> oldItems)
            {
                UnRegisterPropertyChanged(oldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace
                && e.NewItems is IEnumerable<T> newItemsReplace 
                && e.OldItems is IEnumerable<T> oldItemsReplace)
            {
                UnRegisterPropertyChanged(oldItemsReplace);
                RegisterPropertyChanged(newItemsReplace);
            }

            base.OnCollectionChanged(e);
        }
        
        protected override void ClearItems()
        {
            UnRegisterPropertyChanged(this);
            base.ClearItems();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Clear all items in the collection.
        /// </summary>
        public new void Clear()
        {
            this.ClearItems();
        }

        /// <summary>
        /// Unregister all items from propertie change
        /// </summary>
        public void UnregisterItems()
        {
            if (base.Items == null || !base.Items.Any())
            {
                return;
            }

            UnRegisterPropertyChanged(this);
        }

        #endregion

        #region Private methods

        private void RegisterPropertyChanged(IEnumerable<T> items)
        {
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }
        }

        private void UnRegisterPropertyChanged(IEnumerable<T> items)
        {
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }
        }

        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion

    }
}
