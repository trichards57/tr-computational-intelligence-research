import Personality
from math import fabs

from Personality import Personality

class StopPersonality(Personality):

    def __init__(self, hoverThrust):
        Personality.__init__(self, hoverThrust)
        self.thrustStep = 0.5

    def process(self, state):
        print "Stopping"
        if fabs(state.dydt) < self.zeroThreshold:
            self.done = True
            self.maneuverThrust = 0
        else:
            if fabs(state.d2ydt2 / state.dt) / 100 >= fabs(state.dydt):
                self.thrustStep /= 2

            if state.dydt > 0:
                self.maneuverThrust = self.thrustStep
            else:
                self.maneuverThrust = -self.thrustStep

        print "Maneuver Thrust : ", self.maneuverThrust
        print "Hover Thrust    : ", self.hoverThrust
        self.control.up = self.maneuverThrust + self.hoverThrust