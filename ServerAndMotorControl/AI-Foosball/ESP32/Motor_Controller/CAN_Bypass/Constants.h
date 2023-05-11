// Pins
const int constant_id1Pin = 23; // ID 1
const int constant_id2Pin = 22; // ID 2
const int constant_eStopPin = 34; // Emergency Stop
const int constant_traDirPin = 16; // Translational Direction
const int constant_traPulsePin = 17; // Translational Pulse
const int constant_traZeroPin = 4; // Translational Zero
const int constant_rotDirPin = 5; // Rotational Direction
const int constant_rotPulsePin = 18; // Rotational Pulse
const int constant_rotZeroPin = 19; // Rotational Zero
const int constant_canTXPin = 15; // CAN TX
const int constant_canRXPin = 13; // CAN RX
const int constant_blueLEDPin = 2;

// ESP32
const int constant_stepperCore = 0;
const int constant_mainCore = 1;
const int constant_baudRate = 115200;

// Directions
// Clockwise is 1; Counterclockwise is -1; When motor is pointing away from you.
// Order from top down: 3Rod, 5Rod, 2Rod, Goal Rod;
const int constant_traDirs[4] = {
  1,
  1,
  -1,
  -1,
};
const int constant_rotDirs[4] = {
  1,
  1,
  1,
  1,
};

// Enabled = false; Disabled = true;
const bool constant_disableRod[4] = {
  false,
  false,
  false,
  false
};

// Hardocded Values
// Translation
// Order from top down: 3Rod, 5Rod, 2Rod, Goal Rod;
const double constant_traMidpoints[4] = {
  120.9,
  70.4,
  175.0,
  120.9
};
const double constant_traRanges[4] = {
  80.0,
  25.0,
  120.0,
  80.0
};
const int constant_traPulse = 41.27;
const double constant_traMaxAccel = 10000; // mm per second per second
const double constant_traDefaultSpeed = 250;
const double constant_traZeroingSpeed = 100;
const double constant_traMidpoint = 120.90; // m

// Rotation
const double constant_rotPulse = 3210; //pulse per rotation
const double constant_rotMaxAccel = 500; // rotations per second per second
const double constant_rotSpeed = 200;
