## @package Painters
# Contains all of the painters used to display additional data during the
# simulation.
#
# A painter must implement the following functions:
#
# postDraw(self, screen)
# preDraw(self, screen)
#
# If a function is not required, its attribute can be set to None instead.
import pygame

## Target Coordinate Painter
#
# This painter paints a white cross at the pod's current target coordinates.
class TargetCoordinatePainter:
    ## The TargetCoordinatePainter constructor
    def __init__(self):
        ## Post-Draw function. Unused.
        self.postDraw = None
        ## The current target X
        self.target_x = 0
        ## The current target Y
        self.target_y = 0

    ## Pre-Draw Function
    #
    # @param self The object pointer
    # @param screen The screen to paint to
    # @return None
    #
    # Called before the main printing is performed, placing the output below
    # the other elements.
    def preDraw(self, screen):
        col = (255, 255, 255)
        pygame.draw.line(screen, col, (self.target_x - 10, self.target_y), \
            (self.target_x + 10, self.target_y), 3)
        pygame.draw.line(screen, col, (self.target_x, self.target_y - 10), \
            (self.target_x, self.target_y + 10), 3)