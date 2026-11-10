using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ServiceManager.Base.Data.Settings;
using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The settings side of multi provider notifications: an older file has to end up as a target,
    /// and every event has to stay attached to the flag the user ticked.
    /// </summary>
    [TestClass]
    public class NotificationConfigTests
    {
        private static readonly Guid LegacyPlugin = new Guid("11111111-2222-3333-4444-555555555555");

        /// <summary>A settings file exactly as the single provider version wrote it.</summary>
        private const String LegacyJson =
            "{" +
            "\"InstanceName\":\"Test instance\"," +
            "\"PluginId\":\"11111111-2222-3333-4444-555555555555\"," +
            "\"OnRestart\":true," +
            "\"OnStart\":true," +
            "\"OnStop\":false," +
            "\"OnConsoleOutput\":false," +
            "\"OnConsoleError\":true," +
            "\"OnServerStart\":false," +
            "\"OnServerShutdown\":true" +
            "}";

        private static readonly NotificationKind[] AllKinds =
        {
            NotificationKind.ServerStart,
            NotificationKind.ServerShutdown,
            NotificationKind.ServiceStart,
            NotificationKind.ServiceStop,
            NotificationKind.ServiceRestart,
            NotificationKind.ConsoleOutput,
            NotificationKind.ConsoleError,
            NotificationKind.DependencyWait
        };


        [TestMethod]
        public void legacySettings_becomeOneTargetWithTheOldEventSelection()
        {
            var settings = JsonConvert.DeserializeObject<Notifications>(LegacyJson);

            Assert.AreEqual(1, settings.Targets.Count, "a single provider file has to end up with a single target");

            var target = settings.Targets[0];

            Assert.AreEqual(LegacyPlugin, target.PluginId);
            Assert.IsTrue(target.IsEnabled(NotificationKind.ServiceStart));
            Assert.IsTrue(target.IsEnabled(NotificationKind.ServiceRestart));
            Assert.IsTrue(target.IsEnabled(NotificationKind.ConsoleError));
            Assert.IsTrue(target.IsEnabled(NotificationKind.ServerShutdown));
            Assert.IsFalse(target.IsEnabled(NotificationKind.ServiceStop));
            Assert.IsFalse(target.IsEnabled(NotificationKind.ConsoleOutput));
            Assert.IsFalse(target.IsEnabled(NotificationKind.ServerStart));
        }

        [TestMethod]
        public void legacyValuesAreClearedSoTheOldSetupCannotBeAppliedTwice()
        {
            var settings = JsonConvert.DeserializeObject<Notifications>(LegacyJson);

            Assert.IsNull(settings.PluginId);
            Assert.IsFalse(settings.OnStart);
            Assert.IsFalse(settings.OnStop);
            Assert.IsFalse(settings.OnRestart);
            Assert.IsFalse(settings.OnConsoleOutput);
            Assert.IsFalse(settings.OnConsoleError);
            Assert.IsFalse(settings.OnServerStart);
            Assert.IsFalse(settings.OnServerShutdown);
        }

        [TestMethod]
        public void legacyFileWithoutAProvider_keepsNotificationsOff()
        {
            var settings = JsonConvert.DeserializeObject<Notifications>("{\"OnConsoleError\":true}");

            Assert.AreEqual(0, settings.Targets.Count, "without a provider there is nothing to migrate to");
            Assert.AreEqual(0, settings.GetProviders(NotificationKind.ConsoleError).Count);
        }

        [TestMethod]
        public void fileThatAlreadyCarriesTargets_isLeftAlone()
        {
            var written = JsonConvert.SerializeObject(new Notifications()
            {
                Targets = new List<NotificationTarget>
                {
                    new NotificationTarget() { PluginId = LegacyPlugin, OnConsoleOutput = false }
                }
            });

            var settings = JsonConvert.DeserializeObject<Notifications>(written);

            Assert.AreEqual(1, settings.Targets.Count);
            Assert.IsFalse(settings.Targets[0].IsEnabled(NotificationKind.ConsoleOutput), "a saved selection must not be reset by the defaults");
        }

        [TestMethod]
        public void migratedSettings_surviveASecondLoad()
        {
            var settings = JsonConvert.DeserializeObject<Notifications>(LegacyJson);

            var again = JsonConvert.DeserializeObject<Notifications>(JsonConvert.SerializeObject(settings));

            Assert.AreEqual(1, again.Targets.Count, "saving a migrated file must not produce a second target");
            Assert.AreEqual(LegacyPlugin, again.Targets[0].PluginId);
            Assert.IsTrue(again.Targets[0].IsEnabled(NotificationKind.ConsoleError));
            Assert.IsFalse(again.Targets[0].IsEnabled(NotificationKind.ServiceStop));
        }

        [TestMethod]
        public void migratedSettings_getTheDependencyFlagOnBecauseTheOldFileNeverHadOne()
        {
            var settings = JsonConvert.DeserializeObject<Notifications>(LegacyJson);

            Assert.IsTrue(settings.Targets[0].IsEnabled(NotificationKind.DependencyWait),
                "a flag the settings file does not know about keeps its default");
        }

        [TestMethod]
        public void everyKindMapsToItsOwnFlagAndToNoOther()
        {
            foreach (var kind in AllKinds)
            {
                var target = new NotificationTarget();

                Assert.IsTrue(target.IsEnabled(kind), kind + " is on by default");

                SetFlag(target, kind, false);

                foreach (var other in AllKinds)
                    Assert.AreEqual(other != kind, target.IsEnabled(other),
                        String.Format("only {0} may follow the flag of {1}", other, kind));
            }
        }

        [TestMethod]
        public void getProviders_returnsEveryTargetThatAskedForTheEvent()
        {
            var console = new Guid("aaaaaaaa-0000-0000-0000-000000000001");
            var errors = new Guid("aaaaaaaa-0000-0000-0000-000000000002");

            var settings = new Notifications()
            {
                Targets = new List<NotificationTarget>
                {
                    Target(console, NotificationKind.ConsoleOutput),
                    Target(errors, NotificationKind.ConsoleError)
                }
            };

            CollectionAssert.AreEquivalent(
                new[] { console },
                settings.GetProviders(NotificationKind.ConsoleOutput));

            CollectionAssert.AreEquivalent(
                new[] { errors },
                settings.GetProviders(NotificationKind.ConsoleError));

            Assert.AreEqual(0, settings.GetProviders(NotificationKind.ServerShutdown).Count);
        }

        [TestMethod]
        public void kindValuesAreFixedBecauseTheyArePartOfTheSettingsFile()
        {
            // A target stores its selection by flag, but the enum order is documented as stable and the
            // kind travels as a number through the dispatch. A shift here would silently move the
            // selection of every installed configuration, so both the names and the numbers are fixed.
            CollectionAssert.AreEqual(
                new[] { "ServerStart", "ServerShutdown", "ServiceStart", "ServiceStop", "ServiceRestart", "ConsoleOutput", "ConsoleError", "DependencyWait" },
                Enum.GetNames(typeof(NotificationKind)),
                "renaming or inserting a kind changes the meaning of every stored event selection");

            CollectionAssert.AreEqual(
                new[] { 0, 1, 2, 3, 4, 5, 6, 7 },
                Enum.GetValues(typeof(NotificationKind)).Cast<int>().ToArray(),
                "the numeric values are what a settings file and an older provider see");
        }


        /// <summary>A provider that asked for exactly the listed events and for nothing else.</summary>
        private static NotificationTarget Target(Guid id, params NotificationKind[] enabled)
        {
            var target = new NotificationTarget() { PluginId = id };

            foreach (var kind in AllKinds)
                SetFlag(target, kind, false);

            foreach (var kind in enabled)
                SetFlag(target, kind, true);

            return target;
        }


        private static void SetFlag(NotificationTarget target, NotificationKind kind, bool value)
        {
            switch (kind)
            {
                case NotificationKind.ServerStart:
                    target.OnServerStart = value;
                    break;
                case NotificationKind.ServerShutdown:
                    target.OnServerShutdown = value;
                    break;
                case NotificationKind.ServiceStart:
                    target.OnStart = value;
                    break;
                case NotificationKind.ServiceStop:
                    target.OnStop = value;
                    break;
                case NotificationKind.ServiceRestart:
                    target.OnRestart = value;
                    break;
                case NotificationKind.ConsoleOutput:
                    target.OnConsoleOutput = value;
                    break;
                case NotificationKind.ConsoleError:
                    target.OnConsoleError = value;
                    break;
                case NotificationKind.DependencyWait:
                    target.OnDependencyWait = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}
