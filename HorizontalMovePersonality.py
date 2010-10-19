import Personality
## @module HorizontalMovePersonality
# This module contains the personality that handles horizontal movement.
# It concentrates on moving the pod to a specific coordinate on the x-axis,
# using the neural network trained from the HorizontalMoveLearnerPersonality
# data.
#
# If the neural network does not perform a good enough job, this will be removed
# and the acceleration will be calculated mathematically.

from math import cos
from math import pi
from math import fabs
import pickle

from Personality import Personality

## The personality responsible for moving the pod horizontally.
#
# This personality takes a given x-coordinate and positions the pod
# there by manipulating the pod angle and thrust. The vertical component
# of the thrust must always be the hover thrust, so the acceleration is
# controlled only by the angle the pod is currently pointing at.
#
# The flight profile the pod travels with can be changed. At present, only
# a constant speed profile is available. A constant acceleration profile
# (which should be faster but potentially less accurate) can also be used,
# but is not currently implemented.
#
# The personality leaves the pod at the same y-coordinate it started at,
# pointed directly up, with accelerations at or very near zero.
#
# @todo Implement constant acceleration flight profile.
class HorizontalMovePersonality(Personality):
    ## Represents a continuous speed flight profile.
    continuousSpeedFlight = 1
    ## Represents a continuous acceleration flight profile.
    continuousAccelFlight = 2
    ## Represents a flight profile that brings the pod to a halt.
    stopFlight = -1
    ## How long the pod should accelerate for.
    delay = 50

    ## Class initialiser. Initialises all the member variables to their starting
    # states.
    def __init__(self, hoverThrust, state, destinationX):
        Personality.__init__(self, hoverThrust)

        # State machine variables
        ## @stateMachineVar begun the turn that starts the pod accelerating.
        self.startedAccelerating = False
        ## @stateMachineVar finished the turn that starts the pod accelerating.
        self.nowAccelerating = False
        ## @stateMachineVar begun the turn that stops the pod accelerating.
        self.doneAccelerating = False
        ## @stateMachineVar finished the turn that stops the pod accelerating.
        self.finishedAccelerating = False
        ## @stateMachineVar arrived at the destination y coordinate.
        self.finishedMove = False
        ## @stateMachineVar begun the turn that starts the pod decelerating.
        self.startedDecelerating = False
        ## @stateMachineVar finished the turn that starts the pod decelerating.
        self.nowDecelerating = False
        ## @stateMachineVar begun the turn that stops the pod decelerating
        self.doneDecelerating = False
        ## @stateMachineVar finished the turn that stops the pod decelerating
        self.finishedDecelerating = False

        ## The desired x coordinate of the pod.
        self.destinationX = destinationX
        ## The starting x coordinate of the pod.
        self.originX = state.x
        ## The total distance the pod has to cover.
        self.totalDistance = 0

        ## The neural net used to work out the thrust required for a specific
        # accelerating in a specific time. Loaded from a file, currently
        # produced by the pybrainTest module.
        self.controlNet = pickle.load(open('Neural.net', 'r'))

        ## The maximum speed used in the constant speed profile.
        self.maximumSpeed = 10
        ## The thrust used to turn the pod, as determined by the neural net.
        self.turnThrust = self.controlNet.activate([0,0.8])[0]
        ## The direction the pod will need to head in.
        self.direction = 0

        ## Used by the state machine to time how long it has been
        # accelerating or decelerating.
        self.waiting = 0

        ## Determines the flight profile to use.
        self.flightProfile = self.continuousSpeedFlight

    ## Changes the pods rate of turn by firing the left thruster.
    def accelerateRotateLeft(self, thrust):
        self.control.left = thrust
        self.control.right = 0
        print "L"

    ## Changes the pods rate of turn by firing the right thruster.
    def accelerateRotateRight(self, thrust):
        self.control.left = 0
        self.control.right = thrust
        print "R"

    ## Zeros both thrusters.
    def constantRotate(self):
        self.control.left = 0
        self.control.right = 0
        print "C"

    ## Fire the thrusters so the craft starts to accelerate or stops decelerating.
    def startMove(self):
        print self.direction
        if self.direction == 1:
            # Accelerating right
            self.accelerateRotateRight(self.turnThrust)
        else:
            self.accelerateRotateLeft(self.turnThrust)

    ## Fire the thrusters so the craft stops accerating or starts to decelerate
    def stopMove(self):
        print self.direction
        if self.direction == 1:
            # Accelerating right
            self.accelerateRotateLeft(self.turnThrust)
        else:
            self.accelerateRotateRight(self.turnThrust)

    ## Runs the personality processing.
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
            self.done = True
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
