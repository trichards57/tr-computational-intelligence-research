from simulation import *
from Navigators import *

class MainController:
    def __init__(self):
        self.navigator = None
        self.controller = None
        self.painter = None

    def process(self, sensor, state, dt):
        if self.navigator == None:
            self.navigator = KeyboardCoordinateNavigator(state)
        if self.navigator != None:
            (targetX, targetY) = self.navigator.process(sensor, state, dt)
        print (targetX, targetY)
        state.targetX = targetX
        state.targetY = targetY
        control = Control()
        if self.controller != None:
            control = self.controller.process(sensor, state, dt)
        if self.painter != None:
            self.painter.process(sensor, state, dt)

        return control

dt = .1
red = (255,0,0)
brain = MainController()
nSensors = 40
sensorRange = 1000
pod = GravityPod(nSensors, sensorRange, brain, red)
pods = [pod]
world = World("world.txt", pods)
sim = Simulation(world, dt)

sim.run()