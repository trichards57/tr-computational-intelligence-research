from simulation import *
import pygame 

#
#    gravity pod around the track
#


class CursorControl:

    def process(self,sensor,state,dt):
        control=Control()
        keyinput = pygame.key.get_pressed()

        if keyinput[pg.K_LEFT]:
            control.left=1

        if keyinput[pg.K_RIGHT]:
            control.right=1

        if keyinput[pg.K_UP]:
            control.up=1

        if keyinput[pg.K_DOWN]:
            control.down=1

        return control



dt          =.1
brain       = CursorControl()
nSensors    = 40
sensorRange = 2000
#pod         = CarPod(nSensors,sensorRange,brain,(255,0,0))
pod         = GravityPod(nSensors,sensorRange,brain,(255,0,0))
pods        = [pod]
world       = World("rectworld.txt",pods)
sim         = Simulation(world,dt)

#uncomment the next line to hide the walls.
#sim.world.blind=True

sim.slowMotionFactor=1

sim.run()