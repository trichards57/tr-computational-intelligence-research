from simulation import *
#
#     get the gravity pod to hover at y=yHover
#

zeroThreshold = 1e-10  # Any larger number than this and things break. Not sure why yet.

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

    movingForceDiviser = 1

    hoverThrustFound = False
    velocityZeroed = False

    halfWay = False
    stopping = False
    stopped = False

    def cancelAcceleration(self,state,accel):
        # Essentially a search for the thrust that is sufficient to cancel out the acceleration due to gravity.
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

    def resetCancelVelocity(self):
        # Resets variables used to bring the pod to a complete halt.
        self.speedStep = 0.1
        self.stopped = False

    def cancelVelocity(self,state,accel,dt):
        if fabs(state.dydt) < zeroThreshold:
            self.stopped = True
            self.maneuverThrust = 0
        else:
            if fabs(accel / dt) / 100 >= fabs(state.dydt):
                # You're going to slow down too fast and overshoot. Decrease the correcting thrust.
                self.speedStep /= 2

            print "Speed Step : ", self.speedStep
                    
            if state.dydt > 0:
                self.maneuverThrust = self.speedStep
            else:
                self.maneuverThrust = -self.speedStep

    def verticalMove(self, distanceRemaining, state, accel, dt):
        print "Distance remaining : ", distanceRemaining

        if fabs(distanceRemaining) < fabs(self.totalDistance / 2):
            # We're over half way.
            self.halfWay = True
        
        if self.halfWay == False:
            # Are we less than half way?
            print "First Half Move"
            self.maneuverThrust = self.directionToCenter
        else:
            if fabs(distanceRemaining) < 5:
                # Close enough to position.  Come to a halt.
                print "Stopping"
                self.stopping = True

            if self.stopping == True:                        
                if self.stopped:
                    self.stopping = False
                else:
                    self.cancelVelocity(state, accel, dt)
            else:
                print "Second Half Move"
                # We're more than half way. Start slowing down
                self.maneuverThrust = -self.directionToCenter

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
                if self.stopped:
                    print "Velocity now 0"
                    self.velocityZeroed = True
                else:
                    print "Zeroing velocity"
                    self.cancelVelocity(state, accel, dt)
            else:
                # Position centre vertically (ish)
                # Assuming we're pointing straight up (everything else is screwed anyway if we aren't.)

                distanceToTop = sensor[0].val
                distanceToBottom = sensor[20].val
                totalDistance = distanceToBottom + distanceToTop
                # Distance to the point between half way between where the top sensor is reading and where the bottom sensor is reading
                distanceToMiddle = totalDistance / 2 - distanceToBottom

                if self.distanceMeasured == False:
                    # We've only just started moving. Work out how far to go.
                    self.totalDistance = distanceToMiddle

                    # Get the largest possible moving force that
                    # can be canceled in an equal amount of time
                    # without rotating the pod.

                    # Moving force can't be greater than the hover thrust,
                    # otherwise the speeding and slowing thrusts will
                    # unbalanced
                    movingForce = self.hoverThrust
                    # The sum of the moving force and the hover thrust
                    # cannot be greater than 1, otherwise the thruster
                    # will max out and the speeding and slowing thrusts
                    # will be unbalanced
                    if movingForce + self.hoverThrust > 1:
                        movingForce = 1 - self.hoverThrust

                    if self.totalDistance > 0:
                        # We have to go up.
                        self.directionToCenter = fabs(movingForce) / self.movingForceDiviser # Must be +ve
                    else:
                        # We have to go down.
                        self.directionToCenter = -fabs(movingForce) / self.movingForceDiviser # Must be -ve

                    self.distanceMeasured = True
                    self.resetCancelVelocity()
                    self.halfWay = False

                if self.stopped == False:
                    self.verticalMove(distanceToMiddle, state, accel, dt)
                else:
                    print "Stopped."
                    if fabs(distanceToMiddle) > 5:
                        # Start the middle finding process again, we're not quite close enough.
                        self.distanceMeasured = False
                        # But go a bit slower to be more accurate.
                        self.movingForceDiviser *= 2
                
        control.up = self.hoverThrust + self.maneuverThrust

        print "Thrust : ", control.up
        print ""

        self.lastDyDt = state.dydt
        self.lastAccel = accel

        return control



dt          =.1
brain       = LearningControl()
nSensors    = 40
sensorRange = 5000
pod         = GravityPod(nSensors,sensorRange,brain,(255,0,0))
pods        = [pod]
world       = World("freefall_world.txt",pods)
sim         = Simulation(world,dt)
#uncomment the next line to hide the walls.
#sim.world.blind=True

sim.run()
