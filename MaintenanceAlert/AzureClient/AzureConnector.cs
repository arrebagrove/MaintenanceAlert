// Copyrights 2016 Sameer Khandekar
// MIT License

// #define SimulateAzureHub

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Devices.Client;

using Newtonsoft.Json;

namespace MaintenanceAlert.AzureClient
{
    /// <summary>
    /// This class is intended for connecting to Azure, sending data and receiving commands
    /// </summary>
    internal class AzureConnector
    {
        /// <summary>
        /// Delegate + Event to handle commands
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="message">message</param>
        #region events
        internal delegate void IOTCommandHandler(object sender, dynamic message);

        internal event IOTCommandHandler IOTCommandReceived;
        #endregion

        #region Conect to azure, Send and receive data
#if SimulateAzureHub
        internal async void ConnectDeviceToIoTSuite(string deviceId)
        {
            Debug.WriteLine($"ConnectDeviceToIoTSuite deviceId {deviceId}");
        }

        internal async void SendDeviceMetaData(object deviceMetadata)
        {
            Debug.WriteLine($"SendDeviceMetaData");
            string json = JsonConvert.SerializeObject(deviceMetadata);
            Debug.WriteLine($"SendDeviceMetaData deviceMetadata {json}");
        }

        internal async void SendDeviceTelemetryData(object data)
        {
            Debug.WriteLine($"SendDeviceTelemetryData");
            string json = JsonConvert.SerializeObject(data);
            Debug.WriteLine($"SendDeviceTelemetryData data {json}");
        }
#else
        /// <summary>
        /// Establishes connection with IOT hub/suite and initializes receiving data
        /// </summary>
        /// <param name="deviceId"></param>
        internal async void ConnectDeviceToIoTSuite(string deviceId)
        {
            // build the connection string
            var connectionString = "HostName=" + HostName + ";DeviceId=" + deviceId + ";SharedAccessKey=" + DeviceKey;
            try
            {
                // create device client and open the connection
                _deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Http1);
                await _deviceClient.OpenAsync();

                // start the loop to receive messages from azure
                await Task.Run(ReceiveMessagesFromAzure);
            }
            catch
            {
                Debug.Write("Error while trying to connect to IoT Hub");
                _deviceClient = null;
            }
        }

        /// <summary>
        /// Sends the metadata of the device
        /// </summary>
        /// <param name="deviceMetadata"></param>
        internal async void SendDeviceMetaData(object deviceMetadata)
        {
            try
            {
                if (_deviceClient != null)
                {
                    // create message with metadata and send it
                    var msg = new Message(Serialize(deviceMetadata));

                    await _deviceClient.SendEventAsync(msg);
                }
            }
            catch (System.Exception e)
            {
                Debug.Write("Exception while sending device meta data :\n" + e.Message.ToString());
            }

            Debug.Write("Sent meta data to IoT Suite\n" + HostName);
        }

        /// <summary>
        /// Sends telemetry data to azure
        /// This is syntactically different method
        /// </summary>
        /// <param name="data"></param>
        internal async void SendDeviceTelemetryData(object data)
        {
            try
            {
                if (_deviceClient != null)
                {
                    // build message from the data and send it
                    var msg = new Message(Serialize(data));

                    await _deviceClient.SendEventAsync(msg);
                }
            }
            catch (System.Exception e)
            {
                Debug.Write("Exception while sending device telemetry data :\n" + e.Message.ToString());
            }
        }

        /// <summary>
        /// Infinite loop waiting to receive message
        /// When a message is received, it raises an event.
        /// </summary>
        /// <returns></returns>
        private async Task ReceiveMessagesFromAzure()
        {
            while (true)
            {
                Message message = await _deviceClient.ReceiveAsync();
                if (message != null)
                {
                    try
                    {
                        dynamic command = DeSerialize(message.GetBytes());
                        IOTCommandReceived?.Invoke(this, command);

                        // Notify IoTHub we treated it
                        await _deviceClient.CompleteAsync(message);
                    }
                    catch
                    {
                        await _deviceClient.RejectAsync(message);
                    }
                }
            }
        }
#endif
        #endregion
        #region serialiation
        /// <summary>
        /// Serialize the objet to json and convert json to bytes
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private byte[] Serialize(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// Deserialize bytes to json string and 
        /// create object from it
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private dynamic DeSerialize(byte[] data)
        {
            string text = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject(text);
        }
        #endregion

        #region private members
        private DeviceClient _deviceClient;
        #endregion

        #region private constants
        private const string HostName = "<Your IOT Hub Hostname>";
        private const string DeviceKey = "<Your Device Key>";
        #endregion
    }
}
