## @package Painters
# This module contains all of the painters used to display additional data
# on the screen during the simulation.
#
# A painter must implement the following functions:
#
# preDraw(self, screen)\n
# postDraw(self, screen)
#
# If they are not used, they can also be set to None
import pygame

## Target Coordinate Painter
#
# This painter displays a cross-hairs that indicates the current target
# coordinates of the pod's navigator.
class TargetCoordinatePainter:
    ## Initialises the painter's attributes to 0 and sets postDraw as unused.
    #
    # @selfParam
    def __init__(self):
        ## Draws to the screen.  Called after the main drawing is complete.
        self.postDraw = None
        ## The current target x coordinate.
        self.target_x = 0
        ## The current target y coordinate.
        self.target_y = 0

    ## Draws to the screen. Called before the main drawing is started.
    #
    # @selfParam
    # @param screen The screen to draw to.
    # @return None
    def preDraw(self, screen):
        col = (255, 255, 255)
        pygame.draw.line(screen, col, (self.target_x - 10, self.target_y), \
            (self.target_x + 10, self.target_y), 3)
        pygame.draw.line(screen, col, (self.target_x, self.target_y - 10), \
            (self.target_x, self.target_y + 10), 3)