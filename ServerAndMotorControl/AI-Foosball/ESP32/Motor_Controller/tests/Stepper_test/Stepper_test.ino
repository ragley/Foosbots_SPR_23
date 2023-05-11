#include <ESP_FlexyStepper.h>
#include "Controller_Constants.h"

#define CORE 0

const int REVOLUTIONS = 1;

int target_ROT = REVOLUTIONS;
int direction_ROT = 1;
double timer_ROT = millis();

ESP_FlexyStepper Rotational_Driver;
ESP_FlexyStepper Translation_Driver;

void setup() {
  Serial.begin(115200);
  Rotational_Driver.connectToPins(ROTATION_DRIVER_PULSE, ROTATION_DRIVER_DIR);
  Rotational_Driver.setStepsPerRevolution(STEP_PULSE_ROTATION_CONVERSION);
  Rotational_Driver.setSpeedInRevolutionsPerSecond(MAX_SPEED_ROTATION);
  Rotational_Driver.setAccelerationInRevolutionsPerSecondPerSecond(MAX_ACCELERATION_ROTATION);
  Rotational_Driver.setDecelerationInRevolutionsPerSecondPerSecond(MAX_ACCELERATION_ROTATION);
  Rotational_Driver.setTargetPositionToStop();
  Rotational_Driver.startAsService(CORE);
}

void loop() {
  Serial.print("ROT __ ACC: ");
  Serial.print(MAX_ACCELERATION_ROTATION);
  Serial.print(" SPEED: ");
  Serial.print(Rotational_Driver.getCurrentVelocityInRevolutionsPerSecond());
  Serial.print(" REV: ");
  Serial.print(Rotational_Driver.getCurrentPositionInRevolutions());
  Serial.print(" TARGET: ");
  Serial.println(Rotational_Driver.getTargetPositionInRevolutions());

  if (Rotational_Driver.getDistanceToTargetSigned() == 0 && direction_ROT*target_ROT > 0) {
    direction_ROT *= -1;
    timer_ROT = millis() + 1000;
  }
  if (millis() > timer_ROT && direction_ROT*target_ROT < 0) {
    target_ROT =  direction_ROT*REVOLUTIONS;
    Rotational_Driver.setTargetPositionInRevolutions(target_ROT);
    timer_ROT = millis() + 1000;
  }
}
