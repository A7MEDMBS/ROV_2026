#ifndef MOTOR_INTERFACE_H
#define MOTOR_INTERFACE_H

#include "stm32f4xx.h"
#include "cmsis_os.h"
#include <stdint.h>
#include <stdlib.h>
#include <stdbool.h>
#include "mapping.h"

#define MAX_THRUSTERS 7
#define PWM_MAX 20000
#define THRUSTER_SPEED_MIN 0
#define THRUSTER_SPEED_MAX 100
#define PWM_NEUTRAL 1500
#define ABORT_CALIBRATION_FLAG 0x100
#define TICKS_FOR_10_SECONDS 100
#define CALIBRATION_DURATION_MS 10000

typedef enum
{
	move = 1,
	off = 0,
	stop = -1
}movement_state;

typedef struct{
	movement_state move;
}Movement_State;

typedef enum {
	Thruster_DIRECTION_REVERSE = -1,
	Thruster_DIRECTION_STOP = 0,
	Thruster_DIRECTION_FORWARD = 1
} ThrusterDirection;

typedef enum {
	Thruster_STATUS_OK,
	Thruster_STATUS_FAULT,
	Thruster_STATUS_RUNNIG,
	Thruster_STATUS_STOPPED,
	Thruster_STATUS_UNKNOWN
} ThrusterStatuss;




typedef enum {
    THRUSTERS_IDLE = 0,
	THRUSTERS_CALIBRATING,
	THRUSTERS_READY,
	THRUSTERS_ACTIVE
} ThrustersState_t;

typedef enum {
    TH_IDLE = 0,
    TH_CALIBRATING,
    TH_READY,
    TH_ACTIVE
} ThrusterState_t;

typedef enum {
    CMD_NONE = 0,
    CMD_ENABLE_ALL,
    CMD_DISABLE_ALL,
    CMD_ENABLE_ONE,
    CMD_DISABLE_ONE,
    CMD_ABORT
} CmdType_t;

typedef struct {
    CmdType_t type;
    uint8_t id;
} ThrusterCmd_t;

typedef struct
{
	uint8_t id;
	TIM_HandleTypeDef *htim;
	uint32_t channel;
	uint8_t speed;
	ThrusterDirection direction;
	bool isEnabled;
	bool isCalibrated;
	uint8_t ramp_rate;
	uint32_t current_value;
	ThrusterState_t state;
	uint16_t calibCounter;
} Thruster;

extern osThreadId_t thrusterTaskHandle;
extern Thruster thrusters[MAX_THRUSTERS];
extern osMessageQueueId_t thrusterCmdQueue;
extern osThreadId_t thrusterTaskHandle;
extern volatile ThrustersState_t thrusters_state;
extern volatile bool is_system_calibrated;
extern osTimerId_t calibrationTimerHandle;


void Thrusters_System_Init(void);
void Thruster_Component_Disarm(void);
void Thruster_Component_Arm(void);
void Thrusters_Init(void);

void Thruster_Set_timer(uint8_t motor_id, TIM_HandleTypeDef *htim, uint32_t channel);

void CalibrationTask(void *argument);

void Thruster_EnableAll(void);

void Thruster_DisableAll(void);

void Thruster_EnableOne(uint8_t id);

void Thruster_DisableOne(uint8_t id);

void SetSpeed(int16_t*);

void Thruster_SetSpeed(uint8_t, int16_t);

void Thruster_SetDirection(uint8_t, ThrusterDirection);

void Thruster_Stop(uint8_t);

uint8_t Thruster_GetSpeed(uint8_t);

//uint32_t GetTimChannel(uint8_t);

ThrusterDirection Thruster_GetDirection(uint8_t);

void Move_Update(float, float, float, float, float, float);

#endif
/**/
