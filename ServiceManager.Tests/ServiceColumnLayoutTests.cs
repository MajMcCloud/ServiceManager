using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.UI.Columns;

namespace ServiceManager.Tests
{
    [TestClass]
    public class ServiceColumnLayoutTests
    {
        [TestMethod]
        public void the_activity_column_sits_at_the_front()
        {
            ServiceColumnLayout layout = new ServiceColumnLayout();

            Assert.AreEqual(0, layout.IndexOf(eColumn.Activity));
        }

        [TestMethod]
        public void a_layout_without_a_choice_shows_the_default_columns()
        {
            ServiceColumnLayout layout = new ServiceColumnLayout();

            Assert.IsTrue(layout.IndexOf(eColumn.Memory) >= 0);
        }

        [TestMethod]
        public void a_default_that_is_off_takes_no_place()
        {
            ServiceColumnLayout layout = new ServiceColumnLayout();

            Assert.AreEqual(-1, layout.IndexOf(eColumn.Uptime));
        }

        [TestMethod]
        public void a_choice_overrides_the_default()
        {
            Dictionary<String, Boolean> choices = new Dictionary<String, Boolean>();
            choices[eColumn.Uptime.ToString()] = true;
            choices[eColumn.Memory.ToString()] = false;

            ServiceColumnLayout layout = new ServiceColumnLayout(choices);

            Assert.IsTrue(layout.IndexOf(eColumn.Uptime) >= 0);
            Assert.AreEqual(-1, layout.IndexOf(eColumn.Memory));
        }

        [TestMethod]
        public void the_columns_keep_the_order_of_the_definitions()
        {
            Dictionary<String, Boolean> choices = new Dictionary<String, Boolean>();
            choices[eColumn.Uptime.ToString()] = true;
            choices[eColumn.Memory.ToString()] = true;
            choices[eColumn.Threads.ToString()] = true;

            ServiceColumnLayout layout = new ServiceColumnLayout(choices);

            Assert.IsTrue(layout.IndexOf(eColumn.Status) < layout.IndexOf(eColumn.Uptime));
            Assert.IsTrue(layout.IndexOf(eColumn.Uptime) < layout.IndexOf(eColumn.Memory));
            Assert.IsTrue(layout.IndexOf(eColumn.Memory) < layout.IndexOf(eColumn.Threads));
        }

        [TestMethod]
        public void a_column_that_is_never_offered_keeps_its_place()
        {
            Dictionary<String, Boolean> choices = new Dictionary<String, Boolean>();
            choices[eColumn.Activity.ToString()] = false;

            ServiceColumnLayout layout = new ServiceColumnLayout(choices);

            Assert.AreEqual(0, layout.IndexOf(eColumn.Activity));
        }

        [TestMethod]
        public void the_shown_columns_are_the_headings_of_the_list()
        {
            ServiceColumnLayout layout = new ServiceColumnLayout();

            for (int i = 0; i < layout.Shown.Count; i++)
            {
                Assert.AreEqual(i, layout.IndexOf(layout.Shown[i].Column));
            }
        }

        [TestMethod]
        public void every_column_has_its_own_definition()
        {
            HashSet<eColumn> seen = new HashSet<eColumn>();

            foreach (ServiceColumn column in ServiceColumnLayout.All)
            {
                Assert.IsTrue(seen.Add(column.Column), "Column " + column.Column + " is defined twice.");
                Assert.IsFalse(String.IsNullOrEmpty(column.Title), "Column " + column.Column + " has no heading.");
            }

            Assert.AreEqual(Enum.GetValues(typeof(eColumn)).Length, seen.Count);
        }
    }
}
