#ifndef SRC_HMC5883L_H_
#define SRC_HMC5883L_H_

#include "stm32f4xx.h"
#include <math.h>
#include <stdbool.h>

#define HMC5883L_ADDR_DEFAULT  (0x1E << 1)
#define H_WHO_AM_I_REG           0x0A
#define HMC_REG_CONFIG_A       0x00
#define HMC_REG_CONFIG_B       0x01
#define HMC_REG_MODE           0x02
#define HMC_REG_DATA_X_M       0x03
#define HMC_REG_STATUS         0x09
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
} HMC5883L_Handler;

bool HMC5883L_Init(HMC5883L_Handler *dev, I2C_HandleTypeDef *hi2c);
void HMC5883L_Calibrate(HMC5883L_Handler *dev, uint16_t duration_ms);
MagData HMC5883L_Update(HMC5883L_Handler *dev);
float HMC5883L_GetTiltCompensatedHeading(MagData *mag, float roll_deg, float pitch_deg);
#endif
