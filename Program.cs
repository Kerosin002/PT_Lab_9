using System;
using System.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

using System.Xml.Linq;
using System.Runtime.ConstrainedExecution;


namespace Lab9
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Car> myCars = new List<Car>()
            {
                new Car("E250", new Engine(1.8, 204, "CGI"), 2009),
                new Car("E350", new Engine(3.5, 292, "CGI"), 2009),
                new Car("A6", new Engine(2.5, 187, "FSI"), 2012),
                new Car("A6", new Engine(2.8, 220, "FSI"), 2012),
                new Car("A6", new Engine(3.0, 295, "TFSI"), 2012),
                new Car("A6", new Engine(2.0, 175, "TDI"), 2011),
                new Car("A6", new Engine(3.0, 309, "TDI"), 2011),
                new Car("S6", new Engine(4.0, 414, "TFSI"), 2012),
                new Car("S8", new Engine(4.0, 513, "TFSI"), 2012)
            };


            firstTk(myCars);
            secondTk(myCars, "serializedXml.xml");
            thirdTk("serializedXml.xml");
            fourthTk(myCars, "linqSerializedXml.xml");
            fifthTk(myCars, "xhtmlTable.html");
            sixthTk("serializedXml.xml", "serializedXmlNew.xml");
        }


        static void firstTk(List<Car> myCars)
        {
            var projectedCars = myCars
                .Where(car => car.model == "A6")
                .Select(car => new
                {
                    engineType = (car.motor.model == "TDI" ? "diesel" : "petrol"),
                    hppl = (double)(car.motor.horsePower / car.motor.displacement)
                });

            var groupped = projectedCars
                .GroupBy(car => (car.engineType))
                .OrderBy(t => t.Key);

            foreach (var group in groupped)
            {
                Console.WriteLine(group.Key + ": " + group.Average(car => car.hppl));
            }
        }

        static void secondTk(List<Car> myCars, string xmlFileName)
        {
            Serialize(myCars, xmlFileName);
            List<Car> myCarsD = Deserialize(xmlFileName);
            Console.WriteLine("Deserialized contains: ");
            foreach (var x in myCarsD)
            {
                Console.WriteLine($"{x.year}, {x.motor.model}, {x.motor.horsePower}, {x.motor.displacement}");
            }
            
        }

        static void thirdTk(string filename)
        {

            XElement rootNode = XElement.Load(filename);
            double avgHP = (double)rootNode
                .XPathEvaluate(" sum(//car/engine[@model!=\"TDI\"]/horsePower) div count(//car/engine[@model!=\"TDI\"]/horsePower)");
            Console.WriteLine("Avarage power of cars with engines not TDI = " + avgHP);
            Console.WriteLine();

            IEnumerable<XElement> models = rootNode
                .XPathSelectElements("//car/model[not(. = preceding::model)]");


            Console.WriteLine("Car Models:");
            foreach (XElement e in models)
            {
                Console.WriteLine(e.Value);
            }
        }

        static void fourthTk(List<Car> myCars, string filename)
        {
            var nodes = myCars
                .Select(
                    car => new XElement("car",
                                new XElement("model", car.model),
                                new XElement("year", car.year),
                                new XElement("engine", new XAttribute("model", car.motor.model), new XElement("displacement", car.motor.displacement), new XElement("horsePower", car.motor.horsePower)))
                                );

            XElement rootNode = new XElement("cars", nodes);
            rootNode.Save(filename);
            
        }

        static void fifthTk(List<Car> myCars, string filename)
        {
            var tableRows = myCars.Select(
                car => new XElement("tr",
                            new XElement("td", car.model),
                            new XElement("td", car.year),
                            new XElement("td", car.motor.model),
                            new XElement("td", car.motor.displacement),
                            new XElement("td", car.motor.horsePower)
                        )
                );

            var table = new XElement("table", new XAttribute("style", "border: 8px solid black"), tableRows);
            var template = XElement.Load("template.html");
            var body = template.Element("{http://www.w3.org/1999/xhtml}body");
            body?.Add(table);
            template.Save(filename);

            
        }

        static void sixthTk(string origin, string filename)
        {
            var doc = XDocument.Load(origin);

            foreach (var car in doc.Root!.Elements())
            {
                foreach (var field in car.Elements())
                {
                    if (field.Name == "engine")
                    {
                        foreach (var engineField in field.Elements())
                        {
                            if (engineField.Name == "horsePower")
                            {
                                engineField.Name = "hp";
                            }
                        }
                    }
                    if (field.Name == "model")
                    {
                        var yearField = car.Element("year");
                        var attribute = new XAttribute("year", yearField!.Value);
                        field.Add(attribute);
                        yearField.Remove();
                    }
                }
            }

            doc.Save(filename);
            
        }

        static void Serialize(List<Car> myCars, string xmlFileName)
        {
            var serializer = new XmlSerializer(typeof(List<Car>), new XmlRootAttribute("cars"));
            var currentDirectory = Directory.GetCurrentDirectory();
            Path.Combine(currentDirectory, xmlFileName);
            using var writer = new StreamWriter(xmlFileName);

            serializer.Serialize(writer, myCars);
            Console.WriteLine("XML created! Filename: " + xmlFileName + " in " + currentDirectory);
            
        }

        private static List<Car>? Deserialize(string xmlFileName)
        {
            var serializer = new XmlSerializer(typeof(List<Car>), new XmlRootAttribute("cars"));
            using Stream reader = new FileStream(xmlFileName, FileMode.Open);

            Console.WriteLine("XML deserialized from Filename: " + xmlFileName);
            return serializer.Deserialize(reader) as List<Car>;
        }
    }
}
