using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.UI.Config;

namespace ServiceManager.Tests
{
    [TestClass]
    public class UiConfigTests
    {
        [TestMethod]
        public void the_file_of_this_machine_sits_with_the_person_and_not_with_an_instance()
        {
            String folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            Assert.IsTrue(UiConfig.FilePath.StartsWith(folder, StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual("ui.cfg", System.IO.Path.GetFileName(UiConfig.FilePath));
        }

        [TestMethod]
        public void a_choice_list_that_was_lost_still_carries_a_list()
        {
            UiConfig config = new UiConfig();

            config.Columns = null;

            Assert.IsNotNull(config.Columns);
            Assert.AreEqual(0, config.Columns.Count);
        }
    }
}
