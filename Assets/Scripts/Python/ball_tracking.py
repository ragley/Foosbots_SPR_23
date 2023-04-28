
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

cv2.namedWindow('Trackbars')
cv2.moveWindow('Trackbars',1320,530)

cv2.createTrackbar('hueLower', 'Trackbars', 47,  179, nothing)
cv2.createTrackbar('hueUpper', 'Trackbars', 76, 179, nothing)

cv2.createTrackbar('hue2Lower', 'Trackbars', 0, 179, nothing)
cv2.createTrackbar('hue2Upper', 'Trackbars', 0, 179, nothing)

cv2.createTrackbar('satLow', 'Trackbars', 0, 255, nothing)
cv2.createTrackbar('satHigh', 'Trackbars', 255, 255, nothing)
cv2.createTrackbar('valLow','Trackbars', 165, 255, nothing)
cv2.createTrackbar('valHigh','Trackbars', 250, 255, nothing)


dispW = 640
dispH = 480
flip = 2

cam = cv2.VideoCapture(0)
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


# def calculateVelocity(x, y, previous_x, previous_y, previous_Vx, previous_Vy, t1, t2): # new
# 	Vx = ((x - previous_x)/(t1-t2))#/10
# 	Vy = ((y - previous_y)/(t1-t2))#/10

# 	pd_Vx = (np.fabs(previous_Vx - Vx)) / ((previous_Vx + Vx)/2)
# 	pd_Vy = (np.fabs(previous_Vy - Vy)) / ((previous_Vy + Vy)/2)
	
# 	if ((pd_Vx < 0.25) and (pd_Vy < 0.25)):
# 		Vx = previous_Vx
# 		Vy = previous_Vy

# 	return (Vx, Vy)	



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

# adds new elements to end
# ex:
# >>> push(x, [5])
# array([ 0.,  0.,  0.,  0.,  5.])
# >>> push(x, [1,2,3])
# array([ 0.,  5.,  1.,  2.,  3.])

def push(array, value):
    array = np.append(array,value)
    array = np.roll(array,-1)
    array = array[:-1]
    return array

def ballTracking():
	# calculate the mm to pixel ratio
	length_mm_per_pixel_ratio = np.divide(TABLE_LENGTH, DIM[0]) # x value ratio
	width_mm_per_pixel_ratio = np.divide(TABLE_WIDTH, DIM[1]) # y value ratio

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

	# begin image detection
	while True:
		ret, frame = cam.read()
		# performance testing
		startTime = cv2.getTickCount()

		cv2.imshow('nanoCam',frame)
		cv2.moveWindow('nanoCam',0,0)

		hsv=cv2.cvtColor(frame,cv2.COLOR_BGR2HSV)

		hueLow = cv2.getTrackbarPos('hueLower', 'Trackbars')
		hueUp = cv2.getTrackbarPos('hueUpper', 'Trackbars')

		hue2Low = cv2.getTrackbarPos('hue2Lower', 'Trackbars')
		hue2Up = cv2.getTrackbarPos('hue2Upper', 'Trackbars')

		Ls = cv2.getTrackbarPos('satLow', 'Trackbars')
		Us = cv2.getTrackbarPos('satHigh', 'Trackbars')

		Lv = cv2.getTrackbarPos('valLow', 'Trackbars')
		Uv = cv2.getTrackbarPos('valHigh', 'Trackbars')

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
		cv2.imshow('FGmaskComp',FGmaskComp)
		cv2.moveWindow('FGmaskComp',0,530)

		FG = cv2.bitwise_and(frame, frame, mask=FGmaskComp)
		cv2.imshow('FG',FG)
		cv2.moveWindow('FG',700,0)

		bgMask = cv2.bitwise_not(FGmaskComp)
		bgMask = cv2.erode(bgMask, None, iterations=2)
		bgMask = cv2.dilate(bgMask, None, iterations=2)
		cv2.imshow('bgMask',bgMask)
		cv2.moveWindow('bgMask',700,530)

		BG = cv2.cvtColor(bgMask,cv2.COLOR_GRAY2BGR)
		final = cv2.add(FG,BG)
		cv2.imshow('final',final)
		cv2.moveWindow('final',1320,0)
		mask = FGmaskComp
		
		# find contours in the mask and initialize the current
		# (x, y) center of the ball
		cnts = cv2.findContours(mask, cv2.RETR_CCOMP, cv2.CHAIN_APPROX_SIMPLE)
		cnts = imutils.grab_contours(cnts)
		center = None		

		# performance testing
		# startTime = cv2.getTickCount()

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
				# x, y are pixel values, convert to mm values
				real_x = np.multiply(x, length_mm_per_pixel_ratio)
				real_y = np.multiply(y, width_mm_per_pixel_ratio) 

				# TODO: Make switch case for NN and FSM because it changes positional values a lot to get what the NN needs
				# Calculate x and y with respect to the goal for neural network

				real_x = IRL_2_U_X(real_x)
				real_y = IRL_2_U_Z(real_y)
				
				position_vals_x = push(position_vals_x, real_x)
				position_vals_y = push(position_vals_y, real_y)

				#print(position_vals_x)
				if not previous_time == 0: # i.e. not on the first sighting of the ball
					# vel_calc = calculateVelocity(real_x, real_y, previous_x, previous_y, current_time, previous_time)

					
					#vel_calc = calculateVelocity(real_x, real_y, previous_x, previous_y, current_time, previous_time)
					#vel_calc = calculateVelocity(real_x, real_y, previous_x, previous_y, previous_Vx, previous_Vy, current_time, previous_time) #new
					vel_calc = calculateVelocity(position_vals_x, position_vals_y, time_delta) 
					# if (vel_calc == (0, 0)):
					# 	Vx = previous_NZ_Vx
					# 	Vy = previous_NZ_Vy

					# else:
					(Vx, Vy) = vel_calc
					# print(vel_calc)

					# Vx = IRL_2_U_X(Vx)
					# Vy = IRL_2_U_Z(Vy) 	 					
					sendDataToServer(real_x, real_y, Vx, Vy) # send the data to the server			
	
				# print(f"Ball Location (x, y): {real_x}mm, {real_y}mm") # TODO: print statement
				# if not previous_time == 0:
				# 	print(f" Ball Speed (Vx, Vy): {Vx}mm/s, {Vy}mm/s")
				
				#print(current_time - previous_time)

				# update previous x, y, time variables
				previous_x = real_x
				previous_y = real_y
				previous_Vx = Vx
				previous_Vy = Vy
				previous_NZ_Vx = Vx
				previous_NZ_Vy = Vy
				previous_time = current_time
				frame_delay = frame_delay + 1

				# draw a dot on the ball, only proceed if the radius meets a minimum size
				# if radius > 1:
					# draw the circle and centroid on the frame,
					# cv2.circle(undistorted_frame, (int(x), int(x)), int(radius),(0, 255, 255), 3)
				 	# cv2.circle(undistorted_frame, (int(x), int(y)), 3, (255, 0, 255), -1)
			else:
				# Send previous values if ball cannot be found.
				sendDataToServer(previous_x, previous_y, previous_Vx, previous_Vy)
		except (ZeroDivisonError):
			# to avoid crashing on divideByZero error
			continue

		# performance check
		# endTime = cv2.getTickCount()
		# print(f" Processing Time (ms): {((endTime-startTime)/cv2.getTickFrequency())*1000}") 

		# TODO: Remove this for final product
		# show the frame to our screen
		cv2.imshow("Frame", undistorted_frame)
		key = cv2.waitKey(1) & 0xFF
		# if the 'q' key is pressed, stop the loop
		if key == ord("q"):
			break
		
		
	
	# stop camera
	vs.stop()

#SOCKET = connectToServer()
ballTracking()
cam.release()
cv2.destroyAllWindows()
