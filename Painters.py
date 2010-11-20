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