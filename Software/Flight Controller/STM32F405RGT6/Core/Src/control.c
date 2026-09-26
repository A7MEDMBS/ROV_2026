#include "control.h"

static DepthMode current_depth_mode = DEPTH_MODE_MANUAL;
static float target_depth = 0.0f;
osThreadId_t ThrusterControlTaskHandle;
void ThrusterControlTask(void *argument)
{
	for(;;) {
		float fx = target_vx;
		float fy = target_vy;
		float fz = target_vz;
		float yaw = target_yaw;

		float current_depth = TPD_data.depth;
		float current_roll = MPU_data.roll;
		float current_pitch = MPU_data.pitch;

		float final_Fz = 0.0f;
		float final_Mx = 0.0f;
		float final_My = 0.0f;

		if (fabsf(fz) > FZ_DEADZONE) {
			current_depth_mode = DEPTH_MODE_MANUAL;
			final_Fz = fz * 100.0f;
			DepthPID.setpoint = 0.0f;
			DepthPID.integrator = 0.0f;
			DepthPID.prev_error = 0.0f;
			DepthPID.D_lpf = 0.0f;
		}
		else {
			if(isDepthPID) {
				if (current_depth_mode == DEPTH_MODE_MANUAL) {
					DepthPID.setpoint = current_depth;
					current_depth_mode = DEPTH_MODE_HOLD;
				}
				final_Fz =  pid_update(&DepthPID, current_depth, DT_SEC);
			}
			else
				final_Fz = 0;
		}
		if(isRollPID)
			final_Mx = pid_update(&RollPID, current_roll, DT_SEC);
		else
			final_Mx = 0;
		if(isPitchPID)
			final_My = pid_update(&PitchPID, current_pitch, DT_SEC);
		else
			final_My = 0;
		Move_Update(fx, fy, final_Fz, final_Mx, final_My, yaw);
		osDelay(pdMS_TO_TICKS(CONTROL_LOOP_PERIOD_MS));
	}
}
void Control_System_Init(UART_HandleTypeDef *mavlink_huart, I2C_HandleTypeDef *mpu_hi2c, I2C_HandleTypeDef *qmc_hi2c, SPI_HandleTypeDef *msc_hspi,TIM_HandleTypeDef *msc_htim, TIM_HandleTypeDef *light_htim)
{
	system_status.mpu_sensor_status = MPU6050_Init(&mpu1, mpu_hi2c);
	system_status.compass_sesnsor_status = QMC5883L_Init(&qmc1, qmc_hi2c);
	QMC5883L_Calibrate(&qmc1, 5000);
	system_status.pressur_sensor_status = MS5540C_Init(&msc1, msc_hspi, msc_htim);
	light_htim_p = light_htim;
	Thrusters_Init();
	MavLink_Init(mavlink_huart, 1, 1);
    const osThreadAttr_t control_attr = { .name = "thruster_control_task",.stack_size = 2048, .priority = osPriorityNormal };
    ThrusterControlTaskHandle = osThreadNew(ThrusterControlTask, NULL, &control_attr);
}
