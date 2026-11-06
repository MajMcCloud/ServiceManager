using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using ExtensionLoader = ServiceManager.Extensions.Extensions;

namespace ServiceManager.Tests
{
    /// <summary>
    /// Entry points of the loader that hold without any extension on disk. The cases with real
    /// extension assemblies need compiled fixtures and follow separately.
    /// </summary>
    [TestClass]
    public class ExtensionLoaderTests
    {
        [TestMethod]
        public void extensionFolder_isTheExtensionsDirectoryNextToTheExecutable()
        {
            Assert.AreEqual(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extensions"),
                ExtensionLoader.ExtensionFolder,
                "the service is installed as one folder and loads its extensions from <BaseDirectory>\\extensions");
        }

        [TestMethod]
        public void missingExtensionFolder_returnsAnEmptyListInsteadOfThrowing()
        {
            if (Directory.Exists(ExtensionLoader.ExtensionFolder))
                Assert.Inconclusive("an extensions folder exists in this output directory");

            Assert.AreEqual(0, ExtensionLoader.GetNotificationExtensions().Count);
        }

        [TestMethod]
        public void getExtension_withAnUnknownId_returnsNull()
        {
            Assert.IsNull(ExtensionLoader.GetExtension(Guid.Empty));
            Assert.IsNull(ExtensionLoader.GetExtension(null), "no id means no extension, not the first one");
        }

        [TestMethod]
        public void repeatedCalls_returnTheSameInstances()
        {
            var first = ExtensionLoader.GetNotificationExtensions();
            var second = ExtensionLoader.GetNotificationExtensions();

            Assert.AreEqual(first.Count, second.Count);

            foreach (var pair in first.Zip(second, (a, b) => new { a, b }))
                Assert.IsTrue(ReferenceEquals(pair.a, pair.b), "the cached instance has to be reused, its constructor writes a settings file");

            ExtensionLoader.Reload();

            var third = ExtensionLoader.GetNotificationExtensions();

            Assert.AreEqual(first.Count, third.Count);
        }
    }
}
