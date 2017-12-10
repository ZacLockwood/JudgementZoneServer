using System;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Security.Authentication;
using SignalR_Server.Models;

namespace SignalR_Server.Connectors
{
    public class GameStateDB_Connector : IDisposable
    {
        #region Global values

        private bool disposed = false;

        // To do: update the connection string with the DNS name
        // or IP address of your server.
        private string userName = ConnectionConstants.GAMESTATEDB_USERNAME;
        private string host = ConnectionConstants.GAMESTATEDB_HOST;
        private string password = ConnectionConstants.GAMESTATEDB_PASSWORD;

        // The database and collection will be automatically created
        // if they don't already exist.
        private string dbName = ConnectionConstants.GAMESTATEDB_DBNAME;
        private string collectionName = ConnectionConstants.GAMESTATEDB_COLLECTIONNAME;

        #endregion

        #region Constructors

        public GameStateDB_Connector()
        {

        }

        #endregion

        #region Mongo Connection Methods

        public IMongoCollection<M_GameState> GetCollection()
        {
            IMongoDatabase database = GetDatabase();

            var collection = database.GetCollection<M_GameState>(collectionName);
            return collection;
        }

        private IMongoDatabase GetDatabase()
        {
            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(host, 10255);
            settings.UseSsl = true;
            settings.SslSettings = new SslSettings();
            settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;

            MongoIdentity identity = new MongoInternalIdentity(dbName, userName);
            MongoIdentityEvidence evidence = new PasswordEvidence(password);

            settings.Credentials = new List<MongoCredential>()
            {
                new MongoCredential("SCRAM-SHA-1", identity, evidence)
            };

            MongoClient client = new MongoClient(settings);

            return client.GetDatabase(dbName);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                }
            }

            this.disposed = true;
        }

        # endregion
    }
}