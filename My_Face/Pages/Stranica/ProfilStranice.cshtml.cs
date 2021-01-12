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

        public ProfilStraniceModel()
        {
            ErrorMessage = "";
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
                    IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());

                    try
                    {
                        client = new BoltGraphClient(driver: driver);
                        client.Connect();
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (n:Stranica) WHERE n.ID = '" + id + "'return n",
                                                                       new Dictionary<string, object>(), CypherResultMode.Set);

                        List<My_Face.Model.Stranica> k = ((IRawGraphClient)client).ExecuteGetCypherResults<My_Face.Model.Stranica>(query).ToList();

                        zaPrikaz = k[0];

                        var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Stranica)-[r:StranicaObjava]->(b:Objava) WHERE a.ID = '" + id + "' and (r.MojaObjava = true or r.PodeljenaObjava = true) return b",
                                                                       new Dictionary<string, object>(), CypherResultMode.Set);
                        List<Objava> o = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query2).ToList();

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
                IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());

                try
                {
                    client = new BoltGraphClient(driver: driver);
                    client.Connect();

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikStranica]->(b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "'  RETURN r",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<KorisnikKorisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(q).ToList();

                    if (pom[0] == null)
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "' CREATE (a)-[r: StranicaKorisnik {Admin:false,Lajkovao:true,Pratilac:false}]->(b) RETURN type(r)",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(query).ToList();
                        ErrorMessage = "";
                    }
                    else
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikStranica]->(b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "'  set r.Lajkovao=true",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(query).ToList();
                        ErrorMessage = "";
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

        public async Task<IActionResult> OnPostUnlajkujStranicu(int? id)
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

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikStranica]->(b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "'  RETURN r",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<KorisnikKorisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(q).ToList();

                    if (pom[0] == null)
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "' CREATE (a)-[r: StranicaKorisnik {Admin:false,Lajkovao:false,Pratilac:false}]->(b) RETURN type(r)",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(query).ToList();
                        ErrorMessage = "";
                    }
                    else
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikStranica]->(b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "'  set r.Lajkovao=false",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(query).ToList();
                        ErrorMessage = "";
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

        public async Task<IActionResult> OnPostZaprati(int? id)
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

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikStranica]->(b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "' AND r.Pratilac = true RETURN a",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(q).ToList();

                    if (pom[0] == null)
                    {
                        var query1 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikStranica]->(b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "'  DELETE r",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(query1).ToList();


                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Korisnik) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "' CREATE (a)-[r: KorisnikKorisnik {Admin:false,Lajkovao:true,Pratilac:true}}]->(b) RETURN type(r)",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(query).ToList();
                        ErrorMessage = "";
                    }
                    else
                    {
                        ErrorMessage = "Vec pratite ovog korisnika";
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

        public async Task<IActionResult> OnPostOdprati(int? id)
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

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikStranica]->(b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "' AND r.Pratilac = false RETURN a",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(q).ToList();

                    if (pom[0] == null)
                    {
                        var query1 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisniStranica]->(b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "'  DELETE r",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(query1).ToList();


                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Korisnik) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "' CREATE (a)-[r: KorisnikKorisnik {Admin:false,Lajkovao:false,Pratilac:false}}]->(b) RETURN type(r)",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(query).ToList();
                        ErrorMessage = "";
                    }
                    else
                    {
                        ErrorMessage = "Ne pratite ovog korisnika da biste ga otpratili";
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
                IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());

                try
                {
                    client = new BoltGraphClient(driver: driver);
                    client.Connect();

                    var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r:KorisnikObjava]->(b:Objava) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' and b.ID='" + id + "' return b",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                    List<Objava> o = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query2).ToList();

                    if (o[0] != null)
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r:KorisnikObjava]->(b:Objava) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' and b.ID='" + id + "' set r.Lajkovao=true",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                        ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query3).ToList();
                    }
                    else
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Objava) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' and b.ID='" + id + "' CREATE (a)-[r: KorisnikObjava {Lajkovao:true,MojaObjava:false,PodeljenaObjava:false}]->(b) RETURN type(r)",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                        ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query3).ToList();
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

        public async Task<IActionResult> OnPostUnlajkuj(int? id)
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

                    var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r:KorisnikObjava]->(b:Objava) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' and b.ID='" + id + "' return b",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                    List<Objava> o = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query2).ToList();

                    if (o[0] != null)
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r:KorisnikObjava]->(b:Objava) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' and b.ID='" + id + "' set r.Lajkovao=false",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                        ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query3).ToList();
                    }
                    else
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Objava) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' and b.ID='" + id + "' CREATE (a)-[r: KorisnikObjava {Lajkovao:false,MojaObjava:false,PodeljenaObjava:false}]->(b) RETURN type(r)",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                        ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query3).ToList();
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


        public async Task<IActionResult> OnPostPodeli(int? id)
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

                    var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Objava) WHERE a.ID = '" + id + "' return a",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);
                    List<Objava> o = ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query2).ToList();


                    string maxIdPom = getMaxId();
                    var query = new Neo4jClient.Cypher.CypherQuery("CREATE (n:Objava {ID:'" + maxIdPom + "', Tekst:'" + o[0].Tekst + "', Slika:'" + o[0].Slika + "', Datum: '" + DateTime.Now.ToString() + "'}) return n",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query).ToList();

                    var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik),(b:Objava),(c:Objava) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' and b.ID='" + maxIdPom + "' and c.ID='" + id + "' CREATE (a)-[r: KorisnikObjava{MojaObjava: true, PodeljenaObjava: true, Lajkovao: false}]->(b)-[g: ObjavaObjava]->(c) return a",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query3).ToList();


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

        private String getMaxId()
        {
            var query = new Neo4jClient.Cypher.CypherQuery("where exists(n.id) return max(n.id)",
                                                            new Dictionary<string, object>(), CypherResultMode.Set);

            String maxId = ((IRawGraphClient)client).ExecuteGetCypherResults<String>(query).ToList().FirstOrDefault();

            return maxId;
        }
    }
}
