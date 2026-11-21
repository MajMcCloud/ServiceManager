using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data.Settings;
using ServiceManager.Base.Wcf;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.ServiceModel.Channels;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The Users rule lets an administrator through on its rights alone and takes every other account
    /// that is listed. The UsersOnly rule turns the rights off, so an account has to be listed, and a
    /// name never stands for the identically named account of another machine.
    /// </summary>
    [TestClass]
    public class UserAuthorizationManagerTests
    {
        [TestMethod]
        public void a_listed_account_is_allowed()
        {
            var caller = nonAdministratorCaller();

            var users = new List<EndpointUser> { new EndpointUser(caller.Name) };

            Assert.IsTrue(UserAuthorizationManager.IsAllowed(caller, users));
        }

        [TestMethod]
        public void the_short_name_in_the_list_matches_the_signed_in_account()
        {
            var caller = nonAdministratorCaller();

            var shortName = caller.Name.Substring(caller.Name.IndexOf('\\') + 1);

            var users = new List<EndpointUser> { new EndpointUser(shortName) };

            Assert.IsTrue(UserAuthorizationManager.IsAllowed(caller, users));
        }

        [TestMethod]
        public void an_account_that_is_not_listed_is_refused()
        {
            var caller = nonAdministratorCaller();

            var users = new List<EndpointUser> { new EndpointUser("service.watcher") };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(caller, users));
        }

        [TestMethod]
        public void the_like_named_account_of_another_machine_is_refused()
        {
            var caller = nonAdministratorCaller();

            var shortName = caller.Name.Substring(caller.Name.IndexOf('\\') + 1);

            var users = new List<EndpointUser> { new EndpointUser("otherpc\\" + shortName) };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(caller, users, false));
        }

        [TestMethod]
        public void an_anonymous_caller_is_refused()
        {
            var users = new List<EndpointUser> { new EndpointUser("service.watcher") };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(WindowsIdentity.GetAnonymous(), users, false));
        }

        [TestMethod]
        public void an_empty_user_list_refuses_every_caller()
        {
            Assert.IsFalse(UserAuthorizationManager.IsAllowed(WindowsIdentity.GetAnonymous(), new List<EndpointUser>()));
        }

        [TestMethod]
        public void a_missing_user_list_refuses_every_caller()
        {
            Assert.IsFalse(UserAuthorizationManager.IsAllowed(WindowsIdentity.GetAnonymous(), null));
        }

        [TestMethod]
        public void a_missing_identity_is_refused()
        {
            var users = new List<EndpointUser> { new EndpointUser("service.watcher") };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(null, users, false));
        }

        [TestMethod]
        public void an_administrator_is_allowed_by_its_rights()
        {
            var caller = WindowsIdentity.GetCurrent();

            if (!AdministratorAuthorizationManager.IsAdministrator(caller))
            {
                Assert.Inconclusive("the test runner is not an administrator, so nothing is allowed by rights alone here");
            }

            Assert.IsTrue(UserAuthorizationManager.IsAllowed(caller, new List<EndpointUser>()));
        }

        [TestMethod]
        public void the_users_only_rule_refuses_an_administrator_that_is_not_listed()
        {
            var caller = WindowsIdentity.GetCurrent();

            if (!AdministratorAuthorizationManager.IsAdministrator(caller))
            {
                Assert.Inconclusive("the test runner has no administrator rights, so there is nothing to take away here");
            }

            var users = new List<EndpointUser> { new EndpointUser("service.watcher") };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(caller, users, false));
        }

        [TestMethod]
        public void the_users_only_rule_lets_a_listed_administrator_in()
        {
            var caller = WindowsIdentity.GetCurrent();

            if (!AdministratorAuthorizationManager.IsAdministrator(caller))
            {
                Assert.Inconclusive("the test runner is not an administrator, so the rule cannot be tried here");
            }

            var users = new List<EndpointUser> { new EndpointUser(caller.Name) };

            Assert.IsTrue(UserAuthorizationManager.IsAllowed(caller, users, false));
        }

        [TestMethod]
        public void a_listed_account_needs_no_secret_of_its_own()
        {
            var caller = nonAdministratorCaller();

            var users = new List<EndpointUser> { new EndpointUser(caller.Name) };

            Assert.IsTrue(UserAuthorizationManager.IsAllowed(caller, users, false));
        }

        [TestMethod]
        public void a_manager_can_be_told_to_ask_every_caller()
        {
            var manager = new UserAuthorizationManager(new List<EndpointUser> { new EndpointUser("service.watcher") }, false);

            Assert.IsFalse(manager.AllowAdministrators, "the UsersOnly rule reads no rights, only the accounts");
        }

        [TestMethod]
        public void a_manager_without_users_refuses_a_call_without_a_security_context()
        {
            var manager = new UserAuthorizationManager();

            var request = Message.CreateMessage(MessageVersion.Soap12, "urn:servicemanager:Init");

            Assert.IsFalse(manager.CheckAccess(null, ref request));
        }

        [TestMethod]
        public void the_manager_keeps_the_accounts_it_was_given()
        {
            var users = new List<EndpointUser> { new EndpointUser("service.watcher") };

            var manager = new UserAuthorizationManager(users);

            Assert.AreEqual(1, manager.Users.Count);
            Assert.IsTrue(manager.Users[0].MatchesName("service.watcher"));
        }

        // A rule that reads rights answers an elevated test runner by those rights alone.
        private static WindowsIdentity nonAdministratorCaller()
        {
            var caller = WindowsIdentity.GetCurrent();

            if (AdministratorAuthorizationManager.IsAdministrator(caller))
            {
                Assert.Inconclusive("an elevated test runner is allowed by its rights, so the account list is never read here");
            }

            return caller;
        }
    }
}
