import math

class WallDodger:
    def __init__(self, safety_distance):
        self.safe_distance = safety_distance

    def process(self, sensor, state, dt):
        for i in range(0, 40):
            if sensor[i].val < self.safe_distance:
                state.target_x += -50*math.sin(sensor[i].ang)
                state.target_y += -25*math.cos(sensor[i].ang)

        return state