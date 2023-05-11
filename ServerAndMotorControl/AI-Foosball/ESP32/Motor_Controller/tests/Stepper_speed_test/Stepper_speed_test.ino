#include <ESP_FlexyStepper.h>
#include "Controller_Constants.h"

const int GOAL = 10000;

ESP_FlexyStepper stepper_1;

void setup() {
  Serial.begin(115200);
  stepper_1.connectToPins(TRANSLATION_DRIVER_PULSE, TRANSLATION_DRIVER_DIR);
  stepper_1.setStepsPerRevolution(STEP_PULSE_TRANSLATION_CONVERSION);
  stepper_1.setAccelerationInMillimetersPerSecondPerSecond(MAX_ACCELERATION_TRANSLATION);
  stepper_1.setDecelerationInMillimetersPerSecondPerSecond(MAX_ACCELERATION_TRANSLATION);
  stepper_1.setSpeedInMillimetersPerSecond(MAX_SPEED_TRANSLATION);

  stepper_1.startAsService(STEPPER_CORE);
}

void loop() {
    stepper_1.setTargetPositionRelativeInMillimeters(GOAL);
}
