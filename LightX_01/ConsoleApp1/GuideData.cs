using System;
using System.Collections.Generic;
using System.Text;

namespace JsonFileWriter
{
    public class Parameters
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class ParametersList
    {
        public List<Parameters> ParamList { get; set; }

        public ParametersList(GuideData data)
        {
            List<Parameters> ParamList = new List<Parameters>();
            ParamList.Add(new Parameters() { Name = "Grossissement", Value = data.Zoom });
            ParamList.Add(new Parameters() { Name = "Type d'illumination", Value = data.IllumType });
            ParamList.Add(new Parameters() { Name = "Intensité d'illumination", Value = data.IllumIntensity });
            ParamList.Add(new Parameters() { Name = "Angle d'illumination", Value = data.IllumAngle });
        }
    }

    public class GuideData
    {
        public string FileName { get; set; }

        public string TestTitle { get; set; }

        public string Zoom { get; set; }

        public string IllumType { get; set; }

        public string IllumIntensity { get; set; }

        public string IllumAngle { get; set; }

        public string InstructionsNotes { get; set; }
        
        public string ImagePath01 { get; set; }

        public string ImagePath02 { get; set; }

        public string ImagePath03 { get; set; }
    }


}
