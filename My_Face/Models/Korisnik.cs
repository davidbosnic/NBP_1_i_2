using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_Face.Model
{
    public class Korisnik
    {
        public int ID { get; set; }
        public String Ime { get; set; }
        public String Prezime { get; set; }
        public String Email { get; set; }
        public String Sifra { get; set; }
        public string DatumRodjenja { get; set; }
        public String Adresa { get; set; }
        public String Slika { get; set; }
        public String SlikaPozadine { get; set; }
    }
}
