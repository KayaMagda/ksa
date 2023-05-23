using System.Collections.Generic;

namespace ksa.Models
{
    public class Objekt
    {
        public string Nr { get; set; }
        public long Kunde_Nr { get; set; }
        public string Straße { get; set; }
        public int HausNr { get; set; }
        public int PLZ { get; set; }
        public string Ort { get; set; }
        public List<ObjektAbfallArt> ObjektAbfallArt { get; set; } = new List<ObjektAbfallArt>();
    }
}
