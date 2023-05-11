#define ID_1 23
#define ID_2 22
#define ROTATION_DRIVER_PULSE 18
#define ROTATION_DRIVER_DIR 5
#define ROTATION_DRIVER_ZERO 19
#define TRANSLATION_DRIVER_PULSE 17
#define TRANSLATION_DRIVER_DIR 16
#define TRANSLATION_DRIVER_ZERO 4
#define ROTATION_SENSOR  21
#define TRANSLATION_SENSOR  2
#define TXD_CAN 15
#define RXR_CAN 13
#define ENABLE 34

#define ALL_GOOD_LED 2

#define STEPPER_CORE 0
#define MAIN_CORE 1

#define ROTATION 1
#define TRANSLATION 0

//Clockwise motor movement is positive

const int DIRECTIONS[4][2] = {{-1,  1}, //3 rod
                              {-1,  1}, //5 rod
                              { 1, -1}, //2 rod
                              {-1,  1}};//Goal rod

const double MAX_TRANSLATIONS[4] = {181.23, //3 rod
                                   115.265, //5 rod
                                   354, //2 rod
                                   226.77};//Goal rod

const double MM_PER_REVOLUTION = 65.7;
const double DEGREES_PER_REVOLUTION = 360;

const double STEP_PULSE_ROTATION_CONVERSION = 1600; //pulse per rotation (200 pulses on the motor with the driver set at 3200) 
const double STEP_PULSE_TRANSLATION_CONVERSION = STEP_PULSE_ROTATION_CONVERSION/MM_PER_REVOLUTION; //pulse per mm

//const double MAX_SPEED_ROTATION = 89; //rotations per second
const double MAX_SPEED_ROTATION = 20; //rotations per second
const double MAX_SPEED_TRANSLATION = MAX_SPEED_ROTATION*MM_PER_REVOLUTION; //mm per second
const double MAX_ACCELERATION_ROTATION = 500; // rotations per second per second
const double MAX_ACCELERATION_TRANSLATION = MAX_ACCELERATION_ROTATION*MM_PER_REVOLUTION; // mm per second per second
const double HOME_SPEED_TRANSLATION = MAX_SPEED_TRANSLATION / 10;
const double HOME_SPEED_ROTATION = MAX_SPEED_ROTATION / 10;

const int COM_DELAY = 10; //in ms
const int MAX_COM_DELAY = COM_DELAY * 5;

const double SENSOR_PULSE_TRANSLATION_CONVERSION = 0;
const double SENSOR_PULSE_ROTATION_CONVERSION = 0;

const int MINIMUM_SENSOR_PULSES = 10;
