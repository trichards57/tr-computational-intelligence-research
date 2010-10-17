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
    waiting = False
    accelWait = 0
    decelWait = 0

    rotateThrust = 0.0
    rotateTime = 0

    file = None

    def __init__(self, hoverThrust, rotateThrust, rotateTime):
        self.hoverThrust = hoverThrust
        self.rotateThrust = rotateThrust
        self.rotateTime = rotateTime
        #self.file.write(',Start accelerating,,Stop accelerating,,Start decelerating,,Stop decelerating\n')
        #self.file.write('Turn thrust,Time step,Start Turn 1 Velocity, Stop Turn 1 Velocity, Start Turn 2 Velocity, Stop Turn 2 Velocity, Wait, Start Turn 3 Velocity, Stop Turn 3 Velocity, Start Turn 4 Velocity, Stop Turn 4 Velocity, Final\n')

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
            self.file = open('learningData.csv', 'a')
            self.accelerateRotateRight(self.rotateThrust)
            self.turning = True
            self.file.write(str(self.rotateThrust) + ',' + str(self.rotateTime) + ',' + str(state.dt) + ',' + str(state.dxdt) + ',')
        elif self.turnDone == False:
            # Stop turn to accelerate
            self.accelerateRotateLeft(self.rotateThrust)
            self.turnDone = True
        elif self.accelWait < self.rotateTime:
            # Allow the pod to accelerate
            self.constantRotate()
            self.accelWait += 1
        elif self.stopping == False:
            # Start turn to stop accelerating
            self.accelerateRotateLeft(self.rotateThrust)
            self.stopping = True
        elif self.stoppingDone == False:
            # Stop turn to stop accelerating
            self.accelerateRotateRight(self.rotateThrust)
            self.stoppingDone = True
        elif self.waiting == False:
            # Get the final speed
            self.constantRotate()
            self.file.write(str(state.dxdt) + '\n')
            self.waiting = True
        elif self.slowing == False:
            # Start turn to decelerate
            self.accelerateRotateLeft(self.rotateThrust)
            self.slowing = True
        elif self.slowed == False:
            # Stop turn to decelerate
            self.accelerateRotateRight(self.rotateThrust)
            self.slowed = True
        elif self.decelWait < self.rotateTime:
            # Allow the pod to decelerate
            self.constantRotate()
            self.decelWait += 1
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
            self.file.close()

        print "Turn Test"

        # Work out thrust required to hover. Force in the horizontal direction causes the acceleration
        self.control.up = self.hoverThrust / cos(state.ang - pi)