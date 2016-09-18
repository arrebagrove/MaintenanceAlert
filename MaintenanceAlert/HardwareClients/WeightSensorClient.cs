// Copyrights 2016 Sameer Khandekar
// MIT License
// part of code taken from potentiometer sensor

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace MaintenanceAlert.HardwareClients
{
    /// <summary>
    /// Class corresponds to the weight sensor that is connected via SPI
    /// </summary>
    internal class WeightSensorClient
    {
        #region Weigh changed event
        // delegate reprsenting weight change event handler
        internal delegate void WeightChangedHandler(object sender, double weight);

        /// <summary>
        /// This event is raised when weight changes
        /// </summary>
        internal event WeightChangedHandler WeightChanged;
        #endregion

        /// <summary>
        /// Initializes SPI
        /// </summary>
        /// <returns></returns>
        internal async Task InitSPI()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 500000;   // 0.5MHz clock rate
                settings.Mode = SpiMode.Mode0;      // The ADC expects idle-low clock polarity so we use Mode0

                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                _spiADC = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);


                // initialize the timer
                _timer = new Timer(this.Timer_Tick, null, 0, 300);
            }
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        /// <summary>
        /// called periodically
        /// </summary>
        /// <param name="state"></param>
        private void Timer_Tick(object state)
        {
            // read ADC
            ReadADC();
            Debug.WriteLine($"ADC is {_adcValue}");

            // calculate the weight based on the hardware specs
            var weight = CalculateWeight();

            // Invoke the event that weight has changed
            WeightChanged?.Invoke(this, weight);
        }

        /// <summary>
        /// Calculates weight based on the adc value
        /// </summary>
        /// <returns></returns>
        private double CalculateWeight()
        {
            return 20 + (((1024 - _adcValue) * 1980) / 1024);
        }

        /// <summary>
        /// Reads ADC value
        /// </summary>
        private void ReadADC()
        {
            byte[] readBuffer = new byte[3]; // Buffer to hold read data*/
            byte[] writeBuffer = new byte[3] { 0x00, 0x00, 0x00 };

            writeBuffer[0] = MCP3008_CONFIG[0];
            writeBuffer[1] = MCP3008_CONFIG[1];

            _spiADC.TransferFullDuplex(writeBuffer, readBuffer); /* Read data from the ADC                           */
            _adcValue = convertToInt(readBuffer);                /* Convert the returned bytes into an integer value */
        }

        /// <summary>
        /// Convert the raw ADC bytes to an integer
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private int convertToInt(byte[] data)
        {
            int result = 0;

            result = data[1] & 0x03;
            result <<= 8;
            result += data[2];

            return result;
        }

        /// <summary>
        /// Clean up after done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Unloaded(object sender, object args)
        {
            if (_spiADC != null)
            {
                _spiADC.Dispose();
            }
        }

        #region constants and config values
        private const string SPI_CONTROLLER_NAME = "SPI0";  /* Friendly name for Raspberry Pi 2 SPI controller          */
        private const Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */
        private readonly byte[] MCP3008_CONFIG = { 0x01, 0x80 }; /* 00000001 10000000 channel configuration data for the MCP3008 */
        private const int TimerPeriod = 300; // frequency of the timer
        #endregion

        #region members
        private SpiDevice _spiADC;
        private Timer _timer;
        private int _adcValue;
        #endregion

    }
}
