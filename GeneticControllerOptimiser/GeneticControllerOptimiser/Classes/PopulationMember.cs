using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticControllerOptimiser.Classes
{
    class PopulationMember
    {
        public List<double> Genome { get; set; }
        public List<SystemState> Results { get; set; }
        public Controller Controller { get; set; }
        public System System { get; set; }
        public int Fitness { get; set; }

    }
}
