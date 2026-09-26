#include "ms5540c.h"

static uint16_t D1 = 0, D2 = 0;
static uint16_t c1 = 0, c2 = 0, c3 = 0, c4 = 0, c5 = 0, c6 = 0;
volatile float atmospheric_pressure = 1015.0f;
volatile float temperature;
volatile float pressure;
volatile float depth;

bool MS5540C_Init(MS5540C_Handler *dev, SPI_HandleTypeDef *hspi, TIM_HandleTypeDef *htim)
{
	dev->hspi = hspi;
	dev->htim = htim;
	dev->sensor_status = false;
	HAL_TIM_PWM_Start(dev->htim, TIM_CHANNEL_1);
	HAL_Delay(100);
	HAL_SPI_Init(dev->hspi);
	c1 = Read_Calibration_Word(dev, 0x1D, 0x50);
	c2 = Read_Calibration_Word(dev, 0x1D, 0x60);
	c3 = Read_Calibration_Word(dev, 0x1D, 0x90);
	c4 = Read_Calibration_Word(dev, 0x1D, 0xA0);
	c1 = (c1 >> 1) & 0x7FFF;
	c5 = ((c1 & 0x0001) << 10) | ((c2 >> 6) & 0x03FF);
	c6 = c2 & 0x003F;
	c2 = ((c3 & 0x003F) << 6) | (c4 & 0x003F);
	c3 = (c4 >> 6) & 0x03FF;
	c4 = (c3 >> 6) & 0x03FF;

	D1 = Read_Pressure(dev);
	D2 = Read_Temperature(dev);
	if(D1 == 65535 || D2 == 65535 || D1 ==0)
	{
		return false;
	}
	dev->sensor_status = true;
	return true;
}
void MS5540C_Reset(MS5540C_Handler *dev)
{
  uint8_t reset_cmd[] = {0x15, 0x55, 0x40};
  HAL_SPI_Transmit(dev->hspi, reset_cmd, sizeof(reset_cmd), HAL_MAX_DELAY);
}
uint16_t Read_Calibration_Word(MS5540C_Handler *dev, uint8_t cmd1, uint8_t cmd2)
{
  uint8_t tx_buf[2] = {cmd1, cmd2};
  uint8_t rx_buf[2] = {0};
  MS5540C_Reset(dev);
  HAL_SPI_Transmit(dev->hspi, tx_buf, sizeof(tx_buf), HAL_MAX_DELAY);
  dev->hspi->Init.CLKPolarity = SPI_POLARITY_LOW;
  dev->hspi->Init.CLKPhase = SPI_PHASE_2EDGE;
  HAL_SPI_Init(dev->hspi);
  HAL_SPI_Receive(dev->hspi, rx_buf, sizeof(rx_buf), HAL_MAX_DELAY);
  dev->hspi->Init.CLKPolarity = SPI_POLARITY_LOW;
  dev->hspi->Init.CLKPhase = SPI_PHASE_1EDGE;
  HAL_SPI_Init(dev->hspi);
  return (rx_buf[0] << 8) | rx_buf[1];
}
uint16_t Read_Pressure(MS5540C_Handler *dev)
{
  uint8_t press_cmd[] = {0x0F, 0x40};
  uint8_t rx_buf[2] = {0};
  MS5540C_Reset(dev);
  HAL_SPI_Transmit(dev->hspi, press_cmd, sizeof(press_cmd), HAL_MAX_DELAY);
  HAL_Delay(35);
  dev->hspi->Init.CLKPolarity = SPI_POLARITY_LOW;
  dev->hspi->Init.CLKPhase = SPI_PHASE_2EDGE;
  HAL_SPI_Init(dev->hspi);
  HAL_SPI_Receive(dev->hspi, rx_buf, sizeof(rx_buf), HAL_MAX_DELAY);
  dev->hspi->Init.CLKPolarity = SPI_POLARITY_LOW;
  dev->hspi->Init.CLKPhase = SPI_PHASE_1EDGE;
  HAL_SPI_Init(dev->hspi);
  return (rx_buf[0] << 8) | rx_buf[1];
}
uint16_t Read_Temperature(MS5540C_Handler *dev)
{
  uint8_t temp_cmd[] = {0x0F, 0x20};
  uint8_t rx_buf[2] = {0};
  //MS5540_Reset(dev);
  HAL_SPI_Transmit(dev->hspi, temp_cmd, sizeof(temp_cmd), HAL_MAX_DELAY);
  HAL_Delay(35);
  dev->hspi->Init.CLKPolarity = SPI_POLARITY_LOW;
  dev->hspi->Init.CLKPhase = SPI_PHASE_2EDGE;
  HAL_SPI_Init(dev->hspi);
  HAL_SPI_Receive(dev->hspi, rx_buf, sizeof(rx_buf), HAL_MAX_DELAY);
  dev->hspi->Init.CLKPolarity = SPI_POLARITY_LOW;
  dev->hspi->Init.CLKPhase = SPI_PHASE_1EDGE;
  HAL_SPI_Init(dev->hspi);
  return (rx_buf[0] << 8) | rx_buf[1];
}
TPDData MS5540C_Update(MS5540C_Handler *dev)
{
	TPDData TPDdata = {0};
	D1 = Read_Pressure(dev);
	D2 = Read_Temperature(dev);
	if(D1 == 65535 || D2 == 65535 || D1 ==0)
	{
		dev->sensor_status = false;
		//MS5540C_Init(dev, dev->hspi, dev->htim);
		return TPDdata;
	}
	dev->sensor_status = true;
	long UT1 = (c5 << 3) + 20224;
	long dT = D2 - UT1;
	long TEMP = 200 + ((dT * (c6 + 50)) >> 10);
	long OFF = (c2 * 4) + (((c4 - 512) * dT) >> 12);
	long SENS = c1 + ((c3 * dT) >> 10) + 24576;
	long X = ((SENS * (D1 - 7168)) >> 14) - OFF;
	long PCOMP = ((X * 10) >> 5) + 2500;
	TPDdata.temperature = TEMP / 10.0f;
	TPDdata.pressure = PCOMP / 10.0f;
	float pressure_diff = TPDdata.pressure - atmospheric_pressure;
	pressure_diff = roundf(pressure_diff * 100.0f) / 100.0f;
	TPDdata.depth = (pressure_diff > 0) ? (pressure_diff * 1.02f) : 0.0f;
	return TPDdata;
}
