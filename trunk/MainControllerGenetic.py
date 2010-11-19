from Painters import *
from Painters import TargetCoordinatePainter
from WallDodgers import *
from simulation import *
from Navigators import *
from Controllers import *
from Painters import *
from Recorders import *

import random

class MainControllerGenetic:
    def __init__(self, genome):
        self.navigator = None
        self.controller = RuleController()

        self.controller.bigYSpeed = genome[0]
        self.controller.midYSpeed = genome[1]
        self.controller.smlYSpeed = genome[2]
        self.controller.bigXSpeed = genome[3]
        self.controller.midXSpeed = genome[4]
        self.controller.smlXSpeed = genome[5]
        self.controller.bigXError = genome[6]
        self.controller.midXError = genome[7]
        self.controller.bigYError = genome[8]
        self.controller.midYError = genome[9]
        self.controller.upForce = genome[10]
        self.controller.downForce = genome[11]
        self.controller.propelAngle = genome[12]

        self.painter = None
        self.wallDodger = WallDodger(10)

    def process(self, sensor, state, dt):
        # Initialise modules.
        if self.navigator == None:
            self.navigator = RouteNavigator(state, 'routeNew.csv')

        # Run the navigator
        (state.targetX, state.targetY) = self.navigator.process(sensor, state, dt)

        # Set up variables
        control = Control()

        # Run the painter.
        self.painter.targetX = state.targetX
        self.painter.targetY = state.targetY

        # Dodge walls
        state = self.wallDodger.process(sensor, state, dt)

        # Run the controller.
        control = self.controller.process(sensor, state, dt)

        return control

dt = .1
red = (255,0,0)
nSensors = 40
sensorRange = 1000

pods = []

random.seed()

for i in range(0,256):
    g = []
    for j in range(0,13):
        g.append(random.randrange(0, 400, 1)/10)
    b = MainControllerGenetic(g)
    b.painter = TargetCoordinatePainter()
    pods.append(GravityPod(nSensors, sensorRange, b, red))

world = World("world.txt", pods)
sim = Simulation(world, dt)

sim.run()