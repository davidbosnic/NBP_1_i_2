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
    public class PronadjiStraniceeModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public List<My_Face.Model.Stranica> ListaStranica { get; set; }

        public BoltGraphClient client { get; set; }

        public string ErrorMessage { get; set; }

        [BindProperty]

        public Korisnik Korisnik { get; set; }


        public string getUserString(string param)
        {
            return HttpContext.Session.GetString(param);
        }

        public PronadjiStraniceeModel()
        {
            ErrorMessage = "";
        }

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


                    string maxIdPom = getMaxId();

                    string searchStr = "\".*" + HttpContext.Session.GetString("SearchString") + ".*\"";

                    Dictionary<string, object> queryDict = new Dictionary<string, object>();
                    queryDict.Add("srstr", searchStr);

                    //potencijalno mozda da se ubaci da se ovde ne prikazu blokirani ?
                    var query = new Neo4jClient.Cypher.CypherQuery("match (n) where (n:Stranica) and (n.Naziv =~ " + "\".*" + HttpContext.Session.GetString("SearchString") + ".*\"" + " ) return n",
                                                               queryDict, CypherResultMode.Set);

                    ListaStranica = ((IRawGraphClient)client).ExecuteGetCypherResults<My_Face.Model.Stranica>(query).ToList();
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

        public async Task<IActionResult> OnPostPretrazi()
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                HttpContext.Session.SetString("SearchString", string.IsNullOrEmpty(SearchString) ? "" : SearchString.ToString());
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
                IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());

                try
                {
                    client = new BoltGraphClient(driver: driver);
                    client.Connect();

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikStranica]->(b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "'  RETURN r",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<KorisnikStranica> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikStranica>(q).ToList();

                    if (pom[0] == null)
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "' CREATE (a)-[r: StranicaKorisnik {Admin:false,Lajkovao:true,Pratilac:false}]->(b) RETURN r",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikStranica>(query);
                        ErrorMessage = "";
                    }
                    else
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikStranica]->(b:Stranica) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "'  set r.Lajkovao=true return r",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikStranica>(query);
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

        //ovo i ne mora verovatno
        //public async Task<IActionResult> OnPostPogledajProfil(int id)
        //{
        //    int idLog;
        //    bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
        //    if (log)
        //    {
        //        HttpContext.Session.SetString("SearchString", string.IsNullOrEmpty(SearchString) ? "" : SearchString.ToString());
        //        return RedirectToPage();
        //    }
        //    else
        //    {
        //        return RedirectToPage("/Index");
        //    }
        //}


        private String getMaxId()
        {
            var query = new Neo4jClient.Cypher.CypherQuery("match (n) where exists(n.ID) return max(n.ID)",
                                                            new Dictionary<string, object>(), CypherResultMode.Set);

            String maxId = ((IRawGraphClient)client).ExecuteGetCypherResults<String>(query).ToList().FirstOrDefault();

            return maxId;
        }
    }
}

