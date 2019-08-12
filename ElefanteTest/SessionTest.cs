using System.Collections.Generic;

namespace TAP2018_19.AuctionSite.Interfaces.Tests
{
    using System;
    using System.Linq;
    using AlarmClock.Interfaces;
    using Moq;
    using NUnit.Framework;

    public class SessionTests : InstrumentedAuctionSiteTest
    {
        protected ISite Site;
        protected Mock<IAlarmClock> AlarmClock;
        protected IUser User;
        protected ISession Session;
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
        /// <description> User (with UserName = "My Dear Friend" and Pw = "f86d 78ds6^^^55") </description>
        /// </item>
        /// <item>
        /// <term>auctions</term>
        /// <description>empty list</description>
        /// </item>
        /// <item>
        /// <term>sessions</term>
        /// <description>Session for User</description>
        /// </item>
        /// </list>  
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            Site = CreateAndLoadSite(-5, "site for user tests", 360, 7, out AlarmClock);
            Site.CreateUser(UserName, Pw);
            User = Site.GetUsers().SingleOrDefault(u => u.Username == UserName);
            Session = Site.Login(UserName, Pw);
            Assert.That(User, Is.Not.Null, "Set up should be successful");
        }
        /// <summary>
        /// Verify that Logout invalidate a valid session
        /// </summary>
        [Test]
        public void Logout_OnValidSession_InvalidatesTheSession()
        {
            //verify setup ok
            if (!Session.IsValid())
                Assert.Fail("Session should be valid");

            Session.Logout();
            Assert.That(!Session.IsValid());
        }
        /// <summary>
        /// Verify that Logout on an already invalid session
        /// throws InvalidOperationException
        /// </summary>
        [Test]
        public void Logout_OnInvalidSession_Throws()
        {
            Session.Logout();
            Assert.That(() => Session.Logout(), Throws.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verify that CreateAuction on an invalid session
        /// throws InvalidOperationException
        /// </summary>
        [Test]
        public void CreateAuction_OnInvalidSession_Throws()
        {
            Session.Logout();
            Assert.That(() => Session.CreateAuction("a", this.AlarmClock.Object.Now, 10),
                Throws.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verify that CreateAuction on a null auction description
        /// throws ArgumentNullException
        /// </summary>
        [Test]
        public void CreateAuction_NullDescription_Throws()
        {
            Assert.That(() => Session.CreateAuction(null, this.AlarmClock.Object.Now, 10),
                Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that CreateAuction on an empty auction description
        /// throws ArgumentException
        /// </summary>
        [Test]
        public void CreateAuction_EmptyDescription_Throws()
        {
            Assert.That(() => Session.CreateAuction("", this.AlarmClock.Object.Now, 10),
                Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that CreateAuction on a negative starting price
        /// throws ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void CreateAuction_NegativeStartingPrice_Throws()
        {
            Assert.That(() => Session.CreateAuction("a", this.AlarmClock.Object.Now, -1),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Verify that CreateAuction on an edning date in the past
        /// throws UnavailableTimeMachineException
        /// </summary>
        [Test]
        public void CreateAuction_InvalidEndsOnDate_Throws()
        {
            Assert.That(() => Session.CreateAuction("a", this.AlarmClock.Object.Now.AddHours(-24), 10),
                Throws.TypeOf<UnavailableTimeMachineException>());
        }

        /// <summary>
        /// Verify that two distinct auctions are created with distinct Ids
        /// </summary>
        [Test]
        public void CreateAuction_NewAuction_ReturnsNewId1()
        {
            var auction1Id = Session.CreateAuction("first auction", this.AlarmClock.Object.Now.AddHours(48), 1024).Id;
            var auction2Id = Session.CreateAuction("a", this.AlarmClock.Object.Now.AddHours(24), 22).Id;
            Assert.That(auction1Id, Is.Not.EqualTo(auction2Id));
        }
        /// <summary>
        /// Verify that a new auction get a new Id
        /// when other 50 auctions already exist
        /// </summary>
        [Test]
        public void CreateAuction_NewAuction_ReturnsNewId2()
        {
            var randomGen = new Random();
            var usedAuctionIds = new List<int>();
            for (int i = 0; i < 50; i++)
            {
                var startingPrice = randomGen.NextDouble() * 100 + 1;
                var auction = Session.CreateAuction($"The {i}th auction for this session",
                    DateTime.Now.AddDays(randomGen.Next(3650)), startingPrice);
                usedAuctionIds.Add(auction.Id);
            }

            var newAuctionId = Session.CreateAuction("a", AlarmClock.Object.Now.AddHours(24), 22).Id;
            CollectionAssert.DoesNotContain(usedAuctionIds, newAuctionId);
        }

        /// <summary>
        /// Verify that CreateAuction increase the validity time of the session
        /// </summary>
        [Test]
        public void CreateAuction_NewAuction_UpdatesExpirationTime()
        {
            var previousValidUntil = Session.ValidUntil;
            AlarmClock.Setup(ac => ac.Now).Returns(previousValidUntil.AddSeconds(-1));
            Session.CreateAuction("a", AlarmClock.Object.Now.AddHours(24), 22);
            var validUntil = Session.ValidUntil;
            Assert.That(validUntil, Is.GreaterThan(previousValidUntil));
        }
        /// <summary>
        /// Verify that CreateAuction on correct parameters creates
        /// a non-null auction
        /// </summary>
        [Test]
        public void CreateAuction_NewAuction_ReturnsNonNullAuction()
        {
            var newAuction = Session.CreateAuction("a", AlarmClock.Object.Now.AddHours(24), 22);
            Assert.That(newAuction, Is.Not.Null);
        }

        /// <summary>
        /// Verify that CreateAuction on correct parameters creates
        /// an auction with the correct Seller
        /// </summary>
        [Test]
        public void CreateAuction_NewAuction_SellerOk()
        {
            var newAuction = Session.CreateAuction("a", AlarmClock.Object.Now.AddHours(24), 22);
            Assert.That(newAuction.Seller, Is.EqualTo(User));
        }

        /// <summary>
        /// Verify that CreateAuction on correct parameters creates
        /// an auction without current winner
        /// </summary>
        [Test]
        public void CreateAuction_NewAuction_NoCurrentWinner()
        {
            var newAuction = Session.CreateAuction("a", AlarmClock.Object.Now.AddHours(24), 22);
            Assert.That(newAuction.CurrentWinner(), Is.Null);
        }

        /// <summary>
        /// Verify that CreateAuction on correct parameters creates
        /// an auction with the correct starting price
        /// </summary>
        [Test]
        public void CreateAuction_NewAuction_StartingPrice()
        {
            const int startingPrice = 22;
            var newAuction = Session.CreateAuction("a", AlarmClock.Object.Now.AddHours(24), startingPrice);
            Assert.That(newAuction.CurrentPrice(), Is.EqualTo(startingPrice));
        }

        /// <summary>
        /// Verify that CreateAuction on correct parameters creates
        /// an auction with the correct ending time (up to seconds)
        /// </summary>
        [Test]
        public void CreateAuction_NewAuction_EndsOn()
        {
            var endsOn = AlarmClock.Object.Now.AddHours(24);
            var newAuction = Session.CreateAuction("a", endsOn, 22);
            Assert.That(SameDateTime(newAuction.EndsOn, endsOn));
        }

        /// <summary>
        /// Verify that CreateAuction on correct parameters creates
        /// an auction with the correct description
        /// </summary>
        [Test]
        public void CreateAuction_NewAuction_Description()
        {
            const string description = "a";
            var newAuction = Session.CreateAuction(description, AlarmClock.Object.Now.AddHours(24), 22);
            Assert.That(newAuction.Description, Is.EqualTo(description));
        }

        /// <summary>
        /// Verify that CreateAuction on correct parameters creates
        /// an auction with the correct description, for long descriptions too
        /// </summary>
        [Test]
        public void CreateAuction_NewAuction_LongDescription()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz 0123456789 . , ; @ $ & ( ) ? ";
            var random = new Random();
            var descriptionLenght = random.Next(100, 1001);
            var description = new string(
                Enumerable.Repeat(chars, descriptionLenght)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
            ;
            var newAuction = Session.CreateAuction(description, AlarmClock.Object.Now.AddHours(24), 22);
            Assert.That(newAuction.Description, Is.EqualTo(description));
        }
        /// <summary>
        /// Verify that IsValid on a valid session returns true
        /// </summary>
        [Test]
        public void IsValid_ValidSession_True()
        {
            Assert.That(Session.IsValid());
        }
        /// <summary>
        /// Verify that IsValid on an expired session returns false
        /// </summary>
        [Test]
        public void IsValid_InvalidSession_False()
        {
            var previousValidUntil = Session.ValidUntil;
            AlarmClock.Setup(ac => ac.Now).Returns(previousValidUntil.AddSeconds(1));
            //Site = siteFactory.LoadSite(connectionString, Site.Name, AlarmClock.Object); //refresh time
            Assert.That(!Session.IsValid());
        }
        /// <summary>
        /// Verify that creating two auctions update the validity of the
        /// session to the same value as a new login at the same time of
        /// the second auction
        /// (that is, CreateAuction reset validity time,
        /// instead of increasing it of a fixed amount)
        /// </summary>
        [Test]
        public void CreateAuction_TwoInvocations_UpdatesExpirationTimeUpToMax()
        {
            // mySite is created
            var sessionExpirationTimeInSeconds = 600;
            var timeZone = 1;
            Mock<IAlarmClock> alarmClockMoq;
            var mySite = CreateAndLoadSite(timeZone, "pippo", sessionExpirationTimeInSeconds, 5, out alarmClockMoq);
            // Seller is created
            mySite.CreateUser("seller", "user0");
            var session = mySite.Login("seller", "user0");
            var initialExpiringTime = session.ValidUntil;
            // Seller creates a new auction with starting price 10
            alarmClockMoq.Setup(a => a.Now).Returns(DateTime.UtcNow.AddHours(timeZone));
            session.CreateAuction("asta", DateTime.Now, 10);
            // Seller creates another auction with starting price 10
            alarmClockMoq.Setup(a => a.Now).Returns(DateTime.UtcNow.AddHours(timeZone));
            session.CreateAuction("asta2", DateTime.Now, 10);
            var finalExpiringTime = session.ValidUntil;
            // session expiration time is between initialExpiringTime and initialExpiringTime+ sessionExpirationTimeInSeconds+10
            // (tolerance of 10 seconds to be on the safe side)
            Assert.Multiple(() => {
                Assert.That(initialExpiringTime, Is.LessThan(finalExpiringTime));
                Assert.That(finalExpiringTime, Is.LessThanOrEqualTo(alarmClockMoq.Object.Now.AddSeconds(sessionExpirationTimeInSeconds + 10)));
            });
        }
    }
}