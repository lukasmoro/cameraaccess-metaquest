# Camera Access on Meta Quest for Prototyping
Cast the passthrough camera feed of Meta Quest headsets to OBS on Mac/PC to run ML models with Python on it (e.g. YOLO V8) and access the output via TCP/IP in Unity application for prototyping.

## System Requirements
- Apple Silicon (MPS) or Nvidia GPU (CUDA)
- Python 3.11.7 +
- Unity 2022.3.18f1 +
- OBS

## Setup
For using this project the Unity application (client-side) needs to be connected to the Python script (server-side) via TCP/IP. Also OBS needs to create a virtual camera with the screen casting from the same Quest that is then accessed by the Python script to run the model on it. The following steps need to be followed.

### Python on Windows
0. Download and install Python [here](https://www.python.org/downloads/windows/)
1. Open shell and navigate to the directory of the "Python" folder: cd /path/to/your/directory/cameraaccess-metaquest/Python
2. Create a virtual environment: python -m venv venv
3. Activate the virtual environment using PowerShell: .\venv\Scripts\Activate.ps1
4. Upgrade pip to the latest version: pip install --upgrade pip
5. Install dependencies: pip install -r requirements.txt
6. Execute the script: python main.py
7. The script should output the following: Using device: mps/cuda/cpu Waiting for a connection...
8. Now it is ready for connection to the Unity application
9. Stop the script using KeyboardInterupt: CTRL + C

### Python on Mac
0. Download and install Python [here](https://www.python.org/downloads/macos/)
1. Open terminal and navigate to the directory of the "Python" folder: cd /path/to/your/directory/cameraaccess-metaquest/Python
2. Create a virtual environment: python3 -m venv venv
3. Activate the virtual environment: source venv/bin/activate
4. Upgrade pip to the latest version: pip install --upgrade pip
5. Install dependencies: pip install -r requirements.txt
6. Execute the script: python main.py
7. The script should output the following: Using device: mps/cuda/cpu Waiting for a connection...
8. Now it is ready for connection to the Unity application
9. Stop the script using KeyboardInterupt: CTRL + C

### OBS
0. Download OBS and install [here](https://obsproject.com)
1. Create a new scene
2. Add a new ‘Browser’ source
3. Visit https://oculus.com/casting and login with your developer account
4. Start virtual camera

### Unity
0. Open the "Unity" folder with Unity Hub
1. If prompted, restart the Editor to update OVRPlugin
2. Switch platform to Android
3. Open the scene ‘Camera Access YOLO’
4. In the ‘NetworkingClient.cs’ script attached to the ‘Networking Client’ gameobject under ‘Host’ enter the IP address of the PC/Mac running the Python script in the inspector
5. Build the scene for your Quest (Will take some time for first time building, but should be quick afterwards)

### APK
0. Ensure the Python script is waiting for connection
1. Share your screen with OBS via casting
2. Start the APK
3. Press the ‘CONNECT’ button in the app or X on the left controller to connect with the server
4. Object detections appear on the field of view
5. Press the ‘DISCONNECT’ button or Y on the left controller to disconnect from server

## Additional Notes
- Ensure that Python is correctly added to your system's PATH to execute the scripts without any issues.
- If any package fails to install, check the specific package documentation for troubleshooting. You may need to install additional system dependencies, especially for packages like OpenCV.
- On Mac, you might need to grant camera access to the Python application via System Preferences > Security & Privacy > Camera.
- Ensure the port you are using (8080 in this script) is open and not blocked by firewall settings.
- Sometimes OpenCV opens the wrong camera if this happens restart the Python script
- When the ‘CONNECT’ button was pressed and the Python script was restarted, the ‘DISCONNECT’ button needs to be pushed once to then connect again
