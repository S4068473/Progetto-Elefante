using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;

namespace TAP2018_19.AuctionSite.Interfaces.Tests
{
    using System;
    using System.Linq;
    using AlarmClock.Interfaces;
    using Moq;
    using NUnit.Framework;
    using TestBaseClasses;

    [TestFixture]
    public class SiteFactoryBadConnectionTest : InstrumentedAuctionSiteTest
    {
        /// <summary>
        /// Verify that Setup on a connection string with wrong Data Source
        /// throws UnavailableDbException
        /// </summary>
        [Test]
        public void Setup_BadConnectionString_Throws()
        {
            Assert.That(
                () => GetSiteFactory().Setup(@"Data Source=pippo;Initial Catalog=pluto;Integrated Security=True;"),
                Throws.TypeOf<UnavailableDbException>());
        }


        /// <summary>
        /// Verify that GetSiteNames on a connection string with wrong Data Source
        /// throws UnavailableDbException
        /// </summary>
        [Test]
        public void GetSiteNames_BadConnectionString_Throws()
        {
            Assert.That(
                () => GetSiteFactory().GetSiteNames(@"Data Source=puffo;Initial Catalog=blu;Integrated Security=True;"),
                Throws.TypeOf<UnavailableDbException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on a connection string with wrong Data Source
        /// throws UnavailableDbException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_BadConnectionString_Throws()
        {
            Assert.That(
                () => GetSiteFactory()
                    .CreateSiteOnDb("Data Source=pippo;Initial Catalog=pluto;Integrated Security=True;", "pippo", 4,
                        600, 2), Throws.TypeOf<UnavailableDbException>());
        }

        /// <summary>
        /// Verify that GetTheTimezoneOf on a connection string with wrong Data Source
        /// throws UnavailableDbException
        /// </summary>
        [Test]
        public void GetTheTimezoneOf_BadConnectionString_Throws()
        {
            Assert.That(
                () => GetSiteFactory()
                    .GetTheTimezoneOf("Data Source=pippo;Initial Catalog=pluto;Integrated Security=True;", "pippo"),
                Throws.TypeOf<UnavailableDbException>());
        }

        /// <summary>
        /// Verify that LoadSite on a connection string with wrong Data Source
        /// throws UnavailableDbException
        /// </summary>
        [Test]
        public void LoadSite_BadConnectionString_Throws()
        {
            var alarm = new Mock<IAlarmClock>();
            Assert.That(
                () => GetSiteFactory().LoadSite("Data Source=pippo;Initial Catalog=pluto;Integrated Security=True;",
                    "pippo", AlarmClockMock(2, out alarm)), Throws.TypeOf<UnavailableDbException>());
        }
    }

    [TestFixture]
    public class SiteFactoryNullConnectionStringTest : InstrumentedAuctionSiteTest
    {
        /// <summary>
        /// Verify that Setup on a null connection string
        /// throws ArgumentNullException
        /// </summary>
        [Test]
        public void Setup_NullArg_Throws()
        {
            Assert.That(() => GetSiteFactory().Setup(null), Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that GetSiteNames on a null connection string
        /// throws ArgumentNullException
        /// </summary>
        [Test]
        public void GetSiteNames_NullArg_Throws()
        {
            Assert.That(() => GetSiteFactory().GetSiteNames(null), Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on a null connection string
        /// throws ArgumentNullException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_NullConnectionString_Throws()
        {
            Assert.That(() => GetSiteFactory().CreateSiteOnDb(null, "pippo", 4, 600, 2),
                Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that GetTheTimezoneOf on a null connection string
        /// throws ArgumentNullException
        /// </summary>
        [Test]
        public void GetTheTimezoneOf_NullConnectionString_Throws()
        {
            Assert.That(() => GetSiteFactory().GetTheTimezoneOf(null, "troppo giusto"),
                Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that LoadSite on a null connection string
        /// throws ArgumentNullException
        /// </summary>
        [Test]
        public void LoadSite_NullConnectionString_Throws()
        {
            Mock<IAlarmClock> alarm;
            Assert.That(() => GetSiteFactory().LoadSite(null, "pippo", AlarmClockMock(4, out alarm)),
                Throws.TypeOf<ArgumentNullException>());
        }
    }

    [TestFixture]
    public class SiteFactoryBasicTest : InstrumentedAuctionSiteTest
    {
        /// <summary>
        /// Verify that CreateSiteOnDb on a null site name
        /// throws ArgumentNullException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_NullName_Throws()
        {
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, null, 2, 300, 4),
                Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that GetTheTimezoneOf on a null site name
        /// throws ArgumentNullException
        /// </summary>
        [Test]
        public void GetTheTimezoneOf_NullName_Throws()
        {
            Assert.That(() => siteFactory.GetTheTimezoneOf(connectionString, null),
                Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that LoadSite on a null site name
        /// throws ArgumentNullException
        /// </summary>
        [Test]
        public void LoadSite_NullName_Throws()
        {
            var alarm = new Mock<IAlarmClock>();
            Assert.That(() => siteFactory.LoadSite(connectionString, null, AlarmClockMock(4, out alarm)),
                Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that Setup on a null connection string
        /// throws ArgumentNullException
        /// </summary>
        [Test]
        public void LoadSite_NullAlarmClock_Throws()
        {
            siteFactory.CreateSiteOnDb(connectionString, "pippo", 2, 600, 10);
            Assert.That(() => siteFactory.LoadSite(connectionString, "pippo", null),
                Throws.TypeOf<ArgumentNullException>());
        }
        /// <summary>
        /// Verify that Setup on correct arguments
        /// returns a system with no auction sites in it
        /// </summary>
        [Test]
        public void Setup_ValidArg_ReturnsEmptyDB()
        {
            var siteNamesList = siteFactory.GetSiteNames(connectionString);
            Assert.That(siteNamesList.Count(), Is.EqualTo(0));
        }

        /// <summary>
        /// Verify that GetSiteNames on correct arguments
        /// returns a not null (possibly empty) list
        /// </summary>
        [Test]
        public void GetSiteNames_ValidArg_ReturnsNonnull()
        {
            var siteNamesList = siteFactory.GetSiteNames(connectionString);
            Assert.That(siteNamesList, Is.Not.Null);
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on correct arguments
        /// actually adds the new site (and nothing else)
        /// </summary>
        [Test]
        public void GetSiteNames_JustCreatedSite_ReturnsTheNewSite()
        {
            const string siteName = "_boh_";
            siteFactory.CreateSiteOnDb(connectionString, siteName, -3, 6000, 25);
            var siteNamesList = siteFactory.GetSiteNames(connectionString).ToList();
            Assert.That(siteNamesList, Is.EquivalentTo(new List<string> { siteName }));
        }

        /// <summary>
        /// Verify that GetSiteNames on correct arguments
        /// returns an empty list when called before creating any site
        /// </summary>
        [Test]
        public void GetSiteNames_OnEmptyDB_ReturnsEmpty()
        {
            var siteNamesList = siteFactory.GetSiteNames(connectionString);
            Assert.That(siteNamesList, Is.Empty);
        }

        private void GetSiteNames_DbWithNSites_ReturnsThoseSiteNames(List<string> expectedSiteNames)
        {
            foreach (var siteName in expectedSiteNames)
            {
                siteFactory.CreateSiteOnDb(connectionString, siteName, 1, 60, 1);
            }

            var siteNamesList = siteFactory.GetSiteNames(connectionString).ToList();
            Assert.That(siteNamesList, Is.EquivalentTo(expectedSiteNames));
        }

        /// <summary>
        /// Verify that GetSiteNames on correct arguments
        /// returns a list with the three names of the existing three sites
        /// </summary>
        [Test]
        public void GetSiteNames_DbWith3Sites_Returns3Names()
        {
            var expectedSiteNames = new List<string> { "A", "B", "C" };
            GetSiteNames_DbWithNSites_ReturnsThoseSiteNames(expectedSiteNames);
        }

        /// <summary>
        /// Verify that GetSiteNames on correct arguments
        /// returns a list with the names of the existing sites
        /// in the case of a randomly number n of sites with name s0, s1..sn
        /// </summary>
        [Test]
        public void GetSiteNames_DbWithRandomNumberOfSites_ReturnsThatManyNames([Random(0, 20, 1)] int howMany)
        {
            var expectedSiteNames = new List<string>();
            for (int i = 0; i < howMany; i++)
            {
                expectedSiteNames.Add($"s{i}");
            }

            GetSiteNames_DbWithNSites_ReturnsThoseSiteNames(expectedSiteNames);
        }
        /// <summary>
        /// Verify that CreateSiteOnDb on a otherwise correct paramaters
        /// but on a site name already in use by another site
        /// throws NameAlreadyInUseException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_NameTaken_Throws()
        {
            const string taken = "taken!!";
            siteFactory.CreateSiteOnDb(connectionString, taken, 4, 1200, 0.2);
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, taken, 2, 120, 0.25),
                Throws.TypeOf<NameAlreadyInUseException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on a otherwise correct paramaters
        /// but on a site name already in use by another site
        /// throws NameAlreadyInUseException
        /// even if all the parameter coincide with the values of the existing site
        /// </summary>
        [Test]
        public void CreateSiteOnDb_SameDataOfExistingSite_Throws()
        {
            const string taken = "taken!!";
            siteFactory.CreateSiteOnDb(connectionString, taken, 4, 1200, 0.2);//first creation must be ok
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, taken, 4, 1200, 0.2),
                Throws.TypeOf<NameAlreadyInUseException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on an empty site name
        /// throws ArgumentException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_NameEmpty_Throws()
        {
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, "", 0, 600, 0.01),
                Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on a timezone smaller than DomainConstraints.MinTimeZone
        /// throws ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_TimezoneTooSmall_Throws()
        {
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, "troppo giusto", -13, 600, 0.01),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on a timezone much larger than DomainConstraints.MaxTimeZone
        /// throws ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_FarTooLargeTimezone_Throws()
        {
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, "troppo giusto", 1024, 600, 0.01),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on a timezone slightly larger than DomainConstraints.MaxTimeZone
        /// throws ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_TooLargeTimezone_Throws()
        {
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, "troppo giusto", 13, 600, 0.01),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on a negative session expiration time
        /// throws ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_NegativeSessionExpirationTime_Throws()
        {
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, "troppo giusto", 1, -10, 0.01),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on a negative minimum increment
        /// throws ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_NegativeMinimumIncrement_Throws()
        {
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, "troppo giusto", 2, 60, -0.01),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }


        /// <summary>
        /// Verify that GetTheTimezoneOf on a inexistent site 
        /// throws InexistentNameException
        /// </summary>
        [Test]
        public void GetTheTimezoneOf_UnexistingSite_connectionString_Throws()
        {
            Assert.That(() => siteFactory.GetTheTimezoneOf(connectionString, "pippo"),
                Throws.TypeOf<InexistentNameException>());
        }

        /// <summary>
        /// Verify that GetTheTimezoneOf on an empty string as site name
        /// throws ArgumentException
        /// </summary>
        [Test]
        public void GetTheTimezoneOf_EmptySiteName_connectionString_Throws()
        {
            Assert.That(() => siteFactory.GetTheTimezoneOf(connectionString, ""), Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that GetTheTimezoneOf on correct arguments returns the
        /// timezone of the site
        /// </summary>
        [Test]
        public void GetTheTimezoneOf_ValidArg_Returns5()
        {
            const string siteName = "questo va";
            const int expectedTtimezone = 5;
            siteFactory.CreateSiteOnDb(connectionString, siteName, expectedTtimezone, 673, 2.8);
            var timezone = siteFactory.GetTheTimezoneOf(connectionString, siteName);
            Assert.That(timezone, Is.EqualTo(expectedTtimezone));
        }

        /// <summary>
        /// Verify that LoadSite
        /// throws ArgumentException
        /// if the time zone provided as argument does not match the actual time zone of the site
        /// </summary>
        [Test]
        public void LoadSite_InconsistentTimezone_Throws()
        {
            const string siteName = "pippo";

            siteFactory.CreateSiteOnDb(connectionString, siteName, 4, 30, 7.5);

            Mock<IAlarmClock> alarmClockMoq;
            AlarmClockMock(0, out alarmClockMoq);
            Assert.That(() => siteFactory.LoadSite(connectionString, siteName, alarmClockMoq.Object),
                Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that LoadSite
        /// throws InexistentNameException
        /// if the name provided as argument is not the name of an existing site name
        /// </summary>
        [Test]
        public void LoadSite_UnexistingSite_Throws()
        {
            Mock<IAlarmClock> alarmClockMoq;
            AlarmClockMock(7, out alarmClockMoq);

            Assert.That(() => siteFactory.LoadSite(connectionString, "pippo", alarmClockMoq.Object),
                Throws.TypeOf<InexistentNameException>());
        }

        /// <summary>
        /// Verify that LoadSite on correct arguments
        /// actually creates a site by the given name
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsNewSite1()
        {
            const int timeZone = 3;
            const string siteName = "pippo";
            const int sessionExpirationTimeInSeconds = 360;
            const double minimumBidIncrement = .5;
            var newSite = CreateAndLoadSite(timeZone, siteName, sessionExpirationTimeInSeconds, minimumBidIncrement);

            Assert.That(newSite.Name, Is.EqualTo(siteName));
        }

        /// <summary>
        /// Verify that LoadSite on correct arguments
        /// actually creates a site by the given name and with correct time zone
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsNewSite2()
        {
            const int timeZone = 3;
            const string siteName = "pippo";
            const int sessionExpirationTimeInSeconds = 360;
            const double minimumBidIncrement = .5;
            var newSite = CreateAndLoadSite(timeZone, siteName, sessionExpirationTimeInSeconds, minimumBidIncrement);

            Assert.That(newSite.Timezone, Is.EqualTo(timeZone));
        }

        /// <summary>
        /// Verify that LoadSite on correct arguments
        /// actually creates a site by the given name and with correct session expiration time
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsNewSite3()
        {
            const int timeZone = 3;
            const string siteName = "pippo";
            const int sessionExpirationTimeInSeconds = 360;
            const double minimumBidIncrement = .5;
            var newSite = CreateAndLoadSite(timeZone, siteName, sessionExpirationTimeInSeconds, minimumBidIncrement);

            Assert.That(newSite.SessionExpirationInSeconds, Is.EqualTo(sessionExpirationTimeInSeconds));
        }

        /// <summary>
        /// Verify that LoadSite on correct arguments
        /// actually creates a site by the given name and with correct minimum increment
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsNewSite4()
        {
            const int timeZone = 3;
            const string siteName = "pippo";
            const int sessionExpirationTimeInSeconds = 360;
            const double minimumBidIncrement = .5;
            var newSite = CreateAndLoadSite(timeZone, siteName, sessionExpirationTimeInSeconds, minimumBidIncrement);

            Assert.That(newSite.MinimumBidIncrement, Is.EqualTo(minimumBidIncrement));
        }

        /// <summary>
        /// Verify that LoadSite on correct arguments
        /// actually creates a site by the given name and without users
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsNewSite5()
        {
            const int timeZone = 3;
            const string siteName = "pippo";
            const int sessionExpirationTimeInSeconds = 360;
            const double minimumBidIncrement = .5;
            var newSite = CreateAndLoadSite(timeZone, siteName, sessionExpirationTimeInSeconds, minimumBidIncrement);

            Assert.That(!newSite.GetUsers().Any());
        }

        /// <summary>
        /// Verify that LoadSite on correct arguments
        /// actually creates a site by the given name and without auctions
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsNewSite6()
        {
            const int timeZone = 3;
            const string siteName = "pippo";
            const int sessionExpirationTimeInSeconds = 360;
            const double minimumBidIncrement = .5;
            var newSite = CreateAndLoadSite(timeZone, siteName, sessionExpirationTimeInSeconds, minimumBidIncrement);

            Assert.That(!newSite.GetAuctions(false).Any());
        }

        /// <summary>
        /// Verify that LoadSite on correct arguments
        /// actually creates a site by the given name and without sessions
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsNewSite7()
        {
            const int timeZone = 3;
            const string siteName = "pippo";
            const int sessionExpirationTimeInSeconds = 360;
            const double minimumBidIncrement = .5;
            var newSite = CreateAndLoadSite(timeZone, siteName, sessionExpirationTimeInSeconds, minimumBidIncrement);

            Assert.That(!newSite.GetSessions().Any());
        }
    }

    public class SiteFactoryTestsLoadSiteWithUsers : InstrumentedAuctionSiteTest
    {
        const int timeZone = 3;
        const string siteName = "pippo";
        const int sessionExpirationTimeInSeconds = 360;
        const double minimumBidIncrement = .5;
        List<string> userList;
        private ISite newSite;

        /// <summary>
        /// Initializes newSite:
        /// <list type="table">
        /// <item>
        /// <term>name</term>
        /// <description>pippo</description>
        /// </item>
        /// <item>
        /// <term>time zone</term>
        /// <description>3</description>
        /// </item>
        /// <item>
        /// <term>expiration time</term>
        /// <description>360 seconds</description>
        /// </item>
        /// <item>
        /// <term>minimum bid increment</term>
        /// <description>.5</description>
        /// </item>
        /// <item>
        /// <term>users</term>
        /// <description>"Alice", "Barbara", "Carlotta", "Dalila"</description>
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
        public void SetupSite()
        {
            userList = new List<string> { "Alice", "Barbara", "Carlotta", "Dalila" };
            newSite = CreateAndLoadSite(timeZone, siteName, sessionExpirationTimeInSeconds, minimumBidIncrement,
                userList);
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite name
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithUsers1()
        {
            Assert.That(newSite.Name, Is.EqualTo(siteName));
        }


        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite time zone
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithUsers2()
        {
            Assert.That(newSite.Timezone, Is.EqualTo(timeZone));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite expiration time
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithUsers3()
        {
            Assert.That(newSite.SessionExpirationInSeconds, Is.EqualTo(sessionExpirationTimeInSeconds));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite minimum bid increment
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithUsers4()
        {
            Assert.That(newSite.MinimumBidIncrement, Is.EqualTo(minimumBidIncrement));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite users
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithUsers5()
        {
            Assert.That(newSite.GetUsers().Select(u => u.Username), Is.EquivalentTo(userList));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite auctions
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithUsers6()
        {
            Assert.That(newSite.GetAuctions(false), Is.EquivalentTo(new List<IAuction>()));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite sessions
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithUsers7()
        {
            Assert.That(newSite.GetSessions(), Is.EquivalentTo(new List<ISession>()));
        }
    }

    public class SiteFactoryTestsLoadSiteWithSessions : InstrumentedAuctionSiteTest
    {
        const int timeZone = 3;
        const string siteName = "pippo";
        const int sessionExpirationTimeInSeconds = 7200;
        const double minimumBidIncrement = .5;
        const string alice = "Alice";
        const string barbara = "Barbara";
        List<string> userList;
        List<string> loggedUserList;
        List<ISession> expectedSessionList;
        Mock<IAlarmClock> alarmClockMoq;
        ISite newSite;
        List<ISession> sessionList;
        ISession aliceSession;
        ISession barbaraSession;

        /// <summary>
        /// Initializes newSite:
        /// <list type="table">
        /// <item>
        /// <term>name</term>
        /// <description>pippo</description>
        /// </item>
        /// <item>
        /// <term>time zone</term>
        /// <description>3</description>
        /// </item>
        /// <item>
        /// <term>expiration time</term>
        /// <description>7200 seconds</description>
        /// </item>
        /// <item>
        /// <term>minimum bid increment</term>
        /// <description>.5</description>
        /// </item>
        /// <item>
        /// <term>users</term>
        /// <description>"Alice", "Barbara", "Carlotta", "Dalila"</description>
        /// </item>
        /// <item>
        /// <term>auctions</term>
        /// <description>empty list</description>
        /// </item>
        /// <item>
        /// <term>sessions</term>
        /// <description>aliceSession for Alice and Barbarasession for Barbara</description>
        /// </item>
        /// </list>  
        /// </summary>
        [SetUp]
        public void SetUpSiteWithSessions()
        {
            userList = new List<string> { alice, barbara, "Carlotta", "Dalila" };
            loggedUserList = new List<string> { barbara, alice };
            newSite = CreateAndLoadSite(timeZone, siteName, sessionExpirationTimeInSeconds, minimumBidIncrement,
                userList,
                loggedUserList, 1800, out expectedSessionList, out alarmClockMoq);
            sessionList = newSite.GetSessions().ToList();
            aliceSession = sessionList.SingleOrDefault(s => s.User.Username == alice);
            barbaraSession = sessionList.SingleOrDefault(s => s.User.Username == barbara);
        }


        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite name
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithSessions0()
        {
            Assert.That(newSite.Name, Is.EqualTo(siteName));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite time zone
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithSessions2()
        {
            Assert.That(newSite.Timezone, Is.EqualTo(timeZone));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite expiration time
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithSessions3()
        {
            Assert.That(newSite.SessionExpirationInSeconds, Is.EqualTo(sessionExpirationTimeInSeconds));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite minimum bid increment
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithSessions4()
        {
            Assert.That(newSite.MinimumBidIncrement, Is.EqualTo(minimumBidIncrement));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite users
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithSessions5()
        {
            CollectionAssert.AreEquivalent(newSite.GetUsers().Select(u => u.Username).ToList(), userList);
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite auctions
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithSessions6()
        {
            CollectionAssert.IsEmpty(newSite.GetAuctions(false));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite sessions
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsSiteWithSessions7()
        {
            CollectionAssert.AreEquivalent(sessionList, expectedSessionList);
            Assert.That(aliceSession.ValidUntil, Is.GreaterThan(alarmClockMoq.Object.Now.AddHours(1)));
            Assert.That(barbaraSession.ValidUntil, Is.GreaterThan(alarmClockMoq.Object.Now.AddMinutes(30)));
        }
    }

    public class SiteFactoryTestsCreateEmptySite : InstrumentedAuctionSiteTest
    {
        const string SiteName = "troppo giusto";
        const double MinimumBidIncrement = 0.01;
        const int ExpectedTimeZone = 2;
        const int TimeZone = 2;
        const int SessionExpirationTimeInSeconds = 60;
        private ISite CreatedSite { get; set; }

        private Mock<IAlarmClock> _alarmClockMoq;

        /// <summary>
        /// Initializes CreatedSite:
        /// <list type="table">
        /// <item>
        /// <term>name</term>
        /// <description>troppo giusto</description>
        /// </item>
        /// <item>
        /// <term>time zone</term>
        /// <description>2</description>
        /// </item>
        /// <item>
        /// <term>expiration time</term>
        /// <description>60 seconds</description>
        /// </item>
        /// <item>
        /// <term>minimum bid increment</term>
        /// <description>.01</description>
        /// </item>
        /// <item>
        /// <term>users</term>
        /// <description>empty list</description>
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
        public void CreateAndLoadSite()
        {
            siteFactory.CreateSiteOnDb(connectionString, SiteName, TimeZone, SessionExpirationTimeInSeconds,
                MinimumBidIncrement);
            AlarmClockMock(siteFactory.GetTheTimezoneOf(connectionString, SiteName), out _alarmClockMoq);
            CreatedSite = siteFactory.LoadSite(connectionString, SiteName, _alarmClockMoq.Object);
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. CreatedSite name
        /// </summary>
        [Test]
        public void CreateSiteOnDb_ValidArg_CreatesSite1()
        {
            Assert.That(CreatedSite.Name, Is.EqualTo(SiteName));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. CreatedSite time zone
        /// </summary>
        [Test]
        public void CreateSiteOnDb_ValidArg_CreatesSite2()
        {
            Assert.That(TimeZone, Is.EqualTo(ExpectedTimeZone));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. CreatedSite users
        /// </summary>
        [Test]
        public void CreateSiteOnDb_ValidArg_CreatesSite3()
        {
            Assert.That(CreatedSite.GetUsers(), Is.Empty);
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. CreatedSite expiration time
        /// </summary>
        [Test]
        public void CreateSiteOnDb_ValidArg_CreatesSite4()
        {
            Assert.That(CreatedSite.SessionExpirationInSeconds, Is.EqualTo(SessionExpirationTimeInSeconds));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. CreatedSite minimum bid increment
        /// </summary>
        [Test]
        public void CreateSiteOnDb_ValidArg_CreatesSite5()
        {
            Assert.That(CreatedSite.MinimumBidIncrement, Is.EqualTo(MinimumBidIncrement));
        }
        /// <summary>
        /// Verify that the setup is correct w.r.t. CreatedSite sessions
        /// </summary>
        [Test]
        public void CreateSiteOnDb_ValidArg_CreatesSite6()
        {
            Assert.That(CreatedSite.GetSessions(), Is.Empty);
        }
        /// <summary>
        /// Verify that the setup is correct w.r.t. CreatedSite auctions
        /// </summary>
        [Test]
        public void CreateSiteOnDb_ValidArg_CreatesSite7()
        {
            Assert.That(CreatedSite.GetAuctions(false), Is.Empty);
        }

    }

    public class SiteFactoryTestsLoadSiteFullSite : InstrumentedAuctionSiteTest
    {
        const int timeZone = 3;
        const string siteName = "pippo";
        const int sessionExpirationTimeInSeconds = 7200;
        const double minimumBidIncrement = .5;
        const string alice = "Alice";
        const string barbara = "Barbara";
        const string carlotta = "Carlotta";
        const string dalila = "Dalila";
        List<string> userList = new List<string> { dalila, carlotta, barbara, alice };
        Mock<IAlarmClock> alarmClockMoq;
        const string password = "puffo_blu55";
        List<IAuction> expectedAuctionList;
        private List<ISession> expectedSessionList;
        private IAuction aliceAuction;
        DateTime aliceAuctionEndsOn;
        const string aliceAuctionDescription = "fa schifo: non lo comprate";
        const double aliceStartingPrice = .25;
        private IAuction barbaraAuction;
        const string barbaraAuctionDescription = "non lo venderemo mai";
        DateTime barbaraAuctionEndsOn;
        const double barbaraStartingPrice = 3.75;
        ISite newSite;
        /// <summary>
        /// Initializes newSite:
        /// <list type="table">
        /// <item>
        /// <term>name</term>
        /// <description>pippo</description>
        /// </item>
        /// <item>
        /// <term>time zone</term>
        /// <description>3</description>
        /// </item>
        /// <item>
        /// <term>expiration time</term>
        /// <description>7200 seconds</description>
        /// </item>
        /// <item>
        /// <term>minimum bid increment</term>
        /// <description>.5</description>
        /// </item>
        /// <item>
        /// <term>users</term>
        /// <description>"Alice", "Barbara", "Carlotta", "Dalila"</description>
        /// </item>
        /// <item>
        /// <term>auctions</term>
        /// <description>barbaraAuction(barbaraDescription, winner=Carlotta,currentPrice=8,25, endsOn=7/12/2035,maxProposal=9,27)
        ///and aliceAuction(aliceDescription, winner=null,currentPrice=0,25, endsOn=16/12/2035,maxProposal=0)
        /// </description>
        /// </item>
        /// <item>
        /// <term>sessions</term>
        /// <description>aliceSession for Alice and barbaraSession for Barbara</description>
        /// </item>
        /// </list>  
        /// </summary>
        [SetUp]
        public void Setup()
        {
            List<ISession> expectedSessionList1;
            var newSite1 = CreateAndLoadSite(timeZone, siteName, sessionExpirationTimeInSeconds, minimumBidIncrement,
                userList,
                userList, 0, out expectedSessionList1, out alarmClockMoq, password);
            var aliceExpectedSession = expectedSessionList1.FirstOrDefault(s => s.User.Username == alice);
            var barbaraExpectedSession = expectedSessionList1.FirstOrDefault(s => s.User.Username == barbara);
            var carlottaExpectedSession = expectedSessionList1.FirstOrDefault(s => s.User.Username == carlotta);
            var dalilaExpectedSession = expectedSessionList1.FirstOrDefault(s => s.User.Username == dalila);
            barbaraAuctionEndsOn = new DateTime(2035, 12, 7);
            barbaraAuction = barbaraExpectedSession.CreateAuction(barbaraAuctionDescription, barbaraAuctionEndsOn,
                barbaraStartingPrice);
            if (!barbaraAuction.BidOnAuction(dalilaExpectedSession, 7.75))
            {
                Assert.Fail("Dalila's bid should have been accepted");
            }

            dalilaExpectedSession.Logout();
            if (!barbaraAuction.BidOnAuction(carlottaExpectedSession, 9.27))
            {
                Assert.Fail("Carlotta's bid should have been accepted");
            }

            carlottaExpectedSession.Logout();
            aliceAuctionEndsOn = alarmClockMoq.Object.Now;
            aliceAuction =
                aliceExpectedSession.CreateAuction(aliceAuctionDescription, aliceAuctionEndsOn, aliceStartingPrice);
            expectedAuctionList = new List<IAuction> { barbaraAuction, aliceAuction };
            SetNowToFutureTime(60 * 60 * 23, alarmClockMoq);
            barbaraExpectedSession = newSite1.Login(barbara, password);
            SetNowToFutureTime(60 * 30, alarmClockMoq);
            aliceExpectedSession = newSite1.Login(alice, password);
            SetNowToFutureTime(60 * 30, alarmClockMoq);
            var fine = alarmClockMoq.Object.Now;
            newSite1.CleanupSessions();
            expectedSessionList1 = new List<ISession> { barbaraExpectedSession, aliceExpectedSession };
            var yesterday = alarmClockMoq.Object.Now.AddDays(-1);
            var ok = yesterday.Year == aliceAuctionEndsOn.Year && yesterday.Month == aliceAuctionEndsOn.Month &&
                     yesterday.Day == aliceAuctionEndsOn.Day;
            Assert.That(ok, "Wrong setup: current time is not day after Alice's auction ends");
            Assert.That(aliceExpectedSession.ValidUntil, Is.GreaterThanOrEqualTo(alarmClockMoq.Object.Now.AddHours(1)),
                "Wrong setup: Alice's session has not the expected validity");
            Assert.That(barbaraExpectedSession.ValidUntil,
                Is.GreaterThanOrEqualTo(alarmClockMoq.Object.Now.AddMinutes(30)),
                "Wrong setup: Barbara's session has not the expected validity");
            expectedSessionList = expectedSessionList1;
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite name
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsFullSite1()
        {
            newSite = siteFactory.LoadSite(connectionString, siteName, alarmClockMoq.Object);

            Assert.That(newSite.Name, Is.EqualTo(siteName));
        }


        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite time zone
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsFullSite2()
        {
            newSite = siteFactory.LoadSite(connectionString, siteName, alarmClockMoq.Object);

            Assert.That(newSite.Timezone, Is.EqualTo(timeZone));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite expiration time
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsFullSite3()
        {
            newSite = siteFactory.LoadSite(connectionString, siteName, alarmClockMoq.Object);

            Assert.That(newSite.SessionExpirationInSeconds, Is.EqualTo(sessionExpirationTimeInSeconds));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite minimum bid increment
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsFullSite4()
        {
            newSite = siteFactory.LoadSite(connectionString, siteName, alarmClockMoq.Object);

            Assert.That(newSite.MinimumBidIncrement, Is.EqualTo(minimumBidIncrement));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite users
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsFullSite5()
        {
            newSite = siteFactory.LoadSite(connectionString, siteName, alarmClockMoq.Object);

            CollectionAssert.AreEquivalent(newSite.GetUsers().Select(u => u.Username).ToList(), userList);
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite sessions
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsFullSite6()
        {
            newSite = siteFactory.LoadSite(connectionString, siteName, alarmClockMoq.Object);

            var sessionList = newSite.GetSessions().ToList();
            Assert.That(sessionList, Is.EquivalentTo(expectedSessionList));
            var aliceSession = sessionList.SingleOrDefault(s => s.User.Username == alice);
            Assert.That(aliceSession.ValidUntil, Is.GreaterThan(alarmClockMoq.Object.Now.AddHours(1)));
            var barbaraSession = sessionList.SingleOrDefault(s => s.User.Username == barbara);
            Assert.That(barbaraSession.ValidUntil, Is.GreaterThan(alarmClockMoq.Object.Now.AddMinutes(30)));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. newSite auctions
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_ReturnsFullSite7()
        {
            newSite = siteFactory.LoadSite(connectionString, siteName, alarmClockMoq.Object);

            var auctionList = newSite.GetAuctions(false).ToList();
            Assert.That(auctionList.Count(), Is.EqualTo(2));
            var barbaraAuction = auctionList.SingleOrDefault(a => a.Seller.Username == barbara);
            Assert.That(CheckAuctionValues(barbaraAuction,
                expectedAuctionList.SingleOrDefault(a => a.Seller.Username == barbara).Id, barbara,
                barbaraAuctionEndsOn, barbaraAuctionDescription, 8.25, carlotta));
            var aliceAuction = auctionList.SingleOrDefault(a => a.Seller.Username == alice);
            Assert.That(CheckAuctionValues(aliceAuction,
                expectedAuctionList.SingleOrDefault(a => a.Seller.Username == alice).Id, alice, aliceAuctionEndsOn,
                aliceAuctionDescription, .25, null));
        }
    }

    public class SiteFactoryTests : InstrumentedAuctionSiteTest
    {
        /// <summary>
        /// Verify that loading a site implies setting the alarm through the clock passed as argument
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_UsesAlarmClock()
        {
            var siteName = "pippo";
            siteFactory.CreateSiteOnDb(connectionString, siteName, +1, 10, 5);
            Mock<IAlarmClock> alarmClockMoq;
            var sito = LoadSiteFromName(siteName, out alarmClockMoq);

            alarmClockMoq.Verify(es => es.InstantiateAlarm(It.IsAny<int>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Verify that loading a site implies setting the alarm through the clock passed as argument
        /// so that it rings every 5 minutes
        /// </summary>
        [Test]
        public void LoadSite_ValidArg_InstantiatesAlarm5Minutes()
        {
            var siteName = "pippo";
            siteFactory.CreateSiteOnDb(connectionString, siteName, +1, 10, 5);
            Mock<IAlarmClock> alarmClockMoq;
            var sito = LoadSiteFromName(siteName, out alarmClockMoq);

            alarmClockMoq.Verify(es => es.InstantiateAlarm(5 * 60 * 1000), Times.AtLeastOnce());
        }
        /// <summary>
        /// Verify that after the alarm rings no expired session survives
        /// </summary>
        [Test]
        public void CleanupSessionsOnAlarmRaised()
        {
            Mock<IAlarmClock> alarmClockMoq;
            List<ISession> sessions;
            const string siteName = "pippo";
            const int sessionExpirationTimeInSeconds = 10;
            const int minimumBidIncrement = 5;
            const int timeZone = 1;
            const string uniqueUser = "seller";
            var usernameList = new List<string> { uniqueUser };
            Mock<IAlarm> alarmMoq;
            var site = CreateAndLoadSite(timeZone, siteName, sessionExpirationTimeInSeconds, minimumBidIncrement,
                usernameList, usernameList, 0, out sessions, out alarmClockMoq, out alarmMoq);
            var initialExpiringTime = sessions.SingleOrDefault(s => s.User.Username == uniqueUser).ValidUntil;
            alarmClockMoq.Setup(a => a.Now).Returns(initialExpiringTime.AddSeconds(1));
            alarmMoq.Raise(m => m.RingingEvent += null);
            Assert.That(!site.GetSessions().Any(), "not cleaned-up on ringing event");
        }
    }
}