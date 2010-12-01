## @package Navigators
# Contains all of the controllers used to translate the current system state
# in to a set of target coordinates.
#
# A navigator must implement the following functions:
#
# __init__(self, state)
# process(self, sensor, state, dt)
#
# The process function returns a tuple (target_x, target_y).
#
# The navigator must also have an attribute named end which flags if it has
# reached the end of the route.
import csv
import math

import pygame as pg

## Keyboard-set Coordinate Navigator
#
# This navigator controls the pod's target coordinates based on input from the
# keyboard. An arrow key press causes the target coordinate to move 10 pixels
# in that direction.
#
# @note This navigator makes no attempt to avoid walls.
class KeyboardCoordinateNavigator:
    ## The KeyboardCoordinateNavigator constructor
    #
    # @param self The object pointer
    # @param state The current state of the pod. Must include the following properties:
    #        - x
    #        - y
    #
    # Initialises the navigators target coordinate attributes to the initial
    # position of the pod.
    def __init__(self, state):
        ## The current target x coordinate
        self.target_x = state.x
        ## The current target y coordinate
        self.target_y = state.y
        ## Flag showing if the pod has reached the end
        #
        # Always false for this navigator
        self.end = False

    ## The process function for the KeyboardCoordinateNavigator
    #
    # @param self The object pointer
    # @param sensor A list of Sensor objects.  Unused.
    # @param state The current state of the pod. Unused.
    # @param dt The timestep used by the simulator. Unused.
    # @return A tuple (target_x, target_y)
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

## Route-Following Navigator
#
# This navigator controls the pod's target coordinates based on input from a
# specified comma-separated values file with the following format:
#
# target_x_1, target_y_1\n
# target_x_2, target_y_2\n
# target_x_3, target_y_3\n
# ...\n
# target_x_n, target_y_n\n
#
# @note This navigator makes no attempt to avoid walls.
class RouteNavigator:
    ## The RouteNavigator constructor
    #
    # @param self The object pointer
    # @param state The current state of the pod. Unused.
    # @param filename The name of the file to load the route from.
    #
    # Initialises the navigator and loads the route in to the @ref coordinates
    # attribute.
    def __init__(self, state, filename):
        ## The index of the current coordinate in the route
        self.current_coordinate = 0
        ## The list of coordinates that make up the route
        self.coordinates = []
        ## Flag showing if the pod has reached the end
        #
        # Set to true when the pod is within 20 pixels of the final target
        # coordinate.
        self.end = False

        reader = csv.reader(open(filename, 'r'), delimiter=',')

        for row in reader:
            coord = (float(row[0]), float(row[1]))
            self.coordinates.append(coord)

    ## The process function for the RouteNavigator
    #
    # @param self The object pointer
    # @param sensor A list of Sensor objects. Unused.
    # @param state The current state of the pod. Must include the following properties:
    #        - x
    #        - y
    # @param dt The timestep used by the simulator. Unused.
    # @return A tuple (target_x, target_y)
    #
    # This function monitors the current location of the pod.  Once it is within
    # 20 px of the current target coordinate, it moves on to the next one. Once
    # it is within 20 px of the final coordinate, it sets the @ref end attribute
    # to true.
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
