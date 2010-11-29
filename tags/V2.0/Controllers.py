## @package Controllers
# This module contains all of the controllers used to translate target
# coordinates in to thruster instructions.
#
# A controller must implement the following function:
#
# process(self, sensor, state, dt)
#
# This function returns a Control object that instructs the pod how to fire it's
# thrusters.

import math

from simulation import Control

## Rule-Based Controller
#
# This controller controls the pod's thrusters based on a set of discrete rules
# that control the pod's angle, speed and position relative to the
# @ref state.target_x and @ref state.target_y attributes.
class RuleController:
    ## The RuleController constructor
    #
    # Initialises the controller's rule attributes to the required values.
    def __init__(self):
        ## The angle the controller is currently aiming for.
        #
        # @note This angle is a normalised angle (i.e. relative to the top of
        # the pod, working clockwise.
        self.target_ang = 0

        ## The upper speed threshold in the Y direction
        self.big_y_speed = 20
        ## The middle speed threshold in the Y direction
        self.mid_y_speed = 5
        ## The lower speed threshold in the Y direction
        self.sml_y_speed = 2.5

        ## The upper speed threshold in the X direction
        self.big_x_speed = 20
        ## The middle speed threshold in the X direction
        self.mid_x_speed = 5
        ## The lower speed threshold in the X direction
        self.sml_x_speed = 2.5

        ## The upper error threshold in the X direction
        self.big_x_error = 50
        ### The middle error threshold in the X direction
        self.mid_x_error = 20

        ## The upper error threshold in the Y direction
        self.big_y_error = 50
        ## The middle error threshold in the Y direction
        self.mid_y_error = 20

        ## The thruster force used to move the pod up
        #
        # @note The force to exactly overcome gravity is 0.2
        self.up_force = 0.3
        ## The thruster force used to move the pod down
        #
        # @note The force to exactly overcome gravity is 0.2
        self.down_force = 0.1

        ## The angle the controller aims for to move the pod sidewise
        self.propel_angle = 0.1

    ## The process function for the RuleController
    #
    # @param self The object pointer
    # @param sensor A list of @ref Sensor objects.  Unused.
    # @param state The current state of the pod.  Must include the following properties:
    #        - target_x
    #        - target_y
    #        - x
    #        - y
    #        - ang
    #        - dydt
    #        - dxdt
    #        - dangdt
    # @param dt The timestep used by the simulator. Unused.
    #
    # This function performs the processing that transforms the current state
    # into a set of thruster instructions.
    #
    # The algorithm works as follows:
    #
    # @image html Rule_Controller_Algorithm.png "Rule Controller Algorithm"
    #
    # @note
    #       - Angle control is by a standard PD controller, using the following equation:
    #         @f[
    #            6 \times \mbox{angle\_error} + 5 \times \mbox{state.dangdt}
    #         @f]
    #         The controller constants were determined by trial and error to be acceptable
    #       - The returned control values are not limited to 1
    def process(self, sensor, state, dt):
        control = Control()
        target_ang = self.target_ang

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
            target_ang = self.propel_angle
        elif x_error < 0:
            target_ang = -self.propel_angle

        if math.fabs(x_error) > self.big_x_error:
            max_speed = self.big_x_speed
        elif math.fabs(x_error) > self.mid_x_error:
            max_speed = self.mid_x_speed
        else:
            max_speed = self.sml_x_speed
        
        if state.dxdt > max_speed:
            target_ang = -self.propel_angle
        if state.dxdt < -max_speed:
            target_ang = self.propel_angle

        ang_error = target_ang - norm_ang

        side_force = (ang_error * 6 + state.dangdt * 5)

        if side_force > 0:
            control.right = side_force
            control.left = 0
        else:
            control.left = -side_force
            control.right = 0

        return control

## Proportional+Differential Controller
#
# This controller controls the pod's thrusters based on a set of equations derived
# in part through control theory.
class PDController:

    def __init__(self):
        ## The Vertical Control Equation's proportional gain
        self.vertical_prop_gain = 1
        ## The Vertical Control Equation's differential gain
        self.vertical_diff_gain = -5
        ## The Horizontal Control Equation's proportional gain
        self.horizontal_prop_gain = 2
        ## The Horizontal Control Equation's differential gain
        self.horizontal_diff_gain = -18
        ## The Angle Control Equation's input gain
        self.angle_gain = 0.1
        ## The Angle Control Equation's proportional gain
        self.angle_prop_gain = 20
        ## The Angle Control Equation's differential gain
        self.angle_diff_gain = -25
        ## The scaling factor used in the conversion from desired horizontal force to desired angle
        self.horizontal_force_feedback_scale = 20
        ## The thrust commands produced by the system
        self.control = Control()

    ## The process function for the PDController
    #
    # @param self The object pointer
    # @param sensor A list of @ref Sensor objects.  Unused.
    # @param state The current state of the pod.  Must include the following properties:
    #        - target_x
    #        - target_y
    #        - x
    #        - y
    #        - ang
    #        - dydt
    #        - dxdt
    #        - dangdt
    # @param dt The timestep used by the simulator. Unused.
    #
    # This function performs the processing that transforms the current state
    # into a set of thruster instructions.
    #
    # The following equations are used:
    #
    # @f{align*}
    #    \mbox{control.up} &= \mbox{control.up} - \mbox{vertical\_prop\_gain} \times (\mbox{target\_y} - \mbox{actual\_y}) + \mbox{vertical\_diff\_gain} \times \mbox{dydt}\\
    #    \mbox{horiz\_feedback} &= \frac{((\mbox{target\_x} - \mbox{actual\_x}) * \mbox{horizontal\_prop\_gain} + (\mbox{horizontal\_diff\_gain} * \mbox{dxdt}))}{\mbox{horizontal\_force\_feedback\_scale}}\\
    # @f}
    #
    # @note
    #       - Angle control is by a standard PD controller, using the following equation:
    #         @f[
    #            6 \times \mbox{angle\_error} + 5 \times \mbox{state.dangdt}
    #         @f]
    #         The controller constants were determined by trial and error to be acceptable
    #       - The returned control values are not limited to 1
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