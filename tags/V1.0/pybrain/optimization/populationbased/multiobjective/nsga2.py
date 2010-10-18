__author__ = 'Justin Bayer, Tom Schaul, {justin,tom}@idsia.ch'


from scipy import array

from pybrain.optimization.populationbased.ga import GA
from pybrain.tools.nondominated import non_dominated_front, crowding_distance, non_dominated_sort

# TODO: not very elegant, because of the conversions between tuples and arrays all the time...


class MultiObjectiveGA(GA):
    """ Multi-objective Genetic Algorithm: the fitness is a vector with one entry per objective.
    By default we use NSGA-II selection. """
    
    topProportion = 0.5
    elitism = True
    
    populationSize = 100
    mutationStdDev = 1.
    
    allowEquality = True

    mustMaximize = True
    
    def _learnStep(self):
        """ do one generation step """
        # evaluate fitness
        self.fitnesses = dict([(tuple(indiv), self._oneEvaluation(indiv)) for indiv in self.currentpop])
        if self.storeAllPopulations:
            self._allGenerations.append((self.currentpop, self.fitnesses))
        
        if self.elitism:    
            self.bestEvaluable = list(non_dominated_front(map(tuple, self.currentpop),
                                                          key=lambda x: self.fitnesses[x],
                                                          allowequality = self.allowEquality))
        else:
            self.bestEvaluable = list(non_dominated_front(map(tuple, self.currentpop)+self.bestEvaluable,
                                                          key=lambda x: self.fitnesses[x],
                                                          allowequality = self.allowEquality))
        self.bestEvaluation = [self.fitnesses[indiv] for indiv in self.bestEvaluable]        
        self.produceOffspring()
    
    def select(self):        
        return map(array, nsga2select(map(tuple, self.currentpop), self.fitnesses, 
                                      self.selectionSize, self.allowEquality))    
                
    

def nsga2select(population, fitnesses, survivors, allowequality = True):
    """The NSGA-II selection strategy (Deb et al., 2002).
    The number of individuals that survive is given by the survivors parameter."""
    fronts = non_dominated_sort(population,
                                key=lambda x: fitnesses[x],
                                allowequality = allowequality)
    individuals = set()
    for front in fronts:
        remaining = survivors - len(individuals)
        if not remaining > 0:
            break
        if len(front) > remaining:
            # If the current front does not fit in the spots left, use those
            # that have the biggest crowding distance.
            crowd_dist = crowding_distance(front, fitnesses)
            front = sorted(front, key=lambda x: crowd_dist[x], reverse=True)
            front = set(front[:remaining])
        individuals |= front
    
    return list(individuals)
