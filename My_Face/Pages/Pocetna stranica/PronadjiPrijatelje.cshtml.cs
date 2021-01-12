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
    public class PronadjiPrijateljeModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public List<Korisnik> ListaKorisnika { get; set; }

        public BoltGraphClient client { get; set; }

        public string ErrorMessage { get; set; }


        public string getUserString(string param)
        {
            return HttpContext.Session.GetString(param);
        }

        public PronadjiPrijateljeModel()
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
                    string maxIdPom = getMaxId();

                    string searchStr = ".*" + HttpContext.Session.GetString("SearchString") + ".*";

                    Dictionary<string, object> queryDict = new Dictionary<string, object>();
                    queryDict.Add("srstr", searchStr);

                    var query = new Neo4jClient.Cypher.CypherQuery("where (n:Korisnik) and exists(n.Ime) and (n.Ime =~ {srstr} or n.Prezime =~ {srstr}) return n",
                                                               queryDict, CypherResultMode.Projection);

                    List<Korisnik> actors = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query).ToList();
                }
                catch (Exception exc)
                {
                    Console.WriteLine("greska");
                }


                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
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
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostDodajPrijatelja(int id)
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

                    var q = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik)-[r: KorisnikKorisnik]-(b:Korisnik) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "'  RETURN a",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(q).ToList();

                    if (pom[0] != null)
                    {
                        var query = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Korisnik) WHERE a.ID = '" + HttpContext.Session.GetString("idKorisnik") + "' AND b.ID = '" + id + "' CREATE (a)-[r: KorisnikKorisnik {Prijatelj:true,Pratilac:true,Blokiran:false}]->(b) RETURN type(r)",
                                                                 new Dictionary<string, object>(), CypherResultMode.Set);

                        ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikKorisnik>(query).ToList();
                        ErrorMessage = "";
                    }
                    else
                    {
                        ErrorMessage = "Vec ste prijatelji sa odabranim korisnikom!";
                    }


                    return RedirectToPage();
                }
                catch (Exception exc)
                {
                    Console.WriteLine("greska");
                }
                return RedirectToPage();
            }
            else
            {
                return RedirectToPage("/Index");
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
            var query = new Neo4jClient.Cypher.CypherQuery("where exists(n.id) return max(n.id)",
                                                            new Dictionary<string, object>(), CypherResultMode.Set);

            String maxId = ((IRawGraphClient)client).ExecuteGetCypherResults<String>(query).ToList().FirstOrDefault();

            return maxId;
        }
    }
}
