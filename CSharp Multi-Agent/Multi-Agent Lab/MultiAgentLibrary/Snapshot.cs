using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;

namespace MultiAgentLibrary
{
    public class Snapshot
    {
        [XmlAttribute("cycle-count")]
        public int CycleCount { get; set; }
        [XmlAttribute("route-length")]
        public int RouteLength { get; set; }
        [XmlAttribute("agent-count")]
        [DefaultValue(-1)]
        public int AgentCount { get; set; }
    }

    [XmlRoot("resultdata")]
    public class SnapshotCollection
    {
        [XmlArray("results")]
        [XmlArrayItem("result")]
        public List<Snapshot> Snapshots { get; set; }

        public SnapshotCollection()
        {
            Snapshots = new List<Snapshot>();
        }
    }

    public class SnapshotComparer : IEqualityComparer<Snapshot>
    {
        public bool Equals(Snapshot x, Snapshot y)
        {
            return (x.CycleCount == y.CycleCount && x.RouteLength == y.RouteLength);
        }

        public int GetHashCode(Snapshot obj)
        {
            return obj.RouteLength + obj.CycleCount * 100;
        }
    }
}
