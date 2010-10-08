from simulation import *
#
#     get the gravity pod to hover at y=yHover
#

yHover   =  300
dydtMin  = -100
dydtMax  =  10

class LearningControl:

    lastAccel = 0
    lastDyDt = 0
    hoverThrust = 0
    thrustStep = 0.9

    def cancelAcceleration(self,state):
        accel = state.dydt - self.lastDyDt

        if accel > 0:
            # Accelerating down, increase thrust
            if self.lastAccel < 0:
                # Was accelerating up last time, step is too large.
                self.thrustStep /= 2

            self.hoverThrust = self.hoverThrust + self.thrustStep
        else:
            if accel < 0:
                # Accelerating up, decrease thrust
                if self.lastAccel > 0:
                    # Was accelerating down last time, step is too large.
                    self.thrustStep /= 2
                    
                self.hoverThrust = self.hoverThrust - self.thrustStep

        

    def process(self,sensor,state,dt):

        control=Control()

        if accel < 1e-12:
            print "Hover thrust found : ", self.hoverThrust
        else:
            cancelAcceleration(self,state)

        control.up = self.hoverThrust

        print "Current thrust : ", control.up
        print "Current acceleration : ", accel

        #if state.y > yHover:
        #   if state.dydt > dydtMin:
        #      control.up=1

        #if state.dydt > dydtMax:
        #    control.up=1

        self.lastDyDt = state.dydt
        self.lastAccel = accel

        return control



dt          =.1
brain       = LearningControl()
nSensors    = 40
sensorRange = 2000
pod         = GravityPod(nSensors,sensorRange,brain,(255,0,0))
pods        = [pod]
world       = World("freefall_world.txt",pods)
sim         = Simulation(world,dt)
#uncomment the next line to hide the walls.
#sim.world.blind=True

sim.run()
