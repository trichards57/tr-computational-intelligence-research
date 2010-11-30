import csv
import math

import pygame as pg

class KeyboardCoordinateNavigator:
    def __init__(self, state):
        self.target_x = state.x
        self.target_y = state.y
        self.end = False
        
    def process(self, sensor, state, dt):
        keyinput = pg.key.get_pressed()

        if keyinput[pg.K_LEFT]:
            self.target_x -= 10
        if keyinput[pg.K_RIGHT]:
            self.target_x += 10
        if keyinput[pg.K_UP]:
            self.target_y -= 10
        if keyinput[pg.K_DOWN]:
            self.target_y += 10

        return (self.target_x, self.target_y)

class RouteNavigator:
    def __init__(self, state, filename):
        self.current_coordinate = 0

        self.coordinates = []

        self.end = False

        reader = csv.reader(open(filename, 'r'), delimiter=',')

        for row in reader:
            coord = (float(row[0]), float(row[1]))
            self.coordinates.append(coord)

    def process(self, sensor, state, dt):
        x_error = state.x - self.coordinates[self.current_coordinate][0]
        y_error = state.y - self.coordinates[self.current_coordinate][1]

        dist = math.sqrt(math.pow(x_error, 2) + math.pow(y_error, 2))

        if dist < 20 and self.current_coordinate < (len(self.coordinates) - 1):
            self.current_coordinate += 1
        elif dist < 20:
            self.end = True

        return (self.coordinates[self.current_coordinate][0], \
                self.coordinates[self.current_coordinate][1])
