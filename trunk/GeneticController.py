import time
import random
from PDController import *
from simulation import *

class Genome:
    def __init__(self):
        self.id = 0
        self.genome = []

    def breed(self, other):
        cuttingPoint = random.randint(0, len(self.genome) - 1)
        new = Genome()
        new.genome = self.genome[:cuttingPoint] + other.genome[cuttingPoint:]
        return new

    def mutate(self):
        mutatePoint = random.randint(0, len(self.genome) - 1)
        self.genome[mutatePoint] = random.randint(-100,100)

class GeneticController:
    def __init__(self, populationCount, genomeLength):
        self.populationCount = populationCount
        self.genomeLength = genomeLength
        self.population = []
        self.pods = []
        self.lastTime = 0
        self.count = 0
        self.cycleCount = 100

        random.seed()
        # Generate a random population
        print populationCount, self.populationCount
        for i in range(0, self.populationCount):
            genome = Genome()
            genome.id = i
            for j in range(0, self.genomeLength):
                gene = random.randint(-100,100)
                genome.genome.append(gene)
            brain = PDController(genome, self)
            pod = GravityPod(nSensors, sensorRange, brain, (255,0,0))
            pod.id = i
            self.pods.append(pod)

    def process(self):
        
        self.count += 1
        def getKey(pod1, pod2):
            return -cmp(pod1.brain.fitness, pod2.brain.fitness)

        if (self.count % (len(self.pods) * self.cycleCount)) == (len(self.pods) * self.cycleCount) -1:
            # Enough cycles have passed. Now do population stuff.
            sortedPods = sorted(self.pods, cmp=getKey)

            totalFitness = 0
            for pod in sortedPods:
                #if pod.collide_count > 0:
                    # Hit something, quite bad
                   # pod.brain.fitness -= 100
                totalFitness += pod.brain.fitness

            print "Average Fitness :", totalFitness / len(self.pods)

            newGenomes = []
            bestPods = sortedPods[:25]

            for i in range(0, len(sortedPods) - 25):
                index1 = random.randint(0,24)
                index2 = random.randint(0,24)
                newGenomes.append(bestPods[index1].brain.genome.breed(bestPods[index2].brain.genome))
            
            count = 0

            for g in newGenomes:
                if random.randint(0,1000) <= 1:
                    g.mutate()

            for pod in sortedPods[25:]:
                pod.brain.__init__(newGenomes[count], self)
                count += 1

            for pod in sortedPods:
                pod.x = 100
                pod.y = 700
                pod.ang = pi
                pod.dxdt = 0
                pod.dydt = 0
                pod.dangdt = 0
                pod.collide_count = 0
                pod.collide = False
                

           

            


dt = .1
nSensors = 2
sensorRange = 2000
pods = []

controller = GeneticController(1000, 10)
print len(controller.pods)
world = World("rect_world.txt", controller.pods)
sim = Simulation(world, dt)

painter = Painter(world)

sim.painter = painter

sim.run()