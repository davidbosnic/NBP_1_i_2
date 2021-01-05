using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_Face.Model
{
    public class KorisnikStranica
    {
        public Korisnik Korisnik { get; set; }
        public Stranica Stranica { get; set; }
        public bool Admin { get; set; }
        public bool Lajkovao { get; set; }
        public bool Pratilac { get; set; }
    }
}
