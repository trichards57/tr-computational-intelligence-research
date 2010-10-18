from Personality    import Personality
## @package HorizontalMoveLearnerPersonality
# This module contains the personality that handles the horizontal move learning.
# It concentrates on testing horizontal movement to collect data that allows the
# acceleration performance to be approximated with a neural network.
#
# If the neural network does not perform a good enough job, this will be removed
# and the acceleration will be calculated mathematically.
#
# @note The data produced from this is easily calculated using the formula.
# @todo Modify the experiment to allow the pod to rotate for longer

from math import cos
from math import pi

from Personality import Personality

## The personality responsible for producing training data related to horizontal movement.
#
# This personality will produce a CSV that contains the training data required 
# to train the neural network used to control horizontal acceleration.  The file
# produced is of the format:
#
# @code
# Turning Thrust, Acceleration Time, Time Step, Starting Velocity, Ending Velocity
# @endcode
#
# It is then saved in to the file learningData.csv. If the file already exists,
# the new data will be appended to it.
class HorizontalMoveLearnerPersonality(Personality):
    ## Class initialiser. Initialises all the member variables to their starting
    # states.
    def __init__(self, hoverThrust, rotateThrust, accelTime):
        Personality.__init__(hoverThrust)
        # State Machine Variables
        ## @stateMachineVar begun the turn that starts the pod accelerating.
        self.turning = False
        ## @stateMachineVar ended the turn that starts the pod accelerating.
        self.turnDone = False
        ## @stateMachineVar begun the turn that stops the pod accelerating.
        self.stopping = False
        ## @stateMachineVar ended the turn that stops the pod accelerating.
        self.stoppingDone = False
        ## @stateMachineVar begun the turn that starts the pod decelerating.
        self.slowing = False
        ## @stateMachineVar ended the turn that starts the pod deceleraring.
        self.slowed = False
        ## @stateMachineVar begun the turn that stops the pod decelerating.
        self.stopping2 = False
        ## @stateMachineVar ended the turn that stops the pod decelerating.
        self.stoppingDone2 = False
        ## @stateMachineVar taken the speed reading.
        self.waiting = False

        ## The thrust used to rotate the pod.
        self.rotateThrust = rotateThrust
        ## The time the pod should accelerate for.
        self.accelTime = accelTime

        ## How long the pod has been accelerating for.
        self.accelWait = 0
        ## How long the pod has been decelerating for.
        self.decelWait = 0

        ## File variable used to write turn data to.
        self.file = None

    ## Changes the pods rate of turn by firing the left thruster.
    def accelerateRotateLeft(self, thrust):
        self.control.left = thrust
        self.control.right = 0

    ## Changes the pods rate of turn by firing the right thruster.
    def accelerateRotateRight(self, thrust):
        self.control.left = 0
        self.control.right = thrust

    ## Zeros both thrusters.
    def constantRotate(self):
        self.control.left = 0
        self.control.right = 0

    ## Runs the personality processing.
    def process(self, state):
        if self.turning == False:
            # Start turn to accelerate
            self.file = open('learningData.csv', 'a')
            self.accelerateRotateRight(self.rotateThrust)
            self.turning = True
            self.file.write(str(self.rotateThrust) + ',' + str(self.accelTime) + ',' + str(state.dt) + ',' + str(state.dxdt) + ',')
        elif self.turnDone == False:
            # Stop turn to accelerate
            self.accelerateRotateLeft(self.rotateThrust)
            self.turnDone = True
        elif self.accelWait < self.accelTime:
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
        elif self.decelWait < self.accelTime:
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