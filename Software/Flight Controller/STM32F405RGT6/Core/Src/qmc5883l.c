#include "qmc5883l.h"

bool QMC5883L_Init(QMC5883L_Handler *dev, I2C_HandleTypeDef *hi2c) {
    dev->hi2c = hi2c;
    dev->devAddr = QMC5883L_ADDR_DEFAULT;
    dev->x_offset = 0;
    dev->y_offset = 0;
    dev->z_offset = 0;
    dev->sensor_status = false;
    uint8_t check, data;
    HAL_I2C_DeInit(dev->hi2c);
    HAL_I2C_Init(dev->hi2c);
    HAL_Delay(5);
    if (HAL_I2C_Mem_Read(dev->hi2c, dev->devAddr, Q_WHO_AM_I_REG, 1, &check, 1, 100) != HAL_OK || check != 0xFF) return false;
    data = 0x1D;
    if (HAL_I2C_Mem_Write(dev->hi2c, dev->devAddr, QMC_REG_CONTROL_1, 1, &data, 1, 100) != HAL_OK) return false;
    data = 0x01;
    if (HAL_I2C_Mem_Write(dev->hi2c, dev->devAddr, QMC_REG_CONTROL_2, 1, &data, 1, 100) != HAL_OK) return false;
    HAL_Delay(10);
    dev->sensor_status = true;
    return true;
}
void QMC5883L_Calibrate(QMC5883L_Handler *dev, uint16_t duration_ms) {
    int16_t x_min = 32767, x_max = -32768;
    int16_t y_min = 32767, y_max = -32768;
    uint8_t raw[4];
    uint32_t startTime = HAL_GetTick();

    while ((HAL_GetTick() - startTime) < duration_ms) {
        uint8_t status = 0;
        HAL_I2C_Mem_Read(dev->hi2c, dev->devAddr, QMC_REG_STATUS, 1, &status, 1, 10);
        if (status & 0x01) {
            HAL_I2C_Mem_Read(dev->hi2c, dev->devAddr, QMC_REG_DATA_X_L, 1, raw, 4, 10);
            int16_t x = (int16_t)(raw[1] << 8 | raw[0]);
            int16_t y = (int16_t)(raw[3] << 8 | raw[2]);
            if (x < x_min) x_min = x;
            if (x > x_max) x_max = x;
            if (y < y_min) y_min = y;
            if (y > y_max) y_max = y;
        }
        HAL_Delay(10);
    }
    dev->x_offset = (x_max + x_min) / 2;
    dev->y_offset = (y_max + y_min) / 2;
}
MagData QMC5883L_Update(QMC5883L_Handler *dev) {
    MagData data = {0};
    uint8_t raw[6];
    uint8_t status = 0;
    if (__HAL_I2C_GET_FLAG(dev->hi2c, I2C_FLAG_BUSY)) {
        dev->sensor_status = false;
        QMC5883L_Init(dev, dev->hi2c);
        goto fill_output;
    }
    /*HAL_I2C_Mem_Read(dev->hi2c, dev->devAddr, QMC_REG_STATUS, 1, &status, 1, 10);
    if (!(status & 0x01)) {
        goto fill_output;
    }*/
    if (HAL_I2C_Mem_Read(dev->hi2c, dev->devAddr, QMC_REG_DATA_X_L, 1, raw, 6, 10) != HAL_OK) {
        dev->sensor_status = false;
        QMC5883L_Init(dev, dev->hi2c);
        goto fill_output;
    }
    dev->sensor_status = true;
    int16_t x_raw = (int16_t)(raw[1] << 8 | raw[0]);
    int16_t y_raw = (int16_t)(raw[3] << 8 | raw[2]);
    int16_t z_raw = (int16_t)(raw[5] << 8 | raw[4]);
    data.mag_x = (float)(x_raw - dev->x_offset);
    data.mag_y = (float)(y_raw - dev->y_offset);
    data.mag_z = (float)(z_raw - dev->z_offset);
    float heading = atan2f(data.mag_y, data.mag_x);
    if (heading < 0) heading += 2 * PI_VAL;
    data.heading_deg = heading * 180.0f / PI_VAL;
fill_output:
    return data;
}
float QMC5883L_GetTiltCompensatedHeading(MagData *mag, float roll_deg, float pitch_deg) {
    float roll = roll_deg * PI_VAL / 180.0f;
    float pitch = pitch_deg * PI_VAL / 180.0f;
    float X_h = mag->mag_x * cosf(pitch) + mag->mag_z * sinf(pitch);
    float Y_h = mag->mag_x * sinf(roll) * sinf(pitch) + mag->mag_y * cosf(roll) - mag->mag_z * sinf(roll) * cosf(pitch);
    float heading = atan2f(Y_h, X_h);
    if (heading < 0) heading += 2 * PI_VAL;
    return heading * 180.0f / PI_VAL;
}
