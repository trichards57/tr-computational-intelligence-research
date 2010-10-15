from simulation import *
from collections import deque

class HorizontalMoveLearnerPersonality:
    # Setting this to a larger number makes the system more tolerant of residual
    # velocities.
    # If too high, the pod will drift when it is supposed to be hovering.
    # The larger the number, the faster it will drift.
    # If too small, cancelAcceleration and cancelVelocity will take longer.
    zeroThreshold = 1e-4
    # Stores the value of thrust required to cancel out the acceleration due to gravity
    hoverThrust = 0
    # The step used during cancelAcceleration to modify cancelThrust.
    thrustStep = 0.5
    # The Control object that needs to be changed
    control = Control()

    # Has the Personality achieved it's goal (in this case, moved the pod to the required y position)?
    done = False

    turning = False
    turnDone = False
    stopping = False
    stoppingDone = False
    slowing = False
    slowed = False
    stopping2 = False
    stoppingDone2 = False
    waiting = 0

    rotateThrust = 0.0

    def __init__(self, hoverThrust, rotateThrust):
        self.hoverThrust = hoverThrust
        self.rotateThrust = rotateThrust

    def accelerateRotateLeft(self, thrust):
        self.control.left = thrust
        self.control.right = 0

    def accelerateRotateRight(self, thrust):
        self.control.left = 0
        self.control.right = thrust

    def constantRotate(self):
        self.control.left = 0
        self.control.right = 0

    def process(self, state):
        if self.turning == False:
            # Start turn to accelerate
            self.accelerateRotateRight(self.rotateThrust)
            self.turning = True
            print self.turning
        elif self.turnDone == False:
            # Stop turn to accelerate
            self.accelerateRotateLeft(self.rotateThrust)
            self.turnDone = True
        elif self.waiting < 100:
            # Wait for 10 seconds
            self.constantRotate()
            self.waiting += 1
        elif self.stopping == False:
            # Start turn to stop accelerating
            self.accelerateRotateLeft(self.rotateThrust)
            self.stopping = True
        elif self.stoppingDone == False:
            # Stop turn to stop accelerating
            self.accelerateRotateRight(self.rotateThrust)
            self.stoppingDone = True
        elif self.slowing == False:
            # Start turn to decelerate
            self.accelerateRotateLeft(self.rotateThrust)
            self.slowing = True
        elif self.slowed == False:
            # Stop turn to decelerate
            self.accelerateRotateRight(self.rotateThrust)
            self.slowed = True
            # Wait
            self.waiting = 0
        elif self.stopping2 == False:
            # Start turn to stop decelerating
            self.accelerateRotateRight(self.rotateThrust)
            self.stopping2 = True
        elif self.stoppingDone2 == False:
            # Stop turn to stop decelerating
            self.accelerateRotateLeft(self.rotateThrust)
            self.stoppingDone2 = True
        else:
            # Done
            self.done = True
            self.constantRotate()

        # Work out thrust required to hover. Force in the horizontal direction causes the acceleration
        self.control.up = self.hoverThrust / cos(state.ang - pi)