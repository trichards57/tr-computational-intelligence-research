from simulation import *
from csv import *
import pygame 

#
#    Manual drive a car around a track
#


class CursorControl:

    def process(self,sensor,state,dt):
        control=Control()
        keyinput = pygame.key.get_pressed()
        write = False

        if keyinput[pg.K_LEFT]:
            control.left=1
            write = True

        if keyinput[pg.K_RIGHT]:
            control.right=1
            write = True

        if keyinput[pg.K_UP]:
            control.up=1
            write = True

        if keyinput[pg.K_DOWN]:
            control.down=1
            write = True

        if write == True:
            outFile = open('wallFollowData.csv', 'a')

            for s in range(0, 40):
                outFile.write(str(sensor[s].val) + ',')

            outFile.write(str(control.up) + ',' + str(control.right) + ',' + str(control.down) + ',' + str(control.left) + '\n')
            outFile.close

        return control



dt          =.1
brain       = CursorControl()
nSensors    = 40
sensorRange = 2000
# pod         = CarPod(nSensors,sensorRange,brain,(255,0,0))
pod         = SimplePod(nSensors,sensorRange,brain,(255,0,0), 10)
pods        = [pod]
world       = World("world.txt",pods)
sim         = Simulation(world,dt)

#uncomment the next line to hide the walls.
#sim.world.blind=True


sim.run()
