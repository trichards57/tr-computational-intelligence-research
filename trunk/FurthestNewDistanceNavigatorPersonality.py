from math import fabs

from HorizontalMovePersonality import HorizontalMovePersonality
from Personality import Personality
from VerticalMovePersonality import VerticalMovePersonality

class FurthestNewDistanceNavigatorPersonality(Personality):

    # Direction Constants
    # Numbers chosen so that -up is down and -left = right
    up = 1
    down = -1
    left = 2
    right = -2

    def __init__(self, hoverThrust):
        Personality.__init__(self, hoverThrust)
        self.direction = 0
        self.personality = None

    def process(self, state):
        if self.personality != None and self.personality.done == False:
            print "Handling inner personality"
            self.personality.process(state)
            self.control = self.personality.control
        else:
            upSensor = state.sensor[0].val
            rightSensor = state.sensor[10].val
            downSensor = state.sensor[20].val
            leftSensor = state.sensor[30].val

            newDirection = None
            newDistance = 0

            if self.direction != self.down:
                # If we didn't just go down, default to up.
                newDirection = self.up
                newDistance = upSensor
            if self.direction != self.right and newDistance < rightSensor:
                # If we didn't just go left, go right if it will take us further
                newDirection = self.left
                newDistance = rightSensor
            if self.direction != self.up and newDistance < downSensor:
                # If we didn't just go up, go down if it will take us further
                newDirection = self.down
                newDistance = downSensor
            if self.direction != self.left and newDistance < leftSensor:
                # If we didn't just go right, go left if it will take us further
                newDirection = self.right
                newDistance = leftSensor

            newDistance -= 10

            if newDirection == self.up:
                # Going up.
                newCoordinate = state.y - newDistance
            elif newDirection == self.down:
                # Going down.
                newCoordinate = state.y + newDistance
            elif newDirection == self.left:
                # Going left.
                newCoordinate = state.x - newDistance
            elif newDirection == self.right:
                # Going right.
                newCoordinate = state.x + newDistance

            if fabs(newDirection) == self.up:
                self.personality = VerticalMovePersonality(self.hoverThrust, state, newCoordinate)
                self.personality.process(state)
            elif fabs(newDirection) == self.left:
                self.personality = HorizontalMovePersonality(self.hoverThrust, state, newCoordinate)
                self.personality.process(state)

            self.direction = newDirection
            self.control = self.personality.control