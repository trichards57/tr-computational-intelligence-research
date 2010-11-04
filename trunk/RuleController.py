from simulation import *
import pygame
import math

highY = 100
lowY = 700
rightX = 700
leftX = 100

topRight = (rightX, highY)
topLeft = (leftX, highY)
bottomRight = (rightX, lowY)
bottomLeft = (leftX, lowY)

class Painter:
    def __init__(self, world):
        self.postDraw = None
        self.world = world

    def preDraw(self, screen):
        white=(255,255,255)
        red = (255,0,0)
        green = (0,255,0)
        blue = (0,0,255)
        pg.draw.line(screen,white,(0,highY),(self.world.rect.width,highY),2)
        pg.draw.line(screen,red,(0,lowY),(self.world.rect.width,lowY),2)
        pg.draw.line(screen,green,(rightX,0),(rightX,self.world.rect.height),2)
        pg.draw.line(screen,blue,(leftX,0),(leftX,self.world.rect.height),2)

class RuleController:
    def __init__(self):
        self.lastSideForce = 0
        self.targetX = 700
        self.targetY = 100
        self.targetAng = 0

    def process(self, sensor, state, dt):
        control = Control()
        keyinput = pygame.key.get_pressed()

        if keyinput[pg.K_LEFT]:
            self.targetX -= 10

        if keyinput[pg.K_RIGHT]:
            self.targetX += 10

        if keyinput[pg.K_UP]:
            self.targetY -= 10

        if keyinput[pg.K_DOWN]:
            self.targetY += 10

        yError = self.targetY - state.y
        xError = self.targetX - state.x
        normAng = (2 * pi) - ( (state.ang + pi) % (2 * pi))
        if normAng > pi:
            normAng -= 2*pi

        if yError < 0:
            control.up = 0.5

        if fabs(yError) > 100:
            maxSpeed = 20
        elif fabs(yError) > 40:
            maxSpeed = 5
        else:
            maxSpeed = 2.5

        if state.dydt < -maxSpeed:
            control.up = 0.1
        if state.dydt > maxSpeed:
            control.up = 0.3

        if xError > 0:
            self.targetAng = 0.1
        elif xError < 0:
            self.targetAng = -0.1
        if fabs(xError) > 100:
            maxSpeed = 20
        elif fabs(xError) > 40:
            maxSpeed = 5
        else:
            maxSpeed = 2.5
        if state.dxdt > maxSpeed:
            self.targetAng = -0.1
        if state.dxdt < -maxSpeed:
            self.targetAng = 0.1

        angError = self.targetAng - normAng

        sideForce = (angError * 6 + state.dangdt * 5)

        if sideForce > 0:
            control.right = sideForce
            control.left = 0
        else:
            control.left = -sideForce
            control.right = 0

        self.lastSideForce = sideForce

        return control