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
    public class ProfilStraniceModel : PageModel
    {
        [BindProperty]
        public My_Face.Model.Stranica zaPrikaz { get; set; }

        [BindProperty]
        public int? idStranice { get; set; }

        public BoltGraphClient client { get; set; }

        [BindProperty]
        public List<Objava> objaveZaPrikaz { get; set; }

        [BindProperty]
        public String ErrorMessage { get; set; }

        [BindProperty]
        public Korisnik Korisnik { get; set; }

        [BindProperty]
        public string Komentar { get; set; }

        public Dictionary<int, List<Komentar>> KomentariZaObjave;

        public ProfilStraniceModel()
        {
            ErrorMessage = "";
        }

        public async Task<IActionResult> OnPostLogOutAsync()
        {
            HttpContext.Session.SetString("idKorisnik", "");
            return RedirectToPage("../Index");
        }

        public async Task<IActionResult> OnGet(int? id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                if (id != null)
                {
                    idStranice = id;

                    try
                    {
                        client = DataLayer.Neo4jManager.GetClient();

                        var query33 = new Neo4jClient.Cypher.CypherQuery("MATCH (n:Korisnik) WHERE n.ID = " + idLog + " return n",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                        List<Korisnik> k1 = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query33).ToList();

                        Korisnik = k1[0];



                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (n:Stranica) WHERE n.ID = " + id + " return n",
                                                                       new Dictionary<string, object>(), CypherResultMode.Set);

                        List<My_Face.Model.Stranica> k = ((IRawGraphClient)client).ExecuteGetCypherResults<My_Face.Model.Stranica>(query).ToList();

                        zaPrikaz = k[0];

                        var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Stranica)-[r:STRANICAOBJAVA]->(b:Objava) WHERE a.ID = " + id + " and (r.MojaObjava = true or r.PodeljenaObjava = true) return b",
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
                    return RedirectToPage("../Pocetna stranica/VremenskaLinija");
                }

            }
            else
            {
                return RedirectToPage("../Index");
            }

        }

        public async Task<IActionResult> OnPostLajkujStranicu(int? id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {

                try
                {
                    client = DataLayer.Neo4jManager.GetClient();

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + "  RETURN r",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<KorisnikStranica> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikStranica>(q).ToList();

                    if (pom!= null && pom.Count==0)
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + " CREATE (a)-[r: KORISNIKSTRANICA {Admin:false,Lajkovao:true,Pratilac:false}]->(b) RETURN r",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikStranica>(query);
                        ErrorMessage = "";
                    }
                    else
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + "  set r.Lajkovao=true return r",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikStranica>(query);
                        ErrorMessage = "";
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


        public async Task<IActionResult> OnPostUnlajkujStranicu(int? id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                
                try
                {
                    client = DataLayer.Neo4jManager.GetClient();

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + "  RETURN r",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<KorisnikStranica> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikStranica>(q).ToList();

                    if (pom != null && pom.Count==0)
                    {
                        ErrorMessage = "";
                    }
                    else
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + "  set r.Lajkovao=false return r",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikStranica>(query);
                        ErrorMessage = "";
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

        public async Task<IActionResult> OnPostZaprati(int? id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
               

                try
                {
                    client = DataLayer.Neo4jManager.GetClient();

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + "  RETURN a",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(q).ToList();

                    if (pom!= null && pom.Count==0)
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = '" + id + " CREATE (a)-[r: KORISNIKSTRANICA {Admin:false,Lajkovao:true,Pratilac:true}}]->(b) RETURN r",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikStranica>(query);
                        ErrorMessage = "";
                    }
                    else
                    {

                        var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + "' set r.Pratilac=true,r.Lajkovao=true RETURN a",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query2);

                        ErrorMessage = "";
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

        public async Task<IActionResult> OnPostOdprati(int? id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {

                try
                {
                    client = DataLayer.Neo4jManager.GetClient();

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + " RETURN a",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(q).ToList();

                    if (pom!= null && pom.Count!=0)
                    {
                        var query1 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + " set r.Pratilac=false return r",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikStranica>(query1);

                        ErrorMessage = "";
                    }
                    else
                    {
                        ErrorMessage = "";
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

        public async Task<IActionResult> OnPostPostaniAdmin(int? id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {


                try
                {
                    client = DataLayer.Neo4jManager.GetClient();

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + "  RETURN a",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(q).ToList();

                    if (pom != null && pom.Count == 0)
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + " CREATE (a)-[r: KORISNIKSTRANICA {Admin:true,Lajkovao:true,Pratilac:true}]->(b) RETURN r",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikStranica>(query);
                        ErrorMessage = "";
                    }
                    else
                    {

                        var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + " set r.Admin=true set r.Lajkovao=true set r.Pratilac=true RETURN a",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query2);

                        ErrorMessage = "";
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

        public async Task<IActionResult> OnPostUkloniAdmina(int? id)
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {


                try
                {
                    client = DataLayer.Neo4jManager.GetClient();

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + "  RETURN a",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(q).ToList();

                    if (pom != null && pom.Count == 0)
                    {
                        
                        ErrorMessage = "";
                    }
                    else
                    {

                        var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KORISNIKSTRANICA]->(b:Stranica) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + id + " set r.Admin=false RETURN a",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query2);

                        ErrorMessage = "";
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

        //public async Task<IActionResult> OnPostBlokiraj(int? id)
        //{
        //    int idLog;
        //    bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
        //    if (log)
        //    {
        //        IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());

        //        try
        //        {
        //            client = new BoltGraphClient(driver: driver);
        //            client.Connect();

        //            var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikKorisnik]->(b:Korisnik) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "' AND r.Blokiran = false RETURN a",
        //                                                          new Dictionary<string, object>(), CypherResultMode.Set);

        //            List<Korisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(q).ToList();

        //            if (pom[0] != null)
        //            {
        //                var query1 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikKorisnik]->(b:Korisnik) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "'  set r.Blokiran=true",
        //                                                         new Dictionary<string, object>(), CypherResultMode.Set);

        //                ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(query1).ToList();

        //            }
        //            else
        //            {
        //                var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Korisnik) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "' CREATE (a)-[r: KorisnikKorisnik {Prijatelj:false,Pratilac:false,Blokiran:true}]->(b) RETURN type(r)",
        //                                                         new Dictionary<string, object>(), CypherResultMode.Set);

        //                ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(query).ToList();
        //                ErrorMessage = "";
        //            }


        //            return RedirectToPage();
        //        }
        //        catch (Exception exc)
        //        {
        //            Console.WriteLine("greska");
        //        }
        //        return RedirectToPage();
        //    }
        //    else
        //    {
        //        return RedirectToPage("/Index");
        //    }
        //}

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

                    if (o != null && o.Count != 0)
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r:KORISNIKOBJAVA]->(b:Objava) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " and b.ID=" + id + " set r.Lajkovao=true set b.Lajkova = " + (o[0].Lajkova + 1),
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                        ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query3);
                    }
                    else
                    {
                        var query4 = new Neo4jClient.Cypher.CypherQuery("MATCH (b:Objava) WHERE b.ID=" + id + " set b.Lajkova = b.Lajkova + 1 return b",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                        List<Objava> o2 = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query4).ToList();
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

                    if (o != null && o.Count != 0)
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r:KORISNIKOBJAVA]->(b:Objava) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " and b.ID=" + id + " set r.Lajkovao=false set b.Lajkova = " + (o[0].Lajkova - 1),
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
                    var query = new Neo4jClient.Cypher.CypherQuery("CREATE (n:Objava {ID:" + (Convert.ToInt32(maxIdPom) + 1) + ", Tekst:'" + o[0].Tekst + "', Slika:'" + o[0].Slika + "', Datum: '" + DateTime.Now.ToString() + "'}) return n",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query);

                    var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik),(b:Objava),(c:Objava) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " and b.ID=" + (Convert.ToInt32(maxIdPom) + 1) + " and c.ID=" + id + " CREATE (a)-[r: KORISNIKOBJAVA{MojaObjava: true, PodeljenaObjava: true, Lajkovao: false}]->(b)-[g: OBJAVAOBJAVA]->(c) return a",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query3);

                    var query4 = new Neo4jClient.Cypher.CypherQuery("MATCH(a: Korisnik) -[r: KORISNIKKORISNIK] -> (b: Korisnik) WHERE b.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND r.Pratilac=true return a.ID",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    List<string> listaPratilaca = ((IRawGraphClient)client).ExecuteGetCypherResults<string>(query4).ToList();
                    if (listaPratilaca != null)
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
