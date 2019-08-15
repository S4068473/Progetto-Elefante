using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using TAP2018_19.AuctionSite.Interfaces;
using TAP2018_19.AlarmClock.Interfaces;
using System.Data;
using Ninject.Modules;

namespace Elefante
{
    public class SiteFactory : ISiteFactory
    {
        public SiteFactory()
        {
        }

        public void CreateSiteOnDb(string connectionString, string name, int timezone, int sessionExpirationTimeInSeconds, double minimumBidIncrement)
        {
            if (connectionString == null || name == null)
            {
                throw new ArgumentNullException();
            }
            if (name.Length < DomainConstraints.MinSiteName || name.Length > DomainConstraints.MaxSiteName)
            {
                throw new ArgumentException();
            }
            if (timezone < DomainConstraints.MinTimeZone || timezone > DomainConstraints.MaxTimeZone || sessionExpirationTimeInSeconds < 0 || minimumBidIncrement < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (!isValidConnection(connectionString))
            {
                throw new UnavailableDbException();
            }

            SiteFactoryContext ctx = new SiteFactoryContext(connectionString);

            if (this.isSiteAlreadyInDB(name, ctx))
            {
                throw new NameAlreadyInUseException(name);
            }

            Site newSite = new Site(name, minimumBidIncrement, sessionExpirationTimeInSeconds, timezone, connectionString, ctx, null);

            ctx.Sites.Add(newSite);

            ctx.SaveChanges();

            ctx.Database.Connection.Close();
        }

        private bool isSiteAlreadyInDB(string siteName, SiteFactoryContext db)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException();
            }
            var queryResult = db.Sites.SqlQuery("select * from dbo.Sites where Name = '" + siteName + "'").ToList<Site>();

            db.Database.Connection.Close();

            //return queryResult.Count<Site>() > 0;
            return queryResult.Any();
        }

        public IEnumerable<string> GetSiteNames(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException();
            }
            if (!isValidConnection(connectionString))
            {
                throw new UnavailableDbException();
            }

            List<string> allSites = new List<string>();

            SiteFactoryContext ctx = new SiteFactoryContext(connectionString);

            var queryRis = ctx.Sites.SqlQuery("select * from dbo.Sites").ToList<Site>();

            foreach (var item in queryRis)
            {
                allSites.Add(item.Name);
            }

            ctx.Database.Connection.Close();

            return allSites;
        }

        public int GetTheTimezoneOf(string connectionString, string name)
        {
            if (connectionString == null || name == null)
            {
                throw new ArgumentNullException();
            }
            if (name.Length < DomainConstraints.MinSiteName || name.Length > DomainConstraints.MaxSiteName)
            {
                throw new ArgumentException();
            }

            if (!isValidConnection(connectionString))
            {
                throw new UnavailableDbException();
            }

            SiteFactoryContext ctx = new SiteFactoryContext(connectionString);

            var queryRis = ctx.Sites.SqlQuery("select * from dbo.Sites where Name = '" + name + "'").ToList<Site>();

            ctx.Database.Connection.Close();

            //if (queryRis.Count<Site>() == 0)
            if (!queryRis.Any())
            {
                throw new InexistentNameException(name);
            }

            return queryRis.ToArray<Site>()[0].Timezone;
        }

        public ISite LoadSite(string connectionString, string name, IAlarmClock alarmClock)
        {
            if (connectionString == null || name == null || alarmClock == null)
            {
                throw new ArgumentNullException();
            }
            if (!isValidConnection(connectionString))
            {
                throw new UnavailableDbException();
            }
            if (name.Length < DomainConstraints.MinSiteName || name.Length > DomainConstraints.MaxSiteName)
            {
                throw new ArgumentException();
            }

            SiteFactoryContext ctx = new SiteFactoryContext(connectionString);

            var queryRis = ctx.Sites.SqlQuery("select * from dbo.Sites where Name = '" + name + "'").ToList<Site>();

            ctx.Database.Connection.Close();

            //if (queryRis.Count<Site>() == 0)
            if (!queryRis.Any())
            {
                throw new InexistentNameException(name);
            }

            Site theSite = queryRis.ToArray<Site>()[0];
            if (theSite.Timezone != alarmClock.Timezone)
            {
                throw new ArgumentException();
            }

            //return theSite;
            return new Site(theSite.Name, theSite.MinimumBidIncrement, theSite.SessionExpirationInSeconds,
                theSite.Timezone, connectionString, ctx, alarmClock);
        }

        public void Setup(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException();
            }
            if (!isValidConnection(connectionString))
            {
                throw new UnavailableDbException();
            }

            SiteFactoryContext ctx = new SiteFactoryContext(connectionString);

            ctx.Database.Initialize(true);

            /*Dato che non funziona il drop faccio le delete in modo che gli unit test vadano in blocco*/
            /*La cancellazione del DB usando il Database Inizializer DropCreateDatabaseAlways non va a buon fine*/
            /*Il drop causa errore di database in uso (Sql Server Express)*/

            ctx.Database.ExecuteSqlCommand("delete from dbo.Auctions");
            ctx.Database.ExecuteSqlCommand("delete from dbo.Sessions");
            ctx.Database.ExecuteSqlCommand("delete from dbo.Users");
            ctx.Database.ExecuteSqlCommand("delete from dbo.Sites");

            ctx.Database.Connection.Close();
        }

        private bool isValidConnection(string connStr)
        {
            if (connStr == null)
            {
                throw new ArgumentNullException();
            }

            using (var db = new SiteFactoryContext(connStr))
            {
                try
                {
                    db.Database.Connection.Open();

                    if (db.Database.Connection.State == ConnectionState.Open)
                    {
                        Console.WriteLine(@"INFO: ConnectionString: " + db.Database.Connection.ConnectionString
                            + "\n DataBase: " + db.Database.Connection.Database
                            + "\n DataSource: " + db.Database.Connection.DataSource
                            + "\n ServerVersion: " + db.Database.Connection.ServerVersion
                            + "\n TimeOut: " + db.Database.Connection.ConnectionTimeout);

                        db.Database.Connection.Close();

                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                    return false;
                }
            }
        }
    }

    public class SiteFactoryModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISiteFactory>().To<SiteFactory>();
        }
    }
}
