#ifndef SRC_MS5540C_H_
#define SRC_MS5540C_H_

#include "stm32f4xx.h"
#include "math.h"
#include "stdio.h"
#include <stdbool.h>

typedef struct {
    float temperature;
    float pressure;
    float depth;
} TPDData;

typedef struct {
	SPI_HandleTypeDef *hspi;
	TIM_HandleTypeDef *htim;
	bool sensor_status;
} MS5540C_Handler;

extern volatile float atmospheric_pressure;
extern volatile float temperature;
extern volatile float pressure;
extern volatile float depth;

bool MS5540C_Init(MS5540C_Handler *dev, SPI_HandleTypeDef *hspi, TIM_HandleTypeDef *htim);
void MS5540C_Reset(MS5540C_Handler *dev);
uint16_t Read_Calibration_Word(MS5540C_Handler *dev, uint8_t cmd1, uint8_t cmd2);
uint16_t Read_Pressure(MS5540C_Handler *dev);
uint16_t Read_Temperature(MS5540C_Handler *dev);
TPDData MS5540C_Update();


#endif /* SRC_MS5540C_H_ */
