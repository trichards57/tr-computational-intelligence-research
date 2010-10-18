from math import fabs

from Personality import Personality
from StopPersonality import StopPersonality

class VerticalMovePersonality(Personality):
    # The desired end position of the pod
    destinationY = 0
    # The starting position of the pod
    originY = 0
    # The total distance to cover
    totalDistance = 0

    # The thrust used to move the pod
    maneuverThrust = 0
    # The maximum speed the pod will move at
    maximumSpeed = 10

    # Enumeration for flight profiles
    continuousSpeedFlight = 1 # Fly at a set speed
    continuousAccelFlight = 2 # Fly with constant acceleration
    stopFlight = -1

    # Determines which flight profile to use
    flightProfile = continuousSpeedFlight

    stopper = None

    def __init__(self, hoverThrust, state, destinationY):
        self.hoverThrust = hoverThrust
        self.destinationY = destinationY
        self.originY = state.y
        self.stopper = StopPersonality(hoverThrust)
        self.stopper.control = self.control

        # Get the largest possible moving force that
        # can be canceled in an equal amount of time
        # without rotating the pod.

        # Moving force can't be greater than the hover thrust,
        # otherwise the speeding and slowing thrusts will
        # unbalanced
        self.maneuverThrust = hoverThrust

        # The sum of the moving force and the hover thrust
        # cannot be greater than 1, otherwise the thruster
        # will max out and the speeding and slowing thrusts
        # will be unbalanced
        if self.maneuverThrust + hoverThrust > 1:
            self.maneuverThrust = 1 - hoverThrust

    def process(self, state):
        print "Vertical Move"
        # How far do we have left to go?
        distanceRemaining = self.destinationY - state.y
        print "Destination : ", self.destinationY
        print "Distance Remaining : ", distanceRemaining
        if distanceRemaining > 0:
            direction = -1
        else:
            direction = 1
        if self.stopper.done == True:
            self.done = True
            self.control.up = self.hoverThrust
        if self.flightProfile == self.stopFlight:
            # Close enough. Stop.
            print "Stopping"
            self.stopper.process(state)
        elif self.flightProfile == self.continuousSpeedFlight:
            if fabs(distanceRemaining) < 4:
                # Close enough. Stop the pod.
                self.flightProfile = self.stopFlight
                self.stopper.process(state)
            # Flight at a constant speed of self.maximumSpeed
            if fabs(state.dydt) < self.maximumSpeed:
                # Not yet at flight speed. Accelerate/Decelerate as quickly as possible.
                self.control.up = direction * 0.1 + self.hoverThrust
            else:
                # At flight speed. Maintain zero acceleration
                self.control.up = self.hoverThrust
                # TODO : Implement continuous thrust profile
