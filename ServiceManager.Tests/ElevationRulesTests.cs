using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Windows;
using System;
using System.Collections.Generic;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The shield is only a hint, so the rules behind it have to be exact: the operations of the local
    /// Windows service ask for elevated rights, everything that runs through a connected instance does not.
    /// </summary>
    [TestClass]
    public class ElevationRulesTests
    {
        [TestMethod]
        public void the_operations_of_the_local_windows_service_need_elevated_rights()
        {
            List<ElevationRules.eOperation> needs = new List<ElevationRules.eOperation>();

            foreach (ElevationRules.eOperation operation in Enum.GetValues(typeof(ElevationRules.eOperation)))
            {
                if (ElevationRules.NeedsElevation(operation))
                {
                    needs.Add(operation);
                }
            }

            CollectionAssert.AreEquivalent(new List<ElevationRules.eOperation>
            {
                ElevationRules.eOperation.InstallWindowsService,
                ElevationRules.eOperation.UninstallWindowsService,
                ElevationRules.eOperation.WindowsServiceSettings,
                ElevationRules.eOperation.StartWindowsService,
                ElevationRules.eOperation.RestartWindowsService,
                ElevationRules.eOperation.StopWindowsService
            }, needs);
        }

        [TestMethod]
        public void a_managed_application_needs_no_elevated_rights_of_the_own_process()
        {
            Assert.IsFalse(ElevationRules.NeedsElevation(ElevationRules.eOperation.ManagedApplication));
        }

        [TestMethod]
        public void connecting_to_an_instance_needs_no_elevated_rights_of_the_own_process()
        {
            Assert.IsFalse(ElevationRules.NeedsElevation(ElevationRules.eOperation.ConnectToInstance));
        }

        [TestMethod]
        public void the_testing_run_needs_no_elevated_rights()
        {
            Assert.IsFalse(ElevationRules.NeedsElevation(ElevationRules.eOperation.TestingRun));
        }

        [TestMethod]
        public void an_elevated_process_needs_no_shield()
        {
            Assert.IsFalse(ElevationRules.NeedsShield(ElevationRules.eOperation.InstallWindowsService, true));
        }

        [TestMethod]
        public void a_process_without_elevated_rights_gets_the_shield_for_a_local_operation()
        {
            Assert.IsTrue(ElevationRules.NeedsShield(ElevationRules.eOperation.InstallWindowsService, false));
        }

        [TestMethod]
        public void an_operation_of_a_connected_instance_never_carries_the_shield()
        {
            Assert.IsFalse(ElevationRules.NeedsShield(ElevationRules.eOperation.ManagedApplication, false));
        }

        [TestMethod]
        public void an_operation_nobody_classified_is_reported_instead_of_guessed()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ElevationRules.NeedsElevation((ElevationRules.eOperation)9999));
        }
    }
}
