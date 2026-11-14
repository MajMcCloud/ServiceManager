using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Windows;
using System.Security.Principal;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The elevation of the own process is read from the access token rather than from the group
    /// membership, so the read has to agree with what the framework reports for the same process.
    /// </summary>
    [TestClass]
    public class WindowsElevationTests
    {
        [TestMethod]
        public void the_own_process_reads_as_the_windows_principal_reports_it()
        {
            var elevated = new WindowsElevation().IsElevated();

            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var expected = principal.IsInRole(WindowsBuiltInRole.Administrator);

            Assert.AreEqual(expected, elevated);
        }

        [TestMethod]
        public void the_own_process_reads_the_same_way_twice()
        {
            var elevation = new WindowsElevation();

            var first = elevation.IsElevated();
            var second = elevation.IsElevated();

            Assert.AreEqual(first, second);
        }
    }
}
