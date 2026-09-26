#include "mavlink.h"

system_status_t system_status = {0};
mavlink_handler_t mavlink_handler;
TIM_HandleTypeDef *light_htim_p;
static uint8_t mavlink_rx_buffer[MAVLINK_DMA_BUFFER_SIZE];
static uint16_t read_index = 0;

float PID_Parameter[5] = {0};
char parameter_Str[100];
volatile IMUData MPU_data = {0};
volatile MagData QMC_data = {0};
volatile TPDData TPD_data = {0};
volatile float target_vx = 0.0f;
volatile float target_vy = 0.0f;
volatile float target_vz = 0.0f;
volatile float target_yaw = 0.0f;
volatile PID RollPID = {.Kp=0.3f, .Ki=0.0f, .Kd=0.0f, .D_alpha=0.1f, .out_max=15.0f, .out_min=-15.0f, .anti_windup_beta=1.0f};
volatile PID PitchPID = {.Kp=0.3f, .Ki=0.0f, .Kd=0.0f, .D_alpha=0.1f, .out_max=20.f, .out_min=-20.0f, .anti_windup_beta=1.0f};
volatile PID DepthPID  = {.Kp=0.3f, .Ki=0.0f, .Kd=0.0f, .D_alpha=0.1f, .out_max=42.0f, .out_min=-32.0f, .anti_windup_beta=1.0f};
volatile bool isRollPID = false;
volatile bool isPitchPID = false;
volatile bool isDepthPID = false;
MPU6050_Handler mpu1;
QMC5883L_Handler qmc1;
MS5540C_Handler msc1;

osThreadId_t mavlinkRxTaskHandle;
osThreadId_t mavlinkTxTaskHandle;
osThreadId_t statusTxTaskHandle;
osThreadId_t imuTaskHandle;
osThreadId_t pressureTaskHandle;
osMessageQueueId_t mavlinkTxQueueHandle;
osSemaphoreId_t xUartDmaSemaphore;

void ImuTask(void *argument) {
	IMUData mpu_data;
	MagData qmc_data;
    float final_heading;
	uint32_t last_imu_tick = 0;
    for(;;) {
    	mpu_data = MPU6050_Update(&mpu1);
    	qmc_data = QMC5883L_Update(&qmc1);
    	final_heading = QMC5883L_GetTiltCompensatedHeading(&qmc_data, mpu_data.roll, mpu_data.pitch);
    	MPU_data = mpu_data;
		MPU_data.yaw = final_heading;
		QMC_data = qmc_data;
    	uint32_t current_tick = osKernelGetTickCount();
		if ((current_tick - last_imu_tick) >= 300) {
			mavlink_message_t msg;
			mavlink_attitude_t attitude = {
				.time_boot_ms = HAL_GetTick(),
				.roll  = mpu_data.roll,
				.pitch = mpu_data.pitch,
				.yaw   = final_heading
			};
			mavlink_msg_attitude_encode(mavlink_handler.system_id, mavlink_handler.component_id, &msg, &attitude);
			MAVLink_QueueMessage(&msg);
			last_imu_tick = current_tick;
		}
        osDelay(10);
    }
}
void pressureTask(void *pvParameters)
{
	TPDData data;
	uint32_t last_msc_tick = 0;
	for(;;)
	{
		data = MS5540C_Update(&msc1);
		TPD_data = data;
		uint32_t current_tick = osKernelGetTickCount();
		if ((current_tick - last_msc_tick) >= 100) {
			mavlink_message_t msg;
			mavlink_vfr_hud_t vfr = {
				.airspeed    = 0.0f,
				.groundspeed = 0.0f,
				.heading     = 5,
				.throttle    = 0,
				.alt         = data.depth,
				.climb       = 0.0f
			};
			mavlink_msg_vfr_hud_encode(1, 1, &msg, &vfr);
			MAVLink_QueueMessage(&msg);
			mavlink_scaled_pressure_t scp = {
				.time_boot_ms = HAL_GetTick(),
				.press_abs = data.pressure,
				.temperature = (int16_t)(data.temperature)
			};
			mavlink_msg_scaled_pressure_encode(1, 1, &msg, &scp);
			MAVLink_QueueMessage(&msg);
			last_msc_tick = current_tick;
		}
		osDelay(20);
	}
}
void Send_heartbeat_msg(void) {
    mavlink_message_t msg;
    mavlink_msg_heartbeat_pack(
    		mavlink_handler.system_id,
    		mavlink_handler.component_id,
			&msg,
			MAV_TYPE_SUBMARINE,
			MAV_AUTOPILOT_GENERIC,
			MAV_MODE_GUIDED_ARMED,
			0,
			MAV_STATE_ACTIVE);
    MAVLink_QueueMessage(&msg);
}
void Send_status_msg(const char* txt) {
    mavlink_message_t msg;
    mavlink_statustext_t status;
    status.severity = MAV_SEVERITY_INFO;
    memset(status.text, 0, sizeof(status.text));
    strncpy(status.text, txt, 49);
    status.text[49] = '\0';
    status.id = 0;
    status.chunk_seq = 0;
    mavlink_msg_statustext_encode(1, 1, &msg, &status);
    MAVLink_QueueMessage(&msg);
}
void Send_Component_status_msg(void)
{
	mavlink_message_t msg;
	uint32_t sensor_status = 0;
	uint16_t pwm_out[MAX_THRUSTERS] = {0};
	for(int i = 0; i <MAX_THRUSTERS; i++) {
		pwm_out[i] = __HAL_TIM_GET_COMPARE(thrusters[i].htim, thrusters[i].channel);
	}
	sensor_status |= (mpu1.sensor_status << 0);
	sensor_status |= (qmc1.sensor_status << 1);
	sensor_status |= (msc1.sensor_status << 2);
	sensor_status |= (((light_htim_p->Instance->CCR1> 0)? 1:0) << 3);
	sensor_status |= (HAL_GPIO_ReadPin(GPIOB, GPIO_PIN_2) << 4);
	sensor_status |= (HAL_GPIO_ReadPin(GPIOC, GPIO_PIN_2) << 5);
	sensor_status |= (((thrusters_state == THRUSTERS_ACTIVE)?1:0) << 6);
	mavlink_msg_sys_status_pack(
			mavlink_handler.system_id,
			mavlink_handler.component_id,
			&msg,
			sensor_status,
			sensor_status,
			sensor_status,
			pwm_out[0], 0, 0, 0, pwm_out[1], pwm_out[2], pwm_out[3], pwm_out[4], pwm_out[5], pwm_out[6], 0, 0, 0
	);
	MAVLink_QueueMessage(&msg);
}
void Send_pid_msg(void)
{
	float pid_status = (isRollPID << 0 ) | (isPitchPID << 1) | (isDepthPID << 2);
	mavlink_message_t msg;
	mavlink_msg_named_value_float_pack(
			mavlink_handler.system_id,
			mavlink_handler.component_id,
			&msg,
			HAL_GetTick(),
			"",
			pid_status
			);
	MAVLink_QueueMessage(&msg);
}
void MavlinkRxTask(void *argument) {
	static mavlink_message_t msg;
	static mavlink_status_t status;
    read_index = 0;
    for(;;) {
        uint16_t dma_write_ptr = MAVLINK_DMA_BUFFER_SIZE - __HAL_DMA_GET_COUNTER(mavlink_handler.huart->hdmarx);
        while (read_index != dma_write_ptr) {
            uint8_t byte_to_parse = mavlink_rx_buffer[read_index];
            if (mavlink_parse_char(MAVLINK_COMM_0, byte_to_parse, &msg, &status)) {
            	switch (msg.msgid) {
            	case MAVLINK_MSG_ID_SET_POSITION_TARGET_LOCAL_NED: {
            			mavlink_set_position_target_local_ned_t cmd;
            	        mavlink_msg_set_position_target_local_ned_decode(&msg, &cmd);
            	        target_vx = cmd.vx; target_vy = cmd.vy;
						target_vz = cmd.vz; target_yaw = cmd.yaw;
            	        break;
            	}
            	case MAVLINK_MSG_ID_COMMAND_LONG: {
					mavlink_command_long_t cmd;
					mavlink_msg_command_long_decode(&msg, &cmd);
					if (cmd.command == MAV_CMD_COMPONENT_ARM_DISARM) {
						if(cmd.param1 == 1) {
							Thruster_EnableAll();
							system_status.thruster_flag = 1;
						}
						else {
							if(thrusters_state == THRUSTERS_CALIBRATING)
								system_status.thruster_flag = 3;
							if(thrusters_state == THRUSTERS_ACTIVE)
								system_status.thruster_flag = 4;
							Thruster_DisableAll();
						}
						system_status.status_flag = 1;
					}
					if (cmd.command == MAV_CMD_DO_SET_RELAY) {
						if((bool)cmd.param1 != system_status.is_gripper_closed) {
							HAL_GPIO_WritePin(GPIOB, GPIO_PIN_2, cmd.param1 ? GPIO_PIN_SET : GPIO_PIN_RESET);
							system_status.is_gripper_closed = cmd.param1 ? 1 : 0;
							system_status.status_flag = 3;
						}
						if((bool)cmd.param2 != system_status.is_gripper_rotate) {
							HAL_GPIO_WritePin(GPIOC, GPIO_PIN_2, cmd.param2 ? GPIO_PIN_SET : GPIO_PIN_RESET);
							system_status.is_gripper_rotate = cmd.param2 ? 1 : 0;
							system_status.status_flag = 4;
						}
						if((bool)cmd.param3 != system_status.is_light)
						{
							if((bool)cmd.param3)
							{
								HAL_TIM_PWM_Start(light_htim_p, TIM_CHANNEL_1);
								__HAL_TIM_SET_COMPARE(light_htim_p, TIM_CHANNEL_1, system_status.light_value);
								system_status.is_light = true;
							}
							else
							{
								__HAL_TIM_SET_COMPARE(light_htim_p, TIM_CHANNEL_1, 0);
								HAL_TIM_PWM_Stop(light_htim_p, TIM_CHANNEL_1);
								system_status.is_light = false;
							}
							system_status.status_flag = 2;
						}
					}
					if (cmd.command == MAV_CMD_DO_SET_SERVO) {
						system_status.light_value = (uint16_t)cmd.param1;
						if (system_status.is_light)
						{
						    __HAL_TIM_SET_COMPARE(light_htim_p, TIM_CHANNEL_1, (uint16_t)cmd.param1);
						    system_status.status_flag = 2;
						}
					}
					if (cmd.command == MAV_CMD_DO_SET_PARAMETER)
					{
						if(cmd.confirmation == 1)
						{
							if(cmd.param1 == 0) {
								SetPID(&RollPID, cmd.param2, cmd.param3, cmd.param4, cmd.param5, RollPID.out_max, RollPID.out_min, cmd.param6);
								GetPID(&RollPID, PID_Parameter);
								snprintf(parameter_Str, sizeof(parameter_Str), "Roll(%.2f, %.2f, %.2f, %.2f, %.2f)", PID_Parameter[0], PID_Parameter[1], PID_Parameter[2], PID_Parameter[3], PID_Parameter[4]);
							}
							else if(cmd.param1 == 1) {
								SetPID(&PitchPID, cmd.param2, cmd.param3, cmd.param4, cmd.param5, PitchPID.out_max, PitchPID.out_min, cmd.param6);
								GetPID(&PitchPID, PID_Parameter);
								snprintf(parameter_Str, sizeof(parameter_Str), "Pitch(%.2f, %.2f, %.2f, %.2f, %.2f)", PID_Parameter[0], PID_Parameter[1], PID_Parameter[2], PID_Parameter[3], PID_Parameter[4]);
							}
							else if(cmd.param1 == 2) {
								SetPID(&DepthPID, cmd.param2, cmd.param3, cmd.param4, cmd.param5, DepthPID.out_max, DepthPID.out_min, cmd.param6);
								GetPID(&DepthPID, PID_Parameter);
								snprintf(parameter_Str, sizeof(parameter_Str), "Depth(%.2f, %.2f, %.2f, %.2f, %.2f)", PID_Parameter[0], PID_Parameter[1], PID_Parameter[2], PID_Parameter[3], PID_Parameter[4]);
							}
							else if(cmd.param1 == 3) {
								atmospheric_pressure = cmd.param2;
								snprintf(parameter_Str, sizeof(parameter_Str), "Atmospheric Pressure value: %.2f", atmospheric_pressure);
							}
							else if(cmd.param1 == 4) {
								ROLL_OFFSET = MPU_data.roll;
								snprintf(parameter_Str, sizeof(parameter_Str), "Roll OFFSET: %.2f", ROLL_OFFSET);
							}
							else if(cmd.param1 == 5) {
								PITCH_OFFSET = MPU_data.pitch;
								snprintf(parameter_Str, sizeof(parameter_Str), "Pitch OFFSET: %.2f", PITCH_OFFSET);
							}
							system_status.status_flag = 5;
						}
						else if(cmd.confirmation == 0) {
							if(cmd.param1 == 0) {
								GetPID(&RollPID, PID_Parameter);
								snprintf(parameter_Str, sizeof(parameter_Str), "Roll(%.2f, %.2f, %.2f, %.2f, %.2f)", PID_Parameter[0], PID_Parameter[1], PID_Parameter[2], PID_Parameter[3], PID_Parameter[4]);
							}
							else if(cmd.param1 == 1) {
								GetPID(&PitchPID, PID_Parameter);
								snprintf(parameter_Str, sizeof(parameter_Str), "Pitch(%.2f, %.2f, %.2f, %.2f, %.2f)", PID_Parameter[0], PID_Parameter[1], PID_Parameter[2], PID_Parameter[3], PID_Parameter[4]);
							}
							else if(cmd.param1 == 2) {
								GetPID(&DepthPID, PID_Parameter);
								snprintf(parameter_Str, sizeof(parameter_Str), "Depth(%.2f, %.2f, %.2f, %.2f, %.2f)", PID_Parameter[0], PID_Parameter[1], PID_Parameter[2], PID_Parameter[3], PID_Parameter[4]);
							}
							else if(cmd.param1 == 3) {
								snprintf(parameter_Str, sizeof(parameter_Str), "Atmospheric Pressure value: %.2f", atmospheric_pressure);
							}
							else if(cmd.param1 == 4) {
								snprintf(parameter_Str, sizeof(parameter_Str), "Roll OFFSET: %.2f", ROLL_OFFSET);
							}
							else if(cmd.param1 == 5) {
								snprintf(parameter_Str, sizeof(parameter_Str), "Pitch OFFSET: %.2f", PITCH_OFFSET);
							}
							system_status.status_flag = 5;
						}
						else if(cmd.confirmation == 2) {
							if(cmd.param1 == 0) {
								isRollPID = !isRollPID;
								if(!isRollPID) {
									RollPID.setpoint = 0.0f;
									RollPID.integrator = 0.0f;
									RollPID.prev_error = 0.0f;
									RollPID.D_lpf = 0.0f;
								}
								snprintf(parameter_Str, sizeof(parameter_Str), isRollPID? "Roll PID IS ON.": "Roll PID IS OFF.");
							}
							if(cmd.param1 == 1) {
								isPitchPID = !isPitchPID;
								if(!isPitchPID) {
									PitchPID.setpoint = 0.0f;
									PitchPID.integrator = 0.0f;
									PitchPID.prev_error = 0.0f;
									PitchPID.D_lpf = 0.0f;
								}
								snprintf(parameter_Str, sizeof(parameter_Str), isPitchPID? "Pitch PID IS ON.": "Pitch PID IS OFF.");
							}
							if(cmd.param1 == 2) {
								isDepthPID = !isDepthPID;
								if(!isDepthPID) {
									DepthPID.setpoint = 0.0f;
									DepthPID.integrator = 0.0f;
									DepthPID.prev_error = 0.0f;
									DepthPID.D_lpf = 0.0f;
								}
								snprintf(parameter_Str, sizeof(parameter_Str), isDepthPID? "Depth PID IS ON.": "Depth PID IS OFF.");
							}
							system_status.status_flag = 6;
						}
					}
					break;
				}
				default:
					break;
            	}
            }
            read_index = (read_index + 1) % MAVLINK_DMA_BUFFER_SIZE;
        }
        osDelay(5);
    }
}
void StatusTask(void *argument) {
	uint32_t last_Status_tick = 0;
	uint32_t last_heartbeat_tick = 0;
	for(;;) {
		uint32_t current_tick = osKernelGetTickCount();
		if ((current_tick - last_Status_tick) >= 100) {
			Send_Component_status_msg();
			last_Status_tick = current_tick;
		}
		if ((current_tick - last_heartbeat_tick) >= 2000) {
			Send_heartbeat_msg();
			last_heartbeat_tick = current_tick;
		}
		if(system_status.status_flag != 0) {
			switch(system_status.status_flag ) {
			case 1:
				if(system_status.thruster_flag == 1) {
					if(thrusters_state == THRUSTERS_ACTIVE) {
						Send_status_msg("Thrusters Enabled");
						system_status.thruster_flag = 0;
					}
					else if(thrusters_state == THRUSTERS_CALIBRATING) {
						Send_status_msg("Thrusters are being Calibrated...");
						system_status.thruster_flag = 2;
					}
				}
				else {
					if(system_status.thruster_flag == 3)
						Send_status_msg("Calibration Stopped");
					if(system_status.thruster_flag == 4)
						Send_status_msg("Thrusters Disabled");
					system_status.thruster_flag = 0;
				}
				break;
			case 2:
				if(system_status.is_light) {
					char status_buffer[50];
					snprintf(status_buffer, sizeof(status_buffer), "Light ON: %u", (uint16_t)system_status.light_value);
					Send_status_msg(status_buffer);
				}
				else
					Send_status_msg("Light OFF");
				break;
			case 3:
				if(system_status.is_gripper_closed)
					Send_status_msg("Gripper Closed");
				else
					Send_status_msg("Gripper Opened");
				break;
			case 4:
				if(system_status.is_gripper_rotate)
					Send_status_msg("Gripper H");
				else
					Send_status_msg("Gripper V");
				break;
			case 5:
				Send_status_msg(parameter_Str);
				break;
			case 6:
				Send_pid_msg();
				Send_status_msg(parameter_Str);
				break;
			}
			system_status.status_flag = 0;
		}
		if(system_status.thruster_flag != 0) {
			if(system_status.thruster_flag == 2) {
				if(thrusters_state == THRUSTERS_READY) {
					for (uint8_t i = 0; i < MAX_THRUSTERS; i++) {
						thrusters[i].state = TH_ACTIVE;
					}
					thrusters_state = THRUSTERS_ACTIVE;
					Send_status_msg("Thrusters Calibrated");
					Send_status_msg("Thrusters Enabled");
					system_status.thruster_flag = 0;
				}
			}
		}
		osDelay(20);
	}
}
void MavlinkTxTask(void *argument) {
	mavlink_queue_item_t qItem;
    static uint8_t tx_temp_buffer[MAVLINK_MAX_PACKET_LEN];
    for(;;) {
        if (osMessageQueueGet(mavlinkTxQueueHandle, &qItem, NULL, osWaitForever) == osOK) {
            if (osSemaphoreAcquire(xUartDmaSemaphore, 100) == osOK) {
                uint16_t len = mavlink_msg_to_send_buffer(tx_temp_buffer, &qItem.msg);
                if (HAL_UART_Transmit_DMA(mavlink_handler.huart, tx_temp_buffer, len) != HAL_OK) {
                    osSemaphoreRelease(xUartDmaSemaphore); // تحرير في حال فشل الـ DMA في البدء
                }
            } else {
                osSemaphoreRelease(xUartDmaSemaphore);
            }
        }
    }
}
void MavLink_TxCpltCallback(UART_HandleTypeDef *huart) {
    if (huart == mavlink_handler.huart) {
        osSemaphoreRelease(xUartDmaSemaphore);
    }
}
void MAVLink_QueueMessage(mavlink_message_t *msg) {
	mavlink_queue_item_t item;
    memcpy(&item.msg, msg, sizeof(mavlink_message_t));
    osStatus_t status = osMessageQueuePut(mavlinkTxQueueHandle, &item, 0, 0); // اجعل الـ timeout صفر
    if (status != osOK) {}
}
void MavLink_Init(UART_HandleTypeDef *huart, uint8_t sysid, uint8_t compid) {
    mavlink_handler.huart = huart;
    mavlink_handler.system_id = sysid;
    mavlink_handler.component_id = compid;

    PID_SetTargetLimits(&RollPID, -30.0f, 30.0f);
    PID_SetTargetLimits(&PitchPID, -30.0f, 30.0f);
    PID_SetTargetLimits(&DepthPID, 0.0f, 1000.0f);
    PID_SetTarget(&RollPID, 0.0f);
    PID_SetTarget(&PitchPID, 0.0f);

    xUartDmaSemaphore = osSemaphoreNew(1, 1, NULL);
    mavlinkTxQueueHandle = osMessageQueueNew(50, sizeof(mavlink_queue_item_t), NULL);
    const osThreadAttr_t tx_attr   = { .name = "MavTx",    .stack_size = 1024, .priority = osPriorityAboveNormal };
    const osThreadAttr_t rx_attr   = { .name = "MavRx",    .stack_size = 1024, .priority = osPriorityNormal };
    const osThreadAttr_t stat_attr = { .name = "Status",   .stack_size = 1024, .priority = osPriorityNormal };
    const osThreadAttr_t imu_attr  = { .name = "IMU_Task", .stack_size = 1024, .priority = osPriorityAboveNormal };
    const osThreadAttr_t pressure_attr = { .name = "pressure_Task",.stack_size = 1024, .priority = osPriorityBelowNormal };

    mavlinkTxTaskHandle = osThreadNew(MavlinkTxTask, NULL, &tx_attr);
    mavlinkRxTaskHandle = osThreadNew(MavlinkRxTask, NULL, &rx_attr);
    statusTxTaskHandle  = osThreadNew(StatusTask, NULL, &stat_attr);
    imuTaskHandle       = osThreadNew(ImuTask, NULL, &imu_attr);
    pressureTaskHandle  = osThreadNew(pressureTask, NULL, &pressure_attr);
    HAL_UART_Receive_DMA(mavlink_handler.huart, mavlink_rx_buffer, MAVLINK_DMA_BUFFER_SIZE);
}
void HAL_UART_ErrorCallback(UART_HandleTypeDef *huart) {
    if (huart->Instance == mavlink_handler.huart->Instance) {
        HAL_UART_Receive_DMA(mavlink_handler.huart, mavlink_rx_buffer, MAVLINK_DMA_BUFFER_SIZE);
    }
}
