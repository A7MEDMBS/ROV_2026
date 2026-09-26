/*
 * PID.h
 *
 *  Created on: Jul 19, 2025
 *      Author: TC
 */
/**/
#ifndef SRC_PID_H_
#define SRC_PID_H_
#include "stm32f4xx.h"

typedef struct {
  float Kp, Ki, Kd;
  float setpoint;
  float max_setpoint;
  float min_setpoint;
  float integrator, prev_error, D_lpf;
  float D_alpha;
  float out_min, out_max;
  float anti_windup_beta;
} PID;

float clampf(float, float, float);
float pid_update(volatile PID*, float, float);
void SetPID(volatile PID*, float, float, float, float, float, float, float);
void PID_SetTargetLimits(volatile PID*, float, float);
void PID_SetTarget(volatile PID*, float);
void GetPID(volatile PID*, float*);

#endif /* SRC_PID_H_ */
