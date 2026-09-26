#ifndef MPU6050_H
#define MPU6050_H

#include "stm32f4xx.h"
#include <math.h>
#include <stdbool.h>

#define MPU6050_ADDR_DEFAULT  (0x68 << 1)
#define M_WHO_AM_I_REG          0x75
#define PWR_MGMT_1       	  0x6B
#define SIGNAL_PATH_RESET 	  0x68
#define ACCEL_XOUT_H          0x3B
#define RAD_TO_DEG        	  57.2957795f
#define FILTER_ALPHA     	  0.98f

typedef struct {
    float roll;
    float pitch;
    float yaw;
} IMUData;

typedef struct {
    I2C_HandleTypeDef *hi2c;
    uint16_t devAddr;
    uint32_t lastTick;
    float roll_filtered;
    float pitch_filtered;
    float yaw_acc;
    bool sensor_status;
} MPU6050_Handler;
extern volatile float ROLL_OFFSET;
extern volatile float PITCH_OFFSET;
bool MPU6050_Init(MPU6050_Handler *dev, I2C_HandleTypeDef *hi2c);
IMUData MPU6050_Update(MPU6050_Handler *dev);
#endif
