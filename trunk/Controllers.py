import math

from simulation import Control

class RuleController:
    def __init__(self):
        self.last_side_force = 0
        self.target_ang = 0

        self.big_y_speed = 20
        self.mid_y_speed = 5
        self.sml_y_speed = 2.5

        self.big_x_speed = 20
        self.mid_x_speed = 5
        self.sml_x_speed = 2.5

        self.big_x_error = 50
        self.mid_x_error = 20

        self.big_y_error = 50
        self.mid_y_error = 20

        self.up_force = 0.3
        self.down_force = 0.1

        self.propel_angle = 0.1

    def process(self, sensor, state, dt):
        control = Control()

        y_error = state.target_y - state.y
        x_error = state.target_x - state.x
        norm_ang = (2 * math.pi) - ( (state.ang + math.pi) % (2 * math.pi))
        if norm_ang > math.pi:
            norm_ang -= 2* math.pi

        if y_error < 0:
            control.up = 0.5

        if math.fabs(y_error) > self.big_y_error:
            max_speed = self.big_y_speed
        elif math.fabs(y_error) > self.mid_y_error:
            max_speed = self.mid_y_speed
        else:
            max_speed = self.sml_y_speed

        if state.dydt < -max_speed:
            control.up = self.down_force
        elif state.dydt > max_speed:
            control.up = self.up_force

        if x_error > 0:
            self.target_ang = self.propel_angle
        elif x_error < 0:
            self.target_ang = -self.propel_angle

        if math.fabs(x_error) > self.big_x_error:
            max_speed = self.big_x_speed
        elif math.fabs(x_error) > self.mid_x_error:
            max_speed = self.mid_x_speed
        else:
            max_speed = self.sml_x_speed
        
        if state.dxdt > max_speed:
            self.target_ang = -self.propel_angle
        if state.dxdt < -max_speed:
            self.target_ang = self.propel_angle

        ang_error = self.target_ang - norm_ang

        side_force = (ang_error * 6 + state.dangdt * 5)

        if side_force > 0:
            control.right = side_force
            control.left = 0
        else:
            control.left = -side_force
            control.right = 0

        self.last_side_force = side_force

        return control

class PDController:

    def __init__(self):
        self.vertical_prop_gain = 1
        self.vertical_diff_gain = -5
        self.horizontal_prop_gain = 2
        self.horizontal_diff_gain = -18
        self.angle_gain = 0.1
        self.angle_prop_gain = 20
        self.angle_diff_gain = -25
        self.horizontal_force_feedback_scale = 20

        self.first_cycle = True

        self.control = Control()

    def process(self, sensor, state, dt):
        target_x = state.target_x
        target_y = state.target_y

        # Vertical first:
        self.control.up = (self.control.up - (self.vertical_prop_gain * \
            (target_y - state.y) + self.vertical_diff_gain * state.dydt))

        # Horizontal next:
        horiz_feedback = ((target_x - state.x) * self.horizontal_prop_gain \
                            + (self.horizontal_diff_gain * state.dxdt)) \
                         / self.horizontal_force_feedback_scale

        # Limit feedback so that asin never fails.
        if horiz_feedback > 1:
            horiz_feedback = 1
        elif horiz_feedback < -1:
            horiz_feedback = -1

        # This feedback leads to a control angle
        angle_change = math.pi - (self.angle_gain * math.asin(horiz_feedback)) \
            - state.ang

        # Now the angle control
        horiz_force = (self.control.left - self.control.right) + \
            (angle_change * self.angle_prop_gain + \
                (self.angle_diff_gain * state.dangdt))

        if horiz_force > 0:
            self.control.left = horiz_force
            self.control.right = 0
        else:
            self.control.left = 0
            self.control.right = abs(horiz_force)

        self.control.limit()

        return self.control