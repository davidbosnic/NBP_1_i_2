using System;
using System.Collections.Generic;
using System.Text;

namespace DataLayer.QueryEntities
{
    public class Poruka
    {
        public string senderid { get; set; }
        public string receiverid { get; set; }
        public string senttime { get; set; }
        public string id { get; set; }
        public string message { get; set; }
    }
}
