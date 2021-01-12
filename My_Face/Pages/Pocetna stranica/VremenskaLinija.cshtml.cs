using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using My_Face.Model;
using Neo4j.Driver.V1;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace My_Face.Pages.Pocetna_stranica
{
    public class VremenskaLinijaModel : PageModel
    {
        private BoltGraphClient client;
        public Korisnik Korisnik { get; set; }
        public List<Objava> Objave { get; set; }
        public Dictionary<int, List<Komentar>> KomentariZaObjave;
        [BindProperty(SupportsGet = true)]
        public String Komentarcic { get; set; }

        public int BrojPratilaca()
        {
            var queryKorisnik = new Neo4jClient.Cypher.CypherQuery("MATCH (n)<-[r:KORISNIKKORISNIK]-(m) WHERE n.ID=" + Korisnik.ID + " and r.Pratilac=true RETURN count(m) as count", new Dictionary<string, object>(), CypherResultMode.Set);
            int rez = ((IRawGraphClient)client).ExecuteGetCypherResults<int>(queryKorisnik).FirstOrDefault();
            return rez;
        }
        public void OnGet()
        {
            IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());
            try
            {

                client = new BoltGraphClient(driver: driver);
                client.Connect();

                var queryKorisnik = new Neo4jClient.Cypher.CypherQuery("MATCH (s) WHERE s.ID = 1 RETURN s", new Dictionary<string, object>(), CypherResultMode.Set);
                List<Korisnik> korisnici = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(queryKorisnik).ToList();
                Korisnik = korisnici[0];

                var queryZaObjaveKorisnika = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KORISNIKKORISNIK]->(m)-[r1:KORISNIKOBJAVA]->(p) where n.ID = 1 and r.Prijatelj=true and r.Blokiran=false return p", new Dictionary<string, object>(), CypherResultMode.Set);
                List<Objava> Objave1 = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(queryZaObjaveKorisnika).ToList();

                var queryZaObjaveStranice = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KORISNIKSTRANICA]->(m)-[r1:STRANICAOBJAVA]->(p) where n.ID = 1 and r.Prijatelj=true and r.Blokiran=false return p", new Dictionary<string, object>(), CypherResultMode.Set);
                List<Objava> Objave2 = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(queryZaObjaveStranice).ToList();

                Objave = Objave1.Concat(Objave2).ToList();
                KomentariZaObjave = new Dictionary<int, List<Komentar>>();

                foreach (var item in Objave)
                {
                    var queryZaObjave = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KOMENTAR]->(m) where n.ID = " + item.ID + " return r{Korisnik:n,Objava:m}", new Dictionary<string, object>(), CypherResultMode.Set);
                    List<Komentar> pomKom = ((IRawGraphClient)client).ExecuteGetCypherResults<Komentar>(queryZaObjave).ToList();
                    KomentariZaObjave.Add(item.ID, pomKom);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("greska2");
            }
        }
        public async Task<IActionResult> OnPostKomentarisi(int id)
        {
            var query = new Neo4jClient.Cypher.CypherQuery("MATCH(a: Korisnik), (b: Objava) WHERE a.ID = " + Korisnik.ID + " AND b.ID = " + id + " CREATE(a) -[r:KOMENTAR{DatumPostavljanja:" + DateTime.Now.ToString("MM/dd/yyyy") + ", Tekst:'" + Komentarcic + "'}]->(b)", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(query);
            return Page();
        }
        public async Task<IActionResult> OnPostDislajk(int id)
        {
            var queryZaObjaveKorisnika = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KORISNIKOBJAVA]->(m) where n.ID = " + Korisnik.ID + " and r.Lajkovao=true set r.Lajkovao=false ", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(queryZaObjaveKorisnika);
            return Page();
        }
        public async Task<IActionResult> OnPostLajk(int id)
        {
            var queryZaObjaveKorisnika = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KORISNIKOBJAVA]->(m) where n.ID = " + Korisnik.ID + " and r.Lajkovao=false set r.Lajkovao=true return r", new Dictionary<string, object>(), CypherResultMode.Set);
            Objava Objavica = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(queryZaObjaveKorisnika).ToList().FirstOrDefault();

            if (Objavica == null)
            {
                var queryZaObjaveStranice = new Neo4jClient.Cypher.CypherQuery("MATCH(a: Korisnik), (b: Objava) WHERE a.ID = " + Korisnik.ID + " AND b.ID = " + id + " CREATE(a) -[r: KORISNIKOBJAVA{Lajkovao:true, MojaObjava:false, PodeljenaObjava:false}]", new Dictionary<string, object>(), CypherResultMode.Set);
                List<Objava> Objave2 = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(queryZaObjaveStranice).ToList();
            }

            return Page();
        }
    }
}
