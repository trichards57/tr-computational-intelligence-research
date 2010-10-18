from math import cos
from math import pi
from math import fabs
import pickle

from Personality import Personality

class HorizontalMovePersonality(Personality):
    # The desired end position of the pod
    destinationX = 0
    # The starting position of the pod
    originX = 0
    # The total distance to cover
    totalDistance = 0

    # Neural net used to work out thrusts for acceleration.
    controlNet = pickle.load(open('Neural.net', 'r'))

    maximumSpeed = 10
    turnThrust = 0
    direction = 0
    
    # Enumeration for flight profiles
    continuousSpeedFlight = 1 # Fly at a set speed
    continuousAccelFlight = 2 # Fly with constant acceleration
    stopFlight = -1

    # State machine variables
    startedAccelerating = False
    nowAccelerating = False
    doneAccelerating = False
    finishedAccelerating = False
    finishedMove = False
    startedDecelerating = False
    nowDecelerating = False
    doneDecelerating = False
    finishedDecelerating = False

    waiting = 0
    delay = 50

    # Determines which flight profile to use
    flightProfile = continuousSpeedFlight

    def __init__(self, hoverThrust, state, destinationX):
        self.hoverThrust = hoverThrust
        self.destinationX = destinationX
        self.originX = state.x

        # Get a turn thrust that will accelerate us at about 0.4px/s^2 with a 0 cycle delay
        self.turnThrust = self.controlNet.activate([0,0.8])[0]

    def accelerateRotateLeft(self, thrust):
        self.control.left = thrust
        self.control.right = 0
        print "L"

    def accelerateRotateRight(self, thrust):
        self.control.left = 0
        self.control.right = thrust
        print "R"

    def constantRotate(self):
        self.control.left = 0
        self.control.right = 0
        print "C"

    def startMove(self):
        print self.direction
        if self.direction == 1:
            # Accelerating right
            self.accelerateRotateRight(self.turnThrust)
        else:
            self.accelerateRotateLeft(self.turnThrust)

    def stopMove(self):
        print self.direction
        if self.direction == 1:
            # Accelerating right
            self.accelerateRotateLeft(self.turnThrust)
        else:
            self.accelerateRotateRight(self.turnThrust)


    def process(self, state):
        print "Horizontal Move"
        # How far have we got left to go?
        distanceRemaining = self.destinationX - state.x
        print "Destination : ", self.destinationX
        print "Distance remaining : ", distanceRemaining

        if distanceRemaining > 0 and self.direction == 0:
            self.direction = 1
        elif self.direction == 0:
            self.direction = -1

        if self.flightProfile == self.stopFlight:
            self.Done = True
            self.control.up = self.hoverThrust
        elif self.flightProfile == self.continuousSpeedFlight:
            if self.startedAccelerating == False:
                self.startMove()
                self.startedAccelerating = True
                print "SA"
            elif self.nowAccelerating == False:
                self.stopMove()
                self.nowAccelerating = True
                print "NA"
            elif self.waiting < self.delay:
                self.constantRotate()
                self.waiting += 1
            elif self.doneAccelerating == False:
                self.stopMove()
                self.doneAccelerating = True
                print "DA"
            elif self.finishedAccelerating == False:
                self.startMove()
                self.finishedAccelerating = True
                print "FA"
            elif self.finishedMove == False:
                if fabs(distanceRemaining) < 5:
                    self.direction = -self.direction
                    self.finishedMove = True
                self.constantRotate()
                print "MV"
            elif self.startedDecelerating == False:
                self.startMove()
                self.startedDecelerating = True
                print "SD"
            elif self.nowDecelerating == False:
                self.stopMove()
                self.nowDecelerating = True
                self.waiting = 0
                print "ND"
            elif self.doneDecelerating == False:
                self.stopMove()
                self.doneDecelerating = True
                print "DD"
            elif self.finishedDecelerating == False:
                self.startMove()
                self.finishedDecelerating = True
                print "FD"
            else:
                self.flightProfile = self.stopFlight
                self.constantRotate()
                self.done = True
                print "OTHER"

            self.control.up = self.hoverThrust / cos(state.ang - pi)
