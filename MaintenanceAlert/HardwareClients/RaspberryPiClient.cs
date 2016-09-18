// Copyrights 2016 Sameer Khandekar
// MIT License

// Removed UI parts, commented LED jobs
using System;

using Windows.Devices.Gpio;

using System.Diagnostics;

namespace MaintenanceAlert.HardwareClients
{
    /// <summary>
    /// Client for Raspberry Pi
    /// </summary>
    internal class RaspberryPiClient
    {
        #region attached sensor
        /// <summary>
        /// Weight sensor attached to the raspberry pi
        /// </summary>
        public WeightSensorClient WeightSensor { get; private set;  } = new WeightSensorClient();
        #endregion
       
        /// <summary>
        /// Initialize GPIO and SPI
        /// </summary>
        internal async void InitAll()
        {
            try
            {
                InitGpio();         /* Initialize GPIO to toggle the LED */
                await WeightSensor.InitSPI();    /* Initialize the SPI bus for communicating with weight sensor */
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// If command is critical, turn the LED ON
        /// </summary>
        /// <param name="command"></param>
        internal void ProcessCommand(string command)
        {
            SwitchLED(command == Constants.CmdCritical);
        }

        /// <summary>
        /// Init GPIO
        /// </summary>
        private void InitGpio()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                throw new Exception("There is no GPIO controller on this device");
            }

            // open the pin for LED
            _ledPin = gpio.OpenPin(LED_PIN);

            ///* GPIO state is initially undefined, so we assign a default value before enabling as output */
            _ledPin.Write(GpioPinValue.High);
            _ledPin.SetDriveMode(GpioPinDriveMode.Output);
        }

        /// <summary>
        /// Turn LED on/off
        /// </summary>
        /// <param name="on"></param>
        internal void SwitchLED(bool on)
        {
            _ledPin.Write(on ? GpioPinValue.High : GpioPinValue.Low);
        }

        private const int LED_PIN = 4; 
        private GpioPin _ledPin;
    }
}
