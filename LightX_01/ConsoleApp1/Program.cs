using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using JsonFileWriter;

namespace JsonFileWriter
{
    class Program
    {
        static void Main()
        {
            List<GuideData> jsonData = new List<GuideData>();

            GuideData Conjonctive = new GuideData()
            {
                FileName = "Conjonctive.json",
                TestTitle = "Conjonctive",
                Zoom = "10x",
                IllumType = "Diffuse",
                IllumIntensity = "Moyenne",
                IllumAngle = "45°",
                InstructionsNotes = "",
                ImagePath01 = "./Resources/01_01.png",
                ImagePath02 = "./Resources/01_02.png"
            };

            GuideData VanHerick = new GuideData()
            {
                FileName = "VanHerick.json",
                TestTitle = "Van Herick",
                Zoom = "10x",
                IllumType = "Slit (plus mince possible)",
                IllumIntensity = "Max",
                IllumAngle = "60°",
                InstructionsNotes = "On désir estimer l'espace entre la cornée et l'iris en terme d'épaisseur de cornée.",
                ImagePath01 = "./Resources/02_01.png",
                ImagePath02 = "./Resources/02_02.png"
            };

            GuideData Cornea = new GuideData()
            {
                FileName = "Cornea.json",
                TestTitle = "Conée",
                Zoom = "10x",
                IllumType = "Slit (plus mince possible)",
                IllumIntensity = "Max",
                IllumAngle = "45°",
                InstructionsNotes = "On désir observer toutes anomalies, défauts, ammincicement en surface/à l'intérieur/derrière la cornée.",
                ImagePath01 = "./Resources/03_01.png",
                ImagePath02 = "./Resources/03_02.png"
            };

            GuideData Anterior = new GuideData()
            {
                FileName = "AnteriorChamber.json",
                TestTitle = "Chambre Antérieure",
                Zoom = "16x",
                IllumType = "Beam (1mm x 1mm)",
                IllumIntensity = "Max",
                IllumAngle = "40°",
                InstructionsNotes = "On désir observer s'il y a présence de cellules et/ou de flares.  Désire que la prise de vue se fasse devant l'entrée de la pupille pour obtenir un arrière plan noir (meilleurs contraste).",
                ImagePath01 = "./Resources/04_01.png",
                ImagePath02 = "./Resources/04_02.png"
            };

            GuideData Lens = new GuideData()
            {
                FileName = "Lens.json",
                TestTitle = "Cristallin",
                Zoom = "16x",
                IllumType = "Slit (plus mince possible)",
                IllumIntensity = "Max",
                IllumAngle = "40° (ou moins)",
                InstructionsNotes = "On désir observer s'il y a présence d'opacité ou changement de couleur dans le cristallin.",
                ImagePath01 = "./Resources/05_01.png",
                ImagePath02 = "./Resources/05_02.png"
            };

            GuideData PupillaryMargin = new GuideData()
            {
                FileName = "PupillaryMargin.json",
                TestTitle = "Marges Pupillaires",
                Zoom = "25x",
                IllumType = "Beam (1mm x 1mm)",
                IllumIntensity = "Élevé",
                IllumAngle = "40° (ou moins)",
                InstructionsNotes = "On désir observer s'il y a présence d'irrégularité ou de dépôt sur la marge.",
                ImagePath01 = "./Resources/06_01.png",
                ImagePath02 = "./Resources/06_02.png"
            };

            GuideData IrisTransillumination = new GuideData()
            {
                FileName = "IrisTransillumination.json",
                TestTitle = "Transillumination de l'iris",
                Zoom = "10x",
                IllumType = "Beam (2mm x 2mm) Réduire la taille si la pupille est trop petite.",
                IllumIntensity = "Max",
                IllumAngle = "0° (environ)",
                InstructionsNotes = "On désir observer s'il y a présence d'irrégularité ou de dépôt sur la marge.",
                ImagePath01 = "./Resources/07_01.png",
                ImagePath02 = "./Resources/07_02.png"
            };

            GuideData Cobalt = new GuideData()
            {
                FileName = "CobaltFilter.json",
                TestTitle = "Filtre Cobalt",
                Zoom = "10x",
                IllumType = "Diffuse avec filtre cobalt",
                IllumIntensity = "Élevé",
                IllumAngle = "30° (environ)",
                InstructionsNotes = "Administer les gouttes de fluorescéine au patient et laissez agir. On désir observer s'il y a présence d'abrasion/défaut dans la cornée.",
                ImagePath01 = "./Resources/08_01.png",
                ImagePath02 = "./Resources/08_02.png"
            };


            jsonData.Add(Conjonctive);
            jsonData.Add(VanHerick);
            jsonData.Add(Cornea);
            jsonData.Add(Anterior);
            jsonData.Add(Lens);
            jsonData.Add(PupillaryMargin);
            jsonData.Add(IrisTransillumination);
            jsonData.Add(Cobalt);

            WriteJsonFiles(@"..\LightX_01\Resources\", jsonData);
            
        }
        static void WriteJsonFiles(string path, List<GuideData> jsonData)
        {
            foreach (GuideData data in jsonData)
            {
                using (StreamWriter file = File.CreateText($"{path}{data.FileName}"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, data);
                }
            }

        }

    }
}