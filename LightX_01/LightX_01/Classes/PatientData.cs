using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace LightX_01.Classes
{
    [Serializable()]
    public class PatientData : ISerializable
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public DateTime ExamDate { get; set; }

        public List<string> TestList { get; set; }

        public PatientData()
        {
            //get dateCurrent
            TestList = new List<string> { "Overview", "VanHerick", "Cornea", "AnteriorChamber", "Lens", "PupillaryMargin", "IrisTransillumination", "CobaltFilter" };
            ExamDate = DateTime.Now;
        }

        public PatientData(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            Age = (int)info.GetValue("PatientAge", typeof(int));
            FirstName = (String)info.GetValue("PatientFirstName", typeof(string));
            LastName = (String)info.GetValue("PatientLastName", typeof(string));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //You can use any custom name for your name-value pair. But make sure you
            // read the values with the same name. For ex:- If you write EmpId as "EmployeeId"
            // then you should read the same with "EmployeeId"
            info.AddValue("PatientAge", Age);
            info.AddValue("PatientFirstName", FirstName);
            info.AddValue("PatientLastName", FirstName);
        }
    }

    

    
}
