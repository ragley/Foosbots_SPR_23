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

// Spring 2022
/*
const int    DIRECTIONS[4][2] = {{ -1,  1}, //3 rod
                              { -1,  1}, //5 rod
                              { 1, -1}, //2 rod
                              { -1,  1}};//Goal rod

const double MAX_TRANSLATIONS[4] = {181.23, //3 rod
                                    115.265, //5 rod
                                    356,    //2 rod
                                    228.77}; //Goal rod
*/

// When motor is pointing away from you, positive 1 is clockwise
//Fall 2022
const int    DIRECTIONS[4][2] = {{ -1,  1}, //3 rod
                              { -1,  1}, //5 rod
                              { 1, 1}, //2 rod
                              { 1,  1}};//Goal rod

const double MAX_TRANSLATIONS[4] = {181.23, //3 rod
                                    115.265, //5 rod
                                    356,    //2 rod
                                    181.23}; //Goal rod
//
                                   
const double DEGREES_PER_REVOLUTION = 360;

const double STEP_PULSE_ROTATION_CONVERSION = 3200; //pulse per rotation

const double STEP_PULSE_TRANSLATION_CONVERSION[4] = {41.27, //3 rod
                                                    41.27,  //5 rod
                                                    41.27,  //2 rod
                                                    41.27}; //Goal rod pulse per mm

const double MAX_SPEED_ROTATION = 20; //rotations per second
const double MAX_SPEED_TRANSLATION = 2670; //mm per second
const double MAX_ACCELERATION_ROTATION = 125; // rotations per second per second
const double MAX_ACCELERATION_TRANSLATION = 10000; // mm per second per second
const double HOME_SPEED_TRANSLATION = 100;
const double HOME_SPEED_ROTATION = 1;

const int    COM_DELAY = 10; //in ms
const int    MAX_COM_DELAY = COM_DELAY * 6;

const int    SENSOR_DEBOUNCE = 10;

const int    BAUD_RATE = 1000E3;
