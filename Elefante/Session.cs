using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;
using TAP2018_19.AlarmClock.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using Ninject.Modules;

namespace Elefante
{
    public class Session : ISession
    {
        private SiteFactoryContext factoryContext;

        public Session()
        { }

        public Session(string id, User owner, DateTime validUntil, bool isThisSessionActive, Site site, SiteFactoryContext fc)
        {
            this.Id = id;

            this.SessionOwnerUsername = owner.Username;

            this.ValidUntil = validUntil;

            this.IsSessionActive = isThisSessionActive;

            this.SessionSiteName = site.Name;

            //this.SessionSite = site;

            this.factoryContext = fc;

            this.IsSessionActive = true;
        }

        public string Id { get; set; }

        public string SessionOwnerUsername { get; set; }

        public virtual User SessionOwner { get; set; }

        //Necessario fare cosi altrimenti la sqlquery me lo dava null!!!!
        public virtual IUser User
        {
            get { return (SessionOwner as IUser); }
        }

        public string SessionSiteName { get; set; }
        public virtual Site SessionSite { get; set; }

        public DateTime ValidUntil { get; set; }
        public bool IsSessionActive { get; set; }

        public override bool Equals(object objSession)
        {
            if (objSession == null)
                return false;

            if (objSession.GetType() == typeof(Session))
            {
                Session theSession = objSession as Session;
                return Id == theSession.Id && SessionSiteName == theSession.SessionSiteName;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public IAuction CreateAuction(string description, DateTime endsOn, double startingPrice)
        {
            if (!this.IsValid())
            {
                throw new InvalidOperationException();
            }
            if (description == null)
            {
                throw new ArgumentNullException();
            }
            if (description == string.Empty)
            {
                throw new ArgumentException();
            }
            if (startingPrice < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (endsOn < SessionSite.Now)
            {
                throw new UnavailableTimeMachineException();
            }

            int auctionId = factoryContext.Auctions.Count<Auction>();

            Auction newAuction = new Auction(auctionId, User as User, description, endsOn, startingPrice, (User as User).UserSite, factoryContext);

            factoryContext.Auctions.Add(newAuction);

            factoryContext.SaveChanges();

            factoryContext.Database.Connection.Close();

            ValidUntil = ValidUntil.AddSeconds(SessionSite.SessionExpirationInSeconds);

            SaveValidUntil();

            return newAuction;
        }

        private void SaveValidUntil()
        {
            if (factoryContext == null)
                this.factoryContext = new SiteFactoryContext(SessionSite.ConnectionString);
            factoryContext.Database.ExecuteSqlCommand("update dbo.Sessions set ValidUntil = '" + ValidUntil + "' where Id = '" + this.Id + "'");

            factoryContext.Database.Connection.Close();
        }

        private bool IsSessionExist()
        {
            if (factoryContext == null)
                factoryContext = new SiteFactoryContext(SessionSite.ConnectionString);

            var queryResult = factoryContext.Session.SqlQuery("select * from dbo.Sessions where Id = '" + this.Id + "'").ToList<Session>();

            factoryContext.Database.Connection.Close();

            //return queryResult.Count<Session>() > 0;
            return queryResult.Any();
        }

        public bool IsSessionExpired()
        {
            return SessionSite.Now > ValidUntil;
        }

        public bool IsValid()
        {
            if (SessionSite.Now > ValidUntil || !IsSessionActive)
                return false;

            if (!this.IsSessionExist())
                return false;

            return true;
        }

        public void Logout()
        {
            if (!this.IsValid())
            {
                throw new InvalidOperationException();
            }

            IsSessionActive = false;

            this.factoryContext = new SiteFactoryContext(SessionSite.ConnectionString);
            factoryContext.Database.ExecuteSqlCommand("update dbo.Sessions set IsSessionActive = 'false' where Id = '" + this.Id + "'");

            factoryContext.Database.Connection.Close();
        }

        public void Delete()
        {
            if (!this.IsSessionExist())
            {
                throw new InvalidOperationException();
            }

            this.factoryContext = new SiteFactoryContext(SessionSite.ConnectionString);
            factoryContext.Database.ExecuteSqlCommand("delete from dbo.Sessions where Id = '" + this.Id + "'");
            factoryContext.Database.ExecuteSqlCommand("delete from dbo.Auctions where CurrSellerUserName = '" +
                                                    this.SessionOwnerUsername + "'");

            factoryContext.Database.Connection.Close();
        }
    }

    public class SessionModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISession>().To<Session>();
        }
    }
}
