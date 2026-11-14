using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceManager.Base.Data
{
    /// <summary>
    /// The order the dependencies between managed services imply. The graph knows no process and no
    /// settings file: it is handed the services and answers which one starts first, which ones depend on
    /// a given service, and which services sit in a cycle. Everything the manager decides about the
    /// dependencies is decided here so that it can be tested without a single process.
    /// </summary>
    public class ServiceDependencyGraph
    {
        private const int Unvisited = 0;
        private const int OnStack = 1;
        private const int Done = 2;

        /// <summary>The services of the settings file, without duplicates.</summary>
        private readonly List<ServiceItem> m_Services = new List<ServiceItem>();

        /// <summary>The services that name the key as one of their dependencies.</summary>
        private readonly Dictionary<Guid, List<ServiceItem>> m_Dependents = new Dictionary<Guid, List<ServiceItem>>();

        /// <summary>The services that take part in a cycle.</summary>
        private readonly List<Guid> m_Cyclic = new List<Guid>();

        public ServiceDependencyGraph(IEnumerable<ServiceItem> services)
        {
            m_Services = Clean(services);

            foreach (var service in m_Services)
            {
                foreach (Guid id in DependenciesOf(service))
                {
                    List<ServiceItem> list;

                    if (!m_Dependents.TryGetValue(id, out list))
                    {
                        list = new List<ServiceItem>();
                        m_Dependents.Add(id, list);
                    }

                    list.Add(service);
                }
            }

            var state = new Dictionary<Guid, int>();

            foreach (var service in m_Services)
            {
                int value;

                if (state.TryGetValue(service.ID, out value) && value != Unvisited)
                {
                    continue;
                }

                Walk(service.ID, new List<Guid>(), state);
            }
        }

        /// <summary>The services the graph was built from.</summary>
        public List<ServiceItem> Services
        {
            get { return new List<ServiceItem>(m_Services); }
        }

        /// <summary>
        /// The services that take part in a cycle. A settings file that carries one is loaded anyway, this
        /// is what the manager reports instead of hanging the start.
        /// </summary>
        public List<Guid> Cyclic
        {
            get { return new List<Guid>(m_Cyclic); }
        }

        /// <summary>True when the settings file holds at least one cycle.</summary>
        public bool HasCycle
        {
            get { return m_Cyclic.Count > 0; }
        }

        /// <summary>
        /// The services that name the given id as one of their dependencies. A service that was deleted
        /// from the settings file is not a dependent of anything, so an id nobody knows returns nothing.
        /// </summary>
        public List<ServiceItem> DependentsOf(Guid id)
        {
            List<ServiceItem> list;

            if (!m_Dependents.TryGetValue(id, out list))
            {
                return new List<ServiceItem>();
            }

            return new List<ServiceItem>(list);
        }

        /// <summary>
        /// Every service that depends on the given one, directly or through other services. A cycle is
        /// walked once, so the list never holds a service twice and never holds the given one.
        /// </summary>
        public List<ServiceItem> DependentsRecursively(Guid id)
        {
            var found = new List<ServiceItem>();
            var seen = new HashSet<Guid> { id };

            var pending = new Stack<ServiceItem>(DependentsOf(id));

            while (pending.Count > 0)
            {
                var next = pending.Pop();

                if (!seen.Add(next.ID))
                {
                    continue;
                }

                found.Add(next);

                foreach (var dependent in DependentsOf(next.ID))
                {
                    pending.Push(dependent);
                }
            }

            return found;
        }

        /// <summary>
        /// The services the given one names as its dependencies. A name without a value, a reference to
        /// itself and a reference to a deleted service are all left out, they cannot order anything.
        /// </summary>
        public List<Guid> DependenciesOf(ServiceItem service)
        {
            var known = new List<Guid>();

            if (service == null || service.Dependencies == null)
            {
                return known;
            }

            foreach (Guid id in service.Dependencies)
            {
                if (id == Guid.Empty || id == service.ID)
                {
                    continue;
                }

                if (known.Contains(id) || Find(id) == null)
                {
                    continue;
                }

                known.Add(id);
            }

            return known;
        }

        /// <summary>
        /// The services that name no dependency of this settings file. Everything else hangs below one of
        /// them, only the services of a circle do not: a circle has no service that starts it.
        /// </summary>
        public List<ServiceItem> Roots()
        {
            return m_Services.Where(a => DependenciesOf(a).Count == 0).ToList();
        }

        /// <summary>
        /// True when the service reaches itself through its dependencies. The dialog asks with this whether
        /// a list that was just picked is a circle.
        /// </summary>
        public bool ReachesItself(Guid id)
        {
            var seen = new HashSet<Guid>();
            var pending = new Stack<Guid>(DependenciesOf(Find(id)));

            while (pending.Count > 0)
            {
                var next = pending.Pop();

                if (next == id)
                {
                    return true;
                }

                if (!seen.Add(next))
                {
                    continue;
                }

                foreach (Guid dependency in DependenciesOf(Find(next)))
                {
                    pending.Push(dependency);
                }
            }

            return false;
        }

        /// <summary>
        /// The given services in start order: every service comes after the services it depends on. Where
        /// the dependencies allow it the order of the given list is kept. A service inside a cycle is
        /// started where it stands in the list rather than holding the whole start up.
        /// </summary>
        public List<ServiceItem> StartOrder(IEnumerable<ServiceItem> services)
        {
            var pending = Clean(services);
            var order = new List<ServiceItem>();
            var started = new HashSet<Guid>();

            while (pending.Count > 0)
            {
                var next = pending.FirstOrDefault(a => IsStartable(a, pending, started));

                //Nothing is free, which means a cycle. The first service of the list takes its turn rather
                //than leaving the rest of the settings file unstarted.
                if (next == null)
                {
                    next = pending[0];
                }

                pending.Remove(next);
                order.Add(next);
                started.Add(next.ID);
            }

            return order;
        }

        /// <summary>
        /// The given services in stop levels. The first level holds the services nothing else in the list
        /// depends on, the last level the services at the top of the tree. Everything inside one level can
        /// be stopped at the same time, the levels are stopped one after the other.
        /// </summary>
        public List<List<ServiceItem>> StopLevels(IEnumerable<ServiceItem> services)
        {
            var pending = Clean(services);
            var levels = new List<List<ServiceItem>>();

            while (pending.Count > 0)
            {
                var level = pending.Where(a => IsLeaf(a, pending)).ToList();

                //A cycle has no leaf, so what is left is stopped together rather than never at all.
                if (level.Count == 0)
                {
                    level = new List<ServiceItem>(pending);
                }

                levels.Add(level);

                pending = pending.Where(a => !level.Contains(a)).ToList();
            }

            return levels;
        }

        private bool IsStartable(ServiceItem service, List<ServiceItem> pending, HashSet<Guid> started)
        {
            foreach (Guid id in DependenciesOf(service))
            {
                if (started.Contains(id))
                {
                    continue;
                }

                //A dependency that is not part of this run, a disabled service for example, holds nothing back
                if (!pending.Any(a => a.ID == id))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool IsLeaf(ServiceItem service, List<ServiceItem> pending)
        {
            foreach (var dependent in DependentsOf(service.ID))
            {
                if (pending.Any(a => a.ID == dependent.ID))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Marks every service on the path from the dependency that is already being visited up to the
        /// current one. Those are the services of the cycle, the ones that only hang below it are not.
        /// </summary>
        private void Walk(Guid id, List<Guid> path, Dictionary<Guid, int> state)
        {
            state[id] = OnStack;
            path.Add(id);

            foreach (Guid dependency in DependenciesOf(Find(id)))
            {
                int value = Unvisited;
                state.TryGetValue(dependency, out value);

                if (value == OnStack)
                {
                    MarkCycle(path, dependency);
                }
                else if (value == Unvisited)
                {
                    Walk(dependency, path, state);
                }
            }

            path.RemoveAt(path.Count - 1);
            state[id] = Done;
        }

        private void MarkCycle(List<Guid> path, Guid first)
        {
            for (int i = path.IndexOf(first); i < path.Count; i++)
            {
                if (!m_Cyclic.Contains(path[i]))
                {
                    m_Cyclic.Add(path[i]);
                }
            }
        }

        private ServiceItem Find(Guid id)
        {
            return m_Services.FirstOrDefault(a => a.ID == id);
        }

        /// <summary>
        /// The list without null entries and without a service that appears twice, in the order it came in.
        /// </summary>
        private static List<ServiceItem> Clean(IEnumerable<ServiceItem> services)
        {
            var list = new List<ServiceItem>();

            if (services == null)
            {
                return list;
            }

            foreach (var service in services)
            {
                if (service == null || list.Any(a => a.ID == service.ID))
                {
                    continue;
                }

                list.Add(service);
            }

            return list;
        }
    }
}
