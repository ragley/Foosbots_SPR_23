#!/usr/bin/python3
"""
To run: python3 ball_tracking.py

To run at boot:
- copy to /boot directory
	- sudo cp ./ball_tracking.py /boot/ball_tracking.py
- enable service
	- sudo systemctl enable ball_tracking.service
- All done!
See README for additional details
"""
from imutils.video import VideoStream
import numpy as np
import cv2
import imutils # pip install --upgrade imutils
import time
import json
import socket

# Dimensions of the Table (these are VERY important)
TABLE_LENGTH = 1193.8
TABLE_WIDTH = 684.2

# camera fisheye calibration arrays
# These are camara specific! Use fisheye_calibration.py to find these numbers!
DIM = (640, 360) # dimensions of the camera (in pixels)
K = np.array([[529.0990431685142, 0.0, 308.01283344567565], [0.0, 527.5594266286947, 168.13007918363098], [0.0, 0.0, 1.0]])
D = np.array([[-0.11376404127315753], [-0.46295061178792457], [2.167920495942993], [-2.8734014030396717]])

# Constants for sending data to the server!
HOST = "192.168.0.1" # static address of the pi
PORT = 5000 # port that is configured on the server
SOCKET = None # the socket connection

def connectToServer():
	while True:	
		try:
			sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock.connect((HOST, PORT))
			return sock
		except:
			print("Cannot find server... trying again...")		
			time.sleep(2)
			continue

def sendDataToServer(x, y, Vx, Vy):	
	data = {}
	data["action"] = "POST"
	location_data = {
		"ball_x": x,
		"ball_y": y, 
		"ball_Vx": Vx,
		"ball_Vy": Vy
	}
	data.update(location_data)
	json_data = json.dumps(data)
	json_bytes = bytes(json_data, encoding="utf-8")
	global SOCKET 
	try:
		# Send the data to the Server
		SOCKET.sendall(len(json_bytes).to_bytes(4, byteorder="big") + json_bytes)
	except (ConnectionResetError, BrokenPipeError): 
		# If the server shuts down mid game, we don't want to crash
		SOCKET = connectToServer()


def calculateVelocity(x, y, previous_x, previous_y, t1, t2):
	Vx = (x - previous_x)/((t1-t2)*1000)
	Vy = (y - previous_y)/((t1-t2)*1000)
	return (Vx, Vy)

def processImage(undistorted_frame):
	# define the lower and upper boundaries of the "green"
	# ball in the HSV color space, then initialize the list of tracked points
	ballLower0 = np.array([172, 87, 111], np.uint8)
	ballUpper0 = np.array([180, 255, 255], np.uint8)
	ballLower1 = np.array([0, 87, 111], np.uint8) # We use two masks because that's what works!
	ballUpper1 = np.array([5, 255, 255], np.uint8)

	# resize the frame, blur it, and convert it to the HSV
	# color space
	# frame = imutils.resize(frame, width=600)
	hsv = cv2.cvtColor(undistorted_frame, cv2.COLOR_BGR2HSV)
	
	# construct a mask for the color "green", then perform
	# a series of dilations and erosions to remove any small
	# blobs left in the mask
	mask0 = cv2.inRange(hsv, ballLower0, ballUpper0)
	mask1 = cv2.inRange(hsv, ballLower1, ballUpper1)
	mask = mask0 | mask1
	mask = cv2.erode(mask, None, iterations=2)
	mask = cv2.dilate(mask, None, iterations=2)
	return mask

def ballTracking():
	# calculate the cm to pixel ratio
	length_cm_per_pixel_ratio = np.divide(TABLE_LENGTH, DIM[0]) # x value ratio
	width_cm_per_pixel_ratio = np.divide(TABLE_WIDTH, DIM[1]) # y value ratio

	# mapping for fisheye calibration	
	map1, map2 = cv2.fisheye.initUndistortRectifyMap(K, D, np.eye(3), K, DIM, cv2.CV_16SC2)

	# if a video path was not supplied, grab the reference
	# to the webcam
	vs = VideoStream(0)
	vs.start()

	# otherwise, grab a reference to the video file
	# allow the camera or video file to "warm up"
	time.sleep(2.0)

	# begin processing the image. reset/define 
	# important variables for velocity calculations
	previous_x = 0
	previous_y = 0
	previous_time = 0
	frame_delay = 0

	# begin image detection
	while True:
		# performance testing
		startTime = cv2.getTickCount()

		# grab the current frame
		# frame = vs.read()
		
		# remove fisheye, new (undistored) frame is used for the remainder of the app
		undistorted_frame = cv2.remap(vs.read(), map1, map2, interpolation=cv2.INTER_LINEAR, borderMode=cv2.BORDER_CONSTANT)
		mask = processImage(undistorted_frame)
		
		# find contours in the mask and initialize the current
		# (x, y) center of the ball
		cnts = cv2.findContours(mask.copy(), cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
		cnts = imutils.grab_contours(cnts)
		center = None		

		try:
			# only proceed if at least one contour (i.e. ball) was found
			if len(cnts) > 0:
				# find the largest contour in the mask, then use
				# it to compute the minimum enclosing circle and
				# centroid
				contour = max(cnts, key=cv2.contourArea)
				((x, y), radius) = cv2.minEnclosingCircle(contour)
				current_time = cv2.getTickCount()/cv2.getTickFrequency()
				# M = cv2.moments(c)
				# center = (int(M["m10"] / M["m00"]), int(M["m01"] / M["m00"]))

				# calculate "real" position and velocity
				# x, y are pixel values, convert to cm values
				real_x = np.multiply(x, length_cm_per_pixel_ratio)
				real_y = np.multiply(y, width_cm_per_pixel_ratio) 
				if not previous_time == 0: # i.e. not on the first sighting of the bal
					if abs(real_x-previous_x) >= 3 or abs(real_y-previous_y) > 3 :
						(Vx, Vy) = calculateVelocity(real_x, real_y, previous_x, previous_y, current_time, previous_time)
					
					else:
						Vx = 0.0
						Vy = 0.0
					# sendDataToServer(real_x, real_y, Vx, Vy) # send the data to the server
	
				print(f"Ball Location (x, y): {real_x}mm, {real_y}mm") # TODO: print statement
				if not previous_time == 0:
					# print(f" Current_Time: {current_time} \n Previous Time: {previous_time}")
					print(f" Ball Speed (Vx, Vy): {Vx}mm/ms, {Vy}mm/ms")
				
				# update previous x, y, time variables
				previous_x = real_x
				previous_y = real_y
				previous_time = current_time	

				# draw a dot on the ball, only proceed if the radius meets a minimum size
				if radius > 1:
					# draw the circle and centroid on the frame,
					# cv2.circle(undistorted_frame, (int(x), int(x)), int(radius),(0, 255, 255), 3)
				 	cv2.circle(undistorted_frame, (int(x), int(y)), 3, (255, 0, 255), -1)
		except (ZeroDivisonError):
			# to avoid crashing on divideByZero error
			continue

		# performance check
		endTime = cv2.getTickCount()
		print(f" Processing Time (ms): {((endTime-startTime)/cv2.getTickFrequency())*1000}") 

		# TODO: Remove this for final product
		# show the frame to our screen
		cv2.imshow("Frame", undistorted_frame)
		key = cv2.waitKey(1) & 0xFF
		# if the 'q' key is pressed, stop the loop
		if key == ord("q"):
		 	break
		
		
	
	# stop camera
	vs.stop()

# SOCKET = connectToServer()
ballTracking()
cv2.destroyAllWindows()
