from simulation import *

class RouteRecorder:
    def __init__(self, fileName):
        self.file = open(fileName, 'w')

    def process(self, sensor, state):
        sensorData = str(state.x) + ',' + str(state.y)
        self.file.write(sensorData + '\n')

class SensorRecorder:
    def __init__(self, fileName):
        self.file = open(fileName, 'w')

    def process(self, sensor, state):
        sensorData = str(state.x) + ',' + str(state.y) + ','
        for i in range(0,40):
            sensorData += str(sensor[i].ang) + ',' + str(sensor[i].val) + ',' + sensor[i].wall + ','
        self.file.write(sensorData + '\n')
