from simulation import *
from HoverFindPersonality import *
from StopPersonality import *
from VerticalMovePersonality import *
from collections import deque

# Setting this to a larger number makes the system more tolerant of residual
# velocities.
# If too high, the pod will drift when it is supposed to be hovering.
# The larger the number, the faster it will drift.
# If too small, cancelAcceleration and cancelVelocity will take longer.
zeroThreshold = 1e-4
# Setting this to a larger number makes the position finding system less
# accurate but faster.
# Setting this to a smaller number makes the position finding system less
# accurate but slower.
# If the number is too small, larger movements will cause instability.
# About 5 seems to be the best compromise if large movements are needed.
positionThreshold = 5

class LearningControl:

    ## Last State Variables
    # Last acceleration. Used for cancelAcceleration to determine if thrust steps are too large.
    lastAccel = 1
    # Last vertical speed. Used to calculate acceleration. Must be initialized to >= zeroThreshold, otherwise acceleration appears to be 0 during the first cycle.
    lastDyDt = zeroThreshold * 10

    ## Current State Variables
    # Stores the value of thrust required to cancel out the acceleration due to gravity
    hoverThrust = 0
    # Used during the initialisation to flag if the hover thrust search has started.
    hoverThrustSearching = False
    # Used during initialisation to flag if the hover thrust has been found.
    hoverThrustFound = False
    # Used during initialisation to flag if the pod has come to a halt before the first maneuver.
    velocityZeroed = False
    # Used during initialisation to flag if the pod has been position equidistant from the top and bottom walls sensed.
    positionCenterStarted = False

    # Current personality that is running the pod
    personality = None
    # List of personalities to be used
    personalities = deque()

    def process(self,sensor,state,dt):

        control=Control()

        # Calculate extra state variables
        # TODO : Move this straight in to the state variable when accel is nolonger used.
        accel = (state.dydt - self.lastDyDt)/dt

        # Output state
        print "Acceleration : ", accel
        print "Speed : ", state.dydt
        print "Position : ", state.y

        # Add my variables to state
        state.d2ydt2 = accel
        state.last_d2ydt2 = self.lastAccel
        state.dt = dt

        # Is there a current personality that is working?
        if self.personality != None and self.personality.done == False:
            # Run the personality
            self.personality.process(state)
        elif self.personality == None or self.personality.done == True:
            # The current personality is done, or there isn't one.
            if len(self.personalities) > 0:
                # A personality is queued up to run.
                # Set it as the active personality.
                self.personality = self.personalities.pop()
                # Run the first cycle.
                self.personality.process(state)
            elif self.hoverThrustSearching == False:
                # We've just started. Make a HoverFindPersonality the active personality
                self.personality = HoverFindPersonality()
                # Run the first cycle.
                self.personality.process(state)
                # We are now searching for the hover thrust.
                self.hoverThrustSearching = True
            elif self.hoverThrustFound == False:
                # Store the hover thrust for later use.
                self.hoverThrust = self.personality.hoverThrust
                # Now bring the pod to a stop. Make a StopPersonality the active personality
                self.personality = StopPersonality(self.hoverThrust)
                # Run the first cycle.
                self.personality.process(state)
                # We've found the hover thrust.
                self.hoverThrustFound = True
            elif self.positionCenterStarted == False:
                # We need to position ourselves in the centre of the passage, vertically
                # First work out the height of the passage.
                distanceToTop = sensor[0].val
                distanceToBottom = sensor[20].val
                passageHeight = distanceToTop + distanceToBottom

                bottomPosition = state.y + distanceToBottom
                centerHeight = bottomPosition - (passageHeight / 2)

                # Make a VerticalMovePersonality the active personality
                self.personality = VerticalMovePersonality(self.hoverThrust, state, centerHeight)
                # Queue up two more personalities to test movement to and fro
                backToStartMove = VerticalMovePersonality(self.hoverThrust, state, bottomPosition - (passageHeight / 4))
                backToCenterMove = VerticalMovePersonality(self.hoverThrust, state, centerHeight)
                self.personalities.appendleft(backToStartMove)
                self.personalities.appendleft(backToCenterMove)
                # Run the first cycle
                self.personality.process(state)
                # We have started to center.
                self.positionCenterStarted = True
#            else:
#                # Position centre vertically (ish) to give us room to move later.
#                # Assuming we're pointing straight up (everything else is screwed anyway if we aren't.)
#
#                distanceToTop = sensor[0].val
#                distanceToBottom = sensor[20].val
#                totalDistance = distanceToBottom + distanceToTop
#                # Distance to the point between half way between where the top sensor is reading and where the bottom sensor is reading
#                distanceToMiddle = totalDistance / 2 - distanceToBottom
#
#                if self.distanceMeasured == False:
#                    # We've only just started moving. Work out how far to go.
#                    self.totalDistance = distanceToMiddle
#
#                    # Get the largest possible moving force that
#                    # can be canceled in an equal amount of time
#                    # without rotating the pod.
#
#                    # Moving force can't be greater than the hover thrust,
#                    # otherwise the speeding and slowing thrusts will
#                    # unbalanced
#                    movingForce = self.hoverThrust
#                    # The sum of the moving force and the hover thrust
#                    # cannot be greater than 1, otherwise the thruster
#                    # will max out and the speeding and slowing thrusts
#                    # will be unbalanced
#                    if movingForce + self.hoverThrust > 1:
#                        movingForce = 1 - self.hoverThrust
#
#                    if self.totalDistance > 0:
#                        # We have to go up.
#                        self.accelThrust = fabs(movingForce) / self.accelThrustDiviser # Must be +ve
#                    else:
#                        # We have to go down.
#                        self.accelThrust = -fabs(movingForce) / self.accelThrustDiviser # Must be -ve
#
#                    self.distanceMeasured = True
#                    self.resetCancelVelocity()
#                    self.halfWay = False
#
#                if self.stopped == False:
#                    self.verticalMove(distanceToMiddle, state, accel, dt)
#                else:
#                    print "Stopped."
#                    if fabs(distanceToMiddle) > positionThreshold:
#                        # Start the destination finding process again, we're not quite close enough.
#                        self.distanceMeasured = False
#                        # But go a bit slower to be more accurate.
#                        self.accelThrustDiviser *= 2

        #control.up = self.hoverThrust + self.maneuverThrust

        print "Thrust : ", control.up
        print ""

        self.lastDyDt = state.dydt
        self.lastAccel = accel
        
        if (self.personality != None):
            return self.personality.control
        else:
            control = Control()
            control.up = self.hoverThrust
            return control



dt          =.1
brain       = LearningControl()
nSensors    = 40
sensorRange = 5000
pod         = GravityPod(nSensors,sensorRange,brain,(255,0,0))
pods        = [pod]
world       = World("rect_world.txt",pods)
sim         = Simulation(world,dt)
#uncomment the next line to hide the walls.
#sim.world.blind=True

sim.run()
