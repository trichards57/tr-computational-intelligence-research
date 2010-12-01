## @package Recorders
# Contains all of the recorders used to dump sensor and state data to a file
# during the simulation.
#
# A recorder must implement the following function:
#
# process(self, sensor, state)

## Route Recorder
#
# This recorder records the current position of the pod.
class RouteRecorder:
    ## The RouteRecorder constructor
    #
    # @param self The object pointer
    # @param file_name The name of the file to store to
    def __init__(self, file_name):
        ## The file the data is saved to.
        self.file = open(file_name, 'w')

    ## The process function for the RouteRecorder
    #
    # @param self The object pointer
    # @param sensor A list of Sensor objects. Unused.
    # @param state The current state of the pod. Must include the following properties:
    #        - x
    #        - y
    # @return None
    def process(self, sensor, state):
        sensor_data = str(state.x) + ',' + str(state.y)
        self.file.write(sensor_data + '\n')

## Sensor Recorder
#
# This recorder records the current position and sensor values of the pod.
class SensorRecorder:
    ## The RouteRecorder constructor
    #
    # @param self The object pointer
    # @param file_name The name of the file to store to
    def __init__(self, file_name):
        ## The file the data is saved to.
        self.file = open(file_name, 'w')

    ## The process function for the SensorRecorder
    #
    # @param self The object pointer
    # @param sensor A list of Sensor objects. Must be a list of objects with the following properties:
    #        - ang
    #        - val
    #        - wall
    # @param state The current state of the pod. Must include the following properties:
    #        - x
    #        - y
    # @return None
    def process(self, sensor, state):
        sensor_data = str(state.x) + ',' + str(state.y) + ','
        for i in range(0, 40):
            sensor_data += str(sensor[i].ang) + ',' + str(sensor[i].val) \
                + ',' + sensor[i].wall + ','
        self.file.write(sensor_data + '\n')
