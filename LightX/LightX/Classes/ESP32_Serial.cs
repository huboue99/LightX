//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Windows.Forms;
//using System.IO.Ports;
//using System.IO;
//using System.Threading;

//namespace LightX.Classes
//{
//    public enum I2CCommands : byte
//    {
//        Set_target_position = 0xE0,
//        Set_target_velocity = 0xE3,
//        Halt_and_set_position = 0xEC,
//        Halt_and_hold = 0x89,
//        Go_home = 0x97,
//        Reset_command_timeout = 0x8C,
//        De_energize = 0x86,
//        Energize = 0x85,
//        Exit_safe_start = 0x83,
//        Enter_safe_start = 0x8F,
//        Reset = 0xB0,
//        Clear_driver_error = 0x8A,
//        Set_max_speed = 0xE6,
//        Set_starting_speed = 0xE5,
//        Set_max_acceleration = 0xEA,
//        Set_max_deceleration = 0xE9,
//        Set_step_mode = 0x94,
//        Set_current_limit = 0x91,
//        Set_decay_mode = 0x92,
//        Set_AGC_option = 0x98,
//        Get_variable = 0xA1,
//        Get_variable_and_clear_errors_occurred = 0xA2,
//        Get_setting = 0xA8
//    }
//    public enum I2CDriver : int
//    {
//        X = 0,
//        Y = 1,
//        Z = 2,
//        T = 3
//    }

//    class ESP32_Serial : BaseClass
//    {
//        public SerialPort _serialPort;
//        public byte[] _i2cDriverAdresses;
//        public ESP32_Serial(string comPort)
//        {
//            _serialPort = new SerialPort(comPort, 115200, Parity.None, 8, StopBits.One);
//            if (_serialPort.IsOpen)
//                _serialPort.Close();
//            _serialPort.ReadTimeout = 500;
//            _serialPort.WriteTimeout = 500;
//            _serialPort.Open();
//            _serialPort.DiscardOutBuffer();
//            System.Threading.Thread.Sleep(10);
//            //on alloue l'espace nécéssaire pour contenir l'adresse des 4 drivers
//            _i2cDriverAdresses = new byte[4];
//            _i2cDriverAdresses[0] = 14;//X
//            _i2cDriverAdresses[1] = 15;//Y
//            _i2cDriverAdresses[2] = 16;//Z
//            _i2cDriverAdresses[3] = 17;//T
//        }

//        public ESP32_Serial(string comPort, byte i2c_adressX, byte i2c_adressY, byte i2c_adressZ, byte i2c_adressT)
//        {
//            _serialPort = new SerialPort(comPort, 9600, Parity.None, 8, StopBits.One);
//            if (_serialPort.IsOpen)
//                _serialPort.Close();
//            _serialPort.ReadTimeout = 500;
//            _serialPort.WriteTimeout = 500;
//            _serialPort.Open();
//            _serialPort.DiscardOutBuffer();
//            System.Threading.Thread.Sleep(10);
//            //on alloue l'espace nécéssaire pour contenir l'adresse des 4 drivers
//            _i2cDriverAdresses = new byte[4];
//            _i2cDriverAdresses[0] = i2c_adressX;
//            _i2cDriverAdresses[1] = i2c_adressY;
//            _i2cDriverAdresses[2] = i2c_adressZ;
//            _i2cDriverAdresses[3] = i2c_adressT;
//        }

//        ~ESP32_Serial()
//        {
//            _serialPort.Close();
//        }

//        public void sendI2CCommand(int addr, I2CCommands command, int data)
//        {
//            //On envoie la commande de read ben normalement, il faut exécuter la commande Read pour get l'expected Data

//            //Tous les cas 32-bit-write (6 octets)
//            if (command == I2CCommands.Set_target_position || command == I2CCommands.Set_target_velocity || command == I2CCommands.Halt_and_set_position || command == I2CCommands.Set_max_speed || command == I2CCommands.Set_starting_speed || command == I2CCommands.Set_max_acceleration || command == I2CCommands.Set_max_deceleration)
//            {
//                byte[] i2cData = new byte[6];
//                i2cData[0] = Convert.ToByte(addr);
//                i2cData[1] = Convert.ToByte(command);
//                for (int word = 0; word < 4; word++)
//                {
//                    i2cData[word + 2] = Convert.ToByte((data >> 8 * word) & 0x00FF);
//                }
//                _serialPort.Write(i2cData, 0, 6);
//                System.Threading.Thread.Sleep(4);
//            }
//            //Tous les cas 7-bit-write (3 octets) ou tous les cas Block read (3x8 octets aussi, offset est repprésenté par "int data")
//            else if ((command == I2CCommands.Go_home || command == I2CCommands.Set_step_mode || command == I2CCommands.Set_current_limit || command == I2CCommands.Set_decay_mode || command == I2CCommands.Set_AGC_option) || (command == I2CCommands.Get_variable || command == I2CCommands.Get_variable_and_clear_errors_occurred || command == I2CCommands.Get_setting))
//            {
//                byte[] i2cData = new byte[6];
//                i2cData[0] = Convert.ToByte(addr);
//                i2cData[1] = Convert.ToByte(command);
//                i2cData[2] = Convert.ToByte(data & 0x007F);
//                i2cData[3] = 0;
//                i2cData[4] = 0;
//                i2cData[5] = 0;
//                _serialPort.Write(i2cData, 0, 6);
//                System.Threading.Thread.Sleep(4);
//            }
//            //}
//        }
//        public void sendI2CCommandQuick(int addr, I2CCommands command)
//        {
//            //Vérification qu'il s'agit d'un cas quick (2 octets)
//            if (command == I2CCommands.Halt_and_hold || command == I2CCommands.Reset_command_timeout || command == I2CCommands.De_energize || command == I2CCommands.Energize || command == I2CCommands.Exit_safe_start || command == I2CCommands.Enter_safe_start || command == I2CCommands.Reset || command == I2CCommands.Clear_driver_error)
//            {
//                byte[] i2cData = new byte[2];
//                i2cData[0] = Convert.ToByte(addr);
//                i2cData[1] = Convert.ToByte(command);
//                i2cData[2] = 0;
//                i2cData[3] = 0;
//                i2cData[4] = 0;
//                i2cData[5] = 0;
//                _serialPort.Write(i2cData, 0, 6);
//                System.Threading.Thread.Sleep(4);
//            }

//        }
//        public void Read(byte[] data)
//        {
//            int nbBytes = _serialPort.Read(data, 0, 4);
//            Console.WriteLine("4 BYTES RECUS:");
//            for (int i = 0; i < 4; i++)
//            {
//                Console.WriteLine(Convert.ToString(data[i]));
//            }

//        }
//    }
//}
