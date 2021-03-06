using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.QueryEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using My_Face.Model;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace My_Face.Pages.Prijatelji
{
    public class ObavestenjaModel : PageModel
    {
        public BoltGraphClient client { get; set; }

        public string ErrorMessage { get; set; }

        [BindProperty]

        public Korisnik Korisnik { get; set; }

        [BindProperty]

        public List<Notifikacija> notifikacije { get; set; }

        [BindProperty]

        public List<String> imena { get; set; }
        public async Task<IActionResult> OnPostLogOutAsync()
        {
            HttpContext.Session.SetString("idKorisnik", "");
            return RedirectToPage("../Index");
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


                    var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik) WHERE a.ID = " + HttpContext.Session.GetString("idKorisnik") + "  RETURN a",
                                                                   new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query2).ToList();
                    Korisnik = pom[0];

                    notifikacije = DataLayer.DataProvider.GetNotifikacija(idLog.ToString());
                    notifikacije = notifikacije.OrderByDescending(o => o.senttime).ToList();
                    imena = new List<string>();
                    foreach (var item in notifikacije)
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik) WHERE a.ID = " + item.publisherid + "  RETURN a",
                                               new Dictionary<string, object>(), CypherResultMode.Set);

                        List<Korisnik> pom2 = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query3).ToList();
                        if (pom2 != null && pom2.Count != 0)
                            imena.Add(pom2[0].Ime + " " + pom2[0].Prezime);
                        else
                            imena.Add("");
                    }
                    int br = 0;
                    foreach (var item in notifikacije)
                    {
                        var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Stranica) WHERE a.ID = " + item.publisherid + "  RETURN a",
                                               new Dictionary<string, object>(), CypherResultMode.Set);

                        List<My_Face.Model.Stranica> pom2 = ((IRawGraphClient)client).ExecuteGetCypherResults<My_Face.Model.Stranica>(query3).ToList();
                        if(pom2!=null && pom2.Count!=0)
                            imena[br]=pom2[0].Naziv;
                        br++;
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
        public async Task<IActionResult> OnPostObrisiAsync(string id)
        {
            DataLayer.DataProvider.DeleteNotifikacija(id,HttpContext.Session.GetString("idKorisnik"));
            return RedirectToPage();
        }
    }
}
