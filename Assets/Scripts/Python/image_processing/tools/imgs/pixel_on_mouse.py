# Program to allow an individual to click on the video stream
# to get the pixel values.
from collections import deque
from imutils.video import VideoStream
import numpy as np
import argparse
import cv2
import imutils # pip install --upgrade imutils
import time

# if a video path was not supplied, grab the reference
# to the webcam
vs = VideoStream(0)
vs.start()

# otherwise, grab a reference to the video file
# allow the camera or video file to "warm up"
time.sleep(2.0)

def check_boundaries(value, tolerance, ranges, upper_or_lower):
	if ranges == 0:
		# set the boundary for hue
		boundary = 180
	elif ranges == 1:
		# set the boundary for saturation and value
		boundary = 255
	
	if(value + tolerance > boundary):
		value = boundary
	elif (value - tolerance < 0):
		value = 0
	else:
		if upper_or_lower == 1:
			value = value + tolerance
		else:
			value = value - tolerance
	return value

def onMouse(event, x, y, flags, params):
	if event == cv2.EVENT_LBUTTONDOWN:
		b = frame[y, x, 0]
		g = frame[y, x, 1]
		r = frame[y, x, 2]
		text = str(b) + ',' + str(g) + ',' + str(r)
		print(f"Color: {text}") 
		pixel = frame[y,x]
		#HUE, SATURATION, AND VALUE (BRIGHTNESS) RANGES. TOLERANCE COULD BE ADJUSTED.
		# Set range = 0 for hue and range = 1 for saturation and brightness 
		# set upper_or_lower = 1 for upper and upper_or_lower = 0 for lower
		hue_upper = check_boundaries(pixel[0], 10, 0, 1)
		hue_lower = check_boundaries(pixel[0], 10, 0, 0)
		saturation_upper = check_boundaries(pixel[1], 10, 1, 1)
		saturation_lower = check_boundaries(pixel[1], 10, 1, 0)
		value_upper = check_boundaries(pixel[2], 40, 1, 1)
		value_lower = check_boundaries(pixel[2], 40, 1, 0)

		upper =  np.array([hue_upper, saturation_upper, value_upper])
		lower =  np.array([hue_lower, saturation_lower, value_lower])

		print(f"Pixel value: ({pixel[0]}, {pixel[1]}, {pixel[2]})")

# run FOREVER!!!
frame = vs.read()
while(1):
	frame = vs.read()

	# resize the frame, blur it, and convert it to the HSV
	# color space
	frame = imutils.resize(frame, width=600)
	blurred = cv2.GaussianBlur(frame, (11, 11), 0)
	hsv = cv2.cvtColor(blurred, cv2.COLOR_BGR2HSV)

	

	cv2.imshow("Frame", frame)
	cv2.setMouseCallback("Frame", onMouse)

	key = cv2.waitKey(1) & 0xFF
	# if the 'q' key is pressed, stop the loop
	if key == ord("q"):
		break

	
vs.stop()
cv2.destroyAllWindows()
