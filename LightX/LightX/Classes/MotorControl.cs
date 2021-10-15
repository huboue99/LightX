using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;


namespace LightX.Classes
{


    class MotorControl : BaseClass
    {
        #region Fields

        // Il faut modifier le port COM en fonction du port pour que le logiciel soit fonctionnel (port spécifié dans gestionnaire de périphériques -> Ports)
        ESP32_Serial esp32 = new ESP32_Serial("COM4");

        List<int> blurValues = new List<int>();
        List<int> locationBlur = new List<int>();

        private int dontHitTheClientsFace = 15 * 50; // Déplacement en Y de 2 cm avant de bouger theta pour éviter d'accrocher le visage du client
        private int sleepTime = 1 * 1000; // Délais de 1 sec entre certaines commandes envoyées au moteur
        #endregion Fields

        #region Balayage

        // Fonction reliée au bouton Blayage
        public void Balayage() 
        {
            locationBlur.Clear();
            blurValues.Clear();
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_velocity, 300); 
        }

        #endregion Balayage

        #region FocusPositionning
        // Fonction reliée au bouton Focus Positioning
        public bool dataFocusPositioning()
        {
            string path = $@"..\..\imgProcessing\command.txt";
            string imageporcessingresult = null;
            int posY;
            try
            {
                imageporcessingresult = File.ReadAllText(path);
                string[] resultsarray_imageprocessing = imageporcessingresult.Split(' ');
                int blur = Int32.Parse(resultsarray_imageprocessing[0]);
                blurValues.Add(blur);
                Console.WriteLine(blur);
                posY = esp32.GetPosition(esp32._i2cDriverAdresses[1]);
                locationBlur.Add(posY);
                Console.WriteLine(posY);
                int last = locationBlur[locationBlur.Count() - 1];
                int waitTime = 4;
                for (int i = 1; i < waitTime; i++)
                {
                    int before = locationBlur[locationBlur.Count() - i];
                    if(before != last)
                    {
                        break;
                    }
                    if(i == waitTime-1)
                    {
                        return false;
                    }
                }
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("occupe");
            }
            return true;
        }

        // Fonction reliée au bouton Focus Positioning
        public void FocusPositioning()
        {
            int maxBlurValueIndex = blurValues.IndexOf(blurValues.ToArray().Max());
            int posFocus = locationBlur[maxBlurValueIndex];
            int posNow = locationBlur[locationBlur.Count() - 1];
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_max_speed, 4000000);
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, posFocus - posNow);
            
        }

        #endregion FocusPositionning

        #region PupilPositioning

        // Fonction reliée au bouton Pupil Positioning
        public void PupilPositioning()
        {
            string path = $@"..\..\imgProcessing\command.txt";
            string imageporcessingresult = null;
            int H_Pixel = 640;
            int W_Pixel = 960;

            try
            {
                imageporcessingresult = File.ReadAllText(path);
                string[] resultsarray_imageprocessing = imageporcessingresult.Split(' ');

                int posXPixel = Int32.Parse(resultsarray_imageprocessing[1]);
                int posZPixel = Int32.Parse(resultsarray_imageprocessing[2]);

                Console.WriteLine(posXPixel);
                Console.WriteLine(posZPixel);

                int errorXPixel = posXPixel - (W_Pixel / 2);
                int errorZPixel = (H_Pixel / 2) - posZPixel;


                Console.WriteLine(errorXPixel);
                Console.WriteLine(errorZPixel);
                int movXStep = -(50 * errorXPixel / 23); // Conversion: 1mm/pixel et 50 pas/mm
                int movZStep = (21 * errorZPixel / 23); // Conversion: 1mm/pixel et 21 pas/mm

                Console.WriteLine(movXStep);
                Console.WriteLine(movZStep);
                if(posXPixel != 0 && posZPixel != 0)
                {
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, movXStep);
                    System.Threading.Thread.Sleep(4);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[2], I2CCommands.Set_target_position, movZStep);
                }
            }

            catch (System.IO.IOException)
            {
                Console.WriteLine("occupe");
            }
        }

        #endregion PupilPositioning

        #region Reset

        // Reset de la position du moteur
        public void Reset() 
        {
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_starting_speed, 5);
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -1300);
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 750);
            //esp32.sendI2CCommand(esp32._i2cDriverAdresses[2], I2CCommands.Set_target_position, 150);

        }

        #endregion Reset

        #region DeplacementsManuels

        // Déplacement du moteur dans l'axe des X positifs 
        public void XUp()
        {
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, 50);
        }

        // Déplacement du moteur dans l'axe des X négatifs
        public void XDown()
        {
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -50);
        }

        // Déplacement du moteur dans l'axe des Y positifs 
        public void YUp()
        {
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 50);
        }

        // Déplacement du moteur dans l'axe des Y négatifs
        public void YDown()
        {
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -50);
        }

        // Déplacement du moteur dans l'axe des Z positifs 
        public void ZUp()
        {
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[2], I2CCommands.Set_target_position, 50);
        }

        // Déplacement du moteur dans l'axe des Z négatifs
        public void ZDown()
        {
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[2], I2CCommands.Set_target_position, -50);
        }

        // Déplacement de l'axe theta dans le sens horaire (Fonction actuellement non utilisée)
        public void ThetaUp()
        {
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, 1);
        }

        // Déplacement de l'axe theta dans le sens antihoraire (Fonction actuellement non utilisée)
        public void ThetaDown()
        {
            esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, -1);
        }

        #endregion DeplacementManuels

        #region AutomatedTest
        // Positionnement automatique des moteurs en fonction d'un test donné

        public void Conjonctive()
        {
            // Aucun chagement de position pour ce test
        }

        public void FiltreCobalt(int nClick)
        {
            switch (nClick)
            {
                case 1:
                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, -8);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;

                case 2:
                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, 8);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;
            }
        }

        public void VanHerick(int nClick)
        {
            switch (nClick)
            {
                case 1:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, 295);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 77);

                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, 8);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;

                case 2:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -590);

                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, -67);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;

                case 3:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, 295);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -77);

                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, 59);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;
            }
        }

        public void Cornee(int nClick)
        {
            switch (nClick)
            {
                case 1:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, 295);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 78);

                    //Déplacement en theta
                    //esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, 25);
                    Thread.Sleep(sleepTime);
                    //esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;

                case 2:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -49);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -25);
                    break;

                case 3:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -49);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -20);
                    break;

                case 4:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -49);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -15);
                    break;

                case 5:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -49);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -11);
                    break;

                case 6:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -50);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -6);
                    break;

                case 7:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -49);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -2);
                    break;

                case 8:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -49);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 2);

                    //Déplacement en theta
                    //esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, -50);
                    Thread.Sleep(sleepTime);
                    //esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;

                case 9:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -49);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 6);
                    break;

                case 10:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -49);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 11);
                    break;

                case 11:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -50);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 15);
                    break;

                case 12:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -49);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 15);
                    break;

                case 13:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -49);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 25);
                    break;

                case 14:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, 295);

                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -73);

                    //Déplacement en theta
                    //esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, 25);
                    Thread.Sleep(sleepTime);
                    //esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;
            }
        }

        public void ChambresAnterieures(int nClick)
        {
            switch (nClick) // Déplacements sont en Y seulement 
            {
                case 1:
                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 39);

                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, -3);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;

                case 2:
                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -39);

                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, 3);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;
            }
        }

        public void Cristallin(int nClick)
        {
            switch (nClick) 
            {
                case 1:
                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 25);

                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, -3);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;

                case 2:
                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 25);
                    break;

                case 3:
                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 25);
                    break;

                case 4:
                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 25);
                    break;

                case 5:
                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 25);
                    break;

                case 6:
                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 25);
                    break;

                case 7:
                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 25);
                    break;

                case 8:
                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, 25);
                    break;

                case 9:
                    // Déplacement en Y
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -200);

                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, 3);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;
            }
        }

        public void TransilluminationIris(int nClick)
        {
            switch (nClick)
            {
                case 1:
                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, -25);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;

                case 2:
                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, 25);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;
            }
        }

        public void MargesPupillaires(int nClick)
        {
            switch (nClick)
            {
                case 1:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, 106);

                    // Déplacement en Z
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[2], I2CCommands.Set_target_position, 46);

                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, -3);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;

                case 2:
                    // Déplacement en Z
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[2], I2CCommands.Set_target_position, -92);
                    break;

                case 3:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, -212);
                    break;

                case 4:
                    // Déplacement en Z
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[2], I2CCommands.Set_target_position, 92);
                    break;

                case 5:
                    // Déplacement en X
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[0], I2CCommands.Set_target_position, 106);

                    // Déplacement en Z
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[2], I2CCommands.Set_target_position, -46);

                    //Déplacement en theta
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, -dontHitTheClientsFace);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[3], I2CCommands.Set_target_position, 3);
                    Thread.Sleep(sleepTime);
                    esp32.sendI2CCommand(esp32._i2cDriverAdresses[1], I2CCommands.Set_target_position, dontHitTheClientsFace);
                    break;
            }
        }

        #endregion AutomatedTest
    }

    public enum I2CCommands : byte
    {
        Set_target_position = 0xE0,
        Set_target_velocity = 0xE3,
        Halt_and_set_position = 0xEC,
        Halt_and_hold = 0x89,
        Go_home = 0x97,
        Reset_command_timeout = 0x8C,
        De_energize = 0x86,
        Energize = 0x85,
        Exit_safe_start = 0x83,
        Enter_safe_start = 0x8F,
        Reset = 0xB0,
        Clear_driver_error = 0x8A,
        Set_max_speed = 0xE6,
        Set_starting_speed = 0xE5,
        Set_max_acceleration = 0xEA,
        Set_max_deceleration = 0xE9,
        Set_step_mode = 0x94,
        Set_current_limit = 0x91,
        Set_decay_mode = 0x92,
        Set_AGC_option = 0x98,
        Get_variable = 0xA1,
        Get_variable_and_clear_errors_occurred = 0xA2,
        Get_setting = 0xA8
    }

    public enum I2CDriver : int
    {
        X = 0,
        Y = 1,
        Z = 2,
        T = 3
    }

    class ESP32_Serial : BaseClass
    {
        public SerialPort _serialPort;
        public byte[] _i2cDriverAdresses;
        public ESP32_Serial(string comPort)
        {
            _serialPort = new SerialPort(comPort, 57600, Parity.None, 8, StopBits.One);
            if (_serialPort.IsOpen) {
                _serialPort.Close();
            }
                
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.Open();
            _serialPort.DiscardOutBuffer();
            System.Threading.Thread.Sleep(10);
            //on alloue l'espace nécéssaire pour contenir l'adresse des 4 drivers
            _i2cDriverAdresses = new byte[4];
            _i2cDriverAdresses[0] = 14;//X
            _i2cDriverAdresses[1] = 15;//Y
            _i2cDriverAdresses[2] = 16;//Z
            _i2cDriverAdresses[3] = 17;//T
        }

        public ESP32_Serial(string comPort, byte i2c_adressX, byte i2c_adressY, byte i2c_adressZ, byte i2c_adressT)
        {
            _serialPort = new SerialPort(comPort, 115200, Parity.None, 8, StopBits.One);

            _serialPort = new SerialPort(comPort, 57600, Parity.None, 8, StopBits.One);

            if (_serialPort.IsOpen)
                _serialPort.Close();
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.Open();
            _serialPort.DiscardOutBuffer();
            System.Threading.Thread.Sleep(10);
            //on alloue l'espace nécéssaire pour contenir l'adresse des 4 drivers
            _i2cDriverAdresses = new byte[4];
            _i2cDriverAdresses[0] = i2c_adressX;
            _i2cDriverAdresses[1] = i2c_adressY;
            _i2cDriverAdresses[2] = i2c_adressZ;
            _i2cDriverAdresses[3] = i2c_adressT;
        }

        ~ESP32_Serial()
        {
            _serialPort.Close();
        }

        public int GetPosition(byte addr)//(esp32.GetPosition((_i2cDriverAdresses(1))))
        {
            sendI2CCommand(addr, I2CCommands.Get_variable, 0xA1);
            //System.Threading.Thread.Sleep(4);
            byte[] data = new byte[4];
            int nbBytes = _serialPort.Read(data, 0, 4);
            return BitConverter.ToInt32(data, 0);
        }

        public void sendI2CCommand(byte addr, I2CCommands command, int data)
        {
            //On envoie la commande de read ben normalement, il faut exécuter la commande Read pour get l'expected Data

            //Tous les cas 32-bit-write (6 octets)
            if (command == I2CCommands.Set_target_position || command == I2CCommands.Set_target_velocity || command == I2CCommands.Halt_and_set_position || command == I2CCommands.Set_max_speed || command == I2CCommands.Set_starting_speed || command == I2CCommands.Set_max_acceleration || command == I2CCommands.Set_max_deceleration)
            {
                byte[] i2cData = new byte[6];
                i2cData[0] = addr;
                i2cData[1] = Convert.ToByte(command);
                for (int word = 0; word < 4; word++)
                {
                    i2cData[word + 2] = Convert.ToByte((data >> 8 * word) & 0x00FF);
                }
                _serialPort.Write(i2cData, 0, 6);
                
            }
            //Tous les cas 7-bit-write (3 octets) ou tous les cas Block read (3x8 octets aussi, offset est repprésenté par "int data")
            else if ((command == I2CCommands.Go_home || command == I2CCommands.Set_step_mode || command == I2CCommands.Set_current_limit || command == I2CCommands.Set_decay_mode || command == I2CCommands.Set_AGC_option) || (command == I2CCommands.Get_variable || command == I2CCommands.Get_variable_and_clear_errors_occurred || command == I2CCommands.Get_setting))
            {
                byte[] i2cData = new byte[6];
                i2cData[0] = addr;
                i2cData[1] = Convert.ToByte(command);
                i2cData[2] = Convert.ToByte(data & 0x007F);
                i2cData[3] = 0;
                i2cData[4] = 0;
                i2cData[5] = 0;
                _serialPort.Write(i2cData, 0, 6);
                
            }
            //}
        }
        public void sendI2CCommandQuick(byte addr, I2CCommands command)
        {
            //Vérification qu'il s'agit d'un cas quick (2 octets)
            if (command == I2CCommands.Halt_and_hold || command == I2CCommands.Reset_command_timeout || command == I2CCommands.De_energize || command == I2CCommands.Energize || command == I2CCommands.Exit_safe_start || command == I2CCommands.Enter_safe_start || command == I2CCommands.Reset || command == I2CCommands.Clear_driver_error)
            {
                byte[] i2cData = new byte[2];
                i2cData[0] = addr;
                i2cData[1] = Convert.ToByte(command);
                i2cData[2] = 0;
                i2cData[3] = 0;
                i2cData[4] = 0;
                i2cData[5] = 0;
                _serialPort.Write(i2cData, 0, 6);
                
            }

        }

    }
}
