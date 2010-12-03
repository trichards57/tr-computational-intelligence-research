## @package Navigators
# This module contains all of the navigators used to translate current sensor
# and state data in to target coordinates.
#
# A navigator must implement the following function:
#
# process(self, sensor, state, dt)
#
# This function returns a tuple such that: (targetX, targetY)
import csv
import math

import pygame as pg

## Keyboard Controlled Coordinate Navigator
#
# This navigator controls the target coordinates based on arrow-key presses.
# Each key presses moves the target in the given direction by 10px at a time.
class KeyboardCoordinateNavigator:
    ## Initialises the navigator's target coordinates to the pod's current
    # state.
    #
    # @selfParam
    # @stateParam
    def __init__(self, state):
        ## The x-coordinate the navigator is currently set to.
        self.target_x = state.x
        ## The y-coordinate the navigator is currently set to.
        self.target_y = state.y
        ## Indicates if the navigator has reached the end.
        #
        # @note Always false as this navigator has no ultimate destination.
        self.end = False

    ## @navigatorProcess
    #
    # @selfParam
    # @sensorParam Unused
    # @stateParam Unused
    # @timestepParam Unused
    # @return A tuple of (targetX, targetY)
    #
    # Each arrow key press causes the navigator's target to move in the
    # direction specified by 10px.
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

## Pre-programmed Route Navigator
#
# This navigator controls the target coordinates based on a route defined in the
# file specified in __init__.
class RouteNavigator:
    ## Initialises the navigator's target coordinates to the first coordinate
    # on the route.
    #
    # @selfParam
    # @stateParam Unusued
    # @param filename The name of the file to load the route from.
    #
    # The route file must be a CSV file, with the format:
    #
    # targetX1, targetY1
    # targetX2, targetY2
    # targetX3, targetY3
    #
    # The first two columns must only contain floating point numbers (or
    # integers).  Any other columns will be ignored.
    def __init__(self, state, filename):
        ## The current coordinate index the navigator is working with.
        self.current_coordinate = 0
        ## The route as a list of coordinates.
        self.coordinates = []
        ## Indicates if the navigator has reached the end.
        self.end = False

        reader = csv.reader(open(filename, 'r'), delimiter=',')

        for row in reader:
            coord = (float(row[0]), float(row[1]))
            self.coordinates.append(coord)

    ## @navigatorProcess
    #
    # @selfParam
    # @sensorParam Unused
    # @stateParam
    # @timestepParam Unused
    # @return A tuple of (targetX, targetY)
    #
    # The navigator returns the current coordinate, unless it is currently within
    # 20px of that coordinate.  In this case, the navigator moves to the next
    # point in the list.  If there are no more coordinates, it remains on the
    # final one and sets end to true.
    def process(self, sensor, state, dt):
        x_error = state.x - self.coordinates[self.current_coordinate][0]
        y_error = state.y - self.coordinates[self.current_coordinate][1]
        target_dist = 20

        dist = math.sqrt(math.pow(x_error, 2) + math.pow(y_error, 2))

        if dist < target_dist and self.current_coordinate < (len(self.coordinates) - 1):
            self.current_coordinate += 1
        elif dist < target_dist:
            self.end = True

        return (self.coordinates[self.current_coordinate][0], \
                self.coordinates[self.current_coordinate][1])
