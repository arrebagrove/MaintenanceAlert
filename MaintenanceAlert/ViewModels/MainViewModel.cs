// Copyrights 2016 Sameer Khandekar
// MIT License
using System;
using System.ComponentModel;
using System.Windows.Input;
using Windows.UI.Core;

namespace MaintenanceAlert
{
    /// <summary>
    /// ViewModel for the main page
    /// </summary>
    public class MainViewModel : BaseNotifyModel
    {
        #region Accessors
        /// <summary>
        /// Percent of oil remaining
        /// </summary>
        double _oilPercent = 0;
        public double OilPercent
        {
            get
            {
                return _oilPercent;
            }
            set
            {
                if (_oilPercent != (1.0 - value))
                {
                    _oilPercent = 1.0 - value;
                    NotifyPoprtyChanged();
                }
            }
        }

        /// <summary>
        /// Message receied from azure
        /// </summary>
        private string _azMessage;
        public string AzureMessage
        {
            get
            {
                return _azMessage;
            }
            set
            {
                if (_azMessage != value)
                {
                    _azMessage = value;
                    NotifyPoprtyChanged();
                }
            }
        }
        #endregion

        /// <summary>
        /// Start command to start operations
        /// </summary>
        public ICommand Start { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dispatcher"></param>
        public MainViewModel(CoreDispatcher dispatcher)
            :base(dispatcher)
        {
            // instantiate monitoring engine and subscribe to its prop changed
            _monitoringEngine = new MonitoringEngine(Dispatcher);
            _monitoringEngine.PropertyChanged += MonitoringEngine_PropertyChanged;

            Start = new StartCommand(_monitoringEngine);
        }

        /// <summary>
        /// WHen properties of monitoring engine change, update VM properies
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonitoringEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MonitoringEngine.OilPercentRemaining))
            {
                OilPercent = ((MonitoringEngine)sender).OilPercentRemaining;
            }
            else if (e.PropertyName == nameof(MonitoringEngine.AzureMessage))
            {
                AzureMessage = ((MonitoringEngine)sender).AzureMessage;
            }
        }

        /// <summary>
        /// Class for start command
        /// </summary>
        private class StartCommand : ICommand
        {
            public StartCommand(MonitoringEngine monitoringEngine)
            {
                _monitoringEngine = monitoringEngine;
            }
            public event EventHandler CanExecuteChanged;

            /// <summary>
            /// If command should be enabled
            /// </summary>
            /// <param name="parameter"></param>
            /// <returns></returns>
            public bool CanExecute(object parameter)
            {
                return true;
            }

            /// <summary>
            /// Execute the command and disable for further processing
            /// </summary>
            /// <param name="parameter"></param>
            public void Execute(object parameter)
            {
                if (!_started)
                {
                    // start the engine
                    _monitoringEngine.Start();

                    // disable the command
                    _started = true;
                }
            }

            private MonitoringEngine _monitoringEngine;

            private bool _started = false;
        }

        #region private members
        private MonitoringEngine _monitoringEngine;
        #endregion
    }
}
