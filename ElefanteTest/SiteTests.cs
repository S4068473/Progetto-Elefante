using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using TAP2018_19.AuctionSite.Interfaces.Tests;
using TAP2018_19.AlarmClock.Interfaces;

namespace TAP2018_19.AuctionSite.Interfaces.Tests
{
    public class SiteTests : InstrumentedAuctionSiteTest
    {
        protected ISite Site;
        protected Mock<IAlarmClock> AlarmClock;
        /// <summary>
        /// Initializes Site:
        /// <list type="table">
        /// <item>
        /// <term>name</term>
        /// <description>working site</description>
        /// </item>
        /// <item>
        /// <term>time zone</term>
        /// <description>5</description>
        /// </item>
        /// <item>
        /// <term>expiration time</term>
        /// <description>3600 seconds</description>
        /// </item>
        /// <item>
        /// <term>minimum bid increment</term>
        /// <description>3.5</description>
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
        public void SiteInitialize()
        {
            const string workingSite = "working site";
            const int timeZone = 5;
            siteFactory.CreateSiteOnDb(connectionString, workingSite, timeZone, 3600, 3.5);
            AlarmClockMock(timeZone, out AlarmClock);
            Site = siteFactory.LoadSite(connectionString, workingSite, AlarmClock.Object);
        }

        private IEnumerable<IAuction> AddAuctions(DateTime EndsOn1, int howMany1)
        {
            Debug.Assert(howMany1 >= 0);
            var username = "pinco" + DateTime.Now.Ticks;
            Site.CreateUser(username, "pippo.123");
            var sellerSession = Site.Login(username, "pippo.123");
            var result = new List<IAuction>();
            for (int i = 0; i < howMany1; i++)
                result.Add(sellerSession.CreateAuction($"Auction {i} of {howMany1} ending on {EndsOn1}",
                    EndsOn1, 7.7 * i + 11));
            return result;
        }
        /// <summary>
        /// Verify that a call to GetUsers on a deleted site throws InvalidOperationException
        /// </summary>
        [Test]
        public void GetUsers_OnDeletedObject_Throws()
        {
            Site.Delete();
            Assert.That(() => Site.GetUsers(), Throws.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verify that GetUsers on a site without users
        /// returns the empty sequence
        /// </summary>
        [Test]
        public void GetUsers_ValidArg_ReturnsEmpty()
        {
            var users = Site.GetUsers();
            Assert.That(users, Is.Empty);
        }

        /// <summary>
        /// Verify that GetUsers on a site with 5 users
        /// returns those 5 users
        /// </summary>
        [Test]
        public void GetUsers_ValidArg_ReturnsEnumerableOf5()
        {
            var expectedUsers = new List<string> { "primo", "secondo", "terzo", "quarto" };
            foreach (var user in expectedUsers)
            {
                Site.CreateUser(user, "verySTRONGp@ssw0rd");
            }

            var users = Site.GetUsers().Select(u => u.Username);
            Assert.That(expectedUsers, Is.EquivalentTo(users));
        }

        /// <summary>
        /// Verify that a call to GetAuctions on a deleted site throws InvalidOperationException
        /// </summary>
        [Test]
        public void GetAuctions_OnDeletedObject_Throws()
        {
            Site.Delete();
            Assert.That(() => Site.GetAuctions(true), Throws.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verify that GetAuctions on a site without users
        /// returns the empty sequence
        /// </summary>
        [Test]
        public void GetAuctions_ValidArg_ReturnsEmpty1()
        {
            var auctions = Site.GetAuctions(false);
            Assert.That(auctions, Is.Empty);
        }

        /// <summary>
        /// Verify that GetAuctions on a site with only expired auctions
        /// returns the empty sequence if called on true
        /// </summary>
        [Test]
        public void GetAuctions_ValidArg_ReturnsEmpty2()
        {
            var now = AlarmClock.Object.Now;
            AddAuctions(now.AddDays(1), 5);
            AlarmClock.Setup(ac => ac.Now).Returns(now.AddHours(25));
            Site = siteFactory.LoadSite(connectionString, Site.Name, AlarmClock.Object); //needed to refresh time
            var auctions = Site.GetAuctions(true);
            Assert.That(auctions, Is.Empty);
        }

        /// <summary>
        /// Verify that GetAuctions on a site with two expired
        /// and three still open auctions returns all five if called on false
        /// </summary>
        [Test]
        public void GetAuctions_ValidArg_ReturnsEnumerableOf5()
        {
            var now = AlarmClock.Object.Now;
            var expectedAuctions = AddAuctions(now.AddDays(1), 2).Concat(AddAuctions(now.AddDays(12), 3));
            AlarmClock.Setup(ac => ac.Now).Returns(now.AddHours(25));
            Site = siteFactory.LoadSite(connectionString, Site.Name, AlarmClock.Object); //needed to refresh time
            var auctions = Site.GetAuctions(false);
            Assert.That(auctions, Is.EquivalentTo(expectedAuctions));
        }

        /// <summary>
        /// Verify that GetAuctions on a site with five expired
        /// and 3 still open auctions returns (only) the latters if called on true
        /// </summary>
        [Test]
        public void GetAuctions_ValidArg_ReturnsEnumerableOf3Valid()
        {
            var now = AlarmClock.Object.Now;
            AddAuctions(now.AddDays(1), 5);
            var expectedAuctions = AddAuctions(now.AddDays(12), 3);
            AlarmClock.Setup(ac => ac.Now).Returns(now.AddHours(25));
            Site = siteFactory.LoadSite(connectionString, Site.Name, AlarmClock.Object); //needed to refresh time
            var auctions = Site.GetAuctions(true);
            Assert.That(auctions, Is.EquivalentTo(expectedAuctions));
        }

        /// <summary>
        /// Verify that a call to Login on a deleted site throws InvalidOperationException
        /// </summary>
        [Test]
        public void Login_OnDeletedObject_Throws()
        {
            const string userName = "Vincenzo";
            const string password = "gdgajgfjgkgfakg";
            Site.CreateUser(userName, password);
            Site.Delete();
            Assert.That(() => Site.Login(userName, password), Throws.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verify that login on null username throws ArgumentNullException
        /// </summary>
        [Test]
        public void Login_NullUsername_Throws()
        {
            Assert.That(() => Site.Login(null, "puffetta"), Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that login on null password throws ArgumentNullException
        /// </summary>
        [Test]
        public void Login_NullPassword_Throws()
        {
            const string userName = "Agata";
            const string password = "nq3b457asf7";
            Site.CreateUser(userName, password);
            Assert.That(() => Site.Login(userName, null), Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that login on an inexistent user return null session
        /// </summary>
        [Test]
        public void Login_InexistentUser_Returns_null()
        {
            var session = Site.Login("pinco", "ciao");
            Assert.IsNull(session);
        }

        /// <summary>
        /// Verify that login on an existing user with a wrong password return null session
        /// </summary>
        [Test]
        public void Login_WrongPassword_Returns_null()
        {
            const string username = "pinco";
            Site.CreateUser(username, "pippo.123");
            var session = Site.Login(username, "ciao");
            Assert.IsNull(session);
        }

        /// <summary>
        /// Verify that login on correct credentials return a non null session
        /// </summary>
        [Test]
        public void Login_ValidArg_ReturnsNonNullSession()
        {
            const string username = "pinco";
            const string password = "pippo.123";
            Site.CreateUser(username, password);
            var session = Site.Login(username, password);
            Assert.IsNotNull(session, "A non-null session should have been returned");
        }

        /// <summary>
        /// Verify that login on correct credentials return a valid session
        /// for the correct user
        /// </summary>
        [Test]
        public void Login_ValidArg_ReturnsUserSession()
        {
            const string username = "pinco";
            const string password = "pippo.123";
            Site.CreateUser(username, password);
            var session = Site.Login(username, password);
            Assert.That(session.User, Is.EqualTo(Site.GetUsers().First(u => u.Username == username)));
        }
        /// <summary>
        /// Verify that login on correct credentials return a valid session
        /// with a feasible expiration time
        /// </summary>
        [Test]
        public void Login_ValidArg_ReturnsNewSession()
        {
            const string username = "pinco";
            const string password = "pippo.123";
            Site.CreateUser(username, password);
            var session = Site.Login(username, password);
            Assert.That(session.ValidUntil,
                Is.InRange(AlarmClock.Object.Now.AddSeconds(Site.SessionExpirationInSeconds - 5),
                    AlarmClock.Object.Now.AddSeconds(Site.SessionExpirationInSeconds + 5)));
        }
        /// <summary>
        /// Verify that two calls to login for the same user return the same object
        /// </summary>
        [Test]
        public void Login_ValidArg_ReturnsOldSession()
        {
            const string username = "pinco";
            const string password = "pippo.123";
            Site.CreateUser(username, password);
            var expectedSessionId = Site.Login(username, password).Id;
            var sessionId = Site.Login(username, password).Id;
            Assert.That(sessionId, Is.EqualTo(expectedSessionId));
        }

        /// <summary>
        /// Verify that a call to GetSessions on a deleted site throws InvalidOperationException
        /// </summary>
        [Test]
        public void GetSession_OnDeletedObject_Throws()
        {
            Site.CreateUser("pinco", "pippo.123");
            var expectedSessionId = Site.Login("pinco", "pippo.123").Id;
            Site.Delete();
            Assert.That(() => Site.GetSession(expectedSessionId), Throws.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verify that GetSession on null throws ArgumentNullException
        /// </summary>
        [Test]
        public void GetSession_NullArg_Throws()
        {
            Assert.That(() => Site.GetSession(null), Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that GetSession on the empty session id returns a null session
        /// </summary>
        [Test]
        public void GetSession_EmptyArg_Returnsnull()
        {
            var sessionEmptyId = Site.GetSession("");
            Assert.IsNull(sessionEmptyId);
        }

        /// <summary>
        /// Verify that GetSession on a non-existing session id returns a null session
        /// </summary>
        [Test]
        public void GetSession_Inexistent_Arg_Returnsnull()
        {
            Site.CreateUser("pinco", "pippo.123");
            var session = Site.Login("pinco", "pippo.123");
            var sessionId = session.Id;
            session.Logout();
            Site.CleanupSessions();
            var inexistentSession = Site.GetSession(sessionId);
            Assert.IsNull(inexistentSession);
        }

        /// <summary>
        /// Verify that GetSession returns null if called on the id of an invalid session
        /// </summary>
        [Test]
        public void GetSession_ArgInvalidSession_Returnsnull()
        {
            Site.CreateUser("pinco", "pippo.123");
            var session = Site.Login("pinco", "pippo.123");
            SetNowToFutureTime(Site.SessionExpirationInSeconds + 1, AlarmClock);
            var inexistentSession = Site.GetSession(session.Id);
            Assert.IsNull(inexistentSession);
        }
        /// <summary>
        /// Verify that GetSession on the id of a valid session
        /// returns that session
        /// </summary>
        [Test]
        public void GetSession_ValidArg_ReturnsSessionOkId()
        {
            Site.CreateUser("pinco", "pippo.123");
            var expectedSessionId = Site.Login("pinco", "pippo.123").Id;
            var sessionId = Site.GetSession(expectedSessionId).Id;
            Assert.That(sessionId, Is.EqualTo(expectedSessionId));
        }
        /// <summary>
        /// Verify that GetSession on the id of a valid session
        /// returns a session with a feasible validity time
        /// (redundant test, if id is a key as required)
        /// </summary>
        [Test]
        public void GetSession_ValidArg_ReturnsSessionOkValidUntil()
        {
            const string username = "pinco";
            const string password = "pippo.123";
            var userList = new List<string>();
            userList.Add(username);
            var site = CreateAndLoadSite(3, "my site", 60, 1, out AlarmClock, userList, password);
            var sessionId = site.Login(username, password).Id;
            var validUntil = site.GetSession(sessionId).ValidUntil;
            var minValidUntil = AlarmClock.Object.Now.AddSeconds(50);
            Assert.That(validUntil, Is.GreaterThan(minValidUntil));
        }

        /// <summary>
        /// Verify that a call to CreateUser on a deleted site throws InvalidOperationException
        /// </summary>
        [Test]
        public void CreateUser_OnDeletedObject_Throws()
        {
            Site.Delete();
            Assert.That(() => Site.CreateUser("new user", "shdhjajlhkahf"), Throws.TypeOf<InvalidOperationException>());
        }
        /// <summary>
        /// Verify that CreateUser on null username throws ArgumentNullException
        /// </summary>
        [Test]
        public void CreateUser_NullUsername_Throws()
        {
            Assert.That(() => Site.CreateUser(null, "pincopallo"), Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that CreateUser on null password throws ArgumentNullException
        /// </summary>
        [Test]
        public void CreateUser_NullPassword_Throws()
        {
            Assert.That(() => Site.CreateUser("pincopallo", null), Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Verify that CreateUser on a password shorter that
        /// DomainConstraints.MinUserPassword throws ArgumentException
        /// </summary>
        [Test]
        public void CreateUser_TooShort_password_Throws()
        {
            Assert.That(() => Site.CreateUser("pincopallo", "boh"), Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that CreateUser on a username shorter that
        /// DomainConstraints.MinUserName throws ArgumentException
        /// </summary>
        [Test]
        public void CreateUser_TooShort_username_Throws()
        {
            Assert.That(() => Site.CreateUser("aa", "ma si'!"), Throws.TypeOf<ArgumentException>());
        }
        /// <summary>
        /// Verify that CreateUser on a username longer that
        /// DomainConstraints.MaxUserName throws ArgumentException
        /// </summary>
        [Test]
        public void CreateUser_TooLong_username_Throws()
        {
            Assert.That(
                () => Site.CreateUser("abcdefgh12345678abcdefgh12345678abcdefgh12345678abcdefgh12345678A",
                    "vabenecosi'"), Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that the second call to CreateUser on the username and password
        /// throws NameAlreadyInUseException
        /// </summary>
        [Test]
        public void CreateUser_Taken_Username_Throws()
        {
            Site.CreateUser("Giorgio", "corretta");
            Assert.That(() => Site.CreateUser("Giorgio", "corretta"), Throws.TypeOf<NameAlreadyInUseException>());
        }

        /// <summary>
        /// Verify that CreateUser on a username already taken
        /// throws NameAlreadyInUseException
        /// </summary>
        [Test]
        public void CreateUser_Taken_Username_bis_Throws()
        {
            Site.CreateUser("Giorgio", "corretta");

            Assert.That(() => Site.CreateUser("Giorgio", "ma si'!"), Throws.TypeOf<NameAlreadyInUseException>());
        }

        /// <summary>
        /// Verify that CreateUser on correct credentials not yet in use
        /// creates the user
        /// </summary>
        [Test]
        public void CreateUser_ValidArgs_CreateUser()
        {
            const string username = "SonoNuovo";
            const string password = "ma si'!";
            Site.CreateUser(username, password);
            var session = Site.Login(username, password);
            Assert.That(session, Is.Not.Null);
            Assert.That(session.User.Username, Is.EqualTo(username));
        }
        /// <summary>
        /// Verify that newly created users do not have sessions
        /// </summary>
        [Test]
        public void CreateUser_ValidArgs_CreateUser_Without_Session()
        {
            const string username = "pinco";
            const string password = "pippo.123";
            Site.CreateUser(username, password);
            var newUserSessions = Site.GetSessions().Where(s => s.User.Username == username);
            Assert.That(newUserSessions, Is.Empty);
        }
        /// <summary>
        /// Verify that a call to Delete on a deleted site throws InvalidOperationException
        /// </summary>
        [Test]
        public void Delete_OnDeletedObject_Throws()
        {
            Site.Delete();
            Assert.That(() => Site.Delete(), Throws.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verify that after deleting a site, its name is not anymore known to the SiteFactory 
        /// </summary>
        [Test]
        public void Delete_ValidArg_DeletesThis()
        {
            Site.Delete();
            var survived = siteFactory.GetSiteNames(connectionString).Any(s => s == Site.Name);
            Assert.That(!survived);
        }

        /// <summary>
        /// Verify that a call to CleanupSessions on a deleted site throws InvalidOperationException
        /// </summary>
        [Test]
        public void CleanupSessions_OnDeletedObject_Throws()
        {
            Site.Delete();
            Assert.That(() => Site.CleanupSessions(), Throws.TypeOf<InvalidOperationException>());
        }
        /// <summary>
        /// Verify that after a CleanUpSessions on a site with three expired sessions and
        /// two still valid sessions, only the latters survive
        /// </summary>
        [Test]
        public void CleanupSessions_ValidArg_CleanUp()
        {
            var now = AlarmClock.Object.Now;
            const string survivor1 = "pippo";
            const string survivor2 = "pluto";
            var users = new List<string> { "qui", "quo", "qua", survivor1, survivor2 };
            const string pw = "my serious pw";
            foreach (var user in users)
            {
                Site.CreateUser(user, pw);
                Site.Login(user, pw);
            }

            AlarmClock.Setup(ac => ac.Now).Returns(now.AddSeconds(Site.SessionExpirationInSeconds - 1));
            var expectedSessions = new List<ISession>();
            expectedSessions.Add(Site.Login(survivor1, pw)); //reset expiration time
            expectedSessions.Add(Site.Login(survivor2, pw));

            now = AlarmClock.Object.Now;
            AlarmClock.Setup(ac => ac.Now).Returns(now.AddSeconds(2));
            Site.CleanupSessions();
            var sessions = Site.GetSessions();
            Assert.That(sessions, Is.EquivalentTo(expectedSessions));
        }

        /// <summary>
        /// Verify that property Name yield the site name
        /// </summary>
        [Test]
        public void Name_ReturnsSiteName()
        {
            const string name = "mi piace";
            siteFactory.CreateSiteOnDb(connectionString, name, 0, 10, 2);
            Mock<IAlarmClock> alarmClockMoq;
            AlarmClockMock(0, out alarmClockMoq);
            var mySite = siteFactory.LoadSite(connectionString, name, alarmClockMoq.Object);
            Assert.That(mySite.Name, Is.EqualTo(name));
        }
    }
}