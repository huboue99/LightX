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
            List<GuideData> jsonData = new List<GuideData>();

            GuideData Conjonctive = new GuideData()
            {
                Id = Tests.Conjonctive,
                TestTitle = "Conjonctive",
                SlitIntensity = "0",
                IllumAngle = "-",
                DiffuseIntensity = "HIGH",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "16.0", Iso = "6400", BurstNumber = "3"},
                InstructionsNotes = "Faire le foyer sur l'iris.\nObservation des paupières, cils, sclère, conjonctive, rougeurs, iris, vaisseaux sanguins.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_00.png" }

            };

            GuideData VanHerick = new GuideData()
            {
                Id = Tests.VanHerick,
                TestTitle = "Van Herick",
                SlitIntensity = "10",
                IllumAngle = "60°",
                DiffuseIntensity = "LOW",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "11.0", Iso = "3200", BurstNumber = "2"},
                InstructionsNotes = "Faire le foyer sur la cornée.\nOn désir estimer l'espace entre la cornée et l'iris en terme d'épaisseur de cornée.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_00.png" }
            };

            GuideData Cornea = new GuideData()
            {
                Id = Tests.Cornea,
                TestTitle = "Cornée",
                SlitIntensity = "10",
                IllumAngle = "45°",
                DiffuseIntensity = "HIGH",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "8.0", Iso = "6400", BurstNumber = "2"},
                InstructionsNotes = "On désir observer toutes anomalies, défauts, ammincicement en surface/à l'intérieur/derrière la cornée.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_00.png" }
            };

            GuideData Anterior = new GuideData()
            {
                Id = Tests.AnteriorChamber,
                TestTitle = "Chambre Antérieure",
                SlitIntensity = "10",
                IllumAngle = "40°",
                DiffuseIntensity = "OFF",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "5.6", Iso = "3200", BurstNumber = "4" },
                InstructionsNotes = "On désir observer s'il y a présence de cellules et/ou de flares.  Désire que la prise de vue se fasse devant l'entrée de la pupille pour obtenir un arrière plan noir (meilleurs contraste).",
                ImagesPath = new List<string>() { @"..\..\Resources\01_00.png" }
            };

            GuideData Lens = new GuideData()
            {
                Id = Tests.Lens,
                TestTitle = "Cristallin",
                SlitIntensity = "10",
                IllumAngle = "40°",
                DiffuseIntensity = "OFF",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "5.6", Iso = "1600", BurstNumber = "4" },
                InstructionsNotes = "On désir observer s'il y a présence d'opacité ou changement de couleur dans le cristallin.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_00.png" }
            };

            GuideData PupillaryMargin = new GuideData()
            {
                Id = Tests.PupillaryMargin,
                TestTitle = "Marges Pupillaires",
                SlitIntensity = "0",
                IllumAngle = "40°",
                DiffuseIntensity = "LOW",
                CamSettings = new CameraSettings() { Flash = "ON + 2 step", ShutterSpeed = "1/200", FNumber = "2.8", Iso = "100", BurstNumber = "1" },
                InstructionsNotes = "On désir observer s'il y a présence d'irrégularité ou de dépôt sur la marge.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_00.png" }
            };

            GuideData IrisTransillumination = new GuideData()
            {
                Id = Tests.IrisTransillumination,
                TestTitle = "Transillumination de l'iris",
                SlitIntensity = "10",
                IllumAngle = "10°",
                DiffuseIntensity = "OFF",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/200", FNumber = "2.8", Iso = "6400", BurstNumber = "2" },
                InstructionsNotes = "On désir observer s'il y a présence d'irrégularité ou de dépôt sur la marge.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_00.png" }
            };

            GuideData Cobalt = new GuideData()
            {
                Id = Tests.CobaltFilter,
                TestTitle = "Filtre Cobalt",
                SlitIntensity = "5",
                IllumAngle = "30°",
                DiffuseIntensity = "OFF",
                CamSettings = new CameraSettings() { Flash = "OFF", ShutterSpeed = "1/100", FNumber = "11.0", Iso = "6400", BurstNumber = "2" },
                InstructionsNotes = "Administer les gouttes de fluorescéine au patient et laissez agir. On désir observer s'il y a présence d'abrasion/défaut dans la cornée.",
                ImagesPath = new List<string>() { @"..\..\Resources\01_00.png" }
            };


            jsonData.Add(Conjonctive);
            jsonData.Add(VanHerick);
            jsonData.Add(Cornea);
            jsonData.Add(Anterior);
            jsonData.Add(Lens);
            jsonData.Add(PupillaryMargin);
            jsonData.Add(IrisTransillumination);
            jsonData.Add(Cobalt);

            WriteJsonFiles(@"..\..\..\LightX\Resources\", jsonData);
            //WriteJsonFiles(@"..\..\..\LightX\bin\Debug\Resources\", jsonData);

        }
        static void WriteJsonFiles(string path, List<GuideData> jsonData)
        {
            foreach (GuideData data in jsonData)
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
