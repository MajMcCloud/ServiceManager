using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using ExtensionLoader = ServiceManager.Extensions.Extensions;
using ExtensionInfo = ServiceManager.Extensions.Extension;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The loader against real, compiled extension assemblies: what it accepts, what it rejects and
    /// what a rejected assembly must not do to the assemblies behind it.
    /// </summary>
    [TestClass]
    public class ExtensionLoaderFixtureTests
    {
        [AssemblyInitialize]
        public static void RemoveFixturesOfThePreviousRun(TestContext context)
        {
            // The CLR keeps every assembly it loaded, so a fixture cannot be deleted while the test
            // host lives. What a previous run left behind is unlocked by now though.
            ExtensionFixture.WipeExtensionFolder();
        }

        private static ExtensionInfo Find(Guid id)
        {
            return ExtensionLoader.GetNotificationExtensions().FirstOrDefault(a => a.Id == id);
        }

        private static ExtensionInfo FindUnder(String folder)
        {
            return ExtensionLoader.GetNotificationExtensions()
                .FirstOrDefault(a => a.Path.StartsWith(folder, StringComparison.OrdinalIgnoreCase));
        }


        [TestMethod]
        public void validExtension_isFoundWithIdNameAndPath()
        {
            var fixture = ExtensionFixture.Compile("valid", (id, name) => ExtensionFixture.ValidSource("ValidOne", id, name));

            var found = Find(fixture.Id);

            Assert.IsNotNull(found, "a public extension with a parameterless constructor has to be found");
            Assert.AreEqual(fixture.Name, found.Name);
            Assert.AreEqual(fixture.AssemblyPath, found.Path);
            Assert.IsNotNull(found.Notification);
            Assert.AreEqual(fixture.Id, found.Notification.Id);
            Assert.AreEqual(1, fixture.ConstructorCalls);
        }

        [TestMethod]
        public void extensionInANestedFolder_isFound()
        {
            var fixture = ExtensionFixture.Compile("deep", (id, name) => ExtensionFixture.ValidSource("DeepOne", id, name));

            var deeper = Path.Combine(fixture.Folder, "deeper", "deepest");

            Directory.CreateDirectory(deeper);

            File.Move(fixture.AssemblyPath, Path.Combine(deeper, Path.GetFileName(fixture.AssemblyPath)));

            Assert.IsNotNull(Find(fixture.Id), "the scan runs with SearchOption.AllDirectories");
        }

        [TestMethod]
        public void throwingConstructor_isSkippedButTheRestOfTheScanSurvives()
        {
            var broken = ExtensionFixture.Compile("abroken", (id, name) => ExtensionFixture.ThrowingSource("BrokenOne", id, name));
            var healthy = ExtensionFixture.Compile("bhealthy", (id, name) => ExtensionFixture.ValidSource("HealthyOne", id, name));

            // One scan for everything: every GetNotificationExtensions() call instantiates the broken
            // fixture again, so the assertions below have to read one single pass.
            var all = ExtensionLoader.GetNotificationExtensions();

            Assert.IsTrue(File.Exists(broken.CtorLog), "the constructor has to have run, otherwise nothing is proven");
            Assert.IsNull(all.FirstOrDefault(a => a.Id == broken.Id), "a failing constructor must not register a half-built extension");
            Assert.IsNotNull(all.FirstOrDefault(a => a.Id == healthy.Id), "the scan continues after a broken assembly, it does not stop there");
            Assert.AreEqual(1, broken.ConstructorCalls, "the constructor runs once per scan");
        }

        [TestMethod]
        public void brokenExtension_isBuiltAgainOnEveryScan()
        {
            var broken = ExtensionFixture.Compile("noretry", (id, name) => ExtensionFixture.ThrowingSource("RetryOne", id, name));

            ExtensionLoader.GetNotificationExtensions();

            Assert.AreEqual(1, broken.ConstructorCalls);

            ExtensionLoader.GetNotificationExtensions();

            Assert.AreEqual(2, broken.ConstructorCalls,
                "a failing extension is never remembered, so its constructor runs again on every call");
        }

        [TestMethod]
        public void extensionWithoutParameterlessConstructor_isSkipped()
        {
            var fixture = ExtensionFixture.Compile("noctor", (id, name) => ExtensionFixture.NoParameterlessCtorSource("NoCtorOne", id, name));

            Assert.IsNull(Find(fixture.Id));
            Assert.AreEqual(0, fixture.ConstructorCalls, "the constructor is checked before anything is instantiated");
        }

        [TestMethod]
        public void abstractExtension_isSkipped()
        {
            var fixture = ExtensionFixture.Compile("abstract", (id, name) => ExtensionFixture.AbstractSource("AbstractOne", id, name));

            Assert.IsNull(Find(fixture.Id), "an abstract class can never be instantiated");
        }

        [TestMethod]
        public void nonPublicExtension_isSkipped()
        {
            var fixture = ExtensionFixture.Compile("internal", (id, name) => ExtensionFixture.NonPublicSource("HiddenOne", id, name));

            Assert.IsNull(Find(fixture.Id));
            Assert.AreEqual(0, fixture.ConstructorCalls);
        }

        [TestMethod]
        public void assemblyWithoutAnyExtension_isSkippedAndTheScanContinues()
        {
            var plain = ExtensionFixture.Compile("plain", (id, name) => ExtensionFixture.PlainLibrarySource("PlainLibrary"));
            var valid = ExtensionFixture.Compile("plainz", (id, name) => ExtensionFixture.ValidSource("AfterPlainOne", id, name));

            Assert.IsNull(FindUnder(plain.Folder), "a shipped library is not an extension");
            Assert.IsNotNull(Find(valid.Id));
        }

        [TestMethod]
        public void nativeDll_isSkippedAndTheScanContinues()
        {
            var system = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "opengl32.dll");

            if (!File.Exists(system))
                Assert.Inconclusive("no native dll available to probe with");

            var folder = Path.Combine(ExtensionFixture.RootFolder, "native_" + Guid.NewGuid().ToString("N").Substring(0, 8));

            Directory.CreateDirectory(folder);
            File.Copy(system, Path.Combine(folder, "opengl32.dll"));

            var valid = ExtensionFixture.Compile("nativez", (id, name) => ExtensionFixture.ValidSource("AfterNativeOne", id, name));

            Assert.IsNull(FindUnder(folder), "a native dll is not a managed assembly and has to be ignored");
            Assert.IsNotNull(Find(valid.Id), "BadImageFormatException must not end the scan");
        }

        [TestMethod]
        public void twoExtensionsWithTheSameId_areRegisteredOnce()
        {
            var id = Guid.NewGuid();

            var first = ExtensionFixture.Compile("dupa", (i, name) => ExtensionFixture.ValidSource("DupOne", i, name), id);
            var second = ExtensionFixture.Compile("dupb", (i, name) => ExtensionFixture.ValidSource("DupTwo", i, name), id);

            var registered = ExtensionLoader.GetNotificationExtensions().Where(a => a.Id == id).ToList();

            Assert.AreEqual(1, registered.Count, "the second assembly with an already registered Id has to be skipped");

            var winner = registered[0].Path;

            Assert.IsTrue(
                winner.StartsWith(first.Folder, StringComparison.OrdinalIgnoreCase) ||
                winner.StartsWith(second.Folder, StringComparison.OrdinalIgnoreCase),
                "the entry has to come from one of the two fixtures, not from a leftover: " + winner);
        }

        [TestMethod]
        public void repeatedCalls_reuseTheInstanceAndRunTheConstructorOnce()
        {
            var fixture = ExtensionFixture.Compile("cache", (id, name) => ExtensionFixture.ValidSource("CacheOne", id, name));

            var first = Find(fixture.Id);
            var second = Find(fixture.Id);
            var third = Find(fixture.Id);

            Assert.IsNotNull(first);
            Assert.IsTrue(ReferenceEquals(first, second), "the same Extension entry has to come back");
            Assert.IsTrue(ReferenceEquals(first.Notification, third.Notification), "and the same extension instance behind it");

            Assert.AreEqual(1, fixture.ConstructorCalls,
                "the constructor reads and writes a settings file, running it per call costs file IO on every notification");
        }

        [TestMethod]
        public void reload_buildsANewInstanceSoTheSavedSettingsAreRead()
        {
            var fixture = ExtensionFixture.Compile("reload", (id, name) => ExtensionFixture.ValidSource("ReloadOne", id, name));

            var before = Find(fixture.Id).Notification;

            Assert.AreEqual(1, fixture.ConstructorCalls);

            ExtensionLoader.Reload();

            var after = Find(fixture.Id).Notification;

            Assert.IsNotNull(after);
            Assert.IsFalse(ReferenceEquals(before, after), "Reload has to drop the cached instance");
            Assert.AreEqual(2, fixture.ConstructorCalls, "the new instance reads the settings file again");
        }

        [TestMethod]
        public void extensionAddedAtRuntime_isPickedUpByTheNextCall()
        {
            var first = ExtensionFixture.Compile("late1", (id, name) => ExtensionFixture.ValidSource("LateOne", id, name));

            Assert.IsNotNull(Find(first.Id));

            var before = ExtensionLoader.GetNotificationExtensions().Count;

            var second = ExtensionFixture.Compile("late2", (id, name) => ExtensionFixture.ValidSource("LateTwo", id, name));

            var after = ExtensionLoader.GetNotificationExtensions().Count;

            Assert.AreEqual(before + 1, after);
            Assert.IsNotNull(Find(second.Id), "dropping a dll into extensions takes effect without a restart");
        }

        [TestMethod]
        public void copyOfTheHostAssemblyNextToAnExtension_isNotRegisteredAsAnExtension()
        {
            var fixture = ExtensionFixture.Compile("copytrap", (id, name) => ExtensionFixture.ValidSource("CopyTrapOne", id, name), copyHostExtensionsAssembly: true);

            var copy = Path.Combine(fixture.Folder, "ServiceManager.Extensions.dll");

            Assert.IsTrue(File.Exists(copy), "this mirrors the shipped TelegramBotNotifier folder");

            var found = Find(fixture.Id);

            // Same simple name, same version, same path on disk: the runtime binds the fixture to the
            // host assembly that is already loaded instead of to the copy next to it. The copy only
            // becomes a problem when its identity differs, which is what the shipped notifier had.
            Assert.IsNotNull(found, "a copy of the host assembly does not break the interface binding");
            Assert.AreEqual(fixture.AssemblyPath, found.Path);

            var under = ExtensionLoader.GetNotificationExtensions().Count(a => a.Path.StartsWith(fixture.Folder, StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(1, under, "the copy carries no extension of its own and must not be registered separately");
        }

        [TestMethod]
        public void cacheDropsAnEntryWhoseFileIsGone()
        {
            // The CLR locks a loaded assembly, so a real extension can never be removed from a live
            // process. The eviction itself is verified by seeding the cache with a path that is gone.
            var cache = Cache();

            Directory.CreateDirectory(ExtensionFixture.RootFolder);

            var gone = Path.Combine(ExtensionFixture.RootFolder, "gone_" + Guid.NewGuid().ToString("N") + "\\Gone.dll");

            cache[gone] = new ExtensionInfo { Id = Guid.NewGuid(), Name = "ghost", Path = gone };

            Assert.IsTrue(cache.Contains(gone), "the cache is a path keyed dictionary");

            ExtensionLoader.GetNotificationExtensions();

            Assert.IsFalse(cache.Contains(gone), "an extension that is no longer on disk must leave the cache");
        }

        private static IDictionary Cache()
        {
            var field = typeof(ExtensionLoader).GetField("m_Cache", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(field, "the loader no longer has the cache field this test relies on");

            return (IDictionary)field.GetValue(null);
        }
    }
}
