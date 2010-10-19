import Personality
from math import fabs

from Personality import Personality

class HoverFindPersonality(Personality):

    def __init__(self, hoverThrust):
        Personality.__init__(self, hoverThrust)
        self.thrustStep = 0

    def process(self, state):
        print "Finding Hover"
        if fabs(state.d2ydt2) < self.zeroThreshold:
            self.done = True
        else:
            # Essentially a search for the thrust that is sufficient to cancel out the acceleration due to gravity.
            if state.d2ydt2 > 0:
                # Accelerating down, increase thrust
                if state.last_d2ydt2 < 0:
                    # Was accelerating up last time, step is too large.
                    self.thrustStep /= 2
                self.hoverThrust += self.thrustStep
            else:
                # Accelerating up, decrease thrust
                if state.last_d2ydt2 < 0:
                    # Was accelerating down last time, step is too large.
                    self.thrustStep /= 2
                self.hoverThrust -= self.thrustStep

        self.control.up = self.hoverThrust
