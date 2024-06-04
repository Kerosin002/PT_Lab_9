
using System.Xml.Serialization;

namespace Lab9
{
    [XmlType("car")]
    public class Car
    {
        public string model;
        public int year;
        [XmlElement(ElementName = "engine")]
        public Engine motor { get; set; }

        public Car()
        {

        }

        public Car(string modelName, Engine motorOnBoard, int yearOfProduction)
        {
            model = modelName;
            year = yearOfProduction;
            motor = motorOnBoard;
        }

        
    }
}