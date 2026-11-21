using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data;
using ServiceManager.Base.Data.Settings;
using ServiceManager.Base.Wcf;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.ServiceModel.Channels;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The Users rule lets an administrator through on its rights alone and asks everybody else for
    /// the password of a configured account. The UsersOnly rule turns the rights off, so every account
    /// has to be listed. The account named in the header has to be the one that signed in over the
    /// transport, otherwise one password would open the endpoint for everyone.
    /// </summary>
    [TestClass]
    public class UserAuthorizationManagerTests
    {
        private const String Password = "s3cret";

        [TestMethod]
        public void a_configured_account_is_allowed_with_its_password()
        {
            var caller = nonAdministratorCaller();

            var users = new List<EndpointUser> { new EndpointUser(caller.Name, SecretStorage.Protect(Password)) };

            Assert.IsTrue(UserAuthorizationManager.IsAllowed(caller, users, credential(caller.Name, Password)));
        }

        [TestMethod]
        public void the_short_name_in_the_header_matches_the_signed_in_account()
        {
            var caller = nonAdministratorCaller();

            var shortName = caller.Name.Substring(caller.Name.IndexOf('\\') + 1);

            var users = new List<EndpointUser> { new EndpointUser(caller.Name, SecretStorage.Protect(Password)) };

            Assert.IsTrue(UserAuthorizationManager.IsAllowed(caller, users, credential(shortName, Password)));
        }

        [TestMethod]
        public void a_wrong_password_is_refused()
        {
            var caller = nonAdministratorCaller();

            var users = new List<EndpointUser> { new EndpointUser(caller.Name, SecretStorage.Protect(Password)) };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(caller, users, credential(caller.Name, "wrong")));
        }

        [TestMethod]
        public void a_caller_without_a_credential_is_refused()
        {
            var caller = nonAdministratorCaller();

            var users = new List<EndpointUser> { new EndpointUser(caller.Name, SecretStorage.Protect(Password)) };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(caller, users, null));
        }

        [TestMethod]
        public void a_credential_without_a_user_is_refused()
        {
            var caller = nonAdministratorCaller();

            var users = new List<EndpointUser> { new EndpointUser(caller.Name, SecretStorage.Protect(Password)) };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(caller, users, credential(String.Empty, Password)));
        }

        [TestMethod]
        public void an_account_that_is_not_configured_is_refused()
        {
            var caller = nonAdministratorCaller();

            var users = new List<EndpointUser> { new EndpointUser("service.watcher", SecretStorage.Protect(Password)) };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(caller, users, credential("service.watcher", Password)));
        }

        [TestMethod]
        public void a_credential_that_names_another_account_than_the_caller_is_refused()
        {
            var users = new List<EndpointUser> { new EndpointUser("service.watcher", SecretStorage.Protect(Password)) };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(WindowsIdentity.GetAnonymous(), users, credential("service.watcher", Password)));
        }

        [TestMethod]
        public void an_empty_user_list_refuses_every_credential()
        {
            Assert.IsFalse(UserAuthorizationManager.IsAllowed(WindowsIdentity.GetAnonymous(), new List<EndpointUser>(), credential("service.watcher", Password)));
        }

        [TestMethod]
        public void a_missing_user_list_refuses_every_credential()
        {
            Assert.IsFalse(UserAuthorizationManager.IsAllowed(WindowsIdentity.GetAnonymous(), null, credential("service.watcher", Password)));
        }

        [TestMethod]
        public void a_missing_identity_is_refused()
        {
            var users = new List<EndpointUser> { new EndpointUser("service.watcher", SecretStorage.Protect(Password)) };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(null, users, credential("service.watcher", Password)));
        }

        [TestMethod]
        public void an_administrator_is_allowed_without_a_credential()
        {
            var caller = WindowsIdentity.GetCurrent();

            if (!AdministratorAuthorizationManager.IsAdministrator(caller))
            {
                Assert.Inconclusive("the test runner is not an administrator, so nothing is allowed by rights alone here");
            }

            Assert.IsTrue(UserAuthorizationManager.IsAllowed(caller, new List<EndpointUser>(), null));
        }

        [TestMethod]
        public void the_users_only_rule_refuses_an_administrator_that_is_not_listed()
        {
            var caller = WindowsIdentity.GetCurrent();

            if (!AdministratorAuthorizationManager.IsAdministrator(caller))
            {
                Assert.Inconclusive("the test runner has no administrator rights, so there is nothing to take away here");
            }

            var users = new List<EndpointUser> { new EndpointUser("service.watcher", SecretStorage.Protect(Password)) };

            Assert.IsFalse(UserAuthorizationManager.IsAllowed(caller, users, null, false));
        }

        [TestMethod]
        public void the_users_only_rule_lets_a_listed_administrator_with_its_password()
        {
            var caller = WindowsIdentity.GetCurrent();

            if (!AdministratorAuthorizationManager.IsAdministrator(caller))
            {
                Assert.Inconclusive("the test runner is not an administrator, so the rule cannot be tried here");
            }

            var users = new List<EndpointUser> { new EndpointUser(caller.Name, SecretStorage.Protect(Password)) };

            Assert.IsTrue(UserAuthorizationManager.IsAllowed(caller, users, credential(caller.Name, Password), false));
        }

        [TestMethod]
        public void the_users_only_rule_still_lets_a_listed_account_with_its_password_in()
        {
            var caller = nonAdministratorCaller();

            var users = new List<EndpointUser> { new EndpointUser(caller.Name, SecretStorage.Protect(Password)) };

            Assert.IsTrue(UserAuthorizationManager.IsAllowed(caller, users, credential(caller.Name, Password), false));
        }

        [TestMethod]
        public void a_manager_can_be_told_to_ask_every_caller()
        {
            var manager = new UserAuthorizationManager(new List<EndpointUser> { new EndpointUser("service.watcher", SecretStorage.Protect(Password)) }, false);

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
            var users = new List<EndpointUser> { new EndpointUser("service.watcher", SecretStorage.Protect(Password)) };

            var manager = new UserAuthorizationManager(users);

            Assert.AreEqual(1, manager.Users.Count);
            Assert.IsTrue(manager.Users[0].MatchesName("service.watcher"));
        }

        private static EndpointCredential credential(String User, String Password)
        {
            var credential = new EndpointCredential();
            credential.User = User;
            credential.Password = Password;

            return credential;
        }

        // The password rule only matters for a caller without administrator rights, and an elevated
        // test runner has them.
        private static WindowsIdentity nonAdministratorCaller()
        {
            var caller = WindowsIdentity.GetCurrent();

            if (AdministratorAuthorizationManager.IsAdministrator(caller))
            {
                Assert.Inconclusive("an elevated test runner is allowed by its rights, so the password is never asked for");
            }

            return caller;
        }
    }
}
