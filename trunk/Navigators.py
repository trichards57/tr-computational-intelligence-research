from simulation import *
import math

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

class KeyboardDirectionNavigator:
    
    def __init__(self, state):
        self.targetX = state.x
        self.targetY = state.y
        self.firstRun = True
        
    def process(self, sensor, state, dt):
        keyinput = pg.key.get_pressed()

        if self.firstRun:
            self.firstRun = False
            return (self.targetX, self.targetY)

        if keyinput[pg.K_LEFT]:
            self.targetX=state.x-100
        if keyinput[pg.K_RIGHT]:
            self.targetX=state.x+100
        if keyinput[pg.K_UP]:
            self.targetY=state.y-100
        if keyinput[pg.K_DOWN]:
            self.targetY=state.y+100

        return (self.targetX, self.targetY)

class LongestDistanceNavigator:
    def __init__(self, state):
        self.targetX = state.x
        self.targetY = state.y
        self.lastSensor = 0

    def process(self, sensor, state, dt):
        if sensor == None:
            return (self.targetX, self.targetY)
        xError = self.targetX - state.x
        yError = self.targetY - state.y

        dispError = math.sqrt(math.pow(xError,2) + math.pow(yError,2))

        if dispError < 20:
            # Close enough. Find the next point.
            longestSensor = -1
            longestDistance = -1
            oppositeLastSensor = (self.lastSensor + 20) % 40
            for i in range(0,40):
                if i != oppositeLastSensor and sensor[i].val > longestDistance:
                    longestSensor = i
                    longestDistance = sensor[i].val

            print "Longest sensor :", longestSensor
            print "Angle :", sensor[longestSensor].ang % (2 *pi)
            print "Range :", sensor[longestSensor].val

            self.targetX += (sensor[longestSensor].val-50)*math.sin(sensor[longestSensor].ang)
            self.targetY += (sensor[longestSensor].val-50)*math.cos(sensor[longestSensor].ang)

            print self.targetX, self.targetY



        return (self.targetX, self.targetY)

