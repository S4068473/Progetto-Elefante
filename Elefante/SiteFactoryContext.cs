using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Elefante
{
    public class SiteFactoryContext : DbContext
    {
        public SiteFactoryContext() : base("name=AuctionSiteConnection")
        {
            //Database.SetInitializer(new DropCreateDatabaseAlways());
        }
        public SiteFactoryContext(string connString) : base(connString)
        {
            //Database.Initialize(true);
            
            //Database.SetInitializer<SiteFactoryContext>(new DropCreateDatabaseAlways<SiteFactoryContext>());
        }

        public DbSet<Site> Sites { get; set; }
        public DbSet<Session> Session { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<User> Users { get; set; }
    }
}