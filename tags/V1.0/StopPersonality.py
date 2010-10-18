from simulation import *

class StopPersonality:
    # Setting this to a larger number makes the system more tolerant of residual
    # velocities.
    # If too high, the pod will drift when it is supposed to be hovering.
    # The larger the number, the faster it will drift.
    # If too small, cancelAcceleration and cancelVelocity will take longer.
    zeroThreshold = 1e-4
    # Stores the value of thrust required to cancel out the acceleration due to gravity
    hoverThrust = 0
    # The step used during cancelAcceleration to modify cancelThrust.
    thrustStep = 0.5
    # The Control object that needs to be changed
    control = Control()

    # Has the Personality achieved it's goal (in this case, made velocity zero)?
    done = False

    def __init__(self, hoverThrust):
        self.hoverThrust = hoverThrust

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