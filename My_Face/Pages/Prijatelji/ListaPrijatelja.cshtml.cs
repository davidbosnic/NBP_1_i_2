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

namespace My_Face.Pages.Prijatelji
{
    public class ListaPrijateljaModel : PageModel
    {
        private BoltGraphClient client;
        [BindProperty(SupportsGet = true)]
        public Korisnik Korisnik { get; set; }
        [BindProperty]
        public int idKorisnika { get; set; }

        public List<KorisnikKorisnik> Prijatelji;

        public async Task<IActionResult> OnGetAsync()
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

                    var queryKorisnik = new Neo4jClient.Cypher.CypherQuery("MATCH (s) WHERE s.ID = "+idLog+" RETURN s", new Dictionary<string, object>(), CypherResultMode.Set);
                    List<Korisnik> korisnici = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(queryKorisnik).ToList();
                    Korisnik = korisnici[0];
                    idKorisnika = Korisnik.ID;

                    var queryZaObjaveKorisnika1 = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KORISNIKKORISNIK]->(m) where n.ID = " + Korisnik.ID + " and r.Prijatelj=true and r.Blokiran=false return r {Korisnik1:n, Korisnik2:m}", new Dictionary<string, object>(), CypherResultMode.Set);
                    Prijatelji = ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(queryZaObjaveKorisnika1).ToList();
                }
                catch (Exception exc)
                {
                    Console.WriteLine("greska2");
                }
                return Page();
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }

        public async Task<ActionResult> OnPostBlokirajAsync(int id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());
                client = new BoltGraphClient(driver: driver);
                client.Connect();
                var query = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KORISNIKKORISNIK]->(m) where n.ID = " + idKorisnika + " and m.ID = " + id + " and r.Prijatelj=true set r.Blokiran='true' return r", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query);
                return RedirectToPage();
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }
        public async Task<ActionResult> OnPostIzbaciAsync(int id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());
                client = new BoltGraphClient(driver: driver);
                client.Connect();
                var query1 = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KORISNIKKORISNIK]->(m) where n.ID = " + idKorisnika + " and m.ID = " + id + " delete r", new Dictionary<string, object>(), CypherResultMode.Projection);
                ((IRawGraphClient)client).ExecuteCypher(query1);
                var query2 = new Neo4jClient.Cypher.CypherQuery("match (n)<-[r:KORISNIKKORISNIK]-(m) where n.ID = " + idKorisnika + " and m.ID = " + id + " delete r", new Dictionary<string, object>(), CypherResultMode.Projection);
                ((IRawGraphClient)client).ExecuteCypher(query2);
                return RedirectToPage();                
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }
    }
}
