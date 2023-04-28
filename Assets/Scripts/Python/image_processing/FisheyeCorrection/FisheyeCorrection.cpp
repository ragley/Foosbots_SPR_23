#include <opencv2/opencv.hpp>
#include <opencv2/calib3d.hpp>
#include <opencv2/core/traits.hpp>
#include <opencv2/core/types.hpp>
#include <stdio.h>
#include <vector>
using namespace cv;
using namespace std;

/* 
Python Code ===================================================================================
DIM = (640, 360)
K = np.array([[529.0990431685142, 0.0, 308.01283344567565], [0.0, 527.5594266286947, 168.13007918363098], [0.0, 0.0, 1.0]])
D = np.array([[-0.11376404127315753], [-0.46295061178792457], [2.167920495942993], [-2.8734014030396717]])

def fisheye_correction(frame):
	map1, map2 = cv2.fisheye.initUndistortRectifyMap(K, D, np.eye(3), K, DIM, cv2.CV_32F)
	undistorted_frame = cv2.remap(gpu_frame, gpu_map1, gpu_map2, interpolation=cv2.INTER_LINEAR, borderMode=cv2.BORDER_CONSTANT)
	return undistorted_frame
==============================================================================================
*/



int main(int argc, char** argv){
	// Define calibration variables for fisheye undistortion
	Size DIM = {640, 360};

	Mat K(3, 3, DataType<float>::type);
	K.at<float>(0, 0) = 529.0990431685142;
	K.at<float>(0, 1) = 0.0;
	K.at<float>(0, 2) = 308.01283344567565;
	
	K.at<float>(1, 0) = 0.0;
	K.at<float>(1, 1) = 527.5594266286947;
	K.at<float>(1, 2) = 168.13007918363098;
	
	K.at<float>(2, 0) = 0.0;
	K.at<float>(2, 1) = 0.0;
	K.at<float>(2, 2) = 1.0;
	
	Mat D(4, 1, DataType<float>::type);
	D.at<float>(0, 0) = -0.11376404127315753;
	D.at<float>(1, 0) = -0.46295061178792457;
	D.at<float>(2, 0) = 2.167920495942993;
	D.at<float>(3, 0) = -2.8734014030396717;
	Mat E = Mat::eye(3, 3, DataType<double>::type);	

	// frame is type Mat
	Mat frame;
	Mat undistorted_frame;
	Mat map1;
	Mat map2;
	namedWindow("Display");

	VideoCapture cap(0);
	if(!cap.isOpened()) {
		cout << "Cannot open camera..." << endl;
		return -1;
	}
	else{
		TickMeter timer;
		double startTime;
		double endTime;
		double processingTime;
		fisheye::initUndistortRectifyMap(K, D, E, K, DIM, CV_16SC2, map1, map2);
		while(true) {
			cap >> frame;
			timer.start(); // TODO: timer
			
			remap(frame, undistorted_frame, map1, map2, INTER_LINEAR, BORDER_CONSTANT, Scalar(0.0,0.0,0.0));
			timer.stop(); // TODO: timer
			processingTime = timer.getTimeSec();
			timer.reset();	
			cout << "Processing Time: " << processingTime << endl; // =======			
			imshow("Display", undistorted_frame);
			waitKey(25);
		}
	}
	return 0;
}
