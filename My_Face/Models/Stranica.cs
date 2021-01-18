using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_Face.Model
{
    public class Stranica
    {
        public int ID { get; set; }
        public String Naziv { get; set; }
        public string DatumKreiranja { get; set; }
        public String Slika { get; set; }
        public String SlikaPozadina { get; set; }
    }
}
