using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ksa.Models;

namespace ksa;

internal class Program
{
    private static readonly string _directory = Directory.GetCurrentDirectory();

    static void Main(string[] args)
    {
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
            foreach (XElement element in xmlDoc.Descendants("entry"))
            {
                Console.WriteLine(element.Element("obj_kunde_nr"));
            }

            Dictionary<long, List<Objekt>> kundenUndObjekte = xmlDoc.Descendants("entry").ToDictionary(
                el => long.Parse(el.Element("obj_kunde_nr").Value),
                el => returnObjektListe(xmlDoc, long.Parse(el.Element("obj_kunde_nr").Value)));


            Console.WriteLine(filename + " erfolgreich importiert!");
        }
    }

    static List<Objekt> returnObjektListe(XDocument doc, long kunden_nr)
    {
        var objektListe = new List<Objekt>();
        foreach(var element in doc.Descendants("entry"))
        {
            long currentKunde = long.Parse(element.Element("obj_kunde_nr").Value);
            if (currentKunde == kunden_nr)
            {
                Objekt objekt = new Objekt();

                objekt.Nr = element.Element("obj_nr").Value;
                objekt.Kunde_Nr = long.Parse(element.Element("obj_kunde_nr").Value);
                objekt.Straße = element.Element("obj_str").Value;
                objekt.HausNr = int.Parse(element.Element("obj_haus_nr").Value);
                objekt.PLZ = int.Parse(element.Element("obj_plz").Value);
                objekt.Ort = element.Element("obj_ort").Value;

                objektListe.Add(objekt);
            }
        }               

        return objektListe;
    }

    static void JsonImport()
    {
        List<string> filenames = new List<string>();
        foreach (string filename in filenames)
        {
            Console.WriteLine(filename + " erfolgreich importiert!");
        }
    }

}
