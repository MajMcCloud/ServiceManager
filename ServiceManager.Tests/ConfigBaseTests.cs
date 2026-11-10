using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ServiceManager.Tests
{
    /// <summary>
    /// Behaviour of the config base class that the whole solution reads and writes through:
    /// what happens when the file or a directory is missing, what gets persisted, and which
    /// static settings leak between calls.
    /// </summary>
    [TestClass]
    public class ConfigBaseTests
    {
        public class ProbeConfig : ConfigBase.ConfigBase<ProbeConfig>
        {
            public String Name { get; set; }

            public int Count { get; set; }

            public List<String> Items { get; set; }

            public String Unused { get; set; }

            public override void loadDefaultValues()
            {
                Name = "default";
                Count = 7;
                Items = new List<String>();
            }
        }

        private String directory;

        private String file;

        [TestInitialize]
        public void Setup()
        {
            directory = Path.Combine(Path.GetTempPath(), "servicemanager-tests", Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(directory);

            file = Path.Combine(directory, "default.cfg");
        }

        [TestCleanup]
        public void Teardown()
        {
            // The four statics are shared by every instance of the same closed generic type.
            ProbeConfig.BeautifyOnSave = true;
            ProbeConfig.AutoReloadOnChange = false;
            ProbeConfig.Filename = "default.cfg";
            ProbeConfig.ConfigPath = AppContext.BaseDirectory + "config\\";

            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        [TestMethod]
        public void load_missingFile_returnsDefaultsAndWritesThemToDisk()
        {
            var cfg = ProbeConfig.load(file);

            Assert.IsNotNull(cfg, "load() has to fall back to the defaults instead of failing");
            Assert.AreEqual("default", cfg.Name);
            Assert.AreEqual(7, cfg.Count);
            Assert.AreEqual(0, cfg.Items.Count);

            Assert.IsTrue(File.Exists(file), "the defaults it just created have to be persisted");

            var written = JObject.Parse(File.ReadAllText(file));

            Assert.AreEqual("default", (String)written["Name"]);
            Assert.AreEqual(7, (int)written["Count"]);
        }

        [TestMethod]
        public void load_missingParentDirectory_createsIt()
        {
            var cfg = ProbeConfig.load(Path.Combine(directory, "config", "default.cfg"));

            Assert.IsNotNull(cfg);
            Assert.IsTrue(Directory.Exists(Path.Combine(directory, "config")), "load() has to create the config folder");
            Assert.IsTrue(File.Exists(Path.Combine(directory, "config", "default.cfg")));
        }

        [TestMethod]
        public void load_twoMissingDirectoryLevels_createsTheWholeChainAndPersists()
        {
            var nested = Path.Combine(directory, "a", "b", "default.cfg");

            var cfg = ProbeConfig.load(nested);

            Assert.IsNotNull(cfg);
            Assert.AreEqual("default", cfg.Name);

            // The catch only checks the direct parent, but CreateDirectory builds every level,
            // so the defaults do reach the disk here.
            Assert.IsTrue(Directory.Exists(Path.Combine(directory, "a", "b")));
            Assert.IsTrue(File.Exists(nested), "the defaults have to survive the missing directories");

            var again = ProbeConfig.load(nested);

            Assert.AreEqual("default", again.Name);
        }

        [TestMethod]
        public void save_then_load_keepsEveryValue()
        {
            var cfg = ProbeConfig.load(file);

            cfg.Name = "changed";
            cfg.Count = 42;
            cfg.Items.Add("first");
            cfg.Items.Add("second");

            cfg.save(file);

            var again = ProbeConfig.load(file);

            Assert.AreEqual("changed", again.Name);
            Assert.AreEqual(42, again.Count);
            Assert.AreEqual(2, again.Items.Count);
            Assert.AreEqual("first", again.Items[0]);
            Assert.AreEqual("second", again.Items[1]);
        }

        [TestMethod]
        public void save_writesIndentedJson_byDefault()
        {
            new ProbeConfig().save(file);

            var text = File.ReadAllText(file);

            Assert.IsTrue(text.Contains(Environment.NewLine), "BeautifyOnSave is true by default");
            Assert.IsTrue(text.TrimStart().StartsWith("{"));
        }

        [TestMethod]
        public void save_withBeautifyOnSaveFalse_writesSingleLine()
        {
            ProbeConfig.BeautifyOnSave = false;

            new ProbeConfig().save(file);

            var text = File.ReadAllText(file);

            Assert.IsFalse(text.Contains("\n"), "no line breaks expected, got: " + text);
            Assert.IsTrue(text.StartsWith("{\"Name\""));
        }

        [TestMethod]
        public void save_includesNullProperties()
        {
            var cfg = new ProbeConfig();

            cfg.Items = null;
            cfg.Unused = null;

            cfg.save(file);

            var written = JObject.Parse(File.ReadAllText(file));

            Assert.AreEqual(JTokenType.Null, written["Items"].Type, "NullValueHandling.Include is deliberate: a missing property and a null one are the same on disk");
            Assert.AreEqual(JTokenType.Null, written["Unused"].Type);
        }

        [TestMethod]
        public void save_checked_persists_the_configuration()
        {
            var cfg = new ProbeConfig { Name = "checked save" };

            cfg.saveChecked(file);

            Assert.AreEqual("checked save", (String)JObject.Parse(File.ReadAllText(file))["Name"]);
        }

        [TestMethod]
        public void save_checked_reports_a_missing_directory()
        {
            var cfg = new ProbeConfig();
            String missing = Path.Combine(directory, "missing", "default.cfg");

            Assert.ThrowsExactly<DirectoryNotFoundException>(() => cfg.saveChecked(missing));
        }

        [TestMethod]
        public void loadFromFile_overridesTheStaticFilename()
        {
            ProbeConfig.ConfigPath = directory + "\\";

            var cfg = ProbeConfig.loadFromFile("probe.cfg");

            Assert.IsNotNull(cfg);
            Assert.AreEqual("probe.cfg", ProbeConfig.Filename, "loadFromFile() writes the static Filename, every later call uses it");
            Assert.IsTrue(File.Exists(Path.Combine(directory, "probe.cfg")));
        }

        [TestMethod]
        public void load_brokenFile_returnsNullInDebugAndThrowsInRelease()
        {
            File.WriteAllText(file, "{ this is not json");

            try
            {
                var cfg = ProbeConfig.load(file);

#if DEBUG
                Assert.IsNull(cfg, "in DEBUG the exception is swallowed and null is returned");
#else
                Assert.Fail("in Release load() has to rethrow instead of returning null");
#endif
            }
            catch (JsonException ex)
            {
#if DEBUG
                Assert.Fail("DEBUG must not throw here: " + ex.Message);
#else
                Assert.IsInstanceOfType(ex, typeof(JsonException));
#endif
            }
        }

        [TestMethod]
        public void load_emptyFile_returnsNullInDebugAndThrowsInRelease()
        {
            File.WriteAllText(file, "");

            try
            {
                var cfg = ProbeConfig.load(file);

                // JsonConvert hands back null for an empty document, so the constructor is never
                // reached and cfg.Init() trips over it. Worth knowing before anyone trusts a load.
#if DEBUG
                Assert.IsNull(cfg, "in DEBUG the NullReferenceException is swallowed and null is returned");
#else
                Assert.Fail("in Release load() has to rethrow instead of returning null");
#endif
            }
            catch (NullReferenceException ex)
            {
#if DEBUG
                Assert.Fail("DEBUG must not throw here: " + ex.Message);
#else
                Assert.IsInstanceOfType(ex, typeof(NullReferenceException));
#endif
            }
        }

        [TestMethod]
        public void load_partialFile_keepsDefaultsForAbsentProperties()
        {
            File.WriteAllText(file, "{\"Count\": 3}");

            var cfg = ProbeConfig.load(file);

            Assert.AreEqual(3, cfg.Count);
            Assert.AreEqual("default", cfg.Name, "the constructor sets the defaults before deserialization overwrites them");
        }

        [TestMethod]
        public void save_doesNotThrowWhenTheTargetIsUnusable()
        {
            // A directory as target makes File.WriteAllText fail; save() catches everything.
            new ProbeConfig().save(directory);

            Assert.IsTrue(Directory.Exists(directory));
        }
    }
}
