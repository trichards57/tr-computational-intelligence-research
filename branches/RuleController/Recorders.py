## @package Recorders
# This module contains all of the recorders used to store data in CSV files
# so that it can be retrieved later.  They are primarily used to transfer data
# to external programs for more computationally intensive algorithms,
# e.g route finding.
#
# They must implement the following functions:
#
# __init__(self, file_name)\n
# process(self, sensor, state)
#
# The should produce a CSV file with the data stored in columns, where each row
# is at point in time.

## Route Recorder
#
# This recorder stores only the current coordinates of the pod.  This produces
# a file containing the route of the pod through the environment.
class RouteRecorder:
    ## Initialises the recorder to store it's data in the specified file.
    #
    # @selfParam
    # @param file_name The desired name of the output file.  Must have write
    # permissions to the file.
    def __init__(self, file_name):
        ## The file the output is output to.
        self.file = open(file_name, 'w')

    ## Triggers the recorder to store the current location of the pod.
    #
    # @selfParam
    # @sensorParam Unused.
    # @stateParam
    # @return None
    def process(self, sensor, state):
        sensor_data = str(state.x) + ',' + str(state.y)
        self.file.write(sensor_data + '\n')

## Sensor Recorder
#
# This recorder stores the current coordinates of the pod, as well as the
# current state of each of the sensors.  This produces a file containing the
# route of the pod through the environment and everything the sensors are
# reporting about their environment. The sensor data is in terms of the sensor
# angle and reading range and type.
class SensorRecorder:
    ## Initialises the recorder to store it's data in the specified file.
    #
    # @selfParam
    # @param file_name The desired name of the output file.  Must have write
    # permissions to the file.
    def __init__(self, file_name):
        ## The file the output is output to.
        self.file = open(file_name, 'w')

    ## Triggers the recorder to store the current location and sensor state of
    # the pod.
    #
    # @selfParam
    # @sensorParam
    # @stateParam
    # @return None
    def process(self, sensor, state):
        sensor_data = str(state.x) + ',' + str(state.y) + ','
        for i in range(0, 40):
            sensor_data += str(sensor[i].ang) + ',' + str(sensor[i].val) \
                + ',' + sensor[i].wall + ','
        self.file.write(sensor_data + '\n')
