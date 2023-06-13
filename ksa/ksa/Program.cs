using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using iText.Barcodes;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using ksa.Models;
using Newtonsoft.Json.Linq;

namespace ksa;

internal class Program
{
    private static readonly string directory = Directory.GetCurrentDirectory();

    static void Main(string[] args)
    {
        DataAccess.CreateDatabase();

        //Console.WriteLine("Willkommen bei der ksa.exe.\nBitte verwenden Sie einen der folgenden Befehle:\n-n \n-csvimp \n-xmlimp \n-jsonimp \n-etk");
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
            case "-etk":
                GenerateEtiketten();
                break;
            default:
                Console.WriteLine("Unbekannter Befehl. Bekannte Befehle: -n, -csvimp, -xmlimp, -jsonimp, -etk");
                break;
        }

        Console.WriteLine("Befehl wurde ausgeführt, drücken Sie irgendeine Taste zum Beenden");
        Console.ReadKey();
    }

    static void CsvImport()
    {
        try
        {
            string[] filenames = Directory.GetFiles(directory, "*.csv");

            if (filenames.Length <= 0)
            {
                throw new Exception("Keine Datein mit der Endung '.csv' gefunden. Bitte überprüfen Sie, ob die Datein sich in demselben Ordner wie die EXE befinden.");
            }

            foreach (string filename in filenames)
            {
                try
                {
                    Dictionary<long, List<Objekt>> kundenUndObjekte = new Dictionary<long, List<Objekt>>();

                    using (StreamReader sr = new StreamReader(filename))
                    {
                        int counter = 0;
                        string line;

                        // Read and display lines from the file until the end of
                        // the file is reached.
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (counter != 0)
                            {
                                string[] csv = line.Split(';');


                                long kundenNr = long.Parse(csv[1]);
                                string objektNr = csv[0];
                                if (kundenUndObjekte.TryGetValue(kundenNr, out List<Objekt> kundenObjekte))
                                {
                                    Objekt currentObjekt = kundenObjekte.Find((objekt) => objekt.Nr == objektNr);
                                    if (currentObjekt != null)
                                    {
                                        CSVCreateAndAddObjectAbfallArt(csv, currentObjekt, objektNr);
                                    }
                                    else
                                    {
                                        Objekt newObjekt = CSVCreateObject(csv, objektNr, kundenNr);
                                        kundenObjekte.Add(newObjekt);
                                        CSVCreateAndAddObjectAbfallArt(csv, newObjekt, newObjekt.Nr);
                                    }
                                }

                                else
                                {
                                    Objekt objekt = CSVCreateObject(csv, objektNr, kundenNr);
                                    kundenUndObjekte[kundenNr] = new List<Objekt>() { objekt };
                                    CSVCreateAndAddObjectAbfallArt(csv, objekt, objektNr);
                                }
                            }

                            counter++;
                        }
                    }

                    DataAccess.InsertData(kundenUndObjekte);

                    string filenameWithoutExt = Path.GetFileName(filename);
                    Console.WriteLine(filenameWithoutExt + " erfolgreich importiert!");
                }

                catch
                {
                    throw new Exception($"Beim Einlesen der Datei {filename} ist ein Fehler aufgetreten.");
                }
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void CSVCreateAndAddObjectAbfallArt(string[] csv, Objekt objektToAddTo, string objektNr)
    {
        ObjektAbfallArt currentObjektAbfallArt = new ObjektAbfallArt();
        currentObjektAbfallArt.ObjNr = objektNr;
        currentObjektAbfallArt.Abfallart = csv[6];
        currentObjektAbfallArt.Volumen = int.Parse(csv[7]);
        currentObjektAbfallArt.Anzahl = int.Parse(csv[8]);

        objektToAddTo.ObjektAbfallArt.Add(currentObjektAbfallArt);
    }

    static Objekt CSVCreateObject(string[] csv, string objektNr, long kundenNr)
    {
        Objekt objectToAdd = new Objekt();
        objectToAdd.Nr = objektNr;
        objectToAdd.Kunde_Nr = kundenNr;
        objectToAdd.Straße = csv[2];
        objectToAdd.HausNr = int.Parse(csv[3]);
        objectToAdd.PLZ = int.Parse(csv[4]);
        objectToAdd.Ort = csv[5];

        return objectToAdd;
    }

    static void XmlImport()
    {
        try
        {
            string[] filenames = Directory.GetFiles(directory, "*.xml");

            if (filenames.Length <= 0)
            {
                throw new Exception("Keine Datein mit der Endung '.xml' gefunden. Bitte überprüfen Sie, ob die Datein sich in demselben Ordner wie die EXE befinden.");
            }

            foreach (string filename in filenames)
            {
                try
                {
                    XDocument xmlDoc = XDocument.Load(filename);
                    var einträge = xmlDoc.Element("entries");

                    if (einträge != null)
                    {
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

                        string filenameWithoutExt = Path.GetFileName(filename);
                        Console.WriteLine(filenameWithoutExt + " erfolgreich importiert!");
                    }
                }
                catch
                {
                    throw new Exception($"Beim Einlesen der Datei {filename} ist ein Fehler aufgetreten.");
                }
            }
        }

        catch (Exception e)
        {
            Console.WriteLine(e.Message);
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
        try
        {
            string[] filenames = Directory.GetFiles(directory, "*.json");

            if (filenames.Length <= 0)
            {
                throw new Exception("Keine Datein mit der Endung '.json' gefunden. Bitte überprüfen Sie, ob die Datein sich in demselben Ordner wie die EXE befinden.");
            }

            foreach (string filename in filenames)
            {
                try
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


                        if (kundenUndObjekte.TryGetValue(kundenNr, out List<Objekt> kundenObjekte))
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

                catch
                {
                    throw new Exception($"Beim Einlesen der Datei {filename} ist ein Fehler aufgetreten.");
                }
            }
        }

        catch (Exception e)
        {
            Console.WriteLine(e.Message);
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

    static void GenerateEtiketten()
    {
        try
        {
            List<GarbageSticker> data = DataAccess.GetGarbageSticker();

            if (data.Count != 0)
            {
                bool sameDoc = true;

                for (int i = 0; i < data.Count; i++)
                {
                    int count = 1;

                    string outputPdfFile = data[i].ObjektNr + ".pdf";

                    using (PdfWriter writer = new PdfWriter(outputPdfFile))
                    {
                        using (PdfDocument pdf = new PdfDocument(writer))
                        {
                            Document doc = new Document(pdf);
                            doc.SetMargins(30, 30, 30, 30);

                            while (sameDoc)
                            {
                                Table table = new Table(new float[2]).UseAllAvailableWidth();
                                table.SetMarginTop(0);
                                table.SetMarginBottom(0);

                                table.AddCell(new Paragraph($"Objekt-Nr: {data[i].ObjektNr}"));

                                // Barcode Code39 type
                                Barcode39 code39 = new Barcode39(pdf);
                                code39.SetCode(data[i].Tonnennummer.ToString());
                                code39.SetStartStopText(false);
                                code39.FitWidth(135);
                                code39.SetBarHeight(50);
                                code39.SetSize(13.5f);
                                code39.SetBaseline(12f);
                                Cell cell = new Cell(8, 1).Add(new Image(code39.CreateFormXObject(pdf)));
                                cell.SetPadding(5);
                                table.AddCell(cell);

                                table.AddCell(new Paragraph($"Straße: {data[i].Straße}"));
                                table.AddCell(new Paragraph($"Nr: {data[i].HausNr}"));
                                table.AddCell(new Paragraph($"PLZ: {data[i].PLZ}"));
                                table.AddCell(new Paragraph($"Ort: {data[i].Ort}"));

                                table.AddCell(new Paragraph($"\n"));

                                table.AddCell(new Paragraph($"Tonnen-Nr: {data[i].Tonnennummer}"));
                                table.AddCell(new Paragraph($"Abfallsorte: {data[i].Abfallsorte}"));
                                table.AddCell(new Paragraph($"Volumen: {data[i].Volumen}"));


                                foreach (Cell c in table.GetChildren())
                                {
                                    c.SetBorder(Border.NO_BORDER);
                                }

                                doc.Add(table);


                                if (i < data.Count - 1)
                                {
                                    if (data[i + 1].ObjektNr != data[i].ObjektNr)
                                    {
                                        LastSticker(ref doc, ref sameDoc);
                                    }

                                    else
                                    {
                                        NextSticker(ref doc, ref i, ref count);
                                    }
                                }

                                else
                                {
                                    LastSticker(ref doc, ref sameDoc);
                                }
                            }
                        }
                    }
                    sameDoc = true;
                }

                Console.WriteLine($"Alle PDF-Dateien werden im aktuellen Verzeichnis gespeichert. " +
                $"\nEine PDF-Datei pro Objekt. " +
                $"Der Dateiname ist die entsprechende Objektnummer.");
            }

            else
            {
                throw new Exception("Keine Daten vorhanden zum Erstellen der Etikette. Bitte lesen Sie zuerst welche ein.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void LastSticker(ref Document doc, ref bool sameDoc)
    {
        sameDoc = false;

        doc.Close();
    }

    private static void NextSticker(ref Document doc, ref int i, ref int count)
    {
        i++;
        
        if (count != 3)
        {
            doc.Add(new Paragraph($"\n\n\n"));

            count++;
        }

        else
        {
            doc.Add(new AreaBreak());

            count = 1;
        }
    }
}

