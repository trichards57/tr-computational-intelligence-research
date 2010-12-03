## @package WallDodgers
# This module contains the wall dodger object which handles the changing of
# target coordinates so that the pod attempts to avoid walls.

import math

## Wall Dodger
#
# The wall dodger functions by altering the current target coordinates so that
# they move in direction opposite of any walls that are within the specified
# safe distance.  If multiple walls are within the safe distance (inferring that
# the wall is very close), the effect to move the pod away is much stronger than,
# resulting in a faster movement away.  The effects are tailered to give a
# stronger horizontal response, to allow for the slower accelerations and
# maximum horizontal speeds.
class WallDodger:
    ## Initialises the wall dodger's safe_distance to the specified value.
    #
    # @selfParam
    # @param safety_distance The distance the wall dodger will aim to maintain
    # between the pod and the walls.
    def __init__(self, safety_distance):
        ## The distance the wall dodger will aim to maintain between the pod and
        # the walls.
        self.safe_distance = safety_distance

    ## Processes the current state and alters the target coordinates in the
    # state argument to move the pod away from walls.
    #
    # @selfParam
    # @sensorParam
    # @stateParam
    # @timestepParam Unused.
    # @returns The modified state param with the new target coordinates.
    def process(self, sensor, state, dt):
        for i in range(0, 40):
            if sensor[i].val < self.safe_distance:
                state.target_x += -50*math.sin(sensor[i].ang)
                state.target_y += -25*math.cos(sensor[i].ang)

        return state