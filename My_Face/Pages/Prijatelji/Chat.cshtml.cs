using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.QueryEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using My_Face.Model;
using Neo4j.Driver.V1;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace My_Face.Pages.Prijatelji
{
    public class ChatModel : PageModel
    {
        public BoltGraphClient client { get; set; }

        public string ErrorMessage { get; set; }

        [BindProperty]

        public Korisnik Korisnik { get; set; }

        [BindProperty]

        public Korisnik Prijatelj { get; set; }

        [BindProperty]

        public List<Poruka> poruke { get; set; }

        [BindProperty]

        public string tekstPoruke { get; set; }
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

                    int idchat;
                    int.TryParse(HttpContext.Session.GetString("Chatid"), out idchat);

                    var query3 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Korisnik) WHERE a.ID = " + HttpContext.Session.GetString("Chatid") + "  RETURN a",
                                               new Dictionary<string, object>(), CypherResultMode.Set);

                    List<Korisnik> pom2 = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query3).ToList();
                    Prijatelj = pom2[0];




                    poruke = DataLayer.DataProvider.GetPoruka(idLog.ToString(), idchat.ToString());
                    var poruke2 = DataLayer.DataProvider.GetPoruka(idchat.ToString(), idLog.ToString());
                    poruke.AddRange(poruke2);
                    poruke = poruke.OrderBy(o => o.senttime).ToList();


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
        public async Task<IActionResult> OnPostPosalji()
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {

                try
                {
                    int idchat;
                    int.TryParse(HttpContext.Session.GetString("Chatid"), out idchat);


                    DataLayer.DataProvider.AddPoruka(idLog.ToString(), idchat.ToString(), "testID", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), tekstPoruke);

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

    }
}
