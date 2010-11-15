import math

class WallDodger:
    def __init__(self, safetyDistance):
        self.safeDistance = safetyDistance

    def process(self, sensor, state, dt):
        for i in range(0,40):
            if sensor[i].val < self.safeDistance:
                state.targetX += -50*math.sin(sensor[i].ang)
                state.targetY += -25*math.cos(sensor[i].ang)

        return state