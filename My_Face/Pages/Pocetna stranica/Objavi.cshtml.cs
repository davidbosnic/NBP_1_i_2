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
    public class ObjaviModel : PageModel
    {
        [BindProperty]
        public string TekstObjave { get; set; }

        [BindProperty]
        public IFormFile Slika { get; set; }

        [BindProperty] 
        
        public Korisnik Korisnik { get; set; }

        public BoltGraphClient client { get; set; }

        public string getUserString(string param)
        {
            return HttpContext.Session.GetString(param);
        }

        public async Task<IActionResult> OnGet()
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {

                try
                {
                    client = DataLayer.Neo4jManager.GetClient();

                    //Console.WriteLine(objava[0].Tekst);

                    var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + "  RETURN a",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query2).ToList();
                    Korisnik = pom[0];

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

        public async Task<IActionResult> OnPostObjavi()
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {

                try
                {
                    client = DataLayer.Neo4jManager.GetClient();
                    string maxIdPom = getMaxId();
                    var query = new Neo4jClient.Cypher.CypherQuery("CREATE (n:Objava {ID:" + maxIdPom + ", Tekst:" + TekstObjave + ", Slika:" + Slika.FileName + ", Datum: " + DateTime.Now.ToString() + "}) return n",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    ((IRawGraphClient)client).ExecuteGetCypherResults<Objava>(query);

                    //Console.WriteLine(objava[0].Tekst);

                    var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik), (b:Objava) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + " AND b.ID = " + maxIdPom + " CREATE (a)-[r: KORISNIKOBJAVA {MojaObjava: true, PodeljenaObjava: false, Lajkovao: false}]->(b) RETURN r",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    ((IRawGraphClient)client).ExecuteGetCypherResults<KorisnikObjava>(query2);

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

                return RedirectToPage("./VremenskaLinija");
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
