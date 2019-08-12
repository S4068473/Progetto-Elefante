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
    public class Auction : IAuction
    {
        private SiteFactoryContext factoryContext;

        public Auction()
        {
        }

        public Auction(int id, User seller, string descr, DateTime endTime, double startPrice, Site site, SiteFactoryContext fc)
        {
            this.Id = id;
            
            this.Description = descr;
            this.EndsOn = endTime;

            this.StartPrice = startPrice;
            this.AuctionCurrentPrice = startPrice;
            this.Cmo = 0;

            CurrSellerUserName = seller.Username;
            AuctionSiteName = site.Name;

            this.factoryContext = fc;
        }

        public override bool Equals(object objAuction)
        {
            if (objAuction == null)
                return false;

            if (objAuction.GetType() == typeof(Auction))
            {
                Auction a = objAuction as Auction;
                return Id == a.Id && AuctionSite == a.AuctionSite;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int Id { get; set; }

        public string Description { get; set; }

        public DateTime EndsOn { get; set; }

        public double StartPrice { get; set; }

        public double AuctionCurrentPrice { get; set; }

        public double Cmo { get; set; }

        public virtual IUser Seller
        {
            get { return CurrSeller as IUser; }
        }

        public string CurrSellerUserName { get; set; }
        public virtual User CurrSeller { get; set; }

        public string AuctionCurrentWinnerUserName { get; set; }
        public virtual User AuctionCurrentWinner { get; set; }

        public string AuctionSiteName { get; set; }
        public virtual Site AuctionSite { get; set; }


        public bool BidOnAuction(ISession theSession, double offer)
        {
            Session session = theSession as Session;

            if (AuctionSite.Now > EndsOn || !this.IsAuctionInDB(this.Id))
            {
                throw new InvalidOperationException();
            }
            if (offer < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (session == null)
            {
                throw new ArgumentNullException();
            }
            //if (!session.IsValid() || session.User == Seller || (session.User as User).UserSite != (Seller as User).UserSite)
            if (!session.IsValid() || session.User.Equals(Seller) || (session.User as User).UserSite.Name != (Seller as User).UserSite.Name)
            {
                throw new ArgumentException();
            }

            bool isValid = true;

            //if (session.User as User == AuctionCurrentWinner as User && offer < (Cmo + AuctionSite.MinimumBidIncrement))
            if ((session.User as User).Equals(AuctionCurrentWinner) && offer < (Cmo + AuctionSite.MinimumBidIncrement))
            {
                isValid = false;
            }
            //else if (session.User as User != AuctionCurrentWinner && offer < AuctionCurrentPrice)
            else if ( !(session.User as User).Equals(AuctionCurrentWinner) && offer < AuctionCurrentPrice)
            {
                isValid = false;
            }
            //else if (session.User as User != AuctionCurrentWinner && offer < (AuctionCurrentPrice + AuctionSite.MinimumBidIncrement) && Cmo != 0)
            else if ( !(session.User as User).Equals(AuctionCurrentWinner) && offer < (AuctionCurrentPrice + AuctionSite.MinimumBidIncrement) && Cmo != 0)
            {
                isValid = false;
            }
            else if (Cmo == 0)
            {
                Cmo = offer;
                AuctionCurrentWinner = session.User as User;
            }
            //else if (session.User as User == AuctionCurrentWinner)
            else if ( (session.User as User).Equals(AuctionCurrentWinner))
            {
                Cmo = offer;
            }
            //else if (Cmo != 0 && session.User as User != AuctionCurrentWinner && offer > Cmo)
            else if (Cmo != 0 && !(session.User as User).Equals(AuctionCurrentWinner) && offer > Cmo)
            {
                AuctionCurrentWinner = session.User as User;
                AuctionCurrentPrice = min(offer, (Cmo + AuctionSite.MinimumBidIncrement));
                Cmo = offer;
            }
            //else if (Cmo != 0 && session.User as User != AuctionCurrentWinner && offer <= Cmo)
            else if (Cmo != 0 && !(session.User as User).Equals(AuctionCurrentWinner) && offer <= Cmo)
            {
                AuctionCurrentPrice = min((offer + AuctionSite.MinimumBidIncrement), Cmo);
            }

            //In ogni caso faccio il refresh della validità della Session dopo una offerta

            session.ValidUntil = session.ValidUntil.AddSeconds(AuctionSite.SessionExpirationInSeconds);

            if (factoryContext == null)
                this.factoryContext = new SiteFactoryContext(AuctionSite.ConnectionString);

            factoryContext.Database.ExecuteSqlCommand("update dbo.Sessions set ValidUntil = '" + session.ValidUntil + "' where Id = '" + session.Id + "'");

            if (isValid)
            {                
                factoryContext.Database.ExecuteSqlCommand("update dbo.Auctions set AuctionCurrentWinnerUsername = '" + AuctionCurrentWinner.Username + "' where Id = " + this.Id);

                string query = "update dbo.Auctions set AuctionCurrentPrice = " + AuctionCurrentPrice.ToString().Replace(',','.') + " where Id = " +
                               this.Id;
                factoryContext.Database.ExecuteSqlCommand(query);
                factoryContext.Database.ExecuteSqlCommand("update dbo.Auctions set Cmo = " +
                                                          Cmo.ToString().Replace(',', '.') + " where Id = " + this.Id);
            }

            factoryContext.Database.Connection.Close();

            return isValid;
        }

        private double min(double p1, double p2)
        {
            return p1 < p2 ? p1 : p2;
        }

        private bool IsAuctionInDB(int auctionId)
        {
            this.factoryContext = new SiteFactoryContext(AuctionSite.ConnectionString);
            var queryResult = factoryContext.Auctions.SqlQuery("select * from dbo.Auctions where Id = " + auctionId).ToList<Auction>();

            factoryContext.Database.Connection.Close();

            //return queryResult.Count<Auction>() > 0;
            return queryResult.Any();
        }

        public double CurrentPrice()
        {
            if (!this.IsAuctionInDB(this.Id))
                throw new InvalidOperationException();

            return AuctionCurrentPrice;
        }

        public IUser CurrentWinner()
        {
            if (!this.IsAuctionInDB(this.Id))
                throw new InvalidOperationException();

            if (Cmo == 0 || AuctionSite.Now > EndsOn)
            {
                return null;
            }
            return AuctionCurrentWinner;
        }

        public void Delete()
        {
            if (!this.IsAuctionInDB(this.Id))
            {
                throw new InvalidOperationException();
            }

            /*Delete on DB*/
            this.factoryContext = new SiteFactoryContext(AuctionSite.ConnectionString);
            factoryContext.Database.ExecuteSqlCommand("delete from dbo.Auctions where Id = " + this.Id);

            factoryContext.Database.Connection.Close();
        }
    }

    public class AuctionModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IAuction>().To<Auction>();
        }
    }
}
