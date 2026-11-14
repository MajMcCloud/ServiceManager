using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceManager.Base.Data
{
    /// <summary>
    /// The dependencies of the settings file drawn as a tree: the services nothing waits for sit on top,
    /// everything that waits for them hangs below. The Windows services a service waits for hang as leaves.
    /// Nothing is left out - a disabled service and a service that sits on a circle are marked instead.
    /// </summary>
    public class DependencyTree
    {
        private readonly ServiceDependencyGraph m_Graph;

        private readonly String m_HostName;

        private readonly IEnumerable<String> m_HostDependencies;

        public DependencyTree(ServiceDependencyGraph graph)
        {
            m_Graph = graph ?? new ServiceDependencyGraph(new List<ServiceItem>());
        }

        /// <summary>
        /// The same tree with the ServiceManager itself on top. It is a Windows service as well, so the services
        /// of the settings file hang below it together with the Windows services it waits for itself.
        /// </summary>
        public DependencyTree(ServiceDependencyGraph graph, String hostName, IEnumerable<String> hostDependencies)
            : this(graph)
        {
            m_HostName = hostName;
            m_HostDependencies = hostDependencies;
        }

        public DependencyTree(IEnumerable<ServiceItem> services)
            : this(new ServiceDependencyGraph(services))
        {
        }

        public DependencyTree(IEnumerable<ServiceItem> services, String hostName, IEnumerable<String> hostDependencies)
            : this(new ServiceDependencyGraph(services), hostName, hostDependencies)
        {
        }

        /// <summary>
        /// The top of the tree, one node per service nothing waits for. A circle has no such service, so the
        /// services no top reaches are added on their own rather than staying invisible.
        /// </summary>
        public List<DependencyTreeNode> Build()
        {
            var roots = BuildRoots();

            if (String.IsNullOrWhiteSpace(m_HostName))
            {
                return roots;
            }

            var host = new DependencyTreeNode
            {
                Kind = DependencyTreeNode.eNodeKind.Host,
                Title = m_HostName.Trim()
            };

            host.Children.AddRange(roots);

            foreach (String name in OrderedNames(m_HostDependencies))
            {
                host.Children.Add(new DependencyTreeNode
                {
                    Kind = DependencyTreeNode.eNodeKind.WindowsService,
                    Title = name
                });
            }

            return new List<DependencyTreeNode> { host };
        }

        private List<DependencyTreeNode> BuildRoots()
        {
            var roots = new List<DependencyTreeNode>();
            var reached = new HashSet<Guid>();

            foreach (var service in Ordered(m_Graph.Roots()))
            {
                roots.Add(BuildService(service, new List<Guid>(), reached));
            }

            var left = Ordered(m_Graph.Services.Where(a => !reached.Contains(a.ID)));

            foreach (var service in left)
            {
                //The branch of one service can hold another of them, so the list is checked again here.
                if (reached.Contains(service.ID))
                {
                    continue;
                }

                roots.Add(BuildService(service, new List<Guid>(), reached));
            }

            return roots;
        }

        /// <summary>
        /// The service with everything that waits for it below. The services of the current branch are the
        /// guard: a circle is closed by showing the service once more, it is never followed further.
        /// </summary>
        private DependencyTreeNode BuildService(ServiceItem service, List<Guid> path, HashSet<Guid> reached)
        {
            var node = new DependencyTreeNode
            {
                Kind = DependencyTreeNode.eNodeKind.Service,
                Title = service.Title,
                ID = service.ID,
                Enabled = service.Enabled,
                InCycle = m_Graph.Cyclic.Contains(service.ID)
            };

            path.Add(service.ID);
            reached.Add(service.ID);

            foreach (var dependent in Ordered(m_Graph.DependentsOf(service.ID)))
            {
                if (path.Contains(dependent.ID))
                {
                    node.Children.Add(new DependencyTreeNode
                    {
                        Title = dependent.Title,
                        ID = dependent.ID,
                        Enabled = dependent.Enabled,
                        InCycle = true,
                        IsRepeat = true
                    });

                    continue;
                }

                node.Children.Add(BuildService(dependent, path, reached));
            }

            path.RemoveAt(path.Count - 1);

            foreach (String name in OrderedNames(service.WindowsDependencies))
            {
                node.Children.Add(new DependencyTreeNode
                {
                    Kind = DependencyTreeNode.eNodeKind.WindowsService,
                    Title = name
                });
            }

            return node;
        }

        private static List<ServiceItem> Ordered(IEnumerable<ServiceItem> services)
        {
            return services.OrderBy(a => a.Title, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static List<String> OrderedNames(IEnumerable<String> names)
        {
            if (names == null)
            {
                return new List<String>();
            }

            return names.Where(a => !String.IsNullOrWhiteSpace(a)).OrderBy(a => a, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
