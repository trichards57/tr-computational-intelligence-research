from simulation import *

class HoverFindPersonality:
    # Setting this to a larger number makes the system more tolerant of residual
    # velocities.
    # If too high, the pod will drift when it is supposed to be hovering.
    # The larger the number, the faster it will drift.
    # If too small, cancelAcceleration and cancelVelocity will take longer.
    zeroThreshold = 1e-4
    # Stores the value of thrust required to cancel out the acceleration due to gravity
    hoverThrust = 0.2
    # The step used during cancelAcceleration to modify cancelThrust.
    thrustStep = 0.5
    # The Control object that needs to be changed
    control = Control()

    # Has the Personality achieved it's goal (in this case, found a suitable hover thrust)?
    done = False

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
