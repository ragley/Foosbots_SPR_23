#include <ESP_FlexyStepper.h>
#include "Controller_Constants.h"

#define CORE 0

const float stepPerRevolution = 1600; //according to the 1.80 degree step angle from data sheet and 400 on the driver
const int maxRPS = 89;
const int maxACC = 3000;
const int REVOLUTIONS = 10;
const int ACC = 200;

int RPS = maxRPS;
int target_ROT = REVOLUTIONS;
int target_TRANS = REVOLUTIONS;
int direction_ROT = 1;
int direction_TRANS = 1;
double timer_ROT = millis();
double timer_TRANS = millis();



ESP_FlexyStepper Rotational_Driver;
ESP_FlexyStepper Translation_Driver;

void setup() {
  Serial.begin(115200);

  Translation_Driver.connectToPins(TRANSLATION_DRIVER_PULSE, TRANSLATION_DRIVER_DIR);
  Translation_Driver.setStepsPerRevolution(STEP_PULSE_ROTATION_CONVERSION);
  Translation_Driver.setSpeedInRevolutionsPerSecond(maxSpeedRotation);
  Translation_Driver.setAccelerationInRevolutionsPerSecondPerSecond(maxAccelerationRotation*100);
  Translation_Driver.setDecelerationInRevolutionsPerSecondPerSecond(maxAccelerationRotation*100);
  Translation_Driver.setTargetPositionToStop();
  Translation_Driver.startAsService(CORE);

  Translation_Driver.moveToHomeInRevolutions(1,maxSpeedRotation, 100,TRANSLATION_DRIVER_ZERO);
}

void loop() {
}
