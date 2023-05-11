Documentation for the Image Processing portion of the AI-Foosball Spring 2022 Senior Design Project.

If you are reading this, and are not familiar with either _Python_ or _UNIX_ terminal commands, then I'd **highly** suggest familiarizing yourself with them.

The user is a superuser so you shouldn't have any problems with permissions.

# Technologies In Use and Their Purpose

- OpenCV
- Python3
- Sockets

# Running and Startup Execution

For normal execution, `sudo python3 "your_app_here.py"` while in the project directory is sufficient.

## Setting to run at Startup

To run the script at startup we set ball*tracking.py to run as a \_service*.

> This ONLY works on linux!

The service script looks in the /bin folder for the .py file for execution. The .py file needs to be in a root folder so that it can be ran with root privilages at startup.

If you make changes to the code and what the startup script to relect it, you'll have to re-copy the .py in the /bin directory via the following command:

`sudo cp ./ball_tracking.py /bin`

Then to enable/diable the script from running at startup run:

`sudo systemctl enable|disable ball_tracking.service`

That's all there is to it. The [link](https://unix.stackexchange.com/questions/634410/start-python-script-at-startup) is the tutorial we used to set up the service, should you need to add a new/different service.

If you want to _stop_ the script, for instance to test/edit code run:

`sudo systemctl stop ball_tracking.service` and conversly `sudo systemctl start ball_tracking.service` to start the script again.

If, for some reason, the script doesn't appear to be working you can check the status of the script using the command:

`sudo systemctl status ball_tracking.service`

# What We did and Why We Did it.

Along with the ball*tracking.py (which is the main program), several other programs are included. In `./tools/` the \_fisheye_calibration.py* reads images from `./tools/imgs/` to determine the coefficients needed to produce the fisheye correction. The previously mentioned coefficients are stored in the calibration.json. The _calibration_check.py_ shows the corrected video stream, you can use this to check how well the coefficients were calculated.

Believe it or not, taking images on the jetson was surprisingly difficult (I blame it on the arm architecture). So I wrote am program in `./tools/imgs/photos.py` that you can run to take images from the camera (Press **space** to capture, **esc** to close).

## Purpose

The purpose of the jetson is to process image data, extrapolate the balls location and heading, then send the ball data to the pi for use in decision making. To achieve this we made use of OpenCV and Python for general programming, and TCP Sockets for communication.

To detect the ball, we used OpenCV and Python to build a blob detection algorithm to accurately detect the color of the ball. The algorithm would find the center of largest mass of colored pixels and return the camera frame's (x, y) cordinate in pixels. We converted from a pixel (x,y) to a millimeter (x,y) by applying a pixel-to-millimeter ratio based on the view of the camera and the size of the table. This method provided a surprising degree of accuracy that was sutable for the scope of this project.

### Challenges

In the early stages of design, we outlined two parameters that would define the constraints of our vision processing: processing time and reaction speed. In order for the system to respond appropriately to a fast(ish) moving ball we needed the system to process a corresponding amount of frames per second. The figures below show the required processing times, dependent on frame rate, and the distance covered between two frames, at various ball speeds and frames per second. Ultimately, our delieverable was to process a single frame under 10ms meaning greater than 100 frames processed per second, this would provide us with an adequate understanding of the balls location and direction of movement at a variety of ball speeds.

Another key element we took into consideration was lighting. Changes in lighting can have drastic effects on the performance of any vision algorithm. To mitigate these effects we made use of multiple color masks in the Hue, Saturation, Value (HSV) color spectrum. Rather than the RGB spectrum, the HSV spectrum takes into account how light or dark the color is, allowing for greater tollerances in varying lighting conditions. Multiple threashold masks were used to account for variances in color and lighting, at the end of the threadsholding operations, done using both masks, their values would be ORed resulting in a cleaner and more reliable location of the ball.

## Results

We performed numerous tests in various lighting conditions to produce the ensure that the ball would be adequately detected at all times of day and throughout normal play. To ensure performance we made use of Numpy and OpenCV extensively, as in both cases, the python codes are wrapper functions for C code underneath. This ensured that writing the program in python would not result in any drastic performance increases.

# Links and References

- DHCP Basics
  - <https://docs.microsoft.com/en-us/windows-server/troubleshoot/dynamic-host-configuration-protocol-basics>
- Building a DHCP Server into the Pi
  - <https://www.technicallywizardry.com/building-your-own-router-raspberry-pi/>
- Running scripts as startup
  - <https://unix.stackexchange.com/questions/634410/start-python-script-at-startup>
- The Camera we’re using is the **elp usbfhd08s-l36**. It can be found at this link.
  - <http://www.elpcctv.com/elp-full-hd-1080p-free-driver-usb20-high-speed-60-120-260fps-usb-camera-elpusbfhd08sl36-p-129.html>
- Fisheye Correction
  - <https://medium.com/@kennethjiang/calibrate-fisheye-lens-using-opencv-333b05afa0b0>
- Running OpenCV in the GPU
  - <https://www.jetsonhacks.com/2019/11/22/opencv-4-cuda-on-jetson-nano/>
