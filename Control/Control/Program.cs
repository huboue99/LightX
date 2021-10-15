using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Control
{
    class Program
    {
        static void Main(string[] args)
        {

            // To indicate where python is run on the computer
            var psi = new ProcessStartInfo();
            psi.FileName = @"C:\Program Files\Python39\python.exe";

            // To indicate the path of the python script.
            // VOIR COMMENT JE POURRAIS FAIRE UN PATH GÉNÉRAL...
            var imageProcessing = @"C:\Users\cassa\Desktop\LightXGithub\userinterface\imageProcessingOeil.py";

            // Variables initialization
            string results_imageProcessing;     // To register the data from python
            bool flagFocus = false;              // To indicate if the focus had been reach
            bool flagCenter = false;            // To indicate if the pupil is positionned in the center of the frame
            // YANNICK : SEE THE UTILITY AGAIN WITH YANNICK 
            bool flagReadyPicture = false;      // To indicate if is ready to take a picture
            float thresholdBlur = 238000000;        // Minimum value indicating focus
            float factor = 1;                   // Factor of the controller
            float blurValue;                    // Blur score of the present frame
            int posX_Pupil_Pixel;               // Pupil X position of the present frame (pixels)              
            int posZ_Pupil_Pixel;               // Pupil Z position of the present frame (pixels)
            int posX_Slit_Pixel;                // Slit X position of the present frame (pixels)
            float comX;                         // Command send to motor X displacement
            float comY;                         // Command send to motor Y displacement
            float comZ;                         // Command send  to motor Z displacement
            float posX = 0;                     // Position of the LAF in the X axis (mm)
            float posY = 0;                     // Position of the LAF in the Y axis (mm)
            float posZ = 0;                     // Position of the LAF in the Z axis (mm)
            int errorPupilX_Pixel = 1000;       // Distance between the pupil and the center of the frame (pixels)
            int errorPupilZ_Pixel = 1000;       // Distance between the pupil and the center of the frame (pixels)
            // TESTS TO SEE THE BEST TRESHOLD
            int tresholdError = 15;             // Acceptable error for the centered pupil (pixels)    
            float screenYMax = 300;             // Position to reach in the Y axis to effectuate de screening
            int nFrame = 0;                     // Number of frame acquired since the begining of the exam
            int H_Pixel = 1080;                 // Height of one frame of the video (pixels)
            int W_Pixel = 1920;                 // Width of one frame of the video (pixels)
            float velocityX = 10;               // Velocity of the motor in the X axis
            float velocityY = 5;                // Velocity of the motor in the Y axis
            float velocityZ = 20;               // Velocity of the motor in the Z axis
            float freq = 1 / 33;                // Acquisition frequency of the camera 

            List<float> locationBlur = new List<float>();
            List<float> blurValues = new List<float>();

            //// OPTION 1: 
            // (1) Y MOTOR SCREEN THE Y AXIS AND RECORD BLUR VALUES WITH THE RELATED POSITION
            // (2) THE MOTOR MOVE BACK TO THE HIGHEST BLUR VALUE RECORDED
            // (3) THE PUPIL IS CENTERED

            // PATRICK : FAIRE UN GetPosition POUR FAIRE LE DÉPLACEMENT DE LAF SELON L'AXE Y
            // LA VALEUR POSITION QUE NOUS VOULONS ATTEINDRE EST ENREGISTRÉE DANS screenYMax
            while (posY != screenYMax)
            {
                // YANNICK AND GABRIELLE : ALLER CHERCHER LA FRAME DU VIDÉO DANS LE LOGICIEL LIGHTX (À INSÉRER DANS LA VARIABLE frame)

                // Call the python image processing function and obtain the values of blur, position of the pupil, the slit and the flag indicating that we are ready to take a picture.
                // VOIR POUR METTRE CE CODE APPELANT LE SCRIPT PYTHON DANS UNE FONCTION
                // SI ON GARDE CETTE OPTION, VOIR L'UTILITÉ D'AVOIR LES FLAGS
                psi.Arguments = $"\"{imageProcessing}\" \"{flagFocus}\" \"{flagCenter}\" \"{frame}\"";

                using (var process = Process.Start(psi))
                {
                    results_imageProcessing = process.StandardOutput.ReadToEnd();
                }

                string[] resultsArray_imageProcessing = results_imageProcessing.Split(' ');

                blurValue = float.Parse(resultsArray_imageProcessing[0]);
                posX_Pupil_Pixel = int.Parse(resultsArray_imageProcessing[1]);
                posZ_Pupil_Pixel = int.Parse(resultsArray_imageProcessing[2]);
                posX_Slit_Pixel = int.Parse(resultsArray_imageProcessing[3]);
                flagReadyPicture = bool.Parse(resultsArray_imageProcessing[4]);

                // Stock the value of blur and position in lists. 
                // PATRICK: FAIRE UN GetPosition() Y AXIS (posY = getPosition())
                locationBlur.Add(posY);
                blurValues.Add(blurValue);

                // REVOIR POUR L'UTILITÉ DE CETTE VARIABLE.
                nFrame++;
            }

            // To find the focus position.
            float maxBlurValue = blurValues.ToArray().Max();
            int maxBlurValueIndex = blurValues.IndexOf(maxBlurValue);
            float posFocus = locationBlur[maxBlurValueIndex];

            // PATRICK : GetPosition JUSQU'À LA POSITION QUI NOUS PERMET DE DÉPASSER LE FOCUS (SCREENING)

            // To position the Pupil in the center of the frame.
            while(errorPupilX_Pixel > tresholdError && errorPupilZ_Pixel > tresholdError)
            {
                // YANNICK AND GABRIELLE : ALLER CHERCHER LA FRAME DU VIDÉO DANS LE LOGICIEL LIGHTX (À INSÉRER DANS LA VARIABLE frame)

                // Call the python image processing function and obtain the values of blur, position of the pupil, the slit and the flag indicating that we are ready to take a picture.
                // VOIR POUR METTRE CE CODE APPELANT LE SCRIPT PYTHON DANS UNE FONCTION
                psi.Arguments = $"\"{imageProcessing}\" \"{flagFocus}\" \"{flagCenter}\" \"{frame}\"";

                using (var process = Process.Start(psi))
                {
                    results_imageProcessing = process.StandardOutput.ReadToEnd();
                }

                string[] resultsArray_imageProcessing = results_imageProcessing.Split(' ');

                blurValue = float.Parse(resultsArray_imageProcessing[0]);
                posX_Pupil_Pixel = int.Parse(resultsArray_imageProcessing[1]);
                posZ_Pupil_Pixel = int.Parse(resultsArray_imageProcessing[2]);
                posX_Slit_Pixel = int.Parse(resultsArray_imageProcessing[3]);
                flagReadyPicture = bool.Parse(resultsArray_imageProcessing[4]);

                // YANNICK : VOIR POUR AVOIR LE BON TRANSFERT DE L'ORIGINE DES COORDONNÉES PERMETTANT DE DÉTERMINER L'ERREUR
                errorPupilX_Pixel = posX_Pupil_Pixel - W_Pixel / 2;
                errorPupilZ_Pixel = posZ_Pupil_Pixel - H_Pixel / 2;

                // PATRICK INITIATE A SetVelocity DANS LA DIRECTION QUI MINIMISE L'ERREUR
                if (errorPupilX_Pixel > tresholdError)
                {
                    // PATRICK: SEND SetVelocity DANS CET AXE
                }
                else
                {
                    // PATRICK: STOP VELOCITY  DANS CET AXE
                }

                if (errorPupilZ_Pixel > tresholdError)
                {
                    // PATRICK: SEND SetVelocity DANS CET AXE
                }
                else
                {
                    // PATRICK: STOP Velocity DANS CET AXE
                }
            }

            //SERAIT PERTINENT QU'ON ENREGISTRE LA POSITION EN X ET Z À CE MOMENT?

            flagReadyPicture = true;
            // GABRIELLE: VOIR POUR ALLER RELIER AU BOUTON POUR PRENDRE UNE PHOTO
            // Start of the 8 tests.




            //// OPTION 2
            // (1) THE BLUR IS OBTAIN USING A VARIABLE AVERAGE 
            // (2) READJUSTMENT OF THE POSITION DUE TO OVERSHOOT
            // (3) THE PUPIL IS CENTERED

            // Initialisation of the array used to dertemine the variable averages
            float[] lastPos = new float[50];
            float[] mobileAv = new float[11];
            float[] derivative = new float[10];

            // PATRICK: INITIALISER UN GetPosition OU SetVelocity POUR FAIRE AVANCER LA LAF DANS LA DIRECTION DES Y POSITIFS

            while (!flagFocus && !flagCenter)
            {
                // YANNICK AND GABRIELLE : ALLER CHERCHER LA FRAME DU VIDÉO DANS LE LOGICIEL LIGHTX (À INSÉRER DANS LA VARIABLE frame)

                // Call the python image processing function and obtain the values of blur, position of the pupil, the slit and the flag indicating that we are ready to take a picture.
                // VOIR POUR METTRE CE CODE APPELANT LE SCRIPT PYTHON DANS UNE FONCTION
                psi.Arguments = $"\"{imageProcessing}\" \"{flagFocus}\" \"{flagCenter}\" \"{frame}\"";

                using (var process = Process.Start(psi))
                {
                    results_imageProcessing = process.StandardOutput.ReadToEnd();
                }

                string[] resultsArray_imageProcessing = results_imageProcessing.Split(' ');

                blurValue = float.Parse(resultsArray_imageProcessing[0]);
                posX_Pupil_Pixel = int.Parse(resultsArray_imageProcessing[1]);
                posZ_Pupil_Pixel = int.Parse(resultsArray_imageProcessing[2]);
                posX_Slit_Pixel = int.Parse(resultsArray_imageProcessing[3]);
                flagReadyPicture = bool.Parse(resultsArray_imageProcessing[4]);

                if (!flagFocus)
                {
                    // DANS LE CODE AU PROPRE, INSÉRER CE CODE DANS UN FONCTION
                    // To insert the new value of blur in the vector lasPosition (most recent value at the index 0)
                    for (int i = 49; i > 0; i--)
                    {
                        lastPos[i] = lastPos[i - 1];
                    }
                    lastPos[0] = blurValue;

                    // To calculate and record in an array the mobile averages
                    Array.Resize(ref lastPos, 20);
                    for (int i = 10; i > 0; i--)
                    {
                        mobileAv[i] = mobileAv[i - 1];
                    }
                    float[] buffer = new float[20];
                    Array.Copy(lastPos, buffer, 20);
                    mobileAv[0] = buffer.Average();

                    // To create the derivative vector
                    float bufferDerivative;
                    for(int i = 0; i < 11; i++)
                    {
                        bufferDerivative = mobileAv[i] - mobileAv[i + 1];
                        if (bufferDerivative < 0)
                        {
                            derivative[i] = 1;
                        }
                        else
                        {
                            derivative[i] = 0;
                        }
                    }

                    // To determine if the LAF passed the focus
                    if (derivative.Sum() == 10)
                    {
                        if(lastPos.Max() >= thresholdBlur)
                        {
                            flagFocus = true;
                            float maxLastPos = blurValues.ToArray().Max();
                            nFrame = blurValues.IndexOf(maxBlurValue);
                            comY = nFrame * velocityY / freq;
                            // PATRICK: INITIER UN DÉPLACEMENT DANS L'AXE DES Y NÉGATIFS SELON LA VARIABLE comY
                        }
                    }

                }

                // REVOIR CETTE SECTION AVEC BASTIEN
                if (!flagCenter)
                {
                    // YANNICK : VOIR POUR AVOIR LE BON TRANSFERT DE L'ORIGINE DES COORDONNÉES PERMETTANT DE DÉTERMINER L'ERREUR
                    errorPupilX_Pixel = posX_Pupil_Pixel - W_Pixel / 2;
                    errorPupilZ_Pixel = posZ_Pupil_Pixel - H_Pixel / 2;

                    comX = errorPupilX_Pixel * factor;
                    comZ = errorPupilZ_Pixel * factor;

                    // PATRICK: FAIRE BOUGER LES MOTEURS SELON LES VARIABLES comX ET comZ

                    if (comX < 1 && comZ < 1)
                    {
                        flagCenter = true;
                    }
                }

            }

            flagReadyPicture = true;
            // GABRIELLE: VOIR POUR ALLER RELIER AU BOUTON POUR PRENDRE UNE PHOTO
            // Start of the 8 tests.
        }

    }
}

