using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
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
    public class User
    {
        public String login { get; set; }
        public String name { get; set; }
        public String password { get; set; }


    }
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private BoltGraphClient client;

        [BindProperty]
        public String message { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
           
        }

        public async Task<IActionResult> OnPostPosaljiPoruku()
        {
            DataLayer.DataProvider.AddPoruka("1", "2", "123123", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLog()
        {
            HttpContext.Session.SetString("idKorisnik", "6");
            return RedirectToPage("./Pocetna stranica/VremenskaLinija");
        }
    }
}
