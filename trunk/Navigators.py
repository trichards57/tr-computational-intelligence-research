from simulation import *

class KeyboardCoordinateNavigator:
    def __init__(self, state):
        self.targetX = state.x
        self.targetY = state.y
        
    def process(self, sensor, state, dt):
        keyinput = pg.key.get_pressed()

        if keyinput[pg.K_LEFT]:
            self.targetX-=10
        if keyinput[pg.K_RIGHT]:
            self.targetX+=10
        if keyinput[pg.K_UP]:
            self.targetY-=10
        if keyinput[pg.K_DOWN]:
            self.targetY+=10

        return (self.targetX, self.targetY)