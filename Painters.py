## @package Navigators
# This module contains all of the navigators used to translate current sensor
# and state data in to target coordinates.
#
# A navigator must implement the following function:
#
# process(self, sensor, state, dt)
#
# This function returns a tuple such that: (targetX, targetY)
import pygame

class TargetCoordinatePainter:
    def __init__(self):
        self.postDraw = None
        self.target_x = 0
        self.target_y = 0

    def preDraw(self, screen):
        col = (255, 255, 255)
        pygame.draw.line(screen, col, (self.target_x - 10, self.target_y), \
            (self.target_x + 10, self.target_y), 3)
        pygame.draw.line(screen, col, (self.target_x, self.target_y - 10), \
            (self.target_x, self.target_y + 10), 3)