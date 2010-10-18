from simulation import Control

## Base class for all Personalities.
#
# This class initialises the zeroThreshold, hoverThrust and control variables
# that are used by every personality.
class Personality:
    ## The value that is considered small enough to be assumed zero.
    #
    # Settings this value to a larger number makes the system more tolerant of
    # residual velocities.
    #
    # If this value is too high, the pod will drift while it tries to hover.
    # If the value is too small, the pod will take longer to stop.
    zeroThreshold = 1e-4
    
    def __init__(self, hoverThrust):
        ## The downwards thrust required to maintain the pod at zero vertical acceleration.
        self.hoverThrust = hoverThrust
        ## The Control object that describes the thrust values desired.
        self.control = Control()

        ## Indicates if the personality has achieved its goal.
        self.done = False