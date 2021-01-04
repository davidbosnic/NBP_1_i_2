using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_Face.Model
{
    public class KorisnikKorisnik
    {
        public Korisnik Korisnik1 { get; set; }
        public Korisnik Korisnik2 { get; set; }
        public bool Prijatelj { get; set; }
        public bool Pratilac { get; set; }
        public bool Blokiran { get; set; }
    }
}
