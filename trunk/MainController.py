import pygame as pg

from Controllers import PDController
from Controllers import RuleController

from Navigators import KeyboardCoordinateNavigator
from Navigators import RouteNavigator
from Painters import TargetCoordinatePainter
from Recorders import RouteRecorder
from Recorders import SensorRecorder
from WallDodgers import WallDodger

from simulation import Control
from simulation import GravityPod
from simulation import Simulation
from simulation import World

class MainController:
    def __init__(self):
        self.navigator = None
        self.controller = None
        self.painter = None
        self.recorder = None
        self.wall_dodger = WallDodger(10)

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
            self.wall_dodger = None
        if keyinput[pg.K_k]:
            self.wall_dodger = WallDodger(10)
        if keyinput[pg.K_l]:
            self.wall_dodger = WallDodger(20)

        # Run the navigator
        if self.navigator != None:
            (state.target_x, state.target_y) = self.navigator.process(sensor, state, dt)

        # Set up variables
        control = Control()

        # Run the painter.
        self.painter.target_x = state.target_x
        self.painter.target_y = state.target_y

        # Dodge walls
        if self.wall_dodger != None:
            state = self.wall_dodger.process(sensor, state, dt)

        # Run the recorder
        if self.recorder != None:
            self.recorder.process(sensor, state)

        # Run the controller.
        if self.controller != None:
            control = self.controller.process(sensor, state, dt)

        return control

timestep = .1
RED = (255, 0, 0)
BRAIN = MainController()
BRAIN.painter = TargetCoordinatePainter()
N_SENSORS = 40
SENSOR_RANGE = 1000
POD = GravityPod(N_SENSORS, SENSOR_RANGE, BRAIN, RED)
PODS = [POD]
WORLD = World("world.txt", PODS)
SIM = Simulation(WORLD, timestep)

SIM.painter = BRAIN.painter

SIM.run()