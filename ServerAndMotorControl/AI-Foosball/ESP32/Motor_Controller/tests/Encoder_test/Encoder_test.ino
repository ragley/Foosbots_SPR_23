
#define ENCODER_1 4
#define ENCODER_2 2

void IRAM_ATTR incrementMotor_1();
void IRAM_ATTR incrementMotor_2();

int motorPulses_1 = 0;
int motorPulses_2 = 0;

void setup() {
  Serial.begin(115200);
  while (!Serial);
  
  attachInterrupt(ENCODER_1, incrementMotor_1, RISING);
  attachInterrupt(ENCODER_2, incrementMotor_2, RISING);

}

void loop() {
  // put your main code here, to run repeatedly:

}

void IRAM_ATTR incrementMotor_1(){
  motorPulses_1 += 1;
  Serial.print("Motor 1: ");
  Serial.println(motorPulses_1);
}

void IRAM_ATTR incrementMotor_2(){
  motorPulses_2 += 1;
  Serial.print("Motor 2: ");
  Serial.println(motorPulses_2);
}
