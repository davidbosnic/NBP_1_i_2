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

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());

            try
            {

            client = new BoltGraphClient(driver: driver);
            
                client.Connect();
            var query = new Neo4jClient.Cypher.CypherQuery("MATCH (tom {name: \"Tom Hanks\"}) RETURN tom",
                                                           new Dictionary<string, object>(), CypherResultMode.Set);

            List<User> users = ((IRawGraphClient)client).ExecuteGetCypherResults<User>(query).ToList();
            Console.WriteLine(users[0].name);

            }
            catch (Exception exc)
            {
                Console.WriteLine("greska");
            }
        }
    }
}
