## @package MainController
# Contains the main controller used to manage the pod, tying together the
# Navigators, Controllers, Recorders, WallDodgers and Painters.
import pygame as pg

from Controllers import PDController
from Controllers import RuleController
from Controllers import TestRuleController

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

## Main Controller
#
# This controller is acts as the main brain of the pod.  It takes the given
# system and sensor state and outputs a set of thruster instructions.
class MainController:
    ## The MainController constructor
    # Initialises the controller attributes to pre-simulation start states.
    # Only the wall dodger is set up here, as the remaining control modules either
    # default to None or need the system starting state to be initialised.
    def __init__(self):
        ## The current navigator in use
        self.navigator = None
        ## The current controller in use
        self.controller = None
        ## The current painter in use
        self.painter = None
        ## The current recorder in use
        self.recorder = None
        ## The current wall dodger in use
        self.wall_dodger = WallDodger(10)

    ## The process function for the MainController
    #
    # @param self The object pointer
    # @param sensor A list of Sensor objects.
    # @param state The current state of the pod.
    # @param dt The timestep used by the simulator.
    # @return A Control object containing the desired thruster instructions.
    #
    # On the first call to this function, it initialises the navigator to
    # @ref Navigators.KeyboardCoordinateNavigator and the controller to
    # @ref Controllers.RuleController.
    #
    # The process uses key inputs to manage which control modules should be used:
    # - Recorders
    #    - e - None
    #    - r - @ref Recorders.RouteRecorder
    #    - t - @ref Recorders.SensorRecorder
    # - Controllers
    #    - o - @ref Controllers.PDController
    #    - p - @ref Controllers.RuleController
    # - Navigators
    #    - a - @ref Navigators.RouteNavigator (using routeNew.csv as an input file)
    #    - s - @ref Navigators.KeyboardCoordinateNavigator
    # - Wall Dodgers
    #    - j - None
    #    - k - @ref WallDodgers.WallDodger (safe distance = 10)
    #    - l - @ref WallDodgers.WallDodger (safe distance = 20)
    #
    # The modules are called in the following order:
    # -# Navigator
    # -# Wall Dodger
    # -# Recorder
    # -# Controller
    #
    # The painter module is called by the simulation, not by MainController.
    def process(self, sensor, state, dt):
        # Initialise modules.
        if self.navigator == None:
            self.navigator = KeyboardCoordinateNavigator(state)
        if self.controller == None:
            self.controller = TestRuleController()

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

## The simulation timestep
timestep = .1
## The colour red (used as the colour of the pod)
RED = (255, 0, 0)
## The pod's brain (a MainController)
BRAIN = MainController()

BRAIN.painter = TargetCoordinatePainter()
## The number of sensors available to the pod
N_SENSORS = 40
## The maximum range of the sensors
SENSOR_RANGE = 1000
## The pod used in the simulation
POD = GravityPod(N_SENSORS, SENSOR_RANGE, BRAIN, RED)
## The list of pods used in the simulation (simulation supports multiple pods simultaneously)
PODS = [POD]
## The world the pods exist in
WORLD = World("world.txt", PODS)
## The simulation that runs the world
SIM = Simulation(WORLD, timestep)

SIM.painter = BRAIN.painter

SIM.run()