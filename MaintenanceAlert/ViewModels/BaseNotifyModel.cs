// Copyrights 2016 Sameer Khandekar
// MIT License
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.UI.Core;

namespace MaintenanceAlert
{
    /// <summary>
    /// Base view model to raise property changed.
    /// Done using dispatcher to invoke on UI thread
    /// </summary>
    public class BaseNotifyModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dispatcher"></param>
        public BaseNotifyModel(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        /// <summary>
        /// Dispatcher from the window
        /// </summary>
        internal CoreDispatcher Dispatcher { get; set; }

        /// <summary>
        /// Called by derived class to raise event
        /// </summary>
        /// <param name="propName"></param>
        protected void NotifyPoprtyChanged([CallerMemberName]string propName = "")
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(propName));
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
