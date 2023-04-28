import cv2
print(cv2.__version__)
import numpy as np

def nothing(x):
    pass

cv2.namedWindow('Trackbars')
cv2.moveWindow('Trackbars',1320,0)

cv2.createTrackbar('hueLower', 'Trackbars',62,179,nothing)
cv2.createTrackbar('hueUpper', 'Trackbars',98,179,nothing)

cv2.createTrackbar('hue2Lower', 'Trackbars',0,179,nothing)
cv2.createTrackbar('hue2Upper', 'Trackbars',0,179,nothing)

cv2.createTrackbar('satLow', 'Trackbars',36,255,nothing)
cv2.createTrackbar('satHigh', 'Trackbars',113,255,nothing)
cv2.createTrackbar('valLow','Trackbars',99,255,nothing)
cv2.createTrackbar('valHigh','Trackbars',199,255,nothing)


dispW=640
dispH=480
flip=2
#Uncomment These next Two Line for Pi Camera
#camSet='nvarguscamerasrc !  video/x-raw(memory:NVMM), width=3264, height=2464, format=NV12, framerate=21/1 ! nvvidconv flip-method='+str(flip)+' ! video/x-raw, width='+str(dispW)+', height='+str(dispH)+', format=BGRx ! videoconvert ! video/x-raw, format=BGR ! appsink'
#cam= cv2.VideoCapture(camSet)

#Or, if you have a WEB cam, uncomment the next line
#(If it does not work, try setting to '1' instead of '0')
cam=cv2.VideoCapture(0)
while True:
    ret, frame = cam.read()
    #frame=cv2.imread('smarties.png')
    cv2.imshow('nanoCam',frame)
    cv2.moveWindow('nanoCam',0,0)

    hsv=cv2.cvtColor(frame,cv2.COLOR_BGR2HSV)

    hueLow=cv2.getTrackbarPos('hueLower', 'Trackbars')
    hueUp=cv2.getTrackbarPos('hueUpper', 'Trackbars')

    hue2Low=cv2.getTrackbarPos('hue2Lower', 'Trackbars')
    hue2Up=cv2.getTrackbarPos('hue2Upper', 'Trackbars')

    Ls=cv2.getTrackbarPos('satLow', 'Trackbars')
    Us=cv2.getTrackbarPos('satHigh', 'Trackbars')

    Lv=cv2.getTrackbarPos('valLow', 'Trackbars')
    Uv=cv2.getTrackbarPos('valHigh', 'Trackbars')

    l_b=np.array([hueLow,Ls,Lv])
    u_b=np.array([hueUp,Us,Uv])

    l_b2=np.array([hue2Low,Ls,Lv])
    u_b2=np.array([hue2Up,Us,Uv])

    FGmask=cv2.inRange(hsv,l_b,u_b)
    FGmask=cv2.erode(FGmask, None, iterations=2)
    FGmask2=cv2.inRange(hsv,l_b2,u_b2)
    FGmask2=cv2.erode(FGmask2, None, iterations=2)
    FGmaskComp=cv2.add(FGmask,FGmask2)
    cv2.imshow('FGmaskComp',FGmaskComp)
    cv2.moveWindow('FGmaskComp',0,530)

    FG=cv2.bitwise_and(frame, frame, mask=FGmaskComp)
    cv2.imshow('FG',FG)
    cv2.moveWindow('FG',700,0)

    bgMask=cv2.bitwise_not(FGmaskComp)
    bgMask=cv2.erode(bgMask, None, iterations=2)
    cv2.imshow('bgMask',bgMask)
    cv2.moveWindow('bgMask',700,530)

    BG=cv2.cvtColor(bgMask,cv2.COLOR_GRAY2BGR)
    final=cv2.add(FG,BG)
    cv2.imshow('final',final)
    cv2.moveWindow('final',1400,0)

    if cv2.waitKey(1)==ord('q'):
        break
cam.release()
cv2.destroyAllWindows()
