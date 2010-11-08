from Navigators import LongestDistanceNavigator
from Painters import *
from simulation import *
from Navigators import *
from Controllers import *
from Painters import *

class MainController:
    def __init__(self):
        self.navigator = None
        self.controller = None
        self.painter = None

    def process(self, sensor, state, dt):
        if self.navigator == None:
            self.navigator = LongestDistanceNavigator(state)
        if self.controller == None:
            self.controller = RuleController()
        if self.navigator != None:
            (targetX, targetY) = self.navigator.process(sensor, state, dt)
        for i in range(0,40):
            if sensor[i].val < 20:
                targetX += -20*math.sin(sensor[i].ang)
                targetY += -20*math.cos(sensor[i].ang)
                

        state.targetX = targetX
        state.targetY = targetY
        control = Control()
        self.painter.targetX = targetX
        self.painter.targetY = targetY
        if self.controller != None:
            control = self.controller.process(sensor, state, dt)

        return control

dt = .1
red = (255,0,0)
brain = MainController()
brain.painter = TargetCoordinatePainter()
nSensors = 40
sensorRange = 1000
pod = GravityPod(nSensors, sensorRange, brain, red)
pods = [pod]
world = World("world.txt", pods)
sim = Simulation(world, dt)

sim.painter = brain.painter

sim.run()