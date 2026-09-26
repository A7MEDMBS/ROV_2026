#include "motor_interface.h"

volatile ThrustersState_t thrusters_state = THRUSTERS_IDLE;
Thruster thrusters[MAX_THRUSTERS];
osMessageQueueId_t thrusterCmdQueue;
osThreadId_t thrusterTaskHandle;
void ThrusterTask(void *argument)
{
	ThrusterCmd_t cmd;
	while (1) {
        if (osMessageQueueGet(thrusterCmdQueue, &cmd, NULL, 50) == osOK){
            switch (cmd.type) {
                case CMD_ENABLE_ALL:
                    if (thrusters_state == THRUSTERS_IDLE) {
                        for (uint8_t i = 0; i < MAX_THRUSTERS; i++) {
                            HAL_TIM_PWM_Start(thrusters[i].htim, thrusters[i].channel);
                            __HAL_TIM_SET_COMPARE(thrusters[i].htim, thrusters[i].channel, PWM_NEUTRAL);
                            thrusters[i].state = TH_CALIBRATING;
                            thrusters[i].calibCounter = TICKS_FOR_10_SECONDS;
                        }
                        thrusters_state = THRUSTERS_CALIBRATING;
                    }
                    else if (thrusters_state == THRUSTERS_READY) {
                    	for (uint8_t i = 0; i < MAX_THRUSTERS; i++) {
							HAL_TIM_PWM_Start(thrusters[i].htim, thrusters[i].channel);
							__HAL_TIM_SET_COMPARE(thrusters[i].htim, thrusters[i].channel, PWM_NEUTRAL);
							thrusters[i].state = TH_ACTIVE;
						}
                    	thrusters_state = THRUSTERS_ACTIVE;
                    }
                    break;
                case CMD_DISABLE_ALL:
                	if (thrusters_state == THRUSTERS_CALIBRATING)
                	{
                		for (uint8_t i = 0; i < MAX_THRUSTERS; i++) {
							__HAL_TIM_SET_COMPARE(thrusters[i].htim, thrusters[i].channel, 0);
							HAL_TIM_PWM_Stop(thrusters[i].htim, thrusters[i].channel);
							thrusters[i].state = TH_IDLE;
							thrusters[i].calibCounter = 0;
						}
                		thrusters_state = THRUSTERS_IDLE;
                	}
                	else if (thrusters_state == THRUSTERS_ACTIVE)
                	{
                		for (uint8_t i = 0; i < MAX_THRUSTERS; i++) {
							__HAL_TIM_SET_COMPARE(thrusters[i].htim, thrusters[i].channel, 0);
							HAL_TIM_PWM_Stop(thrusters[i].htim, thrusters[i].channel);
							thrusters[i].state = TH_READY;
						}
                		thrusters_state = THRUSTERS_READY;
                	}
                    break;
                case CMD_ENABLE_ONE:
                    if (cmd.id < MAX_THRUSTERS) {
                        Thruster *t = &thrusters[cmd.id];
                        if (t->state == TH_IDLE) {
                            HAL_TIM_PWM_Start(t->htim, t->channel);
                            __HAL_TIM_SET_COMPARE(t->htim, t->channel, PWM_NEUTRAL);
                            t->state = TH_CALIBRATING;
                            t->calibCounter = TICKS_FOR_10_SECONDS;
                        }
                        else if (t->state == TH_READY) {
                            t->state = TH_ACTIVE;
                        }
                    }
                    break;
                case CMD_DISABLE_ONE:
                    if (cmd.id < MAX_THRUSTERS) {
                        Thruster *t = &thrusters[cmd.id];
                        __HAL_TIM_SET_COMPARE(t->htim, t->channel, 0);
                        HAL_TIM_PWM_Stop(t->htim, t->channel);
                        if (t->state == TH_CALIBRATING) {
                        	t->state = TH_IDLE;
                        		t->calibCounter = 0;
                        }
                        else if (t->state == TH_ACTIVE) {
                        	t->state = TH_READY;
                        }
                    }
                    break;
                case CMD_ABORT:
                    for (uint8_t i = 0; i < MAX_THRUSTERS; i++) {
                    	__HAL_TIM_SET_COMPARE(thrusters[i].htim, thrusters[i].channel, 0);
                        HAL_TIM_PWM_Stop(thrusters[i].htim, thrusters[i].channel);
                        thrusters[i].state = TH_IDLE;
                        thrusters[i].calibCounter = 0;
                    }
                    thrusters_state = THRUSTERS_IDLE;
                    break;
                default:
                    break;
            }
        }
        if (thrusters_state == THRUSTERS_CALIBRATING) {
            bool allDone = true;
            for (uint8_t i = 0; i < MAX_THRUSTERS; i++) {
                if (thrusters[i].state == TH_CALIBRATING) {
                    if (thrusters[i].calibCounter > 0) {
                        thrusters[i].calibCounter--;
                        allDone = false;
                    }
                    else {
                        thrusters[i].state = TH_READY;
                    }
                }
            }
            if (allDone) {
            	thrusters_state = THRUSTERS_READY;
            }
        }
	osDelay(10);
    }
}
void Thrusters_Init(void) {
	for (uint8_t i = 0; i < MAX_THRUSTERS; i++) {
		  thrusters[i].id = i;
		  thrusters[i].htim = 0;
		  thrusters[i].channel = 0;
		  thrusters[i].speed = THRUSTER_SPEED_MIN;
		  thrusters[i].direction = Thruster_DIRECTION_STOP;
		  thrusters[i].ramp_rate = 0;
		  thrusters[i].state = TH_IDLE;
		  thrusters[i].calibCounter = 0;
	}
	thrusterCmdQueue = osMessageQueueNew(10, sizeof(ThrusterCmd_t), NULL);
	const osThreadAttr_t attr = {
		.name = "ThrusterTask",
		.stack_size = 512,
		.priority = osPriorityNormal
	};
	thrusterTaskHandle = osThreadNew(ThrusterTask, NULL, &attr);
}
void Thruster_EnableAll(void)
{
    ThrusterCmd_t cmd = {CMD_ENABLE_ALL, 0};
    osMessageQueuePut(thrusterCmdQueue, &cmd, 0, 0);
}
void Thruster_DisableAll(void)
{
    ThrusterCmd_t cmd = {CMD_DISABLE_ALL, 0};
    osMessageQueuePut(thrusterCmdQueue, &cmd, 0, 0);
}
void Thruster_EnableOne(uint8_t id)
{
    ThrusterCmd_t cmd = {CMD_ENABLE_ONE, id};
    osMessageQueuePut(thrusterCmdQueue, &cmd, 0, 0);
}
void Thruster_DisableOne(uint8_t id)
{
    ThrusterCmd_t cmd = {CMD_DISABLE_ONE, id};
    osMessageQueuePut(thrusterCmdQueue, &cmd, 0, 0);
}

void Thruster_Set_timer(uint8_t thruster_id, TIM_HandleTypeDef *htim, uint32_t channel) {
	thrusters[thruster_id].htim= htim;
	thrusters[thruster_id].channel = channel;
}
void Thruster_SetSpeed(uint8_t thruster_id, int16_t speed) {
	if(thrusters[thruster_id].state == TH_ACTIVE)
	{
		__HAL_TIM_SET_COMPARE(thrusters[thruster_id].htim, thrusters[thruster_id].channel, speed);
	}
}
void Thruster_SetDirection(uint8_t thruster_id, ThrusterDirection direction) {
	thrusters[thruster_id].direction = direction;
}
void SetSpeed(int16_t* PWM) {
	for(uint8_t i = 0; i < MAX_THRUSTERS; i++)
	{
		Thruster_SetSpeed(i,PWM[i]);
	}
}
void Thruster_Stop(uint8_t thruster_id) {
	if (thruster_id == 0xFF) {
		for(uint8_t i = 0; i < MAX_THRUSTERS; i++) {
			Thruster_SetSpeed(i, PWM_NEUTRAL);
		}
	}
	else if(thruster_id < MAX_THRUSTERS) {
		Thruster_SetSpeed(thruster_id, PWM_NEUTRAL);
	}
}/*
uint32_t GetTimChannel(uint8_t ch) {
    switch (ch) {
        case 1: return TIM_CHANNEL_1;
        case 2: return TIM_CHANNEL_2;
        case 3: return TIM_CHANNEL_3;
        case 4: return TIM_CHANNEL_4;
        default: return 0;
    }
}*/
uint8_t Thruster_GetSpeed(uint8_t thruster_id)
{
	return thrusters[thruster_id].speed;
}

ThrusterDirection Thruster_GetDirection(uint8_t thruster_id)
{
	return thrusters[thruster_id].direction;

}

ThrusterStatuss Thruster_CheckStatus(uint8_t thruster_id)
{
	/*if(Move.move)
	{
		if(motors[motor_id].enabled)
		{
			if(HAL_TIM_PWM_Start(motors[motor_id].htim, GetTimChannel(motors[motor_id].channel)) == HAL_OK)
			{
				if(motors[motor_id].direction)
				{
				//if(motors[motor_id].current)
				motors[motor_id].state = MOTOR_STATUS_RUNNIG;
				//else
				motors[motor_id].state = MOTOR_STATUS_FAULT;
				}
				else
				{
					if (__HAL_TIM_GET_COMPARE(motors[motor_id].htim, GetTimChannel(motors[motor_id].channel)) == 1500)
					{
						//status = MOTOR_STOPPED;
					}
				}
			}
			else
				motors[motor_id].state = MOTOR_STATUS_FAULT;
		}
		else
			motors[motor_id].state = MOTOR_STATUS_FAULT;
	}
	else
	{
	}*/
	ThrusterStatuss ts = Thruster_STATUS_OK;
	return ts;
}
void Thruster_id_HandleFault(uint8_t thruster_id)
{

}
bool is_mapping_inited = false;

void Move_Update(float Fx, float Fy, float Fz, float Mx, float My, float Mz)
{
    if (!is_mapping_inited) {
        for (int i = 0; i < N_T; i++) {
            Thruster_Map_Init(&maps[i]);
        }
        Build_Allocation_Matrix(A, r, d);
        is_mapping_inited = true;
    }
    float u_cmd[N_U] = {0};
    u_cmd[0] = Fx * 100.0f;
    u_cmd[1] = Fy * 100.0f;
    u_cmd[2] = Fz;
    u_cmd[3] = Mx;
    u_cmd[4] = My;
    u_cmd[5] = Mz * 100.0f;

    int status = Allocate_And_Map(A, u_cmd, lambda, maps, 5, T, PWM);

    if (status == 0) {
        SetSpeed(PWM);
    }
}
