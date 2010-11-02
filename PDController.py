import math
from simulation import Simulation
from simulation import World
from simulation import GravityPod
from simulation import Control
class PDController:
    def __init__(self):
        self.control = Control()
        self.control.up = 0.2
        self.targetX = 130
        self.targetY = 720

    def process(self, sensor, state, dt):

        print state.x, state.y

        #self.targetX = state.x + 10
        #self.targetY = state.y - 10

        # Vertical first:
        self.control.up = (self.control.up - (10 * (self.targetY - state.y) + -5 * state.dydt))

        # Horizontal next:
        horizFeedback = ((self.targetX - state.x) * 5 + (-25 * state.dxdt)) / 20
        
        if horizFeedback > 1:
            horizFeedback = 1
        elif horizFeedback < -1:
            horizFeedback = -1

        # This feedback leads to a control angle
        angleChange = math.pi - (0.1 * math.asin(horizFeedback)) - state.ang

        # Now the angle control
        horizForce = (self.control.left - self.control.right) + (angleChange * 30 + (-25 * state.dangdt))

        if horizForce > 0:
            self.control.left = horizForce
            self.control.right = 0
        else:
            self.control.left = 0
            self.control.right = abs(horizForce)

        self.control.limit()

        return self.control

dt = .1
brain = PDController()
nSensors = 40
sensorRange = 2000
pod = GravityPod(nSensors, sensorRange, brain, (255,0,0))
pods = [pod]
world = World("rect_world.txt", pods)
sim = Simulation(world, dt)

sim.run()