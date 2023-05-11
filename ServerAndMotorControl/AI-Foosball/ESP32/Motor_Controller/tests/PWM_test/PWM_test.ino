#include "driver/ledc.h"

#define PULSE_1 23
#define PULSE_2 22
#define ANALOG 4

#define CH_1 0
#define CH_2 1

int freq = 0;
const int res = 8;
const int large = 100;
const int dutyCycle = (2^res)/2;

const int maxFreq = 1000000;
const int maxAnalog = 1024;

const ledc_mode_t SPEED_MODE = LEDC_HIGH_SPEED_MODE;

const ledc_timer_t DISPLACEMENT_TIMER = LEDC_TIMER_2;

void setup() {
  Serial.begin(115200);
  ledcSetup(CH_1, freq, res);
  ledcSetup(CH_2, freq, res);
  ledcAttachPin(PULSE_1, CH_1);
  ledcAttachPin(PULSE_2, CH_2);
  ledc_bind_channel_timer(SPEED_MODE, CH_1, DISPLACEMENT_TIMER);
}

void loop() {
  freq = analogRead(ANALOG)*maxFreq/maxAnalog;
  ledc_set_freq(SPEED_MODE , DISPLACEMENT_TIMER, freq);
  Serial.println(ledc_get_freq(SPEED_MODE, DISPLACEMENT_TIMER));
}
