using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_Face.Model
{
    public class KorisnikObjava
    {
        public Korisnik Korisnik { get; set; }
        public Objava Objava { get; set; }
        public bool Lajkovao { get; set; }
        public bool MojaObjava { get; set; }
        public bool PodeljenaObjava { get; set; }
    }
}
