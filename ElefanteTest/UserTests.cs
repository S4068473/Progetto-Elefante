using System;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using TAP2018_19.AuctionSite.Interfaces.Tests;
using TAP2018_19.AlarmClock.Interfaces;

namespace TAP2018_19.AuctionSite.Interfaces.Tests
{
    public class UserTests : InstrumentedAuctionSiteTest
    {
        protected ISite Site;
        protected Mock<IAlarmClock> AlarmClock;
        protected IUser User;
        protected const string UserName = "My Dear Friend";
        protected const string Pw = "f86d 78ds6^^^55";

        /// <summary>
        /// Initializes Site:
        /// <list type="table">
        /// <item>
        /// <term>name</term>
        /// <description>site for user tests</description>
        /// </item>
        /// <item>
        /// <term>time zone</term>
        /// <description>-5</description>
        /// </item>
        /// <item>
        /// <term>expiration time</term>
        /// <description>360 seconds</description>
        /// </item>
        /// <item>
        /// <term>minimum bid increment</term>
        /// <description>7</description>
        /// </item>
        /// <item>
        /// <term>users</term>
        /// <description>username = "My Dear Friend", pw = "f86d 78ds6^^^55"</description>
        /// </item>
        /// <item>
        /// <term>auctions</term>
        /// <description>empty list</description>
        /// </item>
        /// <item>
        /// <term>sessions</term>
        /// <description>empty list</description>
        /// </item>
        /// </list>  
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            Site = CreateAndLoadEmptySite(-5, "site for user tests", 360, 7, out AlarmClock);
            Site.CreateUser(UserName, Pw);
            User = Site.GetUsers().SingleOrDefault(u => u.Username == UserName);
            Assert.That(User, Is.Not.Null, "Set up should be successful");
        }

        /// <summary>
        /// Verify that after deleting a user, its name is not anymore known to the site 
        /// </summary>
        [Test]
        public void Delete_ExistingUser_Deletes()
        {
            User.Delete();
            var survived = Site.GetUsers().Any(u => u.Username == UserName);
            Assert.That(!survived);
        }

        /// <summary>
        /// Verify that a call to Delete on a deleted user throws InvalidOperationException
        /// </summary>
        [Test]
        public void Delete_DeletedUser_Throws()
        {
            User.Delete();
            Assert.That(() => User.Delete(), Throws.TypeOf<InvalidOperationException>());
        }
        /// <summary>
        /// Verify that a newly created use has no won auctions
        /// </summary>
        [Test]
        public void WonAuctions_NewUser_NoAuctions()
        {
            var wonAuctions = User.WonAuctions();
            Assert.That(wonAuctions, Is.Empty);
        }
        /// <summary>
        /// Verify that WonAuctions returns the won auctions of a user who has won some 
        /// </summary>
        /// <param name="howManyAuctions"></param>
        [Test]
        public void WonAuctions_UserWithWonAuctions_NonEmpty([Random(1, 10, 1)] int howManyAuctions)
        {
            var userSession = Site.Login(UserName, Pw);
            const string sellerName = "very lucky seller";
            const string sellerPw = "seller's password";
            Site.CreateUser(sellerName, sellerPw);
            var seller = Site.GetUsers().SingleOrDefault(u => u.Username == sellerName);
            var sellerSession = Site.Login(sellerName, sellerPw);
            var randomGen = new Random();
            var auctions = new List<IAuction>();
            for (int i = 0; i < howManyAuctions; i++)
            {
                var startingPrice = randomGen.NextDouble() * 100 + 1;
                var auction = sellerSession.CreateAuction($"The {i}th auction for {sellerName}",
                    AlarmClock.Object.Now.AddDays(randomGen.Next(3650)), startingPrice);
                auctions.Add(auction);
                auction.BidOnAuction(userSession, startingPrice * 2);
            }

            SetNowToFutureTime(3650 * 24 * 60 * 60 + 1, AlarmClock);
            var wonAuctions = User.WonAuctions();
            Assert.That(auctions, Is.EquivalentTo(wonAuctions));
        }
    }
}