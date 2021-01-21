using Neo4j.Driver.V1;
using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataLayer
{
    public static class Neo4jManager
    {

        public static BoltGraphClient client;

        public static BoltGraphClient GetClient()
        {
            if (client == null)
            {
                IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "1234"), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());
                client = new BoltGraphClient(driver: driver);
                client.Connect();

                var query1 = new Neo4jClient.Cypher.CypherQuery("MATCH (n:PocetniID) where n.ID=1 return n.ID",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                List<int> pom = ((IRawGraphClient)client).ExecuteGetCypherResults<int>(query1).ToList();

                if (pom != null && pom.Count == 0)
                {
                    var query = new Neo4jClient.Cypher.CypherQuery("CREATE (n:PocetniID {ID:" + 1 + "}) return n",
                                                                  new Dictionary<string, object>(), CypherResultMode.Set);

                    ((IRawGraphClient)client).ExecuteCypher(query);
                }
            }

            return client;
        }
    }
}
