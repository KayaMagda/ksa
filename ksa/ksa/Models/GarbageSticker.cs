namespace ksa.Models
{
    public class GarbageSticker
    {
        public string Straße { get; set; }
        public int HausNr { get; set; }
        public int PLZ { get; set; }
        public string Ort { get; set; }
        public long Tonnennummer { get; set; }
        public string Abfallsorte { get; set; }
        public int Volumen { get; set; }
    }
}
