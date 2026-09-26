#include "mpu6050.h"

volatile float ROLL_OFFSET = 0;
volatile float PITCH_OFFSET = 0;

bool MPU6050_Init(MPU6050_Handler *dev, I2C_HandleTypeDef *hi2c) {
    dev->hi2c = hi2c;
    dev->devAddr = (0x68 << 1);
    dev->sensor_status = false;
    uint8_t check, data;
    HAL_I2C_DeInit(dev->hi2c);
    HAL_I2C_Init(dev->hi2c);
    HAL_Delay(5);
    if (HAL_I2C_Mem_Read(dev->hi2c, dev->devAddr, M_WHO_AM_I_REG, 1, &check, 1, 100) != HAL_OK || check != 0x68) return true;
    data = 0x80;
    if (HAL_I2C_Mem_Write(dev->hi2c, dev->devAddr, PWR_MGMT_1, 1, &data, 1, 100) != HAL_OK) return true;
    HAL_Delay(100);
    data = 0x01;
    if (HAL_I2C_Mem_Write(dev->hi2c, dev->devAddr, PWR_MGMT_1, 1, &data, 1, 100) != HAL_OK) return true;
    data = 0x07;
    if (HAL_I2C_Mem_Write(dev->hi2c, dev->devAddr, SIGNAL_PATH_RESET, 1, &data, 1, 100) != HAL_OK) return true;
    HAL_Delay(10);
    dev->roll_filtered = 0.0f;
    dev->pitch_filtered = 0.0f;
    dev->lastTick = HAL_GetTick();
    dev->sensor_status = true;
    return false;
}
IMUData MPU6050_Update(MPU6050_Handler *dev) {
    IMUData imudata = {0};
    uint8_t raw[14];
    if (__HAL_I2C_GET_FLAG(dev->hi2c, I2C_FLAG_BUSY)) {
		dev->sensor_status = false;
		MPU6050_Init(dev, dev->hi2c);
		goto fill_output;
	}
	if (HAL_I2C_Mem_Read(dev->hi2c, dev->devAddr, ACCEL_XOUT_H, 1, raw, 14, 5) != HAL_OK) {
		dev->sensor_status = false;
		MPU6050_Init(dev, dev->hi2c);
		goto fill_output;
	}
    dev->sensor_status = true;
    int16_t ax_raw = (int16_t)(raw[0] << 8 | raw[1]);
    int16_t ay_raw = (int16_t)(raw[2] << 8 | raw[3]);
    int16_t az_raw = (int16_t)(raw[4] << 8 | raw[5]);
    int16_t gx_raw = (int16_t)(raw[8] << 8 | raw[9]);
    int16_t gy_raw = (int16_t)(raw[10] << 8 | raw[11]);
    int16_t gz_raw = (int16_t)(raw[12] << 8 | raw[13]);
    float ax = ax_raw / 16384.0f;
    float ay = ay_raw / 16384.0f;
    float az = az_raw / 16384.0f;
    float gx = gx_raw / 131.0f;
    float gy = gy_raw / 131.0f;
    float gz = gz_raw / 131.0f;
    uint32_t now = HAL_GetTick();
    float dt = (now - dev->lastTick) / 1000.0f;
    dev->lastTick = now;
    if (dt <= 0 || dt > 0.1f) dt = 0.02f;
    float r_acc = atan2f(-ay, sqrtf(ax*ax + az*az)) * RAD_TO_DEG;
    float p_acc = atan2f(-ax, sqrtf(ay*ay + az*az)) * RAD_TO_DEG;
    dev->roll_filtered = FILTER_ALPHA * (dev->roll_filtered - gx * dt) + (1.0f - FILTER_ALPHA) * r_acc;
    dev->pitch_filtered = FILTER_ALPHA * (dev->pitch_filtered + gy * dt) + (1.0f - FILTER_ALPHA) * p_acc;
    dev->yaw_acc += gz * dt;
fill_output:
    imudata.roll = (roundf(dev->roll_filtered * 100.0f) / 100.0f) - ROLL_OFFSET;
    imudata.pitch = (roundf(dev->pitch_filtered * 100.0f) / 100.0f) - PITCH_OFFSET;
    imudata.yaw = roundf(dev->yaw_acc * 100.0f) / 100.0f;
    return imudata;
}
