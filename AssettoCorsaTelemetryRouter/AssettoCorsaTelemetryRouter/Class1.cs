using AssettoCorsaSharedMemory;
using System;
using System.IO.Ports;
using System.Threading;

namespace Asseto_Corsa_Telemetry_Router
{
    class Program
    {
        static private SerialPort _port;

        static private short _staticMaxRpm = 0;
        static private short _currentRpm = 0;
        static private short _currentSpeed = 0;
        static private short _currentGear = 0;


        static void Main(string[] args)
        {
            string[] ports = SerialPort.GetPortNames();
            Console.WriteLine("The following serial ports were found:");
            int ii = 0;
            foreach (string port in ports)
            {
                Console.Write(ii);
                Console.Write(": ");
                Console.WriteLine(port);
                ii++;
            }

            int port_number = -1;

            while (port_number < 0)
            {
                Console.WriteLine("Choose port by leading number");
                ConsoleKeyInfo input = Console.ReadKey();
                Console.WriteLine();
                if (char.IsDigit(input.KeyChar))
                {
                    port_number = int.Parse(input.KeyChar.ToString());
                }
                if (port_number < 0 || port_number > ports.Length - 1)
                {
                    Console.WriteLine("Invalid");
                    port_number = -1;
                }
            }

            _port = new SerialPort(ports[port_number], 115200);

            Console.WriteLine("Waiting for Arduino to connect...");
            _port.Open();
            while (!_port.IsOpen)
            {
                Thread.Sleep(10);
            }
            Console.WriteLine("Connected to Arduino");

            AssettoCorsa ac = new AssettoCorsa();
            ac.StaticInfoInterval = 5000;
            ac.PhysicsInterval = 10;
            ac.StaticInfoUpdated += ac_StaticInfoUpdated;
            ac.PhysicsUpdated += ac_PhysicUpdated;
            ac.Start();

            Console.WriteLine("Waiting for game to connect...");
            while (!ac.IsRunning)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("Connected to game!");

            Console.ReadKey();
        }

        // Arduino -> [0]: 'R', [1:2] rpm, [3:4] rpmmax, [5]: 'S', [6:7] speed, [8]: 'G', [9] gear - 10: reverse, 0: neutral
        // ACC -> gear - 0: reverse, 1: neutral

        static void ac_StaticInfoUpdated(object sender, StaticInfoEventArgs e)
        {
            _staticMaxRpm = (short)e.StaticInfo.MaxRpm;
        }

        static void ac_PhysicUpdated(object sender, PhysicsEventArgs e)
        {
            _currentGear = (short)e.Physics.Gear;
            _currentRpm = (short)e.Physics.Rpms;
            _currentSpeed = (short)e.Physics.SpeedKmh;

            send_Data();
        }

        static void send_Data()
        {
            byte[] buffer = new byte[10];

            buffer[0] = (byte)'R';
            buffer[1] = (byte)(_currentRpm >> 8);
            buffer[2] = (byte)_currentRpm;
            buffer[3] = (byte)(_staticMaxRpm >> 8);
            buffer[4] = (byte)_staticMaxRpm;

            buffer[5] = (byte)'S';
            buffer[6] = (byte)(_currentSpeed >> 8);
            buffer[7] = (byte)_currentSpeed;

            buffer[8] = (byte)'G';
            if(_currentGear == 0)
            {
                buffer[9] = (byte)10;
            } else
            {
                buffer[9] = (byte)(_currentGear - 1);
            }

            try
            {
                _port.Write(buffer, 0, 10);
            } 
            catch (Exception ex)
            {
                Console.WriteLine("Writing failed! \nError: " + ex.Message);
            }
        }
    }
}
