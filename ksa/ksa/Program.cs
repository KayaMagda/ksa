using System;
using System.Collections.Generic;
using System.Linq;

namespace ksa;

internal class Program
{
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
        List<string> filenames = new List<string>();
        foreach (string filename in filenames)
        {
            Console.WriteLine(filename + " erfolgreich importiert!");
        }
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
