import pybrain.supervised.trainers
import pybrain.datasets.supervised
import pybrain.structure.connections.full
import pybrain.structure.modules
import pybrain.structure.networks
from simulation import *
import csv
import pygame
import pybrain

#
#    Manual drive a car around a track
#


class CursorControl:

    def __init__(self):
        dataReader = csv.reader(open('wallFollowData.csv'), delimiter=',')
        self.inputList = list()
        self.outputList = list()

        for row in dataReader:
            self.inputList.append(row[0:40])
            self.outputList.append(row[40:44])

        net = pybrain.structure.networks.FeedForwardNetwork()
        inLayer = pybrain.structure.modules.SigmoidLayer(40)
        midLayer = pybrain.structure.modules.SigmoidLayer(50)
        outLayer = pybrain.structure.modules.SigmoidLayer(4)

        inToMid = pybrain.structure.connections.full.FullConnection(inLayer, midLayer)
        midToOut = pybrain.structure.connections.full.FullConnection(midLayer, outLayer)

        net.addInputModule(inLayer)
        net.addModule(midLayer)
        net.addOutputModule(outLayer)
        net.addConnection(inToMid)
        net.addConnection(midToOut)

        net.sortModules()

        dataSet = pybrain.datasets.supervised.SupervisedDataSet(40,4)

        data = zip(self.inputList, self.outputList)

        for d in data:
            dataSet.appendLinked(d[0], d[1])

        print len(dataSet)
        print dataSet

        trainer = pybrain.supervised.trainers.BackpropTrainer(net, dataSet, 0.1)

        for i in range(0,15):
            print trainer.train()

        print net

        print net.activate(self.inputList[0])

        print type(self.inputList[0])

        self.net = net


    def process(self,sensor,state,dt):
        control=Control()

        data = list()
        for s in sensor:
            data.append(s.val)

        resp = self.net.activate(data)
        maxVal = -1
        maxIndex = -1

        for i in range(0,4):
            if resp[i] > maxVal:
                maxIndex = i
                maxVal = resp[i]

        print resp

        if maxIndex == 0:
            print "Up"
            control.up = 1
        elif maxIndex == 1:
            print "Down"
            control.right = 1
        elif maxIndex == 2:
            print "Left"
            control.down = 1
        else:
            print "Right"
            control.left = 1



        return control



dt          =.1
brain       = CursorControl()
nSensors    = 40
sensorRange = 2000
# pod         = CarPod(nSensors,sensorRange,brain,(255,0,0))
pod         = SimplePod(nSensors,sensorRange,brain,(255,0,0), 10)
pods        = [pod]
world       = World("world.txt",pods)
sim         = Simulation(world,dt)

#uncomment the next line to hide the walls.
#sim.world.blind=True


sim.run()
