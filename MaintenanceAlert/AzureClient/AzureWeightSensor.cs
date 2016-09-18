// Copyrights 2016 Sameer Khandekar
// MIT License
// part of the code taken from AzureIoTSuiteUWPDevice sample
using System;
using System.Runtime.Serialization;

namespace MaintenanceAlert.AzureClient
{
    /// <summary>
    /// Reperesnts device in Azure IOT hub
    /// </summary>
    internal class AzureWeightSensor
    {
        /// <summary>
        /// Returns device metadata
        /// </summary>
        /// <returns></returns>
        internal object GetDeviceMetaData()
        {
            // Build the device properties
            DeviceProperties deviceProps = new DeviceProperties();
            deviceProps.HubEnabledState = true;
            deviceProps.DeviceID = DeviceId;
            deviceProps.Manufacturer = "Interlink";
            deviceProps.ModelNumber = "30-81794";
            deviceProps.SerialNumber = "12345";
            deviceProps.FirmwareVersion = "10";
            deviceProps.Platform = "Windows 10";
            deviceProps.Processor = "SnapDragon";
            deviceProps.InstalledRAM = "2GB";
            deviceProps.DeviceState = "normal";

            deviceProps.Latitude = 47.1234;
            deviceProps.Longitude = -122.34567;
            
            // build properties for weight sensor
            WeightSensor wtSensor = new WeightSensor();
            wtSensor.ObjectType = "DeviceInfo";
            wtSensor.IsSimulatedDevice = false;
            wtSensor.Version = "1.0";
            // attach device props to weight sensor
            wtSensor.DeviceProperties = deviceProps;
            
            // create command for the sensor
            Command levelAlarm = CreateCommand();
            // add command to the sensor
            wtSensor.Commands = new Command[] { levelAlarm };

            return wtSensor;
        }

        /// <summary>
        /// Builds telemetry data from the weight
        /// </summary>
        /// <param name="weight"></param>
        /// <returns></returns>
        internal object GetDeviceTelemetryData(double weight)
        {
            TelemetryData data = new TelemetryData();
            data.DeviceId = DeviceId;
            data.Weight = weight;

            return data;
        }

        #region Accessors
        public string DeviceId { get; } = OilAlertDeviceID;
        #endregion

        /// <summary>
        /// Builds command for the weight sensor
        /// </summary>
        /// <returns></returns>
        private static Command CreateCommand()
        {
            // define the parameter name and type
            CommandParameter param = new CommandParameter();
            param.Name = LevelCommandParamName;
            param.Type = "String";

            // create the command
            Command levelAlarm = new Command();
            levelAlarm.Name = LevelCommandName;
            levelAlarm.Parameters = new CommandParameter[] { param };

            return levelAlarm;
        }

        /// <summary>
        /// Various data contracts representing respective data
        /// </summary>
        #region Data Contracts

        [DataContract]
        internal class DeviceProperties
        {
            [DataMember]
            internal string DeviceID;

            [DataMember]
            internal bool HubEnabledState;

            [DataMember]
            internal string CreatedTime;

            [DataMember]
            internal string DeviceState;

            [DataMember]
            internal string UpdatedTime;

            [DataMember]
            internal string Manufacturer;

            [DataMember]
            internal string ModelNumber;

            [DataMember]
            internal string SerialNumber;

            [DataMember]
            internal string FirmwareVersion;

            [DataMember]
            internal string Platform;

            [DataMember]
            internal string Processor;

            [DataMember]
            internal string InstalledRAM;

            [DataMember]
            internal double Latitude;

            [DataMember]
            internal double Longitude;

        }

        [DataContract]
        internal class CommandParameter
        {
            [DataMember]
            internal string Name;

            [DataMember]
            internal string Type;
        }

        [DataContract]
        internal class Command
        {
            [DataMember]
            internal string Name;

            [DataMember]
            internal CommandParameter[] Parameters;
        }

        [DataContract]
        internal class WeightSensor
        {
            [DataMember]
            internal DeviceProperties DeviceProperties;

            [DataMember]
            internal Command[] Commands;

            [DataMember]
            internal bool IsSimulatedDevice;

            [DataMember]
            internal string Version;

            [DataMember]
            internal string ObjectType;
        }

        [DataContract]
        internal class TelemetryData
        {
            [DataMember]
            internal string DeviceId;

            [DataMember]
            internal double Weight;
        }

        #endregion

        #region private constants
        internal const string LevelCommandName = "LevelCommand";
        internal const string LevelCommandParamName = "Level";
        internal const string OilAlertDeviceID = "<Your Device Id>";
        #endregion
    }
}
