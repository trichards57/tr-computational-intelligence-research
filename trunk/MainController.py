from Painters import *
from WallDodgers import *
from simulation import *
from Navigators import *
from Controllers import *
from Painters import *
from Recorders import *

class MainController:
    def __init__(self):
        self.navigator = None
        self.controller = None
        self.painter = None
        self.recorder = None
        self.wallDodger = WallDodger(10)

    def process(self, sensor, state, dt):
        # Initialise modules.
        if self.navigator == None:
            self.navigator = KeyboardCoordinateNavigator(state)
        if self.controller == None:
            self.controller = RuleController()

        keyinput = pg.key.get_pressed()

        # Manage modules.
        # Recorders
        if keyinput[pg.K_e]:
            self.recorder = None
        if keyinput[pg.K_r]:
            self.recorder = RouteRecorder('routeData.csv')
        if keyinput[pg.K_t]:
            self.recorder = SensorRecorder('sensorData.csv')

        # Controllers
        if keyinput[pg.K_o]:
            self.controller = PDController()
        if keyinput[pg.K_p]:
            self.controller = RuleController()

        # Navigators
        if keyinput[pg.K_a]:
            self.navigator = RouteNavigator(state, 'routeNew.csv')
        if keyinput[pg.K_s]:
            self.navigator = KeyboardCoordinateNavigator(state)

        # Wall Dodgers
        if keyinput[pg.K_j]:
            self.wallDodger = None
        if keyinput[pg.K_k]:
            self.wallDodger = WallDodger(10)
        if keyinput[pg.K_l]:
            self.wallDodger = WallDodger(20)

        # Run the navigator
        if self.navigator != None:
            (state.targetX, state.targetY) = self.navigator.process(sensor, state, dt)

        # Set up variables
        control = Control()

        # Run the painter.
        self.painter.targetX = state.targetX
        self.painter.targetY = state.targetY

        # Dodge walls
        if self.wallDodger != None:
            state = self.wallDodger.process(sensor, state, dt)

        # Run the recorder
        if self.recorder != None:
            self.recorder.process(sensor, state)

        # Run the controller.
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