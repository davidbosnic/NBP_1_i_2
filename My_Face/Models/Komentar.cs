using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_Face.Model
{
    public class Komentar
    {
        public Korisnik Korisnik { get; set; }
        public Objava Objava { get; set; }
        public string DatumPostavljanja { get; set; }
        public String Tekst { get; set; }
    }

    public class KomentarStr
    {
        public Stranica Stranica { get; set; }
        public Objava Objava { get; set; }
        public string DatumPostavljanja { get; set; }
        public String Tekst { get; set; }
    }
}
