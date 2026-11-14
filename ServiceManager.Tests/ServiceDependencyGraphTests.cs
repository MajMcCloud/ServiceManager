using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data;
using System;
using System.Collections.Generic;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The graph decides the start order, the stop levels and the cycles of the settings file. It knows no
    /// process, so every decision the manager takes about dependencies can be checked here.
    /// </summary>
    [TestClass]
    public class ServiceDependencyGraphTests
    {
        [TestMethod]
        public void start_order_puts_the_dependency_first()
        {
            var database = Service("database");
            var api = Service("api", database.ID);

            var all = new List<ServiceItem> { api, database };

            var order = new ServiceDependencyGraph(all).StartOrder(all);

            Assert.IsTrue(order.IndexOf(database) < order.IndexOf(api));
        }

        [TestMethod]
        public void start_order_keeps_the_list_where_the_dependencies_allow_it()
        {
            var first = Service("first");
            var second = Service("second");
            var third = Service("third");

            var all = new List<ServiceItem> { first, second, third };

            var order = new ServiceDependencyGraph(all).StartOrder(all);

            CollectionAssert.AreEqual(all, order);
        }

        [TestMethod]
        public void a_dependency_that_is_not_part_of_the_run_holds_nothing_back()
        {
            var database = Service("database");
            var api = Service("api", database.ID);

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { database, api });

            var order = graph.StartOrder(new List<ServiceItem> { api });

            CollectionAssert.AreEqual(new List<ServiceItem> { api }, order);
        }

        [TestMethod]
        public void a_dependency_of_a_deleted_service_holds_nothing_back()
        {
            var api = Service("api", Guid.NewGuid());
            var other = Service("other");

            var all = new List<ServiceItem> { api, other };

            var order = new ServiceDependencyGraph(all).StartOrder(all);

            CollectionAssert.AreEqual(all, order);
        }

        [TestMethod]
        public void cyclic_holds_only_the_services_of_the_cycle()
        {
            var first = Service("first");
            var second = Service("second");
            var third = Service("third");
            var frontend = Service("frontend", third.ID);

            first.Dependencies = new List<Guid> { second.ID };
            second.Dependencies = new List<Guid> { third.ID };
            third.Dependencies = new List<Guid> { first.ID };

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { first, second, third, frontend });

            Assert.IsTrue(graph.HasCycle);

            CollectionAssert.AreEquivalent(new List<Guid> { first.ID, second.ID, third.ID }, graph.Cyclic);
        }

        [TestMethod]
        public void a_cycle_does_not_leave_services_unstarted()
        {
            var first = Service("first");
            var second = Service("second");
            var third = Service("third");

            first.Dependencies = new List<Guid> { second.ID };
            second.Dependencies = new List<Guid> { first.ID };

            var all = new List<ServiceItem> { first, second, third };

            var order = new ServiceDependencyGraph(all).StartOrder(all);

            CollectionAssert.AreEquivalent(all, order);
        }

        [TestMethod]
        public void stop_levels_stop_the_dependents_before_their_dependency()
        {
            var database = Service("database");
            var api = Service("api", database.ID);
            var frontend = Service("frontend", api.ID);

            var all = new List<ServiceItem> { database, api, frontend };

            var levels = new ServiceDependencyGraph(all).StopLevels(all);

            Assert.AreEqual(3, levels.Count);

            CollectionAssert.AreEqual(new List<ServiceItem> { frontend }, levels[0]);
            CollectionAssert.AreEqual(new List<ServiceItem> { api }, levels[1]);
            CollectionAssert.AreEqual(new List<ServiceItem> { database }, levels[2]);
        }

        [TestMethod]
        public void services_that_nothing_depends_on_are_stopped_together()
        {
            var database = Service("database");
            var api = Service("api", database.ID);
            var mailer = Service("mailer", database.ID);

            var all = new List<ServiceItem> { database, api, mailer };

            var levels = new ServiceDependencyGraph(all).StopLevels(all);

            Assert.AreEqual(2, levels.Count);

            CollectionAssert.AreEquivalent(new List<ServiceItem> { api, mailer }, levels[0]);
            CollectionAssert.AreEqual(new List<ServiceItem> { database }, levels[1]);
        }

        [TestMethod]
        public void services_without_any_dependency_are_stopped_in_one_level()
        {
            var first = Service("first");
            var second = Service("second");

            var all = new List<ServiceItem> { first, second };

            var levels = new ServiceDependencyGraph(all).StopLevels(all);

            Assert.AreEqual(1, levels.Count);

            CollectionAssert.AreEquivalent(all, levels[0]);
        }

        [TestMethod]
        public void stop_levels_leave_out_the_services_that_are_not_running()
        {
            var database = Service("database");
            var api = Service("api", database.ID);
            var mailer = Service("mailer", database.ID);

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { database, api, mailer });

            var levels = graph.StopLevels(new List<ServiceItem> { database, mailer });

            Assert.AreEqual(2, levels.Count);

            CollectionAssert.AreEqual(new List<ServiceItem> { mailer }, levels[0]);
            CollectionAssert.AreEqual(new List<ServiceItem> { database }, levels[1]);
        }

        [TestMethod]
        public void a_cycle_is_stopped_in_one_last_level()
        {
            var first = Service("first");
            var second = Service("second");
            var third = Service("third");

            first.Dependencies = new List<Guid> { second.ID };
            second.Dependencies = new List<Guid> { first.ID };

            var all = new List<ServiceItem> { first, second, third };

            var levels = new ServiceDependencyGraph(all).StopLevels(all);

            Assert.AreEqual(2, levels.Count);

            CollectionAssert.AreEqual(new List<ServiceItem> { third }, levels[0]);
            CollectionAssert.AreEquivalent(new List<ServiceItem> { first, second }, levels[1]);
        }

        [TestMethod]
        public void dependents_are_the_services_that_name_the_service()
        {
            var database = Service("database");
            var api = Service("api", database.ID);
            var other = Service("other");

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { database, api, other });

            CollectionAssert.AreEqual(new List<ServiceItem> { api }, graph.DependentsOf(database.ID));
        }

        [TestMethod]
        public void an_id_that_is_not_in_the_settings_file_has_no_dependents()
        {
            var database = Service("database");

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { database });

            Assert.AreEqual(0, graph.DependentsOf(Guid.NewGuid()).Count);
        }

        [TestMethod]
        public void dependents_recursively_are_transitive_and_hold_nothing_twice()
        {
            var database = Service("database");
            var api = Service("api", database.ID);
            var frontend = Service("frontend", api.ID);
            var console = Service("console", database.ID, api.ID);

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { database, api, frontend, console });

            CollectionAssert.AreEquivalent(new List<ServiceItem> { api, frontend, console }, graph.DependentsRecursively(database.ID));
        }

        [TestMethod]
        public void dependents_recursively_never_hold_the_service_itself()
        {
            var first = Service("first");
            var second = Service("second");

            first.Dependencies = new List<Guid> { second.ID };
            second.Dependencies = new List<Guid> { first.ID };

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { first, second });

            CollectionAssert.AreEqual(new List<ServiceItem> { second }, graph.DependentsRecursively(first.ID));
        }

        [TestMethod]
        public void reaches_itself_is_true_for_a_circle_over_two_services()
        {
            var first = Service("first");
            var second = Service("second");

            first.Dependencies = new List<Guid> { second.ID };
            second.Dependencies = new List<Guid> { first.ID };

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { first, second });

            Assert.IsTrue(graph.ReachesItself(first.ID));
        }

        [TestMethod]
        public void reaches_itself_is_true_through_the_services_in_between()
        {
            var first = Service("first");
            var second = Service("second");
            var third = Service("third");

            first.Dependencies = new List<Guid> { second.ID };
            second.Dependencies = new List<Guid> { third.ID };
            third.Dependencies = new List<Guid> { first.ID };

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { first, second, third });

            Assert.IsTrue(graph.ReachesItself(third.ID));
        }

        [TestMethod]
        public void reaches_itself_is_false_without_a_circle()
        {
            var database = Service("database");
            var api = Service("api", database.ID);
            var frontend = Service("frontend", api.ID);

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { database, api, frontend });

            Assert.IsFalse(graph.ReachesItself(frontend.ID));
        }

        [TestMethod]
        public void a_service_that_names_itself_is_not_a_circle()
        {
            var database = Service("database");

            database.Dependencies = new List<Guid> { database.ID, Guid.NewGuid() };

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { database });

            Assert.IsFalse(graph.HasCycle);
            Assert.IsFalse(graph.ReachesItself(database.ID));
        }

        [TestMethod]
        public void a_service_that_appears_twice_is_held_once()
        {
            var database = Service("database");

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { database, database, null });

            Assert.AreEqual(1, graph.Services.Count);
        }

        [TestMethod]
        public void roots_are_the_services_without_a_dependency()
        {
            var database = Service("database");
            var api = Service("api", database.ID);

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { api, database });

            CollectionAssert.AreEqual(new List<ServiceItem> { database }, graph.Roots());
        }

        [TestMethod]
        public void a_service_that_waits_for_a_deleted_service_is_a_root()
        {
            var api = Service("api", Guid.NewGuid());

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { api });

            CollectionAssert.AreEqual(new List<ServiceItem> { api }, graph.Roots());
        }

        [TestMethod]
        public void dependencies_of_name_only_the_services_of_the_file()
        {
            var database = Service("database");
            var gone = Guid.NewGuid();
            var api = Service("api", database.ID, gone, Guid.Empty);

            var graph = new ServiceDependencyGraph(new List<ServiceItem> { api, database });

            CollectionAssert.AreEqual(new List<Guid> { database.ID }, graph.DependenciesOf(api));
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
