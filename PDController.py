import math
from simulation import *
import pygame
from RuleController import RuleController

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


class PDController:

    def __init__(self):
        self.verticalPropGain = 1
        self.verticalDiffGain = -5
        self.horizontalPropGain = 2
        self.horizontalDiffGain = -18
        self.angleGain = 0.1
        self.anglePropGain = 20
        self.angleDiffGain = -25
        self.horizontalForceFeedbackScale = 20

        self.targetX = 700
        self.targetY = 100
        self.firstCycle = True

        self.control = Control()        

    def process(self, sensor, state, dt):

        keyinput = pygame.key.get_pressed()

        if keyinput[pg.K_LEFT]:
            self.targetX -= 10

        if keyinput[pg.K_RIGHT]:
            self.targetX += 10

        if keyinput[pg.K_UP]:
            self.targetY -= 10

        if keyinput[pg.K_DOWN]:
            self.targetY += 10

        print self.targetX, self.targetY

        targetX = self.targetX
        targetY = self.targetY

        # Vertical first:
        self.control.up = (self.control.up - (self.verticalPropGain * (targetY - state.y) + self.verticalDiffGain * state.dydt))

        # Horizontal next:
        horizFeedback = ((targetX - state.x) * self.horizontalPropGain + (self.horizontalDiffGain * state.dxdt)) / self.horizontalForceFeedbackScale

        # Limit feedback so that asin never fails.
        if horizFeedback > 1:
            horizFeedback = 1
        elif horizFeedback < -1:
            horizFeedback = -1

        # This feedback leads to a control angle
        angleChange = math.pi - (self.angleGain * math.asin(horizFeedback)) - state.ang

        # Now the angle control
        horizForce = (self.control.left - self.control.right) + (angleChange * self.anglePropGain + (self.angleDiffGain * state.dangdt))

        if horizForce > 0:
            self.control.left = horizForce
            self.control.right = 0
        else:
            self.control.left = 0
            self.control.right = abs(horizForce)

        self.control.limit()

        return self.control

dt          =.1
brain1      = PDController()
brain2      = RuleController()
nSensors    = 40
sensorRange = 2000
#pod        = CarPod(nSensors,sensorRange,brain,(255,0,0))
pod1        = GravityPod(nSensors,sensorRange,brain1,(255,0,0))
pod2        = GravityPod(nSensors,sensorRange,brain2,(0,255,0))
pods        = [pod1, pod2]
world       = World("rect_world.txt",pods)
sim         = Simulation(world,dt)

#uncomment the next line to hide the walls.
#sim.world.blind=True

sim.slowMotionFactor=1

painter = Painter(world)
sim.painter = painter

sim.run()
