using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace IOTDeviceManagement
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=APQIOTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=LX1COqYbMb07bi+Q1oOD/VxxYjTvZC5huOtzSHFkqNY=";

        static void Main(string[] args)
        {
            for (int loop = 0; loop < 5; loop++)
            {
                AuthADevice().Wait();
            }

            //registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            //AddDevicAsync().Wait();
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static async Task AuthADevice()
        {
            string connStr = "APQIOTHub.azure-devices.net";
            string conn2 = "HostName=APQIOTHub.azure-devices.net;DeviceId=APQ1;SharedAccessKey=nFIIlMG3Ne2NA6pgB0TD+kC2g3INRdTlLOGWCFe/AbQ=";
            X509Certificate2 x509Certificate = new X509Certificate2(@"C:\tools\putty\client.p12", "pass@word");
            var authMethod = new DeviceAuthenticationWithX509Certificate("devicex509", x509Certificate);
            var deviceClient = DeviceClient.Create(connStr, authMethod, Microsoft.Azure.Devices.Client.TransportType.Mqtt_Tcp_Only);
            //var deviceClient = DeviceClient.CreateFromConnectionString(conn2, Microsoft.Azure.Devices.Client.TransportType.Mqtt_Tcp_Only);
            await deviceClient.SendEventAsync(GetMessage());
            

        }

        private static Microsoft.Azure.Devices.Client.Message GetMessage()
        {
            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();

            double currentTemperature = minTemperature + rand.NextDouble() * 15;
            double currentHumidity = minHumidity + rand.NextDouble() * 20;

            var telemetryDataPoint = new
            {
                deviceId = "devicex509",
                temperature = currentTemperature,
                humidity = currentHumidity
            };

            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));
            message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

            return message;
        }

        private static async Task AddDevicAsync()
        {
            string deviceId = "device1";
            string deviceId2 = "devicex509";

            string thumb = "aefb6ba4177ade1e185a1936594794b6109c983c";
            thumb = thumb.ToUpper();

            Device device = new Device(deviceId);
            var device2 = new Device(deviceId2)
            {
                Authentication = new AuthenticationMechanism()
                {
                    X509Thumbprint = new X509Thumbprint()
                    {
                        PrimaryThumbprint = thumb
                    }
                }
            };


            try
            {
                //device = await registryManager.AddDeviceAsync(device);
                device2 = await registryManager.AddDeviceAsync(device2);
            }
            catch (DeviceAlreadyExistsException)
            {
                //device = await registryManager.GetDeviceAsync(deviceId);
                device2 = await registryManager.GetDeviceAsync(deviceId2);
            }
            catch (Exception e)
            {

            }


            //Console.WriteLine("Generated Device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
            Console.WriteLine("Generated Device key: {0}", device2.Authentication.X509Thumbprint.PrimaryThumbprint);
        }
    }
}
