from simulation import *
import math

class RuleController:
    def __init__(self):
        self.lastSideForce = 0
        self.targetAng = 0

    def process(self, sensor, state, dt):
        control = Control()

        yError = state.targetY - state.y
        xError = state.targetX - state.x
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

        self.firstCycle = True

        self.control = Control()

    def process(self, sensor, state, dt):
        targetX = state.targetX
        targetY = state.targetY

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