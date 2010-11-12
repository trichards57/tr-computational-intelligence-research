from simulation import *
import math
import csv

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

class RouteNavigator:
    def __init__(self, state, filename):
        self.currentCoordinate = 0

        self.coordinates = []

        reader = csv.reader(open(filename, 'r'), delimiter=',')

        for row in reader:
            coord = (float(row[0]), float(row[1]))
            self.coordinates.append(coord)

    def process(self, sensor, state, dt):
        xErr = state.x - self.coordinates[self.currentCoordinate][0]
        yErr = state.y - self.coordinates[self.currentCoordinate][1]

        dist = math.sqrt(math.pow(xErr,2) + math.pow(yErr,2))

        if dist < 4 and self.currentCoordinate < (len(self.coordinates) - 1):
            self.currentCoordinate += 1

        return (self.coordinates[self.currentCoordinate][0], self.coordinates[self.currentCoordinate][1])
