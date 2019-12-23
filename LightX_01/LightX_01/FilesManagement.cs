using LightX_01.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LightX_01
{
    public class Functions
    {
        public void WriteBinFile(PatientData obj, string fileName)
        {
            Stream stream = File.Open(fileName, FileMode.Create);
            BinaryFormatter bformatter = new BinaryFormatter();

            Console.WriteLine("Writing Employee Information");
            try
            {
                bformatter.Serialize(stream, obj);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
                Console.WriteLine("The passed object doesn't have a serialization function.");
            }

            stream.Close();
        }
        
        public PatientData ReadBinFile(string fileName)
        {
            //Open the file written above and read values from it.
            Stream stream = File.Open(fileName, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();

            Console.WriteLine("Reading Employee Information");
            PatientData pd = new PatientData();
            pd = (PatientData)bformatter.Deserialize(stream);
            stream.Close();

            Console.WriteLine("Patient Age: {0}", pd.Age.ToString());
            Console.WriteLine("Patient First Name: {0}", pd.FirstName);
            Console.WriteLine("Patient Last Name: {0}", pd.LastName);
            return pd;
        }
    }
}
