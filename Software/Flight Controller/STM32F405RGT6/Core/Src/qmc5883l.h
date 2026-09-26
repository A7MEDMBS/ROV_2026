#ifndef SRC_QMC5883L_H_
#define SRC_QMC5883L_H_

#include "stm32f4xx.h"
#include <math.h>
#include <stdbool.h>

#define QMC5883L_ADDR_DEFAULT  (0x0D << 1)
#define Q_WHO_AM_I_REG      	   0x0D
#define QMC_REG_DATA_X_L       0x00
#define QMC_REG_CONTROL_1      0x09
#define QMC_REG_CONTROL_2      0x0A
#define QMC_REG_STATUS         0x06
#define PI_VAL                 3.14159265f

typedef struct {
    float heading_deg;
    float mag_x;
    float mag_y;
    float mag_z;
} MagData;

typedef struct {
    I2C_HandleTypeDef *hi2c;
    uint16_t devAddr;
    int16_t x_offset;
    int16_t y_offset;
    int16_t z_offset;
    bool sensor_status;
} QMC5883L_Handler;

bool QMC5883L_Init(QMC5883L_Handler *dev, I2C_HandleTypeDef *hi2c);
void QMC5883L_Calibrate(QMC5883L_Handler *dev, uint16_t duration_ms);
MagData QMC5883L_Update(QMC5883L_Handler *dev);
float QMC5883L_GetTiltCompensatedHeading(MagData *mag, float roll_deg, float pitch_deg);

#endif
