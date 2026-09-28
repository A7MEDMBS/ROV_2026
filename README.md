# 🌊 Assiut Robotics - ROV 2026 System
> **MATE ROV Competition - Pioneers Class (2026)**
> The official software, hardware, and architecture repository for the **Assiut Robotics** Remotely Operated Vehicle.

This repository houses the complete engineering lifecycle of our ROV system, featuring a deeply optimized C/C++ firmware built on a Real-Time Operating System (FreeRTOS) and a high-performance Ground Control Station (GCS) engineered in C# WPF.

---

## 🌟 Core Technical Highlights
*   **Mathematical Thrust Allocation:** 6-DoF control utilizing a custom $6 \times 7$ allocation matrix solved via a bounded **Cholesky decomposition** algorithm for optimal thrust distribution.
*   **Real-Time Determinism:** Firmware architecture relies entirely on **FreeRTOS** tasks, message queues, and semaphores to guarantee deterministic execution of flight-critical loops at 50Hz.
*   **Zero-Blocking Network IO:** MAVLink telemetry is processed entirely via **UART Direct Memory Access (DMA)**, ensuring the CPU never blocks during packet transmission or reception.
*   **Multi-Process AI Pipeline:** The GCS isolates Computer Vision workloads by dynamically spawning isolated Python processes, communicating bounding boxes and scores back to the C# UI via a high-speed local UDP JSON bridge.
*   **Bespoke Vector UI:** No standard controls are used. The GCS features custom-built, mathematically calculated XAML vector gauges rendering at 60FPS.

---

## 💻 1. Flight Controller Firmware (STM32F405)
Developed in C utilizing the STM32 HAL, focused on fault tolerance and sensor fusion.

### Kinematics & Control Dynamics
*   **Thruster Mapping:** Implements `Build_Allocation_Matrix` to calculate rotational torques using the cross product of physical thruster positions ($r$) and direction vectors ($d$).
*   **Bounded Solving:** `Cholesky_Solve` ensures the overactuated 7-thruster array distributes force optimally without exceeding the hardware PWM constraints.
*   **Live PID Tuning:** Proportional-Integral-Derivative controllers for Roll, Pitch, and Depth (with anti-windup and derivative low-pass filtering) can be tuned mid-dive over-the-air via MAVLink `DO_SET_PARAMETER` commands.
*   **Auto Depth-Hold:** A state-machine seamlessly transitions into `DEPTH_MODE_HOLD` when no vertical manual input is detected, locking the vehicle's altitude automatically.

### Sensor Fusion
*   **Custom I2C/SPI Drivers:** Bare-metal implementations for the MPU6050 (IMU), QMC5883L (Compass), and MS5540C (Pressure).
*   **Tilt Compensation:** The QMC5883L driver mathematically fuses magnetic vectors with the IMU's Alpha-filtered Roll and Pitch data to provide highly accurate heading regardless of vehicle orientation.

---

## 🖥️ 2. Ground Control Station (GCS)
A multi-threaded Windows desktop application built with **C# WPF (.NET)** using a strict **MVVM (Model-View-ViewModel)** architectural pattern.

### Advanced User Interface
*   **Mathematical Gauges:** Custom UserControls (`DepthGauge`, `Compass`) dynamically calculate tick placements and needle animations using C# trigonometry, applying `TranslateTransform` and `RotateTransform` via the `Dispatcher`.
*   **Tactile Input Mapping:** Integrates `SharpDX.DirectInput` to poll physical joysticks at 30ms intervals. Raw axes are normalized and packaged into MAVLink `SET_POSITION_TARGET_LOCAL_NED` commands.
*   **Dynamic Configuration:** The `ConfigManager` serializes IPs, UDP Ports, Python script paths, and PID coefficients into a `config.json` file, allowing poolside modifications without recompiling the application.

### Video Streaming & Computer Vision
*   **Zero-Dependency Streaming:** The `CAMStream` module receives raw JPEG/MJPEG bytes over UDP and renders them directly into memory-safe WPF `BitmapImage` objects using `MemoryStream`, completely avoiding heavy third-party media players.
*   **Dynamic Matrix Routing:** The pilot can hot-swap 3 concurrent UDP camera feeds across different UI viewports without tearing down the underlying socket connections.
*   **AI Integration (`VisionTask`):** Spawns Python scripts (`Task1.py`, `Task2.py`) via `ProcessStartInfo`. An asynchronous UDP loop receives JSON-encoded `VisionPacket` data, deserializing it to draw tracking bounding boxes (`Detection`) directly onto a transparent XAML `Canvas` overlaid on the video feed.

---

## 📡 3. Telemetry & Communications
The system relies on the **MAVLink Protocol** to ensure lightweight, checksum-verified data transmission over long physical tethers.
*   **Firmware:** A 512-byte circular DMA buffer captures incoming streams asynchronously.
*   **GCS Backend:** The `MAVLinkHandler` decodes byte streams in real-time (`HEARTBEAT`, `SYS_STATUS`, `VFR_HUD`, `ATTITUDE`) and fires synchronized C# events.
*   **Live Analytics:** Integrates **OxyPlot** with a `ConcurrentQueue` to render a sliding 10-second historical graph (350 data points) of water/tube pressure and temperature without locking the UI thread.

---

## 🚀 Getting Started

### Prerequisites
*   [STM32CubeIDE](https://www.st.com/en/development-tools/stm32cubeide.html) (Firmware)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/) (.NET Desktop Development)
*   [Python 3.12+](https://www.python.org/) with OpenCV (`cv2`) for Vision Tasks.

### Build Instructions
1.  **Firmware:** Navigate to the STM32 project directory, open the `.ioc` file, generate code, compile, and flash via ST-Link.
2.  **GCS:** Open `ROV_GUI_Control.sln` in Visual Studio, restore NuGet packages (OxyPlot, SharpDX, OpenCVSharp), and build the solution. Ensure `config.json` paths point to your local Python executable.

---

## 👨‍💻 Development
**Assiut Robotics - Assiut University**  
*(Computer and Control Systems Department)*
*   **Ahmed Mostafa Bakr Selim** - *Control Software Lead & Architecture*