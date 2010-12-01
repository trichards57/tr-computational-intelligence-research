## @package WallDodgers
# Contains the wall dodger used to alter target coordinates so that the pod avoids
# walls.
import math

## Wall Dodger
#
# This takes the current pod state and its target coordinates, and then
# manipulates the coordinates to ensure that the pod remains a specified distance
# from the walls.
class WallDodger:
    ## The WallDodger constructor
    #
    # @param self The object pointer
    # @param safety_distance The distance the wall dodger should try and keep
    # between the pod and the wall.
    def __init__(self, safety_distance):
        ## The distance the wall dodger will try and keep between the pod and
        # the wall.
        self.safe_distance = safety_distance

    ## The process function for the WallDodger
    #
    # @param self The object pointer
    # @param sensor A list of Sensor objects. Must be a list of objects with the following properties:
    #        - ang
    #        - val
    #        - wall
    # @param state The current state of the pod. Must include the following properties:
    #        - target_x
    #        - target_y
    # @param dt The timestep of the simulator. Unused.
    # @return Returns state with the modified target_x and target_y attributes.
    #
    # The algorithm works by checking each sensor reading. If the sensor reports
    # a wall closer than @ref safe_distance, it manipulates the target coordinates
    # so that the pod moves away from that wall.  If multiple walls are detected
    # within the specified range, the 'repulsive force' from each sensor will
    # be added together. This has the side-effect of causing stronger responses
    # when nearer the wall (as more sensors report a wall too close).
    def process(self, sensor, state, dt):
        for i in range(0, 40):
            if sensor[i].val < self.safe_distance:
                state.target_x += -50*math.sin(sensor[i].ang)
                state.target_y += -25*math.cos(sensor[i].ang)

        return state