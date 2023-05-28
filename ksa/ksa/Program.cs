using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using ksa.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ksa;

internal class Program
{
    private static readonly string _directory = Directory.GetCurrentDirectory();

    static void Main(string[] args)
    {
        Console.WriteLine("Willkommen bei der ksa.exe.\nBitte verwenden Sie einen der folgenden Befehle:\n-n \n-csvimp \n-xmlimp \n-jsonimp \n");
        string command = args.Count() > 0 ? args[0] : Console.ReadLine();

        switch (command)
        {
            case "-n":
                Console.WriteLine("Annika Schäfer, Marika Lübbers, Kaya Kopp");
                break;
            case "-csvimp":
                CsvImport();
                break;
            case "-xmlimp":
                XmlImport();
                break;
            case "-jsonimp":
                JsonImport();
                break;
            default:
                Console.WriteLine("Unbekannter Befehl. Bekannte Befehle: -n, -csvimp, -xmlimp, -jsonimp");
                break;
        }

        Console.WriteLine("Befehl wurde ausgeführt, drücken Sie irgendeine Taste zum Beenden");
        Console.ReadKey();
    }

    static void CsvImport()
    {
        List<string> filenames = new List<string>();
        foreach (string filename in filenames)
        {
            Console.WriteLine(filename + " erfolgreich importiert!");
        }
    }
    static void XmlImport()
    {
        string[] filenames = Directory.GetFiles(_directory, "*.xml");
        foreach (string filename in filenames)
        {
            XDocument xmlDoc = XDocument.Load(filename);


            Dictionary<long, List<Objekt>> kundenUndObjekte = new Dictionary<long, List<Objekt>>();

            foreach (XElement element in xmlDoc.Descendants("entry"))
            {
                long kundenNr = long.Parse(element.Element("obj_kunde_nr").Value);
                string objektNr = element.Element("obj_nr").Value;
                if (kundenUndObjekte.TryGetValue(kundenNr, out List<Objekt> kundenObjekte))
                {
                    Objekt currentObjekt = kundenObjekte.Find((objekt) => objekt.Nr == objektNr);
                    if (currentObjekt != null)
                    {
                        CreateAndAddObjektAbfallArt(element, currentObjekt, objektNr);
                    }
                    else
                    {
                        Objekt newObjekt = CreateObjekt(element, objektNr, kundenNr);
                        kundenObjekte.Add(newObjekt);
                        CreateAndAddObjektAbfallArt(element, newObjekt, newObjekt.Nr);
                    }

                }
                else
                {
                    Objekt objekt = CreateObjekt(element, objektNr, kundenNr);
                    kundenUndObjekte[kundenNr] = new List<Objekt>() { objekt };
                    CreateAndAddObjektAbfallArt(element, objekt, objektNr);
                }

            }
            DataAccess.InsertData(kundenUndObjekte);
            Console.WriteLine(filename + " erfolgreich importiert!");
        }
    }

    static void CreateAndAddObjektAbfallArt(XElement element, Objekt objektToAddTo, string objektNr)
    {
        ObjektAbfallArt currentObjektAbfallArt = new ObjektAbfallArt();
        currentObjektAbfallArt.ObjNr = objektNr;
        currentObjektAbfallArt.Abfallart = element.Element("abfallart").Value;
        currentObjektAbfallArt.Volumen = int.Parse(element.Element("volumen").Value);
        currentObjektAbfallArt.Anzahl = int.Parse(element.Element("anzahl").Value);

        objektToAddTo.ObjektAbfallArt.Add(currentObjektAbfallArt);
    }

    static Objekt CreateObjekt(XElement element, string objektNr, long kundenNr)
    {
        Objekt objektToAdd = new Objekt();
        objektToAdd.Nr = objektNr;
        objektToAdd.Kunde_Nr = kundenNr;
        objektToAdd.Straße = element.Element("obj_str").Value;
        objektToAdd.HausNr = int.Parse(element.Element("obj_haus_nr").Value);
        objektToAdd.PLZ = int.Parse(element.Element("obj_plz").Value);
        objektToAdd.Ort = element.Element("obj_ort").Value;

        return objektToAdd;
    }


    static void JsonImport()
    {
        string[] filenames = Directory.GetFiles(_directory, "*.json");

        foreach (string filename in filenames)
        {
            StreamReader streamReader = new StreamReader(filename);
            string jsonString = streamReader.ReadToEnd();

            Dictionary<long, List<Objekt>> kundenUndObjekte = new Dictionary<long, List<Objekt>>();            

            var jsonArray = JArray.Parse(jsonString);

            int index = 0;

            foreach (var data in jsonArray)
            {
                long kundenNr = (long)data["obj_kunde_nr"];
                string objektNr = (string)data["obj_nr"];  
                

                if(kundenUndObjekte.TryGetValue(kundenNr, out List<Objekt> kundenObjekte))
                {
                    Objekt currentObjekt = kundenObjekte.Find((objekt) => objekt.Nr == objektNr);
                    if (currentObjekt != null)
                    {                        
                        JSONCreatandAddAbfallart(jsonArray, currentObjekt, objektNr, index);
                    }
                    else
                    {
                        Objekt newObjekt = JSONCreateObjekt(jsonArray, objektNr, kundenNr, index);
                        kundenObjekte.Add(newObjekt);
                        JSONCreatandAddAbfallart(jsonArray, newObjekt, newObjekt.Nr, index);
                    }

                }
                else
                {
                    Objekt objekt = JSONCreateObjekt(jsonArray, objektNr, kundenNr, index);
                    kundenUndObjekte[kundenNr] = new List<Objekt>() { objekt };
                    JSONCreatandAddAbfallart(jsonArray, objekt, objektNr, index);
                }

                index++;
            }
            DataAccess.InsertData(kundenUndObjekte);
            string filenameWithoutExt = Path.GetFileName(filename);
            Console.WriteLine(filenameWithoutExt + " erfolgreich importiert!");
        }

    }    

    static void JSONCreatandAddAbfallart(JArray jsonArray, Objekt objektToAddTo, string objektNr, int index)
    {        
            ObjektAbfallArt currentObjektAbfallArt = new ObjektAbfallArt();
            currentObjektAbfallArt.ObjNr = objektNr;
            currentObjektAbfallArt.Abfallart = (string)jsonArray[index]["abfallart"];
            currentObjektAbfallArt.Volumen = (int)jsonArray[index]["volumen"];
            currentObjektAbfallArt.Anzahl = (int)jsonArray[index]["anzahl"];

            objektToAddTo.ObjektAbfallArt.Add(currentObjektAbfallArt);        
    }

    static Objekt JSONCreateObjekt(JArray jsonArray, string objektNr, long kundenNr, int index)
    {
        
            Objekt objektToAdd = new Objekt();
            objektToAdd.Nr = objektNr;
            objektToAdd.Kunde_Nr = kundenNr;
            objektToAdd.Straße = (string)jsonArray[index]["obj_str"];

            objektToAdd.HausNr = (int)jsonArray[index]["obj_haus_nr"];
            objektToAdd.PLZ = (int)jsonArray[index]["obj_plz"];
            objektToAdd.Ort = (string)jsonArray[index]["obj_ort"];

            return objektToAdd;
        

    }

}

