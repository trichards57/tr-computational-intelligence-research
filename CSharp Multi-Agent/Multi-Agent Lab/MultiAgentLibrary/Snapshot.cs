using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MultiAgentLibrary
{
    public class Snapshot
    {
        [XmlAttribute("cycle-count")]
        public int CycleCount { get; set; }
        [XmlAttribute("route-length")]
        public int RouteLength { get; set; }
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
}
