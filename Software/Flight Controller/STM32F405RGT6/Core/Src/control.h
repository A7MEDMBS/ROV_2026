#ifndef SRC_CONTROL_H_
#define SRC_CONTROL_H_

#include "stm32f4xx.h"
#include "mavlink.h"

typedef enum {
    DEPTH_MODE_MANUAL = 0,
    DEPTH_MODE_HOLD
} DepthMode;

#define ROLL_TARGET_FLAT 0.0f
#define PITCH_TARGET_FLAT 0.0f
#define FZ_DEADZONE 0.05f
#define CONTROL_LOOP_PERIOD_MS 20
#define DT_SEC ((float)CONTROL_LOOP_PERIOD_MS / 1000.0f)
void Control_System_Init(UART_HandleTypeDef *, I2C_HandleTypeDef *, I2C_HandleTypeDef *, SPI_HandleTypeDef *,TIM_HandleTypeDef *, TIM_HandleTypeDef *);
#endif /* SRC_CONTROL_H_ */
