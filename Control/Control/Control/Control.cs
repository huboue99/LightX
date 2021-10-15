using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace Control.Control
{
    class Control
    {
        private static int[] imProcessingControl(bool flagFocus, bool flagCenter, float[,] frame)
        {
            // VOIR COMMENT JE POURRAIS FAIRE UN PATH GÉNÉRAL...
            var psi = new ProcessStartInfo();
            psi.FileName = @"C:\Program Files\Python39\python.exe";
            var imageProcessing = @"C:\Users\cassa\Desktop\LightXGithub\userinterface\imageProcessingOeil.py";

            // Revoir l'utilité de flag!
            psi.Arguments = $"\"{imageProcessing}\" \"{flagFocus}\" \"{flagCenter}\" \"{frame}\"";

            string results_imageProcessing;
            using (var process = Process.Start(psi))
            {
                results_imageProcessing = process.StandardOutput.ReadToEnd();
            }

            string[] resultsArray_imageProcessing = results_imageProcessing.Split(' ');

            int blurValue = int.Parse(resultsArray_imageProcessing[0]);
            int posX_Pupil_Pixel = int.Parse(resultsArray_imageProcessing[1]);
            int posZ_Pupil_Pixel = int.Parse(resultsArray_imageProcessing[2]);
            int posX_Slit_Pixel = int.Parse(resultsArray_imageProcessing[3]);

            int[] datasImagePocessing = { blurValue, posX_Pupil_Pixel, posZ_Pupil_Pixel, posX_Slit_Pixel };
            return datasImagePocessing;
        }

        public static bool automatedPositioningLAF(float[,] frame)
        {
            List<float> locationBlur = new List<float>();
            List<float> blurValues = new List<float>();

            float posY = 0;
            bool flagFocus = false;
            bool flagCenter = false;

            // PATRICK: COMMANDE DE POSITION POUR FAIRE AVANCER LE MOTEUR
            float screenYMax = 30;               // À changer pour la bonne valeur.
            while (posY != screenYMax)
            {
                int[] datasImagePocessing = imProcessingControl(flagFocus, flagCenter, frame);

                // PATRICK: FAIRE UN GetPosition() Y AXIS (posY = getPosition())

                locationBlur.Add(posY);
                blurValues.Add(datasImagePocessing[0]);

            }

            float maxBlurValue = blurValues.ToArray().Max();
            int maxBlurValueIndex = blurValues.IndexOf(maxBlurValue);
            float posFocus = locationBlur[maxBlurValueIndex];

            // PATRICK : GetPosition JUSQU'À LA POSITION QUI NOUS PERMET DE DÉPASSER LE FOCUS (SCREENING)

            int errorPupilX_Pixel = 1000;       // Distance between the pupil and the center of the frame (pixels)
            int errorPupilZ_Pixel = 1000;       // Distance between the pupil and the center of the frame (pixels)
            int tresholdError = 15;             // Acceptable error for the centered pupil (pixels)    
            int H_Pixel = 1080;                 // Height of one frame of the video (pixels)
            int W_Pixel = 1920;                 // Width of one frame of the video (pixels)


            while (errorPupilX_Pixel > tresholdError && errorPupilZ_Pixel > tresholdError)
            {
                // YANNICK AND GABRIELLE : ALLER CHERCHER LA FRAME DU VIDÉO DANS LE LOGICIEL LIGHTX (À INSÉRER DANS LA VARIABLE frame)
                int[] datasImagePocessing = imProcessingControl(flagFocus, flagCenter, frame);

                // YANNICK : VOIR POUR AVOIR LE BON TRANSFERT DE L'ORIGINE DES COORDONNÉES PERMETTANT DE DÉTERMINER L'ERREUR
                errorPupilX_Pixel = datasImagePocessing[1] - (W_Pixel / 2);
                errorPupilZ_Pixel = (H_Pixel / 2) - datasImagePocessing[2];

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

            bool flagReadyPicture = true;
            return flagReadyPicture;
        }
    }
}
