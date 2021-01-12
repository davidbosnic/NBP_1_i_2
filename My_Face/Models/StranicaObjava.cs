using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_Face.Model
{
    public class StranicaObjava
    {
        public Stranica Stranica { get; set; }
        public Objava Objava { get; set; }
        public bool Lajkovano { get; set; }
        public bool MojaObjava { get; set; }
        public bool PodeljenaObjava { get; set; }
    }
}
