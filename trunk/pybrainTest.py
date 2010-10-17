from pickle import *
from pybrain.structure.connections.full import FullConnection
from pybrain.structure.networks.feedforward import FeedForwardNetwork
from pybrain.supervised.trainers.backprop import BackpropTrainer
from pybrain.datasets.supervised import SupervisedDataSet
from pybrain.structure.modules.sigmoidlayer import SigmoidLayer
from pybrain.structure.modules.linearlayer import LinearLayer

zeroThreshold = 1e-6

outputs = [0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.1,0.1,0.1,0.2,0.1,0.1,0.2,
           0.3,0.1,0.1,0.2,0.4,0.1,0.3,0.1,0.2,0.5,0.1,0.2,0.3,0.4,0.6,0.2,0.7,
           0.3,0.5,0.2,0.4,0.8,0.2,0.3,0.6,0.9,0.2,0.4,0.5,0.3,0.7,0.2,0.3,0.4,
           0.6,0.8,0.5,0.3,0.9,0.4,0.7,0.3,0.5,0.6,0.4,0.8,0.3,0.5,0.7,0.4,0.6,
           0.9,0.4,0.5,0.8,0.6,0.7,0.4,0.5,0.9,0.6,0.8,0.7,0.5,0.6,0.9,0.5,0.7,
           0.8,0.6,0.7,0.9,0.8,0.6,0.7,0.8,0.9,0.7,0.8,0.9,0.8,0.9,0.9]

inputs = [[1,1.22461E-14],[2,1.46953E-14],[3,1.71446E-14],[4,1.95938E-14],
         [5,2.2043E-14],[6,2.44922E-14],[7,2.69414E-14],[8,2.93907E-14],
         [9,3.18399E-14],[0,1.59199E-13],[0,0.088000478],[1,0.132000717],
         [2,0.176000955],[0,0.176001807],[3,0.220001194],[4,0.264001433],
         [1,0.264002711],[0,0.26400484],[5,0.308001672],[6,0.352001911],
         [2,0.352003614],[0,0.352010429],[7,0.39600215],[1,0.396007261],
         [8,0.440002388],[3,0.440004518],[0,0.440019426],[9,0.484002627],
         [4,0.528005422],[2,0.528009681],[1,0.528015644],[0,0.528032683],
         [5,0.616006325],[0,0.616051052],[3,0.660012101],[1,0.660029139],
         [6,0.704007229],[2,0.704020859],[0,0.704075385],[7,0.792008132],
         [4,0.792014521],[1,0.792049024],[0,0.792106536],[8,0.880009036],
         [3,0.880026073],[2,0.880038852],[5,0.924016942],[1,0.924076577],
         [9,0.96800994],[6,1.056019362],[4,1.056031288],[2,1.056065365],
         [1,1.056113078],[3,1.100048565],[7,1.188021782],[1,1.188159805],
         [5,1.232036503],[2,1.232102103],[8,1.320024202],[4,1.320058278],
         [3,1.320081707],[6,1.408041718],[2,1.40815077],[9,1.452026622],
         [5,1.540067991],[3,1.540127629],[7,1.584046932],[4,1.584098048],
         [2,1.584213073],[8,1.760052147],[6,1.760077704],[3,1.760188463],
         [5,1.848114389],[4,1.848153155],[9,1.936057362],[7,1.980087417],
         [3,1.980266341],[6,2.112130731],[4,2.112226156],[5,2.15617868],
         [8,2.20009713],[7,2.376147072],[4,2.376319609],[9,2.420106843],
         [6,2.464204206],[5,2.464263848],[8,2.640163413],[7,2.772229732],
         [5,2.772372877],[6,2.816301541],[9,2.904179755],[8,3.080255258],
         [7,3.168339233],[6,3.168426146],[9,3.388280783],[8,3.520376926],
         [7,3.564479414],[9,3.872414618],[8,3.960532682],[9,4.35658595]]

net = FeedForwardNetwork()
inLayer = LinearLayer(2)
hiddenLayer = SigmoidLayer(10)
outLayer = LinearLayer(1)

inToHidden = FullConnection(inLayer, hiddenLayer)
hiddenToOut = FullConnection(hiddenLayer, outLayer)

net.addInputModule(inLayer)
net.addModule(hiddenLayer)
net.addOutputModule(outLayer)
net.addConnection(inToHidden)
net.addConnection(hiddenToOut)

net.sortModules()

dataSet = SupervisedDataSet(2,1)

data = zip(inputs, outputs)

for d in data:
    dataSet.appendLinked(d[0], d[1])

print len(dataSet)
print dataSet

trainer = BackpropTrainer(net, dataSet, 0.1)

for i in range(0,250):
    print trainer.train()

print net.activate([0,0.88])

f = open('Neural.net', 'w')

dump(net, f)