/*
 * PID.c
 *
 *  Created on: Jul 19, 2025
 *      Author: TC
 */
#include "pid.h"

float clampf(float v, float lo, float hi){
	if(v < lo)
		return lo;
	if(v > hi)
		return hi;
	return v;
}

float pid_update(volatile PID* p, float measured_value, float dt){
	float error = p->setpoint - measured_value;
		float P = p->Kp * error;
	p->integrator += p->Ki * error * dt;
	float deriv = (error - p->prev_error) / dt;
	p->D_lpf += p->D_alpha * (deriv - p->D_lpf);
	float D = p->Kd * p->D_lpf;
	float u = P + p->integrator + D;
		float u_sat = clampf(u, p->out_min, p->out_max);
		p->integrator += p->anti_windup_beta * (u_sat - u);
	p->prev_error = error;
	return u_sat;
}

void SetPID(volatile PID* p, float kp, float ki, float kd, float d_alpha, float max, float min, float anti){
	p->Kp = kp;
	p->Ki = ki;
	p->Kd = kd;
	p->D_alpha = d_alpha;
	p->out_max = max;
	p->out_min = min;
	p->anti_windup_beta = anti;
	p->setpoint = 0.0f;
	p->max_setpoint = 180.0f;
	p->min_setpoint = -180.0f;
	p->integrator = 0.0f;
	p->prev_error = 0.0f;
    p->D_lpf = 0.0f;
}

void PID_SetTargetLimits(volatile PID* p, float min_angle, float max_angle) {
	p->min_setpoint = min_angle;
	p->max_setpoint = max_angle;
}

void PID_SetTarget(volatile PID* p, float target){
	if (target > p->max_setpoint) {
		p->setpoint = p->max_setpoint;
	} else if (target < p->min_setpoint) {
		p->setpoint = p->min_setpoint;
	} else {
		p->setpoint = target;
	}
}

void GetPID(volatile PID* p, float* arr)
{
	arr[0] = p->Kp;
	arr[1] = p->Ki;
	arr[2] = p->Kd;
	arr[3] = p->D_alpha;
	arr[4] = p->anti_windup_beta;
}
/**/
