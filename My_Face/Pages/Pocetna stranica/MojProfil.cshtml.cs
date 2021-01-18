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
    public class MojProfilModel : PageModel
    {
        [BindProperty]
        public Korisnik zaPrikaz { get; set; }

        [BindProperty]
        public int? idKorisnika { get; set; }

        public BoltGraphClient client { get; set; }

        [BindProperty]
        public List<Objava> objaveZaPrikaz { get; set; }

        [BindProperty]
        public String ErrorMessage { get; set; }

        public Dictionary<int, List<Komentar>> KomentariZaObjave;

        [BindProperty]
        public String Komentar { get; set; }

        public MojProfilModel()
        {
            ErrorMessage = "";
        }



        public async Task<IActionResult> OnGet()
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {

                idKorisnika = idLog;

                try
                {
                    client = DataLayer.Neo4jManager.GetClient();
                    var query = new Neo4jClient.Cypher.CypherQuery("MATCH (n:Korisnik) WHERE n.ID = " + idLog + " return n",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> k = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query).ToList();

                    zaPrikaz = k[0];

                    var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r:KORISNIKOBJAVA]->(b:Objava) WHERE a.ID = " + idLog + " and (r.MojaObjava = true or r.PodeljenaObjava = true) return b",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                    objaveZaPrikaz = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query2).ToList();

                    KomentariZaObjave = new Dictionary<int, List<Komentar>>();

                    foreach (var item in objaveZaPrikaz)
                    {
                        var queryZaObjave = new Neo4jClient.Cypher.CypherQuery("match (n)-[r:KOMENTAR]->(m) where m.ID = " + item.ID + " return r{Korisnik:n,Objava:m,Tekst:r.Tekst,DatumPostavljanja:r.DatumPostavljanja}", new Dictionary<string, object>(), CypherResultMode.Set);
                        List<Komentar> pomKom = ((IRawGraphClient)client).ExecuteGetCypherResults<Komentar>(queryZaObjave).ToList();
                        KomentariZaObjave.Add(item.ID, pomKom);
                    }

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


        public async Task<IActionResult> OnPostKomentarisi(int id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                client = DataLayer.Neo4jManager.GetClient();
                var query = new Neo4jClient.Cypher.CypherQuery("MATCH(a: Korisnik), (b: Objava) WHERE a.ID = " + idLog + " AND b.ID = " + id + " CREATE(a) -[r:KOMENTAR{DatumPostavljanja:" + DateTime.Now.ToString("MM/dd/yyyy") + ", Tekst:'" + Komentar + "'}]->(b)", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query);
                return RedirectToPage();
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }






        public async Task<IActionResult> OnPostLajkuj(int? id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {

                try
                {
                    client = DataLayer.Neo4jManager.GetClient();
                    var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r:KORISNIKOBJAVA]->(b:Objava) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " and b.ID=" + id + " return b",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                    List<Objava> o = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query2).ToList();

                    if (o[0] != null)
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r:KORISNIKOBJAVA]->(b:Objava) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " and b.ID=" + id + " set r.Lajkovao=true set b.Lajkova = " + (o[0].Lajkova + 1),
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                        ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query3);
                    }
                    else
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Objava) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " and b.ID=" + id + " CREATE (a)-[r: KORISNIKOBJAVA {Lajkovao:true,MojaObjava:false,PodeljenaObjava:false}]->(b) RETURN b",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                        ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query3);
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine("greska");
                }
                return RedirectToPage();
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }

        public async Task<IActionResult> OnPostUnlajkuj(int? id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {

                try
                {
                    client = DataLayer.Neo4jManager.GetClient();

                    var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r:KORISNIKOBJAVA]->(b:Objava) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " and b.ID=" + id + " return b",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                    List<Objava> o = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query2).ToList();

                    if (o[0] != null)
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r:KORISNIKOBJAVA]->(b:Objava) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " and b.ID=" + id + " set r.Lajkovao=false set b.Lajkova = "+(o[0].Lajkova - 1),
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                        ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query3);
                    }
                    else
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Objava) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " and b.ID=" + id + " CREATE (a)-[r: KORISNIKOBJAVA {Lajkovao:false,MojaObjava:false,PodeljenaObjava:false}]->(b) RETURN b",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                        ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query3);
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine("greska");
                }
                return RedirectToPage();
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }


        public async Task<IActionResult> OnPostPodeli(int? id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {

                try
                {
                    client = DataLayer.Neo4jManager.GetClient();

                    var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Objava) WHERE a.ID = " + id + " return a",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                    List<Objava> o = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query2).ToList();


                    string maxIdPom = getMaxId();
                    var query = new Neo4jClient.Cypher.CypherQuery("CREATE (n:Objava {ID:" + (Convert.ToInt32(maxIdPom) + 1) + ", Tekst:" + o[0].Tekst + ", Slika:" + o[0].Slika + ", Datum: " + DateTime.Now.ToString() + "}) return n",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    var n = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query).ToList();

                    var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik),(b:Objava),(c:Objava) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " and b.ID=" + maxIdPom + " and c.ID=" + id + " CREATE (a)-[r: KORISNIKOBJAVA{MojaObjava: true, PodeljenaObjava: true, Lajkovao: false}]->(b)-[g: OBJAVAOBJAVA]->(c) return a",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query3);

                    var query4 = new Neo4jClient.Cypher.CypherQuery("MATCH(a: Korisnik) -[r: KORISNIKKORISNIK] -> (b: Korisnik) WHERE b.ID = " + HttpContext.Session.GetString("idKorisnik")+ " AND r.Pratilac=true return a.ID",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                    List<string> listaPratilaca = ((IRawGraphClient)client).ExecuteGetCypherResults<string>(query4).ToList();
                    if (listaPratilaca!=null)
                    {
                        foreach (var item in listaPratilaca)
                        {
                            DataLayer.DataProvider.AddNotifikacija(idLog.ToString(), item, "testID", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                    }

                }
                catch (Exception exc)
                {
                    Console.WriteLine("greska");
                }
                return RedirectToPage();
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }

        private String getMaxId()
        {
            client = DataLayer.Neo4jManager.GetClient();


            var query = new Neo4jClient.Cypher.CypherQuery("match (n) where exists(n.ID) return max(n.ID)",
                                                            new Dictionary<string, object>(), CypherResultMode.Set);

            String maxId = ((IRawGraphClient)client).ExecuteGetCypherResults<String>(query).ToList().FirstOrDefault();

            return maxId;
        }
    }
}
