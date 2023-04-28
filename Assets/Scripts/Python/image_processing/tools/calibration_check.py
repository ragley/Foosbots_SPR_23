# This program lets you check an image to see if the calibration works as intended.
# https://medium.com/@kennethjiang/calibrate-fisheye-lens-using-opencv-333b05afa0b0

import cv2
import numpy as np
import os
import json

# read data from calibration.json
#with open('calibration.json') as json_file:
#    data = json.load(json_file)
#    dim_width = data['dim_width']
#    dim_height = data['dim_height']
#    DIM = (int(dim_width), int(dim_height))
#    K=np.array(data['k'])
#    D=np.array(data['d'])
DIM=(640, 360)
K=np.array([[529.7982121993252, 0.0, 312.7158779979683], [0.0, 527.7751535552674, 175.59551889975003], [0.0, 0.0, 1.0]])
D=np.array([[-0.10259242250486603], [-0.4053339824788657], [2.145677156701134], [-3.3630763147728104]])

def undistort(img_path):
    img = cv2.imread(img_path)
    h,w = img.shape[:2]
    map1, map2 = cv2.fisheye.initUndistortRectifyMap(K, D, np.eye(3), K, DIM, cv2.CV_16SC2)
    undistorted_img = cv2.remap(img, map1, map2, interpolation=cv2.INTER_LINEAR, borderMode=cv2.BORDER_CONSTANT)
    cv2.imshow("undistorted", undistorted_img)
    cv2.waitKey(0)
    cv2.destroyAllWindows()
if __name__ == '__main__':
    # for p in sys.argv[1:]:
    # Type name of image to check!
    undistort("./imgs/opencv_frame_7.jpg")
