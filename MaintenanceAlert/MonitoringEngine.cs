// Copyrights 2016 Sameer Khandekar
// MIT License
using System;
using System.Diagnostics;
using Windows.UI.Core;
using MaintenanceAlert.AzureClient;
using MaintenanceAlert.HardwareClients;

namespace MaintenanceAlert
{
    /// <summary>
    /// Engine that coordinates devices, azure and manages local business logic
    /// </summary>
    public class MonitoringEngine : BaseNotifyModel
    {
        /// <summary>
        /// Constructor to initiate hardware, azure
        /// And setting up event handlers
        /// </summary>
        /// <param name="dispatcher"></param>
        public MonitoringEngine(CoreDispatcher dispatcher)
            :base(dispatcher)
        {
            // insantiate hardware
            _raspberryPiClient = new RaspberryPiClient();
            _raspberryPiClient.WeightSensor.WeightChanged += WeightSensor_WeightChanged;

            // insantiate azure connection
            _azureConnector = new AzureConnector();
            _azureConnector.IOTCommandReceived += AzureConnector_IOTCommandReceived;

            // initiate azure device
            _azureWeightSensor = new AzureWeightSensor();
        }

        private void AzureConnector_IOTCommandReceived(object sender, dynamic command)
        {
            if (command.Name == AzureWeightSensor.LevelCommandName)
            {
                var message = command.Parameters.Level.ToString();
                Debug.WriteLine($"Azure command level {message}");

                // raise event for UI and for Raspberry Pi client
                _raspberryPiClient.ProcessCommand(message);

                AzureMessage = message;
            }
        }

        /// <summary>
        /// Start the initialiation
        /// </summary>
        internal void Start()
        {
            // set up the hardware
            _raspberryPiClient.InitAll();

            // connect the device to IOT Suite
            _azureConnector.ConnectDeviceToIoTSuite(_azureWeightSensor.DeviceId);

            // send the device metadata
            _azureConnector.SendDeviceMetaData(_azureWeightSensor);
        }

        /// <summary>
        /// Called when weight changed event is sent by the hardware sensor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="weight"></param>
        private void WeightSensor_WeightChanged(object sender, double weight)
        {
            // compute scale for oil in %
            Debug.WriteLine("weight = " + weight);
            // notify the app
            NotifyApp(weight);
            // notify azure
            NotifyAzure(weight);
        }

        /// <summary>
        /// Notify the app.
        /// </summary>
        /// <param name="weight"></param>
        private void NotifyApp(double weight)
        {
            var percentRemaining = (weight - MinWeight) / (MaxWeight - MinWeight);

            // discard erroneous readings. These can be caused by hardware
            if (percentRemaining >= 0 && percentRemaining <= 1.0)
            {
                OilPercentRemaining = percentRemaining;
                Debug.WriteLine("Percent = " + OilPercentRemaining * 100);
            }

            // this is for debugging only
            SimulateAzureMessage(percentRemaining);
        }

        /// <summary>
        /// This is for debugging only
        /// </summary>
        /// <param name="percentRemaining"></param>
        private void SimulateAzureMessage(double percentRemaining)
        {
            var message = "Good";
            if (percentRemaining <= 0.2)
            {
                message = Constants.CmdCritical;
            }
            else if (percentRemaining <= 0.5)
            {
                message = Constants.CmdWarn;
            }

            _raspberryPiClient.ProcessCommand(message);

            AzureMessage = message;
        }

        /// <summary>
        /// send telemetry data to azure
        /// </summary>
        /// <param name="weight"></param>
        private void NotifyAzure(double weight)
        {
            var telemetryData = _azureWeightSensor.GetDeviceTelemetryData(weight);
            Debug.WriteLine($"Sending weight to azue {weight}");
            _azureConnector.SendDeviceTelemetryData(telemetryData);
        }


        #region Accesors
        private double _oilPercentRemaining;
        public double OilPercentRemaining
        {
            get
            {
                return _oilPercentRemaining;
            }
            set
            {
                if (_oilPercentRemaining != value)
                {
                    _oilPercentRemaining = value;
                    NotifyPoprtyChanged();
                }
            }
        }

        private string _azureMessage;
        public string AzureMessage
        {
            get
            {
                return _azureMessage;
            }
            set
            {
                if (_azureMessage != value)
                {
                    _azureMessage = value;
                    NotifyPoprtyChanged();
                }
            }
        }

        #endregion

        #region private members
        private RaspberryPiClient _raspberryPiClient;
        private AzureConnector _azureConnector;
        private AzureWeightSensor _azureWeightSensor;
        #endregion

        #region private const
        const double MinWeight = 500;
        const double MaxWeight = 1180;
        #endregion
    }
}
