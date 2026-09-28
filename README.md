# Assiut Robotics - ROV 2026 System 🌊🤖

This repository contains the complete software, hardware, and architecture designs for the Assiut Robotics team's Remotely Operated Vehicle (ROV), developed for the **MATE Pioneers ROV competition**.

## 📂 Repository Structure

The repository is organized into three main modules:

### 1. `Architecture/`
Contains system block diagrams, logic flowcharts, and the overall structural plan of the ROV's control and power systems.

### 2. `Hardware/`
Includes schematics, PCB designs, and wiring diagrams for the ROV's internal electronics and power distribution.

### 3. `Software/`
Contains the core codebase for both the vehicle and the surface control station:
- **`Flight Controller/ (STM32F405RGT6)`**: The embedded firmware developed in C/C++. 
  - Utilizes **FreeRTOS** for deterministic task scheduling.
  - Implements the **MAVLink** protocol over UART/DMA for robust telemetry and command queuing.
  - Handles direct interfacing with onboard sensors (MPU6050, HMC5883L, MS5540C) via I2C/SPI.
- **`GUI/ (ROV GUI Control)`**: The Ground Control Station (GCS) software.
  - Built with **C# and WPF** using the MVVM architecture.
  - Handles real-time serial port data parsing, joystick input mapping, and graphical telemetry display.
- **`Video Streaming/`**: Python scripts deployed on a Raspberry Pi for low-latency multi-camera streaming over UDP sockets.

## ⚙️ Technologies & Protocols
- **Embedded Systems:** C, C++, STM32 HAL, FreeRTOS.
- **Desktop Application:** C#, .NET, XAML (WPF).
- **Communication:** MAVLink, UART, I2C, SPI, UDP/TCP.

## 🚀 Getting Started

### Prerequisites
- [STM32CubeIDE](https://www.st.com/en/development-tools/stm32cubeide.html) for firmware compilation.
- [Visual Studio 2022](https://visualstudio.microsoft.com/) for building the C# GUI.

### Installation
1. Clone this repository:
   ```bash
   git clone [https://github.com/A7MEDMBS/ROV_2026.git](https://github.com/A7MEDMBS/ROV_2026.git)