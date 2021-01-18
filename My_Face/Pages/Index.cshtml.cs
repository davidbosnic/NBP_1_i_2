using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using My_Face.Model;
using Neo4j.Driver;
using Neo4j.Driver.V1;
using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_Face.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private BoltGraphClient client;

        [BindProperty]
        public Korisnik novi { get; set; }
        [BindProperty]
        public String name { get; set; }
        [BindProperty]
        public String password { get; set; }
        [BindProperty]
        public String ErrorMessage { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {

        }

        public async Task<IActionResult> OnPostLog()
        {
            var id = getId(name, password);
            if (!string.IsNullOrEmpty(id))
            {
                HttpContext.Session.SetString("idKorisnik", id);
                return RedirectToPage("./Pocetna stranica/VremenskaLinija");
            }
            else
                ErrorMessage = "Pogresno";
            return Page();
        }

        public async Task<IActionResult> OnPostSign()
        {
            var id = getId(novi.Email,novi.Sifra);
            if (string.IsNullOrEmpty(id))
            {
                client = DataLayer.Neo4jManager.GetClient();
                string maxIdPom = getMaxId();
                var query = new Neo4jClient.Cypher.CypherQuery("CREATE (n:Korisnik {ID:" + (Convert.ToInt32(maxIdPom) + 1) + ", Adresa:'" + novi.Adresa + "', Slika:'" + ((novi.Slika == null) ? "" : novi.Slika) + "', DatumRodjenja: '" + novi.DatumRodjenja + "', Ime: '"+novi.Ime+"', Prezime:'"+novi.Prezime+"', Email:'"+novi.Email+"', Sifra:'"+novi.Sifra+"', SlikaPozadine:'"+novi.SlikaPozadine+"'}) return n",
                                                               new Dictionary<string, object>(), CypherResultMode.Set);

                ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query);
                HttpContext.Session.SetString("idKorisnik", (Convert.ToInt32(maxIdPom) + 1).ToString());
                return RedirectToPage("./Pocetna stranica/VremenskaLinija");
            }
            else
            {
                ErrorMessage = "postoji takav nalog";
            }
            return Page();

        }

        private String getId(string nam, string pass)
        {
            client = DataLayer.Neo4jManager.GetClient();
            var query = new Neo4jClient.Cypher.CypherQuery("match (n:Korisnik) where n.Email='" + nam + "' and n.Sifra='" + pass + "' return n.ID",
                                                            new Dictionary<string, object>(), CypherResultMode.Set);

            return ((IRawGraphClient)client).ExecuteGetCypherResults<String>(query).ToList().FirstOrDefault();
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