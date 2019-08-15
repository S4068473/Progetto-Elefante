using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;
using TAP2018_19.AlarmClock.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Ninject.Modules;

namespace Elefante
{
    public class Site : ISite
    {
        private IAlarm siteAlarm;
        private IAlarmClock alarmClock;
        private SiteFactoryContext factoryContext;
        
        private Site()
        {
            //Valori di default che utilizzo per eventuali debug su EF
            this.Name = "site0";
            this.MinimumBidIncrement = 1;
            this.SessionExpirationInSeconds = 11;
            this.Timezone = 12;
        }
        

        public Site(string name, double minBidIncr, int sessionExpiration, int timezone, string connStr, SiteFactoryContext fc, IAlarmClock alarmClock)
        {
            this.Name = name;
            this.MinimumBidIncrement = minBidIncr;
            this.SessionExpirationInSeconds = sessionExpiration;
            this.Timezone = timezone;

            this.ConnectionString = connStr;

            this.alarmClock = alarmClock;

            if (this.alarmClock != null)
            {
                siteAlarm = this.alarmClock.InstantiateAlarm(300000);
                siteAlarm.RingingEvent += SiteAlarm_RingingEvent;
            }

            this.factoryContext = fc;
        }

        private void SiteAlarm_RingingEvent()
        {
            this.CleanupSessions();
        }

        [Key]
        public string Name { get; set; }

        public double MinimumBidIncrement { get; set; }

        public int SessionExpirationInSeconds { get; set; }

        public int Timezone { get; set; }

        public string ConnectionString { get; set; }

        public DateTime Now => DateTime.Now;
        /*{
            get
            {
                return DateTime.Now;
            }
        }*/

        public override bool Equals(object objSite)
        {
            if (objSite == null)
                return false;

            if (objSite.GetType() == typeof(Site))
            {
                Site theSite = objSite as Site;
                return this.Name == theSite.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void CleanupSessions()
        {
            if (!this.IsSiteExist(this.Name))
            {
                throw new InvalidOperationException();
            }

            Session currSession = null;
            
            IEnumerator<ISession> en = this.GetSessions().GetEnumerator();
            while (en.MoveNext())
            {
                currSession = en.Current as Session;
                if (currSession != null)
                {
                    //if (currSession.ValidUntil < Now || !currSession.IsSessionActive)
                    if (currSession.ValidUntil < Now)
                        currSession.Delete();
                }
            }

            en.Dispose();
        }

        public void CreateUser(string username, string password)
        {
            if (username == null || password == null)
                throw new ArgumentNullException();
            if (username.Length < DomainConstraints.MinUserName || username.Length > DomainConstraints.MaxUserName || password.Length < DomainConstraints.MinUserPassword)
                throw new ArgumentException();
            if (this.IsUserInSite(username))
                throw new NameAlreadyInUseException(username);

            if (!this.IsSiteExist(this.Name))
            {
                throw new InvalidOperationException();
            }

            User newUser = new User(this, username, password, factoryContext);

            //Attach è necessario perchè quando creo l'utente, il fatto che ci sia la FK al Sito cerca di creare
            //di nuovo il Sito che quindi essendoci già da errore di violazione della PK!!!

            //factoryContext.Configuration.ProxyCreationEnabled = false;

            factoryContext.Users.Add(newUser);
            factoryContext.SaveChanges();

            factoryContext.Database.Connection.Close();
        }

        public void Delete()
        {
            if (!this.IsSiteExist(this.Name))
            {
                throw new InvalidOperationException();
            }

            if (factoryContext == null)
                factoryContext = new SiteFactoryContext(this.ConnectionString);

            factoryContext.Database.ExecuteSqlCommand("delete from dbo.Auctions where AuctionSiteName = '" + this.Name + "'");
            factoryContext.Database.ExecuteSqlCommand("delete from dbo.Sessions where SessionSiteName = '" + this.Name + "'");
            factoryContext.Database.ExecuteSqlCommand("delete from dbo.Users where UserSiteName = '" + this.Name + "'");
            factoryContext.Database.ExecuteSqlCommand("delete from dbo.Sites where Name = '" + this.Name + "'");

            factoryContext.Database.Connection.Close();
        }

        public IEnumerable<IAuction> GetAuctions(bool onlyNotEnded)
        {
            if (!this.IsSiteExist(this.Name))
            {
                throw new InvalidOperationException();
            }

            if (factoryContext == null)
                factoryContext = new SiteFactoryContext(this.ConnectionString);

            var queryRis = factoryContext.Auctions.SqlQuery("select * from dbo.Auctions where AuctionSiteName = '" + Name + "'").ToList<Auction>();

            factoryContext.Database.Connection.Close();

            if (!queryRis.Any())
            {
                return new List<IAuction>();
            }

            List<IAuction> allAuctions = new List<IAuction>();

            foreach (var item in queryRis)
            {
                if (!onlyNotEnded || item.EndsOn < Now)
                    allAuctions.Add(item);
            }


            return allAuctions;
        }        
        
        public ISession GetSession(string sessionId)
        {
            //if (sessionId == null || sessionId == string.Empty)
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentNullException();
            }

            if (!this.IsSiteExist(this.Name))
            {
                throw new InvalidOperationException();
            }

            if (factoryContext == null)
                factoryContext = new SiteFactoryContext(this.ConnectionString);

            var queryRis = factoryContext.Session.SqlQuery(
                "select * from dbo.Sessions where SessionSiteName = '" + Name + "' and Id = '" + sessionId + "'").ToList<Session>();

            factoryContext.Database.Connection.Close();

            if (!queryRis.Any())
            {
                return null;
            }

            Session s = queryRis.ToArray<Session>()[0];
            if (!s.IsValid())
                return null;

            return s;
        }       
        
        public IEnumerable<ISession> GetSessions()
        {
            if (!this.IsSiteExist(this.Name))
            {
                throw new InvalidOperationException();
            }

            if (factoryContext == null)
                factoryContext = new SiteFactoryContext(this.ConnectionString);
            
            var queryRis = factoryContext.Session.SqlQuery("select * from dbo.Sessions where SessionSiteName = '" + Name + "'").ToList<Session>();

            factoryContext.Database.Connection.Close();

            if (!queryRis.Any())
            {                
                return new List<ISession>();
            }

            List<ISession> allSessions = new List<ISession>();

            foreach (var item in queryRis)
            {
                allSessions.Add(item);
            }

            return allSessions;
        }       

        public bool IsSiteExist(string userName)
        {
            if (factoryContext == null)
                factoryContext = new SiteFactoryContext(this.ConnectionString);

            var queryResult = factoryContext.Sites.SqlQuery("select * from dbo.Sites where Name = '" + this.Name + "'").ToList<Site>();

            factoryContext.Database.Connection.Close();

            //return queryResult.Count<Site>() > 0;
            return queryResult.Any();
        }

        
        public IEnumerable<IUser> GetUsers()
        {
            if (!this.IsSiteExist(this.Name))
            {
                throw new InvalidOperationException();
            }

            if (factoryContext == null)
                factoryContext = new SiteFactoryContext(this.ConnectionString);
            
            var queryRis = factoryContext.Users.SqlQuery("select * from dbo.Users where UserSiteName = '" + Name + "'").ToList<User>();

            factoryContext.Database.Connection.Close();

            if (!queryRis.Any())
            {
                return new List<IUser>();
            }

            List<IUser> allUser = new List<IUser>();

            foreach (var item in queryRis)
            {
                allUser.Add(item);
            }

            return allUser;
        }
        

        public bool IsUserInSite(string username)
        {
            if (factoryContext == null)
                factoryContext = new SiteFactoryContext(this.ConnectionString);

            var queryRis =
                factoryContext.Users.SqlQuery("select * from dbo.Users where UserSiteName = '" + Name +
                                                       "' and UserName='" + username + "'").ToList<User>();

            factoryContext.Database.Connection.Close();

            return queryRis.Any();
        }

        private User GetUserByUsername(string username)
        {
            if (factoryContext == null)
                factoryContext = new SiteFactoryContext(this.ConnectionString);

            var queryRis = factoryContext.Users.SqlQuery("select * from dbo.Users where UserSiteName = '" + Name + "'").ToList<User>();

            factoryContext.Database.Connection.Close();

            if (!queryRis.Any())
            {
                return null;
            }

            foreach (var item in queryRis)
            {
                if (item.Username == username)
                {
                    return item;
                }
            }

            return null;
        }

        private void SaveLogin(ISession session)
        {
            if (factoryContext == null)
                this.factoryContext = new SiteFactoryContext(this.ConnectionString);
            factoryContext.Database.ExecuteSqlCommand("update dbo.Sessions set ValidUntil = '" + session.ValidUntil + "' where Id = '" + session.Id + "'");
            factoryContext.Database.ExecuteSqlCommand("update dbo.Sessions set IsSessionActive = 'true' where Id = '" + session.Id + "'");

            factoryContext.Database.Connection.Close();
        }

        public ISession Login(string username, string password)
        {
            if (username == null || password == null)
                throw new ArgumentNullException();
            if (username.Length < DomainConstraints.MinUserName || username.Length > DomainConstraints.MaxUserName || password.Length < DomainConstraints.MinUserPassword)
                throw new ArgumentException();

            if (!this.IsSiteExist(this.Name))
            {
                throw new InvalidOperationException();
            }

            if(!IsUserInSite(username))            
            {
                return null;
            }

            User theUser = this.GetUserByUsername(username);

            if (theUser.Pwd != password)
                return null;
            

            theUser = new User(this, username, password, factoryContext);

            Session currSession = null;            
            IEnumerator<ISession> en = this.GetSessions().GetEnumerator();
            while (en.MoveNext())
            {                
                currSession = en.Current as Session;
                
                if (currSession.User.Username == username && (currSession.User as User).Pwd == password &&
                    !currSession.IsSessionExpired())
                {
                    currSession.IsSessionActive = true;
                    currSession.ValidUntil = currSession.ValidUntil.AddSeconds(this.SessionExpirationInSeconds);
                    SaveLogin(currSession);
                    return currSession;
                }
            }

            en.Dispose();

            if (factoryContext == null)
                factoryContext = new SiteFactoryContext(this.ConnectionString);

            int sessionId = factoryContext.Session.Count<Session>();
            Session newSession = new Session(sessionId.ToString(), theUser, Now.AddSeconds(this.SessionExpirationInSeconds), true, this, factoryContext);

            //factoryContext.Sites.Attach(this);
            
            //factoryContext.Entry(theUser).State = EntityState.Added;
            //factoryContext.Users.Attach(theUser);

            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //factoryContext.Configuration.ProxyCreationEnabled = false;
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            factoryContext.Session.Add(newSession);
            factoryContext.SaveChanges();

            factoryContext.Database.Connection.Close();

            return newSession;
        }
        
    }

    public class SiteModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISite>().To<Site>();
        }
    }
}