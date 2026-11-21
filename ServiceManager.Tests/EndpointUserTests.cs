using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data.Settings;
using System;

namespace ServiceManager.Tests
{
    /// <summary>
    /// A configured endpoint account names one Windows account, and the spelling of the name decides
    /// which one it is. That is the only thing that stands between two identically named accounts.
    /// </summary>
    [TestClass]
    public class EndpointUserTests
    {
        [TestMethod]
        public void a_plain_name_is_the_account_of_this_machine()
        {
            var user = new EndpointUser("bob");

            Assert.IsTrue(user.MatchesName(Environment.MachineName + "\\bob"));
        }

        [TestMethod]
        public void the_local_account_matches_a_plain_name()
        {
            var user = new EndpointUser(Environment.MachineName + "\\bob");

            Assert.IsTrue(user.MatchesName("bob"));
        }

        [TestMethod]
        public void a_plain_name_never_names_the_account_of_another_machine()
        {
            var user = new EndpointUser("otherpc\\bob");

            Assert.IsFalse(user.MatchesName("bob"));
        }

        [TestMethod]
        public void the_two_spellings_of_one_account_match_each_other()
        {
            var user = new EndpointUser("CORP\\bob");

            Assert.IsTrue(user.MatchesName("bob@CORP"));
        }

        [TestMethod]
        public void a_name_is_matched_by_the_short_spelling_of_the_other_side()
        {
            var user = new EndpointUser("bob@CORP");

            Assert.IsTrue(user.MatchesName("CORP\\bob"));
        }

        [TestMethod]
        public void the_account_of_another_domain_does_not_match()
        {
            var user = new EndpointUser("CORP\\bob");

            Assert.IsFalse(user.MatchesName("OTHERCORP\\bob"));
        }

        [TestMethod]
        public void the_domain_has_to_be_written_the_way_windows_names_it()
        {
            var user = new EndpointUser("CORP\\bob");

            Assert.IsFalse(user.MatchesName("bob@corp.example"));
        }

        [TestMethod]
        public void names_are_compared_ignoring_the_case()
        {
            Assert.IsTrue(EndpointUser.MatchesName("Service.Watcher", "service.watcher"));
        }

        [TestMethod]
        public void two_different_accounts_do_not_match()
        {
            Assert.IsFalse(EndpointUser.MatchesName("service.watcher", "service.reader"));
        }

        [TestMethod]
        public void a_domain_in_front_of_a_different_account_does_not_match()
        {
            Assert.IsFalse(EndpointUser.MatchesName("CORP\\service.watcher", "CORP\\service.reader"));
        }

        [TestMethod]
        public void a_user_without_a_name_matches_nobody()
        {
            var user = new EndpointUser();

            Assert.IsFalse(user.MatchesName("service.watcher"));
        }

        [TestMethod]
        public void a_missing_name_never_matches()
        {
            Assert.IsFalse(EndpointUser.MatchesName("service.watcher", null));
            Assert.IsFalse(EndpointUser.MatchesName("service.watcher", String.Empty));
        }
    }
}
