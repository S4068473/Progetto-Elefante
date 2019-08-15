using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;
using TAP2018_19.AlarmClock.Interfaces;
using System.ComponentModel.DataAnnotations;
using Ninject.Modules;

namespace Elefante
{
    public class User : IUser
    {
        private SiteFactoryContext factoryContext;

        public User()
        {            
            Username = "user0";
            Pwd = "user0";
        }
        

        public User(Site currSite, string user, string pwd, SiteFactoryContext fc)
        {            
            UserSiteName = currSite.Name;
            Username = user;
            Pwd = pwd;

            this.factoryContext = fc;
        }

        public override bool Equals(object objUser)
        {
            if (objUser == null)
                return false;

            if (objUser.GetType() == typeof(User))
            {
                User user = objUser as User;
                return Username == user.Username && UserSite.Name == user.UserSite.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        

        [Key]
        public string Username { get; set; }

        public string Pwd { get; set; }

        public string UserSiteName { get; set; }
        public virtual Site UserSite { get; set; }

        public void Delete()
        {
            if (!UserSite.IsUserInSite(this.Username))
            {
                throw new InvalidOperationException();
            }

            IAuction currAuction = null;
            IEnumerable<IAuction> allOpenAuctions = UserSite.GetAuctions(true); //Solo aste aperte
            IEnumerator<IAuction> auctionIterator = allOpenAuctions.GetEnumerator();

            while (auctionIterator.MoveNext())
            {
                currAuction = auctionIterator.Current;
                
                if ((currAuction.Seller as User).Equals(this) || (currAuction.CurrentWinner() as User).Equals(this))
                {
                    throw new InvalidOperationException();
                }
            }

            auctionIterator.Dispose();

            /*
            Se arrivo qui vuol dire che l'utente può essere tranquillamente cancellato in quanto 
            non fa parte di aste in corso
            */

            this.factoryContext = new SiteFactoryContext(UserSite.ConnectionString);

            IEnumerable<IAuction> allAuctions = UserSite.GetAuctions(false);
            auctionIterator = allAuctions.GetEnumerator();

            while (auctionIterator.MoveNext())
            {
                currAuction = auctionIterator.Current;
                if ((currAuction.Seller as User).Equals(this))
                {
                    /*
                    Se sono arrivato a poter fare questo if vuol dire che sto valutando 
                    per forza un'asta chiusa 
                    (se fosse aperta sarei caduto nell'exception precedente!)
                    */
                    currAuction.Delete();
                }
                else
                {
                    if ((currAuction.CurrentWinner() as User).Equals(this))
                    {
                        /*
                        Stesso discorso di sopra; 
                        se sono arrivato qui sono sicuro che quest'asta è chiusa altrimenti 
                        sarei caduto nell'exception precedente!
                        */

                        /*Update Auction on DB*/
                        factoryContext.Database.ExecuteSqlCommand("update dbo.Auctions set AuctionCurrentWinner_Username = '' where Id = " + currAuction.Id);

                    }
                }
            }

            auctionIterator.Dispose();

            /*In ogni caso cancello l'utente dal db*/
            factoryContext.Database.ExecuteSqlCommand("delete from dbo.Users where Username = '" + this.Username + "'");

            factoryContext.Database.Connection.Close();
        }

        public IEnumerable<IAuction> WonAuctions()
        {
            if (!UserSite.IsUserInSite(this.Username))
            {
                throw new InvalidOperationException();
            }

            IAuction currAuction = null;
            IEnumerable<IAuction> wonAuctions;
            IEnumerable<IAuction> allAuctions = UserSite.GetAuctions(false);
            IEnumerator<IAuction> auctionIterator = allAuctions.GetEnumerator();

            List<IAuction> l = new List<IAuction>();

            while (auctionIterator.MoveNext())
            {
                currAuction = auctionIterator.Current;
                //if (currAuction.CurrentWinner() as User == this && currAuction.EndsOn >= UserSite.Now)
                if (currAuction != null)
                {
                    if ((currAuction.CurrentWinner() as User).Equals(this) && currAuction.EndsOn >= UserSite.Now)
                    {
                        l.Add(currAuction);
                    }
                }
            }

            auctionIterator.Dispose();

            wonAuctions = l;
            return wonAuctions;
        }
    }

    public class UserModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IUser>().To<User>();
        }
    }
}
