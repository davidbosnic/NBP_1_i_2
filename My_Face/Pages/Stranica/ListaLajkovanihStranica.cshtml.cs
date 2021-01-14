using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using My_Face.Model;
using Neo4j.Driver.V1;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace My_Face.Pages.Stranica
{
    public class ListaLajkovanihStranicaModel : PageModel
    {
        public List<My_Face.Model.Stranica> zaPrikaz { get; set; }

        [BindProperty]
        public String ErrorMessage { get; set; }

        public BoltGraphClient client { get; set; }

        [BindProperty]

        public Korisnik Korisnik { get; set; }



        public async Task<IActionResult> OnGet()
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());

                try
                {
                    client = new BoltGraphClient(driver: driver);
                    client.Connect();

                    var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + "  RETURN a",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query2).ToList();
                    Korisnik = pom[0];

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikStranica]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " and r.Lajkovao=true  RETURN b",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    zaPrikaz = ((IRawGraphClient)client).ExecuteGetCypherResults<My_Face.Model.Stranica>(q).ToList();

                }
                catch (Exception exc)
                {
                    Console.WriteLine("greska");
                }

                return Page();
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }
    }
}
