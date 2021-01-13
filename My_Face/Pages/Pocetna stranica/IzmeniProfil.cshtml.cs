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
    public class IzmeniProfilModel : PageModel
    {
        private BoltGraphClient client;
        [BindProperty(SupportsGet = true)]
        public Korisnik Korisnik { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());
                client = new BoltGraphClient(driver: driver);
                client.Connect();

                var queryKorisnik = new Neo4jClient.Cypher.CypherQuery("MATCH (s) WHERE s.ID = "+idLog+" RETURN s", new Dictionary<string, object>(), CypherResultMode.Set);
                List<Korisnik> korisnici = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(queryKorisnik).ToList();
                Korisnik = korisnici[0];
                return Page();
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }
        public async Task<ActionResult> OnPostPromeniAsync()
        {
            int idLog;
            bool log = int.TryParse(HttpContext.Session.GetString("idKorisnik"), out idLog);
            if (log)
            {
                IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());
                client = new BoltGraphClient(driver: driver);
                client.Connect();
                var query = new Neo4jClient.Cypher.CypherQuery("match (n:Korisnik) where n.ID = " + Korisnik.ID + " set n.Ime = '" + Korisnik.Ime + "', n.Prezime = '" + Korisnik.Prezime + "', n.Adresa = '" + Korisnik.Adresa + "', n.Sifra = '" + Korisnik.Sifra + "', n.Slika = '" + Korisnik.Slika + "', n.SlikaPozadine = '" + Korisnik.SlikaPozadine + "', n.DatumRodjenja = '" + Korisnik.DatumRodjenja + "'", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query);
                return RedirectToPage();
            }
            else
            {
                return RedirectToPage("../Index");
            }
        }
    }
}
