from Painters import *
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

    def process(self, sensor, state, dt):
        # Initialise modules.
        if self.navigator == None:
            self.navigator = KeyboardCoordinateNavigator(state)
        if self.controller == None:
            self.controller = RuleController()

        keyinput = pg.key.get_pressed()

        # Manage modules.
        # Recorders
        if keyinput[pg.K_r]:
            self.recorder = RouteRecorder('routeData.csv')
        if keyinput[pg.K_t]:
            self.recorder = SensorRecorder('sensorData.csv')
        if keyinput[pg.K_e]:
            self.recorder = None

        # Controllers
        if keyinput[pg.K_o]:
            self.controller = PDController()
        if keyinput[pg.K_p]:
            self.controller = RuleController()

        # Run the navigator
        if self.navigator != None:
            (targetX, targetY) = self.navigator.process(sensor, state, dt)

        # Dodge walls
        for i in range(0,40):
            if sensor[i].val < 20:
                targetX += -50*math.sin(sensor[i].ang)
                targetY += -25*math.cos(sensor[i].ang)

        # Set up variables
        state.targetX = targetX
        state.targetY = targetY
        control = Control()

        # Run the recorder
        if self.recorder != None:
            self.recorder.process(sensor, state)

        # Run the controller.
        if self.controller != None:
            control = self.controller.process(sensor, state, dt)

        # Run the painter.
        self.painter.targetX = targetX
        self.painter.targetY = targetY

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