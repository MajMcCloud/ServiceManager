using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The tree is the settings file as a person has to read it: the services nothing waits for on top, the
    /// services that wait for them below. It decides nothing about the start order, so the test checks what
    /// is shown and what is marked.
    /// </summary>
    [TestClass]
    public class DependencyTreeTests
    {
        [TestMethod]
        public void an_empty_settings_file_builds_no_tree()
        {
            var roots = new DependencyTree(new List<ServiceItem>()).Build();

            Assert.AreEqual(0, roots.Count);
        }

        [TestMethod]
        public void the_service_that_waits_for_another_hangs_below_it()
        {
            var database = Service("database");
            var api = Service("api", database.ID);

            var roots = new DependencyTree(new List<ServiceItem> { api, database }).Build();

            Assert.AreEqual(1, roots.Count);
            Assert.AreEqual("database", roots[0].Title);

            CollectionAssert.AreEqual(new List<String> { "api" }, Children(roots[0]));
        }

        [TestMethod]
        public void a_service_that_waits_for_a_deleted_service_sits_on_top()
        {
            var api = Service("api", Guid.NewGuid());

            var roots = new DependencyTree(new List<ServiceItem> { api }).Build();

            Assert.AreEqual(1, roots.Count);
            Assert.AreEqual("api", roots[0].Title);
            Assert.AreEqual(0, roots[0].Children.Count);
        }

        [TestMethod]
        public void a_service_that_waits_for_itself_is_no_branch()
        {
            var api = Service("api");
            api.Dependencies = new List<Guid> { api.ID };

            var roots = new DependencyTree(new List<ServiceItem> { api }).Build();

            Assert.AreEqual(1, roots.Count);
            Assert.AreEqual(0, roots[0].Children.Count);
            Assert.IsFalse(roots[0].InCycle);
        }

        [TestMethod]
        public void a_disabled_service_stays_in_the_tree_and_is_marked()
        {
            var database = Service("database");
            database.Enabled = false;

            var api = Service("api", database.ID);

            var roots = new DependencyTree(new List<ServiceItem> { api, database }).Build();

            Assert.AreEqual(1, roots.Count);
            Assert.IsFalse(roots[0].Enabled);
            Assert.AreEqual(1, roots[0].Children.Count);
            Assert.IsTrue(roots[0].Children[0].Enabled);
        }

        [TestMethod]
        public void the_windows_services_a_service_waits_for_hang_as_leaves()
        {
            var api = Service("api");
            api.WindowsDependencies = new List<String> { "Tcpip", "Dhcp" };

            var roots = new DependencyTree(new List<ServiceItem> { api }).Build();

            CollectionAssert.AreEqual(new List<String> { "Dhcp", "Tcpip" }, Children(roots[0]));

            Assert.IsTrue(roots[0].Children.All(a => a.Kind == DependencyTreeNode.eNodeKind.WindowsService));
        }

        [TestMethod]
        public void a_windows_service_name_hangs_below_every_service_that_waits_for_it()
        {
            var database = Service("database");
            database.WindowsDependencies = new List<String> { "Dhcp" };

            var api = Service("api", database.ID);
            api.WindowsDependencies = new List<String> { "Dhcp" };

            var roots = new DependencyTree(new List<ServiceItem> { api, database }).Build();

            CollectionAssert.AreEqual(new List<String> { "api", "Dhcp" }, Children(roots[0]));
        }

        [TestMethod]
        public void the_services_below_a_service_are_ordered_by_title()
        {
            var database = Service("database");

            var api = Service("api", database.ID);
            var backend = Service("backend", database.ID);

            var roots = new DependencyTree(new List<ServiceItem> { backend, api, database }).Build();

            CollectionAssert.AreEqual(new List<String> { "api", "backend" }, Children(roots[0]));
        }

        [TestMethod]
        public void the_services_on_top_are_ordered_by_title()
        {
            var second = Service("second");
            var first = Service("first");

            var roots = new DependencyTree(new List<ServiceItem> { second, first }).Build();

            CollectionAssert.AreEqual(new List<String> { "first", "second" }, roots.Select(a => a.Title).ToList());
        }

        [TestMethod]
        public void a_circle_is_marked_and_closes_the_branch()
        {
            var first = Service("first");
            var second = Service("second");

            first.Dependencies = new List<Guid> { second.ID };
            second.Dependencies = new List<Guid> { first.ID };

            var roots = new DependencyTree(new List<ServiceItem> { first, second }).Build();

            Assert.AreEqual(1, roots.Count);
            Assert.AreEqual("first", roots[0].Title);
            Assert.IsTrue(roots[0].InCycle);

            Assert.AreEqual("second", roots[0].Children[0].Title);
            Assert.IsTrue(roots[0].Children[0].InCycle);

            var closed = roots[0].Children[0].Children[0];

            Assert.AreEqual("first", closed.Title);
            Assert.IsTrue(closed.IsRepeat);
            Assert.AreEqual(0, closed.Children.Count);
        }

        [TestMethod]
        public void a_service_below_a_circle_is_not_marked_as_cyclic()
        {
            var first = Service("first");
            var second = Service("second");
            var frontend = Service("frontend", second.ID);

            first.Dependencies = new List<Guid> { second.ID };
            second.Dependencies = new List<Guid> { first.ID };

            var roots = new DependencyTree(new List<ServiceItem> { first, second, frontend }).Build();

            Assert.AreEqual(1, roots.Count);

            var below = roots[0].Children[0].Children.First(a => a.Title == "frontend");

            Assert.IsFalse(below.InCycle);
            Assert.IsFalse(below.IsRepeat);
        }

        [TestMethod]
        public void a_circle_without_a_service_on_top_is_still_shown()
        {
            var first = Service("first");
            var second = Service("second");
            var other = Service("other");

            first.Dependencies = new List<Guid> { second.ID };
            second.Dependencies = new List<Guid> { first.ID };

            var roots = new DependencyTree(new List<ServiceItem> { first, second, other }).Build();

            CollectionAssert.AreEqual(new List<String> { "other", "first" }, roots.Select(a => a.Title).ToList());

            Assert.IsTrue(roots[1].InCycle);
            Assert.AreEqual("second", roots[1].Children[0].Title);
        }

        [TestMethod]
        public void a_service_that_waits_for_two_services_hangs_below_both()
        {
            var left = Service("left");
            var right = Service("right");
            var api = Service("api", left.ID, right.ID);

            var roots = new DependencyTree(new List<ServiceItem> { api, left, right }).Build();

            CollectionAssert.AreEqual(new List<String> { "left", "right" }, roots.Select(a => a.Title).ToList());

            foreach (var root in roots)
            {
                CollectionAssert.AreEqual(new List<String> { "api" }, Children(root));
            }
        }

        [TestMethod]
        public void the_servicemanager_hangs_above_the_services_of_the_file()
        {
            var database = Service("database");
            var api = Service("api", database.ID);

            var roots = new DependencyTree(new List<ServiceItem> { api, database }, "ServiceManager", null).Build();

            Assert.AreEqual(1, roots.Count);
            Assert.AreEqual("ServiceManager", roots[0].Title);
            Assert.AreEqual(DependencyTreeNode.eNodeKind.Host, roots[0].Kind);

            CollectionAssert.AreEqual(new List<String> { "database" }, Children(roots[0]));
        }

        [TestMethod]
        public void the_windows_services_of_the_servicemanager_hang_below_it()
        {
            var api = Service("api");

            var roots = new DependencyTree(new List<ServiceItem> { api }, "ServiceManager", new List<String> { "Tcpip", "Dhcp", " " }).Build();

            CollectionAssert.AreEqual(new List<String> { "api", "Dhcp", "Tcpip" }, Children(roots[0]));
        }

        [TestMethod]
        public void a_blank_host_name_leaves_the_services_on_top()
        {
            var api = Service("api");

            var roots = new DependencyTree(new List<ServiceItem> { api }, "  ", new List<String> { "Tcpip" }).Build();

            CollectionAssert.AreEqual(new List<String> { "api" }, roots.Select(a => a.Title).ToList());
        }

        [TestMethod]
        public void a_circle_keeps_its_shape_below_the_servicemanager()
        {
            var first = Service("first");
            var second = Service("second");

            first.Dependencies = new List<Guid> { second.ID };
            second.Dependencies = new List<Guid> { first.ID };

            var roots = new DependencyTree(new List<ServiceItem> { first, second }, "ServiceManager", new String[0]).Build();

            Assert.AreEqual("first", roots[0].Children[0].Title);
            Assert.IsTrue(roots[0].Children[0].InCycle);
        }

        [TestMethod]
        public void the_service_node_carries_the_id_of_the_settings_file()
        {
            var database = Service("database");

            var roots = new DependencyTree(new List<ServiceItem> { database }, "ServiceManager", new List<String> { "Tcpip" }).Build();

            Assert.AreEqual(database.ID, roots[0].Children[0].ID);
        }

        private static List<String> Children(DependencyTreeNode node)
        {
            return node.Children.Select(a => a.Title).ToList();
        }

        private static ServiceItem Service(String title, params Guid[] dependencies)
        {
            var item = new ServiceItem();

            item.ID = Guid.NewGuid();
            item.Title = title;
            item.Dependencies = new List<Guid>(dependencies);

            return item;
        }
    }
}
