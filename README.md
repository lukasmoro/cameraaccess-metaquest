# Camera Access on Meta Quest for Prototyping
Cast the passthrough camera feed of Meta Quest headsets to OBS on Mac/PC to run ML models with Python on it (e.g. YOLO V8) and access the output via TCP/IP in Unity application for prototyping.

## System Requirements
- Apple Silicon (MPS) or Nvidia GPU (CUDA)
- Python 3.11.7 +
- Unity 2022.3.18f1 +
- OBS

## Setup
For using this project the Unity application (client-side) needs to be connected to the Python script (server-side) via TCP/IP. Also OBS needs to create a virtual camera with the screen casting from the same Quest that is then accessed by the Python script to run the model on it. The following steps need to be followed.

### Python
