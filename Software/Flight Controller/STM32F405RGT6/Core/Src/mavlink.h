#ifndef SRC_MAVLINK_H_
#define SRC_MAVLINK_H_

#include "stm32f4xx.h"
#include "mavlink/all/mavlink.h"
#include "pid.h"
#include "mpu6050.h"
#include "qmc5883l.h"
#include "ms5540c.h"
#include "motor_interface.h"

#define MAVLINK_DMA_BUFFER_SIZE 512
#define MAVLINK_TX_QUEUE_SIZE   30

typedef struct{
	uint8_t status_flag;
	uint8_t thruster_flag;
	bool is_power;
	bool is_light;
	bool is_gripper_closed;
	bool is_gripper_rotate;
	bool mpu_sensor_status;
	bool pressur_sensor_status;
	bool compass_sesnsor_status;
	uint8_t light_value;
}system_status_t;

typedef struct {
    UART_HandleTypeDef *huart;
    uint8_t system_id;
    uint8_t component_id;
} mavlink_handler_t;

typedef struct {
    mavlink_message_t msg;
} mavlink_queue_item_t;

extern mavlink_handler_t mavlink_handler;
extern system_status_t system_status;
extern TIM_HandleTypeDef *light_htim_p;
extern MPU6050_Handler mpu1;
extern QMC5883L_Handler qmc1;
extern MS5540C_Handler msc1;
extern volatile IMUData MPU_data;
extern volatile MagData QMC_data;
extern volatile TPDData TPD_data;
extern volatile PID RollPID;
extern volatile PID PitchPID;
extern volatile PID DepthPID;
extern float PID_Parameter[5];
extern char parameter_Str[100];
extern volatile float target_vx;
extern volatile float target_vy;
extern volatile float target_vz;
extern volatile float target_yaw;
extern volatile bool isRollPID;
extern volatile bool isPitchPID;
extern volatile bool isDepthPID;

void ImuTask(void *argument);
void pressureTask(void *pvParameters);

void Send_heartbeat_msg(void);
void Send_status_msg(const char* txt);
void Send_Component_status_msg(void);
void Send_pid_msg(void);

void MavlinkRxTask(void *argument);
void StatusTask(void *argument);
void MavlinkTxTask(void *argument);
void MavLink_TxCpltCallback(UART_HandleTypeDef *huart);
void MAVLink_QueueMessage(mavlink_message_t *msg);
void MavLink_Init(UART_HandleTypeDef *huart, uint8_t sysid, uint8_t compid);
#endif /* SRC_MAVLINK_H_ */
