from math import sin
from math import cos
from pydoc import deque

from HorizontalMoveLearnerPersonality import HorizontalMoveLearnerPersonality
from HorizontalMovePersonality import HorizontalMovePersonality
from HoverFindPersonality import HoverFindPersonality
from StopPersonality import StopPersonality
from VerticalMovePersonality import VerticalMovePersonality
from FurthestNewDistanceNavigatorPersonality import FurthestNewDistanceNavigatorPersonality
from simulation import Control
from simulation import GravityPod
from simulation import Simulation
from simulation import World

# Setting this to a larger number makes the system more tolerant of residual
# velocities.
# If too high, the pod will drift when it is supposed to be hovering.
# The larger the number, the faster it will drift.
# If too small, cancelAcceleration and cancelVelocity will take longer.
zeroThreshold = 1e-4

class LearningControl:



    # Navigator Types
    furthestNewDistance = 1
    wallFollowing = 2

    def __init__(self):
        # Last State Variables
        # Last acceleration. Used for cancelAcceleration to determine if thrust steps are too large.
        self.lastAccel = 1
        # Last vertical speed. Used to calculate acceleration. Must be initialized to >= zeroThreshold,
        # otherwise acceleration appears to be 0 during the first cycle, mucking up the hover thrust finding.
        self.lastDyDt = zeroThreshold * 10

        # Current State Variables
        # Stores the value of thrust required to cancel out the acceleration due to gravity
        self.hoverThrust = 0
        # Used during the initialisation to flag if the hover thrust search has started.
        self.hoverThrustSearching = False
        # Used during initialisation to flag if the hover thrust has been found.
        self.hoverThrustFound = False
        # Used during initialisation to flag if the pod has come to a halt before the first maneuver.
        self.velocityZeroed = False
        # Used during initialisation to flag if the pod has been position equidistant from the top and bottom walls sensed.
        self.positionCenterStarted = False
        # Used for the state machine to make sure it doesn't try to restart the turning.
        self.startTurningTest = True
        self.horizontalMoveStarted = True

        # Current personality that is running the pod
        self.personality = None
        # List of personalities to be used
        self.personalities = deque()

        self.navigationTechnique = self.furthestNewDistance

        self.direction = 0

        self.sensorDataFile = open('sensorData.csv', 'w')

    def process(self,sensor,state,dt):

        control=Control()

        # Calculate extra state variables
        # TODO : Move this straight in to the state variable when accel is nolonger used.
        accel = (state.dydt - self.lastDyDt)/dt

        # Output state
        print "Y"
        print "Acceleration : ", accel
        print "Speed        : ", state.dydt
        print "Position     : ", state.y

        print "X"
        print "Speed        : ", state.dxdt
        print "Position     : ", state.x

        print "Angle"
        print "Speed        : ", state.dangdt
        print "Position     : ", state.ang

        # Add my variables to state
        state.d2ydt2 = accel
        state.last_d2ydt2 = self.lastAccel
        state.dt = dt
        state.sensor = sensor

        # Is there a current personality that is working?
        if self.personality != None and self.personality.done == False:
            # Run the personality
            self.personality.process(state)
        elif self.personality == None or self.personality.done == True:
            # The current personality is done, or there isn't one.
            if len(self.personalities) > 0:
                # A personality is queued up to run.
                # Set it as the active personality.
                self.personality = self.personalities.pop()
                # Run the first cycle.
                self.personality.process(state)
            elif self.hoverThrustSearching == False:
                # We've just started. Make a HoverFindPersonality the active personality
                self.personality = HoverFindPersonality(0.2)
                # Run the first cycle.
                self.personality.process(state)
                # We are now searching for the hover thrust.
                self.hoverThrustSearching = True
            elif self.hoverThrustFound == False:
                # Store the hover thrust for later use.
                self.hoverThrust = self.personality.hoverThrust
                # Now bring the pod to a stop. Make a StopPersonality the active personality
                self.personality = StopPersonality(self.hoverThrust)
                # Run the first cycle.
                self.personality.process(state)
                # We've found the hover thrust.
                self.hoverThrustFound = True
            elif self.positionCenterStarted == False:
                # We need to position ourselves in the centre of the passage, vertically
                # First work out the height of the passage.
                distanceToTop = sensor[0].val
                distanceToBottom = sensor[20].val
                passageHeight = distanceToTop + distanceToBottom

                bottomPosition = state.y + distanceToBottom
                centerHeight = bottomPosition - (passageHeight / 2)

                # Make a VerticalMovePersonality the active personality
                self.personality = VerticalMovePersonality(self.hoverThrust, state, centerHeight)
                # Queue up two more personalities to test movement to and fro
                #backToStartMove = VerticalMovePersonality(self.hoverThrust, state, bottomPosition - (passageHeight / 4))
                #backToCenterMove = VerticalMovePersonality(self.hoverThrust, state, centerHeight - (passageHeight / 4))
                #self.personalities.appendleft(backToStartMove)
                #self.personalities.appendleft(backToCenterMove)
                # Run the first cycle
                self.personality.process(state)
                # We have started to center.
                self.positionCenterStarted = True
            elif self.startTurningTest == False:
                for t in range(0, 10, 1):
                    for u in range(0, 10, 1):
                        self.personalities.appendleft(HorizontalMoveLearnerPersonality(self.hoverThrust, t / 10.0, u))
                self.startTurningTest = True
            elif self.horizontalMoveStarted == False:
                self.personality = HorizontalMovePersonality(self.hoverThrust, state, 200)
                self.personality.process(state)
                self.horizontalMoveStarted = True
            elif self.navigationTechnique == self.furthestNewDistance:
                # We're done learning. Time to navigate.
                # Find the furthest direction that we haven't just traveled in.
                self.personality = FurthestNewDistanceNavigatorPersonality(self.hoverThrust)
                self.personality.process(state)
            elif self.navigationTechnique == self.wallFollowing:
                self.personality = None
                # Alternativly, navigate by following the wall.

                # First, find the sensor reporting the closest object.
                closestReading = 5000
                closestSensor = -1
                for i in range(0,40):
                    if closestReading > sensor[i].val:
                        closestReading = sensor[i].val
                        closestSensor = i

                print "Closest wall is on sensor : ", closestSensor

                nextSensor = closestSensor + 1
                if nextSensor >= 40:
                    nextSensor -= 40

                x1 = state.x + sensor[closestSensor].val * sin(sensor[closestSensor].ang)
                y1 = state.y + sensor[closestSensor].val * cos(sensor[closestSensor].ang)

                x2 = state.x + sensor[nextSensor].val * sin(sensor[nextSensor].ang)
                y2 = state.y + sensor[nextSensor].val * cos(sensor[nextSensor].ang)

                print "Point 1 : (", x1, ",", y1, ")"
                print "Point 2 : (", x2, ",", y2, ")"

                lineM = 0
                lineC = 0

                if x1 == x2:
                    # Vertical line. If a discontinuity is detected, but not on
                    # sensor 0 or 20 (which are pointing parallel to the line),
                    # move up to it. If no discontinuity is detected, creep along
                    # the line until one is found.
                    print "Vertical Wall"
                elif y1 == y2:
                    # Horizontal line. If a discontinuity is detected, but not on
                    # sensor 10 or 30 (which are pointing parallel to the line),
                    # move up to it. If no discontinuity is detected, creep along
                    # the line until one is found.
                    print "Horizontal Wall"
                else:
                    # Arbitary diagonal wall. If a discontinuity is detected on
                    # a sensor that should be able to detect the wall, move up to
                    # it. If no discontinuity is detected, creep along the line
                    # until one is found.
                    print "Diagonal Wall"

        self.lastDyDt = state.dydt
        self.lastAccel = accel

        # Store sensor data:
        sensorData = str(state.x) + ',' + str(state.y) + ','
        for i in range(0,40):
            sensorData += str(sensor[i].ang) + ',' + str(sensor[i].val) + ',' + sensor[i].wall + ','
        self.sensorDataFile.write(sensorData + '\n')

        if (self.personality != None):
            print "Thrusters"
            print "Up : ", self.personality.control.up
            print "Left : ", self.personality.control.left
            print "Right : ", self.personality.control.right
            print ""
            return self.personality.control
        else:
            control.up = self.hoverThrust
            print ""
            return control

dt          =.1
brain       = LearningControl()
nSensors    = 40
sensorRange = 5000
pod         = GravityPod(nSensors,sensorRange,brain,(255,0,0))
pods        = [pod]
world       = World("world.txt",pods)
sim         = Simulation(world,dt)
#uncomment the next line to hide the walls.
#sim.world.blind=True

sim.run()
