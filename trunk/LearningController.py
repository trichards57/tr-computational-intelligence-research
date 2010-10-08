from simulation import *
#
#     get the gravity pod to hover at y=yHover
#

zeroThreshold = 1e-9  # Any larger number than this and things break. Not sure why yet.

class LearningControl:

    lastAccel = 1
    lastDyDt = 1e-10
    hoverThrust = 0
    maneuverThrust = 0
    thrustStep = 0.5
    speedStep = 0.1

    distanceMeasured = False
    totalDistance = 0
    directionToCenter = 0

    hoverThrustFound = False
    velocityZeroed = False

    def cancelAcceleration(self,state,accel):
        # Essentially a binary search for the thrust that is sufficient to cancel out the acceleration due to gravity.
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

    def cancelVelocity(self,state,accel,dt):
        speed = state.dydt
        if fabs(accel / dt) / 100 >= fabs(state.dydt):
            # You're going to slow down too fast and overshoot. Decrease the correcting thrust.
            self.speedStep /= 2

        print "Speed Step : ", self.speedStep
                
        if state.dydt > 0:
            self.maneuverThrust = self.speedStep
        else:
            self.maneuverThrust = -self.speedStep

    def process(self,sensor,state,dt):

        control=Control()

        accel = (state.dydt - self.lastDyDt)/dt

        print "Acceleration : ", accel
        print "Speed : ", state.dydt
        print "Position : ", state.y

        if self.hoverThrustFound == False:
            # We've just started. Work out the thrust needed to counter gravity.
            if fabs(accel) < zeroThreshold:
                # Close enough to 0 to make no difference.
                print "Hover thrust found : ", self.hoverThrust
                self.hoverThrustFound = True
            else:
                self.cancelAcceleration(state, accel)
        else:
            if self.velocityZeroed == False:
                # We can hover safely. Now correct vertical drift.
                if fabs(state.dydt) < zeroThreshold:
                    # Close enough to 0 to make no difference.
                    print "Velocity now 0"
                    self.velocityZeroed = True
                    # Stop maneuvering.
                    self.maneuverThrust = 0
                else:
                    print "Zeroing velocity"
                    self.cancelVelocity(state, accel, dt)
            else:
                # Position centre vertically (ish)
                # Assuming we're pointing straight up (everything else is screwed anyway if we aren't.)

                distanceToTop = sensor[0].val * sensor[0].range
                distanceToBottom = sensor[20].val * sensor[20].range
                totalDistance = distanceToBottom + distanceToTop
                # Distance to the point between half way between where the top sensor is reading and where the bottom sensor is reading
                distanceToMiddle = totalDistance / 2 - distanceToBottom

                if self.distanceMeasured == False:
                    # We've only just started moving. Work out how far to go.
                    self.totalDistance = distanceToMiddle

                    if self.totalDistance > 0:
                        # We have to go up.
                        self.directionToCenter = 0.1
                    else:
                        # We have to go down.
                        self.directionToCenter = -0.1

                    self.distanceMeasured = True
                    self.speedStep = 0.1 # Prepare for bringing to a full stop later.


                if fabs(distanceToMiddle) > fabs(self.totalDistance / 2):
                    # Are we less than half way?
                    self.maneuverThrust = self.directionToCenter
                    print "First Half Move"
                else:
                    print "Second Half Move"
                    if fabs(state.dydt) < 1:
                        # Probably close enough to position.  Come to a halt.
                        self.cancelVelocity(state, accel, dt)
                        
                    else:
                        # We're more than half way. Start slowing down
                        self.maneuverThrust = -self.directionToCenter

        control.up = self.hoverThrust + self.maneuverThrust

        print "Thrust : ", control.up
        print ""

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
