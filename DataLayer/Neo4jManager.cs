using Neo4j.Driver.V1;
using Neo4jClient;
using System;
using System.Collections.Generic;
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
            }

            return client;
        }
    }
}
