using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data;
using ServiceManager.Base.Data.Settings;
using System;

namespace ServiceManager.Tests
{
    /// <summary>
    /// A configured endpoint user is matched by its Windows account, and its password is the one
    /// sealed on this machine. Both answers decide whether a caller may use the endpoint.
    /// </summary>
    [TestClass]
    public class EndpointUserTests
    {
        [TestMethod]
        public void the_domain_spelling_matches_the_plain_account()
        {
            var user = new EndpointUser("CORP\\service.watcher", SecretStorage.Protect("s3cret"));

            Assert.IsTrue(user.MatchesName("service.watcher"));
        }

        [TestMethod]
        public void the_upn_spelling_matches_the_plain_account()
        {
            var user = new EndpointUser("service.watcher@corp.example", SecretStorage.Protect("s3cret"));

            Assert.IsTrue(user.MatchesName("service.watcher"));
        }

        [TestMethod]
        public void the_two_spellings_of_one_account_match_each_other()
        {
            var user = new EndpointUser("CORP\\service.watcher", SecretStorage.Protect("s3cret"));

            Assert.IsTrue(user.MatchesName("service.watcher@corp.example"));
        }

        [TestMethod]
        public void the_full_spelling_is_matched_by_the_short_one_of_the_other_side()
        {
            var user = new EndpointUser("service.watcher", SecretStorage.Protect("s3cret"));

            Assert.IsTrue(user.MatchesName("CORP\\service.watcher"));
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

        [TestMethod]
        public void the_stored_password_is_accepted()
        {
            var user = new EndpointUser("service.watcher", SecretStorage.Protect("s3cret"));

            Assert.IsTrue(user.CheckPassword("s3cret"));
        }

        [TestMethod]
        public void a_different_password_is_refused()
        {
            var user = new EndpointUser("service.watcher", SecretStorage.Protect("s3cret"));

            Assert.IsFalse(user.CheckPassword("wrong"));
        }

        [TestMethod]
        public void an_empty_password_is_refused()
        {
            var user = new EndpointUser("service.watcher", SecretStorage.Protect("s3cret"));

            Assert.IsFalse(user.CheckPassword(String.Empty));
            Assert.IsFalse(user.CheckPassword(null));
        }

        [TestMethod]
        public void a_user_without_a_stored_password_refuses_every_attempt()
        {
            var user = new EndpointUser("service.watcher", null);

            Assert.IsFalse(user.CheckPassword("s3cret"));
        }

        [TestMethod]
        public void a_password_that_cannot_be_read_on_this_machine_is_refused()
        {
            var user = new EndpointUser("service.watcher", "dpapi:not base64!");

            Assert.IsFalse(user.CheckPassword("s3cret"));
        }
    }
}
