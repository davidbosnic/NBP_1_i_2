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

namespace My_Face.Pages.Pocetna_stranica
{
    public class VremenskaLinijaModel : PageModel
    {
        private BoltGraphClient client;
        public Korisnik Korisnik { get; set; }
        public List<Objava> Objave { get; set; }
        public Dictionary<int, List<Komentar>> KomentariZaObjave;
        public List<KorisnikObjava> ObjaveKorisnika { get; set; }
        public List<StranicaObjava> ObjaveStranice { get; set; }
        [BindProperty(SupportsGet = true)]
        public String Komentarcic { get; set; }

        public int BrojPratilaca()
        {
            client = DataLayer.Neo4jManager.GetClient();
            var queryKorisnik = new Neo4jClient.Cypher.CypherQuery("MATCH (n)<-[r:KORISNIKKORISNIK]-(m) WHERE n.ID=" + Korisnik.ID + " and r.Pratilac=true RETURN count(m) as count", new Dictionary<string, object>(), CypherResultMode.Set);
            int rez = ((IRawGraphClient)client).ExecuteGetCypherResults<int>(queryKorisnik).FirstOrDefault();
            return rez;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                try
                {

                    client = DataLayer.Neo4jManager.GetClient();

                    var queryKorisnik = new Neo4jClient.Cypher.CypherQuery("MATCH (s) WHERE s.ID = "+idLog+" RETURN s", new Dictionary<string, object>(), CypherResultMode.Set);
                    List<Korisnik> korisnici = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(queryKorisnik).ToList();
                    Korisnik = korisnici[0];

                    var queryZaObjaveKorisnika = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KORISNIKKORISNIK]->(m)-[r1:KORISNIKOBJAVA]->(p) where n.ID = " + idLog + " and r.Prijatelj=true and r.Blokiran=false return r1{Korisnik:n,Objava:m}", new Dictionary<string, object>(), CypherResultMode.Set);
                    List<KorisnikObjava> Objave1 = ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikObjava>(queryZaObjaveKorisnika).ToList();

                    var queryZaObjaveStranice = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KORISNIKSTRANICA]->(m)-[r1:STRANICAOBJAVA]->(p) where n.ID = " + idLog + " and r.Prijatelj=true and r.Blokiran=false return r1{Korisnik:n,Objava:m}", new Dictionary<string, object>(), CypherResultMode.Set);
                    List<StranicaObjava> Objave2 = ((IRawGraphClient)client).ExecuteGetCypherResults<StranicaObjava>(queryZaObjaveStranice).ToList();

                    ObjaveStranice = Objave2;
                    ObjaveKorisnika = Objave1;
                    KomentariZaObjave = new Dictionary<int, List<Komentar>>();

                    foreach (var item in Objave1)
                    {
                        var queryZaObjave = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KOMENTAR]->(m) where n.ID = " + item.Objava.ID + " return r{Korisnik:n,Objava:m}", new Dictionary<string, object>(), CypherResultMode.Set);
                        List<Komentar> pomKom = ((IRawGraphClient)client).ExecuteGetCypherResults<Komentar>(queryZaObjave).ToList();
                        KomentariZaObjave.Add(item.Objava.ID, pomKom);
                    }
                    foreach (var item in Objave2)
                    {
                        var queryZaObjave = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KOMENTAR]->(m) where n.ID = " + item.Objava.ID + " return r{Korisnik:n,Objava:m}", new Dictionary<string, object>(), CypherResultMode.Set);
                        List<Komentar> pomKom = ((IRawGraphClient)client).ExecuteGetCypherResults<Komentar>(queryZaObjave).ToList();
                        KomentariZaObjave.Add(item.Objava.ID, pomKom);
                    }
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
        public async Task<IActionResult> OnPostKomentarisi(int id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                client = DataLayer.Neo4jManager.GetClient();
                var query = new Neo4jClient.Cypher.CypherQuery("MATCH(a: Korisnik), (b: Objava) WHERE a.ID = " + Korisnik.ID + " AND b.ID = " + id + " CREATE(a) -[r:KOMENTAR{DatumPostavljanja:" + DateTime.Now.ToString("MM/dd/yyyy") + ", Tekst:'" + Komentarcic + "'}]->(b)", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query);
                return Page();
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }
        public async Task<IActionResult> OnPostDislajk(int id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                client = DataLayer.Neo4jManager.GetClient();
                var queryZaObjaveKorisnika = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KORISNIKOBJAVA]->(m) where n.ID = " + Korisnik.ID + " and r.Lajkovao=true set r.Lajkovao=false ", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(queryZaObjaveKorisnika);
                return Page();                
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }
        public async Task<IActionResult> OnPostLajk(int id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                client = DataLayer.Neo4jManager.GetClient();
                var queryZaObjaveKorisnika = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KORISNIKOBJAVA]->(m) where n.ID = " + Korisnik.ID + " and r.Lajkovao=false set r.Lajkovao=true return r", new Dictionary<string, object>(), CypherResultMode.Set);
                Objava Objavica = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(queryZaObjaveKorisnika).ToList().FirstOrDefault();

                if (Objavica == null)
                {
                    var queryZaObjaveStranice = new Neo4jClient.Cypher.CypherQuery("MATCH(a: Korisnik), (b: Objava) WHERE a.ID = " + Korisnik.ID + " AND b.ID = " + id + " CREATE(a) -[r: KORISNIKOBJAVA{Lajkovao:true, MojaObjava:false, PodeljenaObjava:false}]", new Dictionary<string, object>(), CypherResultMode.Set);
                    List<Objava> Objave2 = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(queryZaObjaveStranice).ToList();
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
