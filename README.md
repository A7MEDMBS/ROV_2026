# 🌊 Assiut Robotics - ROV 2026 System
![GitHub repo size](https://img.shields.io/github/repo-size/A7MEDMBS/ROV_2026)
![GitHub stars](https://img.shields.io/github/stars/A7MEDMBS/ROV_2026?style=social)

> **MATE ROV Competition - Pioneers Class (2026)**
> The official repository for the complete Software, Hardware, and Architecture designs for the **Assiut Robotics** Remotely Operated Vehicle.

---

## 📑 Table of Contents
- [System Architecture](#-system-architecture)
- [Hardware Stack](#-hardware-stack)
- [Software Modules](#-software-modules)
  - [1. Flight Controller (Firmware)](#1-flight-controller-firmware)
  - [2. Ground Control Station (GCS)](#2-ground-control-station-gcs)
  - [3. Vision & Telemetry Bridge (Companion Computer)](#3-vision--telemetry-bridge-companion-computer)
  - [4. Telemetry Backend Server](#4-telemetry-backend-server)
- [Communication Protocol (MAVLink)](#-communication-protocol)
- [Repository Structure](#-repository-structure)
- [Getting Started](#-getting-started)

---

## 🏗️ System Architecture
Our ROV operates on a highly distributed control system designed for high reliability, deterministic execution, and ultra-low latency. The system architecture bridges the **Surface Segment (GCS)** and the **Underwater Segment (ROV)** via a high-speed Ethernet tether.

---

## 🔌 Hardware Stack
- **Main Microcontroller:** STM32F405RGT6 (ARM Cortex-M4).
- **Companion Computer:** Raspberry Pi 4.
- **Sensor Suite:**
  - **IMU:** MPU6050 (6-DoF Accelerometer & Gyroscope) interfaced via I2C.
  - **Magnetometer:** HMC5883L (Digital Compass) interfaced via I2C.
  - **Depth/Pressure Sensor:** MS5540C interfaced via SPI.

---

## 💻 Software Modules

### 1. Flight Controller (Firmware) 🧠
Located in `Software/Flight Controller/STM32F405RGT6`.
Developed entirely in **C/C++** utilizing the **STM32 HAL** and **FreeRTOS**.
- **Deterministic Scheduling:** FreeRTOS manages real-time tasks including sensor data acquisition, PID control loops, and telemetry transmission without blocking system resources.
- **Non-blocking UART (DMA):** Utilizes **Direct Memory Access (DMA)** to queue and transmit MAVLink packets asynchronously, saving CPU cycles for critical control loops.
- **Custom Hardware Drivers:** Bare-metal integration and custom filtering algorithms for the MPU6050, HMC5883L, and MS5540C sensors.

### 2. Ground Control Station (GCS) 🖥️
Located in `Software/GUI/ROV GUI Control`.
A robust Windows desktop application built with **C# and WPF (.NET)**.
- **MVVM Architecture:** Strictly isolates UI design (XAML) from backend logic, ensuring highly maintainable code.
- **Custom Controls & Data Converters:** Provides real-time visual feedback for ROV orientation (Artificial Horizon), Depth, Temperature, and Thruster diagnostic status.
- **Asynchronous Parsing:** A custom `UARTCommunication` class handles `SerialPort` events and MAVLink packet decoding in real-time.
- **Input Handling:** Real-time joystick/gamepad input mapping to motor mixer commands.

### 3. Vision & Telemetry Bridge (Companion Computer) 📷
Python-based backend scripts deployed on the Raspberry Pi.
- **UDP Camera Streamer (`camera_streamer.py`):** Captures multi-camera feeds and streams them to the surface via UDP sockets for minimal latency.
- **Serial-Ethernet Bridge (`uart_udp_bridge.py`):** Relays MAVLink serial data from the STM32 to the Ethernet tether, wrapping UART packets into UDP datagrams.

### 4. Telemetry Backend Server 🌐
High-performance backend data server written in **Go (Golang)**.
- Utilizes **Gorilla WebSockets** for real-time, bi-directional event broadcasting to web dashboards.
- HTTP handlers for logging system events, sensor spikes, and mission-critical data.

---

## 📡 Communication Protocol
The core communication relies on the **MAVLink Protocol**.
- Ensures lightweight, checksum-verified data packets over long physical tethers.
- Used for sending Attitude, Depth, and System Status from the ROV to the GCS.
- Used for sending RC Channels (Joystick inputs) and commands from the GCS to the ROV.

---

## 📂 Repository Structure
```text
ROV_2026/
├── Architecture/           # System block diagrams and logic flowcharts
├── Hardware/               # PCB designs (Proteus/Altium), wiring, and schematics
├── Software/
│   ├── Flight Controller/  # STM32 Firmware (C/C++, FreeRTOS, HAL)
│   ├── GUI/                # GCS Project (C# WPF, MVVM)
│   ├── Video Streaming/    # Python UDP streaming & serial bridging scripts
│   └── Backend/            # Go WebSocket server for telemetry logging
└── README.md               # Documentation