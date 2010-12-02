## @package MainController
# This is the main module of the program.  It includes the MainController, which
# handles ties together the Navigators, Controllers, Painters, Recorders and
# WallDodgers to control the pod.  It also contains the simulation setup and
# start routine.

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
# This is the main class which acts as the brain for the pod.  It controls the
# pod and the environment by calling various Navigators, Controllers, Painters,
# Recorders and WallDodgers to process the current sensor data and produce
# a set of commands for the thrusters.
class MainController:
    ## Initialises the controller's attributes.
    #
    # @selfParam
    #
    # Sets the wall_dodger to try to keep 10px away from the wall. All
    # remaining initialisation requires the initial pod state, and is handled
    # on the first call to process.
    def __init__(self):
        ## The member of Navigators which is controlling where the pod goes.
        self.navigator = None
        ## The member of Controllers which is controlling how the pod moves.
        self.controller = None
        ## The member of Painters that is displaying additional data on the
        #  screen.
        self.painter = None
        ## The member of Recorders that records the trip.
        self.recorder = None
        ## The member of WallDodgers that handles wall avoidance.
        self.wall_dodger = WallDodger(10)

    ## Transforms sensor and state data in to thruster commands
    #
    # @selfParam
    # @sensorParam
    # @stateParam
    # @timestepParam
    #
    # This controller passes the sensor, state and dt parameters to each selected
    # module in turn.
    #
    # -# Requests a set of coordinates from the navigator.
    # -# Passes coordinates to the painter for display.
    # -# Passes coordinates to the wall_dodger for modification to avoid walls.
    # -# State is passed to the recorder to record if required.
    # -# State and coordinates is passed to the controller to get thruster commands
    # -# Returns thruster commands.
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

## The timestep to be used by the simulator.
TIMESTEP = .1
## The color of red, used to color the pod.
RED = (255, 0, 0)
## The instance of MainController used to control the pod.
BRAIN = MainController()
BRAIN.painter = TargetCoordinatePainter()
## The number of sensors the pod should have.
N_SENSORS = 40
## The range the sensors can see.
SENSOR_RANGE = 1000
## The pod to run around the simulator.
POD = GravityPod(N_SENSORS, SENSOR_RANGE, BRAIN, RED)
## The list of pods to run around the simulator.
PODS = [POD]
## The world the pods move in.
WORLD = World("world.txt", PODS)
## The simulator that runs the world.
SIM = Simulation(WORLD, TIMESTEP)
SIM.painter = BRAIN.painter
SIM.run()