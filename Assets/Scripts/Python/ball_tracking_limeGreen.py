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
from converters import *
from FSMConstants import *

def nothing(x):
    pass

# cv2.namedWindow('Trackbars')
# cv2.moveWindow('Trackbars',1320,530)

# cv2.createTrackbar('hueLower', 'Trackbars', 47,  179, nothing)
# cv2.createTrackbar('hueUpper', 'Trackbars', 76, 179, nothing)

# cv2.createTrackbar('hue2Lower', 'Trackbars', 0, 179, nothing)
# cv2.createTrackbar('hue2Upper', 'Trackbars', 0, 179, nothing)

# cv2.createTrackbar('satLow', 'Trackbars', 0, 255, nothing)
# cv2.createTrackbar('satHigh', 'Trackbars', 255, 255, nothing)
# cv2.createTrackbar('valLow','Trackbars', 165, 255, nothing)
# cv2.createTrackbar('valHigh','Trackbars', 250, 255, nothing)


dispW = 640
dispH = 480
flip = 2

cam = cv2.VideoCapture(0)

# Dimensions of the Table (these are VERY important)
TABLE_LENGTH = 1170.0
TABLE_WIDTH = 680.0

# camera fisheye calibration arrays
# These are camara specific! Use fisheye_calibration.py to find these numbers!
DIM = (640, 360) # dimensions of the camera (in pixels)
K = np.array([[528.8185377264571, 0.0, 308.06768106487243], [0.0, 527.2517977278035, 168.1135640889442], [0.0, 0.0, 1.0]])
D = np.array([[-0.11166910657188828], [-0.48457757077294716], [2.2362014171402826], [-2.942429600807647]])

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
	print(json_data) # TODO: Print Statement
	json_bytes = bytes(json_data, encoding="utf-8")
	global SOCKET 
	# try:
		# Send the data to the Server
	SOCKET.sendall(len(json_bytes).to_bytes(4, byteorder="big") + json_bytes)
	# except (ConnectionResetError, BrokenPipeError): 
		# If the server shuts down mid game, we don't want to crash
	# 	SOCKET = None
	# 	SOCKET = connectToServer()

def calculateVelocity(x_array, y_array, time_delta):
	x_diff = np.diff(x_array)
	x_mean_diff = np.mean(x_diff)

	y_diff = np.diff(y_array)
	y_mean_diff = np.mean(y_diff)

	Vx = x_mean_diff / (time_delta)
	Vy = y_mean_diff / (time_delta)
	
	if (Vx > 140 or Vx < -140):
		Vx = (Vx / np.abs(Vx)) * 140

	if (Vy > 6 or Vy < -6):
		Vy = (Vy / np.abs(Vx)) * 6
	
	return (Vx, Vy)


def push(array, value):
    array = np.append(array,value)
    array = np.roll(array,-1)
    array = array[:-1]
    return array


def ballTracking():
	length_cm_per_pixel_ratio = np.divide(TABLE_LENGTH, DIM[0]) # x value ratio
	width_cm_per_pixel_ratio = np.divide(TABLE_WIDTH, DIM[1]) # y value ratio
		# important variables for velocity calculations
	previous_x = 0
	previous_y = 0
	previous_Vx = 0
	previous_Vy = 0
	previous_time = 0
	frame_delay = 0
	vel_calc = (0,0)
	# approximately
	time_delta = 0.001


	num_prev_pos_vals = 50
	position_vals_x = np.zeros(num_prev_pos_vals)
	position_vals_y = np.zeros(num_prev_pos_vals)

	Vx = 0
	Vy = 0

	timessinceslow = 0
	centerX = int(DIM[0]) / 2
	centerY = int(DIM[1]) / 2

	# begin image detection
	while True:
		ret, frame = cam.read()
		# performance testing
		startTime = cv2.getTickCount()

		# cv2.imshow('nanoCam',frame)
		# cv2.moveWindow('nanoCam',0,0)

		hsv=cv2.cvtColor(frame,cv2.COLOR_BGR2HSV)

		# hueLow = cv2.getTrackbarPos('hueLower', 'Trackbars')
		# hueUp = cv2.getTrackbarPos('hueUpper', 'Trackbars')

		# hue2Low = cv2.getTrackbarPos('hue2Lower', 'Trackbars')
		# hue2Up = cv2.getTrackbarPos('hue2Upper', 'Trackbars')

		# Ls = cv2.getTrackbarPos('satLow', 'Trackbars')
		# Us = cv2.getTrackbarPos('satHigh', 'Trackbars')

		# Lv = cv2.getTrackbarPos('valLow', 'Trackbars')
		# Uv = cv2.getTrackbarPos('valHigh', 'Trackbars')
		
		hueLow = 47
		hueUp = 76

		hue2Low = 0
		hue2Up = 0

		Ls = 75
		Us = 255

		Lv = 126
		Uv = 250

		l_b = np.array([hueLow,Ls,Lv])
		u_b = np.array([hueUp,Us,Uv])

		l_b2 = np.array([hue2Low,Ls,Lv])
		u_b2 = np.array([hue2Up,Us,Uv])

		FGmask = cv2.inRange(hsv,l_b,u_b)
		FGmask = cv2.erode(FGmask, None, iterations=2)
		FGmask = cv2.dilate(FGmask, None, iterations=2)
		FGmask2 = cv2.inRange(hsv,l_b2,u_b2)
		FGmask2 = cv2.erode(FGmask2, None, iterations=2)
		FGmask2 = cv2.dilate(FGmask2, None, iterations=2)
		FGmaskComp = cv2.add(FGmask,FGmask2)
		# cv2.imshow('FGmaskComp',FGmaskComp)
		# cv2.moveWindow('FGmaskComp',0,530)

		FG = cv2.bitwise_and(frame, frame, mask=FGmaskComp)
		# cv2.imshow('FG',FG)
		# cv2.moveWindow('FG',700,0)

		bgMask = cv2.bitwise_not(FGmaskComp)
		bgMask = cv2.erode(bgMask, None, iterations=2)
		bgMask = cv2.dilate(bgMask, None, iterations=2)
		# cv2.imshow('bgMask',bgMask)
		# cv2.moveWindow('bgMask',700,530)

		BG = cv2.cvtColor(bgMask,cv2.COLOR_GRAY2BGR)
		final = cv2.add(FG,BG)
		# cv2.imshow('final',final)
		# cv2.moveWindow('final',1320,0)
		mask = FGmaskComp
		
		# find contours in the mask and initialize the current
		# (x, y) center of the ball
		cnts = cv2.findContours(mask, cv2.RETR_CCOMP, cv2.CHAIN_APPROX_SIMPLE)
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


				# TODO: Make switch case for NN and FSM because it changes positional values a lot to get what the NN needs
				# Calculate x and y with respect to the goal for neural network

				real_x = IRL_2_U_X(real_x)
				real_y = IRL_2_U_Z(real_y)
				
				position_vals_x = push(position_vals_x, real_x)
				position_vals_y = push(position_vals_y, real_y)

				if not previous_time == 0: # i.e. not on the first sighting of the ball
					vel_calc = calculateVelocity(position_vals_x, position_vals_y, time_delta)
					
					(Vx, Vy) = vel_calc
					# if radius > 5:
					sendDataToServer(real_x, real_y, Vx, Vy) # send the data to the server
					# 	print(real_x, real_y, Vx, Vy) # send the data to the server		
				# print(f"Ball Location (x, y): {real_x}mm, {real_y}mm") # TODO: print statement
				# if not previous_time == 0:
				# 	print(f" Ball Speed (Vx, Vy): {Vx}mm/s, {Vy}mm/s")
				
				# update previous x, y, time variables
				previous_x = real_x
				previous_y = real_y
				previous_time = current_time
				frame_delay = frame_delay + 1

				# draw a dot on the ball, only proceed if the radius meets a minimum size
				if radius > 5:
				# draw the circle and centroid on the frame,
					#print(radius)
					cv2.circle(frame, (int(x), int(y)), int(radius), (0, 255, 255), 3)
					cv2.circle(frame, (int(x), int(y)), 3, (255, 0, 255), -1)
					cv2.circle(frame, (int(x), int(y)), int(radius), (0, 255, 255), 3)
					cv2.circle(frame, (int(centerX), int(centerY)), 3, (255, 0, 255), -1)
			else:
				# Send previous values if ball cannot be found.
				sendDataToServer(previous_x, previous_y, previous_Vx, previous_Vy)
				
		except (ZeroDivisonError):
			# to avoid crashing on divideByZero error
			continue

		# performance check
		endTime = cv2.getTickCount()
		if ((((endTime-startTime)/cv2.getTickFrequency())*1000) >= 10):
			print(f" Processing Time (ms): {((endTime-startTime)/cv2.getTickFrequency())*1000} and {timessinceslow} times since slow")
			timessinceslow = 0
		else:
			timessinceslow+=1
		# TODO: Remove this for final product
		# show the frame to our screen
		cv2.imshow("Circle", frame)
		cv2.moveWindow('Circle',1200,530)
		key = cv2.waitKey(1) & 0xFF
		#if the 'q' key is pressed, stop the loop
		if key == ord("q"):
			break

SOCKET = connectToServer()
ballTracking()
cam.release()
cv2.destroyAllWindows()
