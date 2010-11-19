import pygame

class TargetCoordinatePainter:
    def __init__(self):
        self.postDraw = None
        self.targetX = 0
        self.targetY = 0

    def preDraw(self, screen):
        col = (255,255,255)
        pygame.draw.line(screen, col, (self.targetX - 10, self.targetY), (self.targetX + 10, self.targetY), 3)
        pygame.draw.line(screen, col, (self.targetX, self.targetY - 10), (self.targetX, self.targetY + 10), 3)