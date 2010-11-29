class RouteRecorder:
    def __init__(self, file_name):
        self.file = open(file_name, 'w')

    def process(self, sensor, state):
        sensor_data = str(state.x) + ',' + str(state.y)
        self.file.write(sensor_data + '\n')

class SensorRecorder:
    def __init__(self, file_name):
        self.file = open(file_name, 'w')

    def process(self, sensor, state):
        sensor_data = str(state.x) + ',' + str(state.y) + ','
        for i in range(0, 40):
            sensor_data += str(sensor[i].ang) + ',' + str(sensor[i].val) \
                + ',' + sensor[i].wall + ','
        self.file.write(sensor_data + '\n')
