using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using LightX.Classes;
using System;

namespace JsonFileWriter
{
    class Program
    {
        static void Main()
        {
            List<TestInstructions> jsonData = new List<TestInstructions>();


            #region Conjonctive

            TestInstructions Conjonctive = new TestInstructions()
            {
                Id = Tests.Conjonctive,
                TestTitle = "Conjonctive"
            };

            Conjonctive.Instructions.Add( new Instruction()
                {
                    SlitIntensity = "0",
                    IllumAngle = "-",
                    DiffuseIntensity = "HIGH",
                    CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "16.0", Iso = "6400", BurstNumber = "3"},
                    InstructionsNotes = "Faire le foyer sur l'iris.\nObservation des paupières, cils, sclère, conjonctive, rougeurs, iris, vaisseaux sanguins.",
                    ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
                });

            #endregion Conjonctive

            #region VanHerick

            TestInstructions VanHerick = new TestInstructions()
            {
                Id = Tests.VanHerick,
                TestTitle = "Van Herick",
            };

            VanHerick.Instructions.Add(new Instruction()
            {
                SlitIntensity = "10",
                IllumAngle = "60° temporal",
                DiffuseIntensity = "LOW",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "11.0", Iso = "3200", BurstNumber = "2"},
                InstructionsNotes = "Faire le foyer sur la cornée.\nOn désir estimer l'espace entre la cornée et l'iris en terme d'épaisseur de cornée.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
            });

            VanHerick.Instructions.Add(new Instruction()
            {
                SlitIntensity = "10",
                IllumAngle = "60° nasal",
                DiffuseIntensity = "LOW",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "11.0", Iso = "3200", BurstNumber = "2" },
                InstructionsNotes = "Faire le foyer sur la cornée.\nOn désir estimer l'espace entre la cornée et l'iris en terme d'épaisseur de cornée.",
                ImagesPath = new List<string>() { @"..\..\Resources\02_01.png" }
            });

            #endregion VanHerick

            #region Cornea

            TestInstructions Cornea = new TestInstructions()
            {
                Id = Tests.Cornea,
                TestTitle = "Cornée",
            };

            Cornea.Instructions.Add(new Instruction()
            {
                SlitIntensity = "10",
                IllumAngle = "45° temporal",
                DiffuseIntensity = "HIGH",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "8.0", Iso = "6400", BurstNumber = "2"},
                InstructionsNotes = "Balayer l'oeil du côté temporal au centre. Garder le foyer sur la cornée.\nOn désir observer toutes anomalies, défauts ou ammincicement en surface/à l'intérieur/derrière la cornée.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
            });

            Cornea.Instructions.Add(new Instruction()
            {
                SlitIntensity = "10",
                IllumAngle = "45° nasal",
                DiffuseIntensity = "HIGH",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "8.0", Iso = "6400", BurstNumber = "2" },
                InstructionsNotes = "Balayer l'oeil du centre au côté nasal. Garder le foyer sur la cornée.\nOn désir observer toutes anomalies, défauts ou ammincicement en surface/à l'intérieur/derrière la cornée.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
            });

            #endregion Cornea

            #region Anterior Chamber

            TestInstructions Anterior = new TestInstructions()
            {
                Id = Tests.AnteriorChamber,
                TestTitle = "Chambre Antérieure",
            };

            Anterior.Instructions.Add(new Instruction()
            {
                SlitIntensity = "10",
                IllumAngle = "40°",
                DiffuseIntensity = "OFF",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "5.6", Iso = "3200", BurstNumber = "4" },
                InstructionsNotes = "Faire le foyer sur le centre du volume éclairé par le faisceau dans la chambre antérieur. Le volume est délimité aux extrémités par des bande blanches.  Une plus étroite et intense; la cornée.  L’autre plus diffuse et large; le cristallin.  La portion d’intérêt est la bande noire.\nOn désir observer s'il y a présence de cellules et/ou de flares.  Désire que la prise de vue se fasse devant l'entrée de la pupille pour obtenir un arrière plan noir (meilleurs contraste).",
                ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
            });

            #endregion Anterior Chamber

            #region Lens

            TestInstructions Lens = new TestInstructions()
            {
                Id = Tests.Lens,
                TestTitle = "Cristallin",
            };

            Lens.Instructions.Add(new Instruction()
            {
                SlitIntensity = "10",
                IllumAngle = "40° ou moins",
                DiffuseIntensity = "OFF",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "5.6", Iso = "1600", BurstNumber = "4" },
                InstructionsNotes = "Faire le foyer à la surface antérieur du cristallin.\nOn désir observer s'il y a présence d'opacité ou changement de couleur dans le cristallin.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
            });

            Lens.Instructions.Add(new Instruction()
            {
                SlitIntensity = "10",
                IllumAngle = "40° ou moins",
                DiffuseIntensity = "OFF",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "5.6", Iso = "1600", BurstNumber = "4" },
                InstructionsNotes = "Faire le foyer à la surface postérieur du cristallin.\nOn désir observer s'il y a présence d'opacité ou changement de couleur dans le cristallin.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
            });

            Lens.Instructions.Add(new Instruction()
            {
                SlitIntensity = "8",
                IllumAngle = "40° ou moins",
                DiffuseIntensity = "OFF",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "5.6", Iso = "1600", BurstNumber = "4" },
                InstructionsNotes = "Faire le foyer au centre du cristallin.\nOn désir observer s'il y a présence d'opacité ou changement de couleur dans le cristallin.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
            });

            #endregion Lens

            #region Pupillary Margin

            TestInstructions PupillaryMargin = new TestInstructions()
            {
                Id = Tests.PupillaryMargin,
                TestTitle = "Marges Pupillaires",
            };

            PupillaryMargin.Instructions.Add(new Instruction()
            {
                SlitIntensity = "0",
                IllumAngle = "-",
                DiffuseIntensity = "LOW",
                CamSettings = new CameraSettings() { Flash = "ON (Temportal)", ShutterSpeed = "1/200", FNumber = "2.8", Iso = "100", BurstNumber = "1" },
                InstructionsNotes = "1- Déconnectez la caméra.\n2- Modifier les paramètres du flash dans le menu de la caméra.\n3- Reconnectez la caméra\n\n Faire le foyer à la surface de l'iris.\nOn désir observer s'il y a présence d'irrégularité ou de dépôt sur la marge.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
            });

            PupillaryMargin.Instructions.Add(new Instruction()
            {
                SlitIntensity = "0",
                IllumAngle = "-",
                DiffuseIntensity = "LOW",
                CamSettings = new CameraSettings() { Flash = "ON (Nasal)", ShutterSpeed = "1/200", FNumber = "2.8", Iso = "100", BurstNumber = "1" },
                InstructionsNotes = "1- Déconnectez la caméra.\n2- Modifier les paramètres du flash dans le menu de la caméra.\n3- Reconnectez la caméra\n\n Faire le foyer à la surface de l'iris.\nOn désir observer s'il y a présence d'irrégularité ou de dépôt sur la marge.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
            });

            #endregion Pupillary Margin

            #region Iris Transillumination

            TestInstructions IrisTransillumination = new TestInstructions()
            {
                Id = Tests.IrisTransillumination,
                TestTitle = "Transillumination de l'iris",
            };

            IrisTransillumination.Instructions.Add(new Instruction()
            {
                SlitIntensity = "10 (1mm x 1mm)",
                IllumAngle = "10°",
                DiffuseIntensity = "OFF",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/200", FNumber = "2.8", Iso = "6400", BurstNumber = "2" },
                InstructionsNotes = "Faire le focus sur l'iris.\nOn désir observer si la réflexion de la lumière sur la rétine est visible à travers l'iris.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
            });

            #endregion Iris Transillumination

            #region Cobalt

            TestInstructions Cobalt = new TestInstructions()
            {
                Id = Tests.CobaltFilter,
                TestTitle = "Filtre Cobalt",
            };

            Cobalt.Instructions.Add(new Instruction()
            {
                SlitIntensity = "5 (Filtre cobalt)",
                IllumAngle = "30°",
                DiffuseIntensity = "OFF",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "11.0", Iso = "6400", BurstNumber = "2" },
                InstructionsNotes = "Faire le foyer sur la cornée et la conjonctive.\nAdminister les gouttes de fluorescéine au patient et laissez agir.\nOn désir observer s'il y a présence d'abrasion/défaut dans la cornée.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_01.png" }
            });

            #endregion Cobalt

            jsonData.Add(Conjonctive);
            jsonData.Add(VanHerick);
            jsonData.Add(Cornea);
            jsonData.Add(Anterior);
            jsonData.Add(Lens);
            jsonData.Add(PupillaryMargin);
            jsonData.Add(IrisTransillumination);
            jsonData.Add(Cobalt);

            WriteJsonFiles(@"..\..\..\LightX\Resources\", jsonData);
        }

        static void WriteJsonFiles(string path, List<TestInstructions> jsonData)
        {
            foreach (TestInstructions data in jsonData)
            {
                using (StreamWriter file = File.CreateText($"{path}{data.Id.ToString()}.json"))
                {
                    Console.WriteLine("Writing {0}.json to disk...", data.Id.ToString());
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, data);
                }
            }
        }
    }
}
