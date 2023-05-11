#include <PID_v1.h>

double Kp1 = 2;
double Ki1 = 5;
double Kd1 = 1;

double Kp2 = 2;
double Ki2 = 5;
double Kd2 = 1;

double feedback1;
double desired1;
double dutyCycle1 = 0;

double feedback2;
double desired2;
double dutyCycle2 = 0;

double interval = 10; //in milliseconds

PID motor1PID(&feedback1, &dutyCycle1, &desired1, Kp1, Ki1, Kd1, DIRECT);
PID motor2PID(&feedback2, &dutyCycle2, &desired2, Kp2, Ki2, Kd2, DIRECT);

void setup() {
  Serial.begin(115200);
  while (!Serial);
  
  desired1 = 100;
  desired2 = 50;
  feedback1 = 75;
  feedback2 = 75;

  //Set PID specifications
  motor1PID.SetOutputLimits(-255,255); //using -255 so that the PID can specify reverse
  motor2PID.SetOutputLimits(-255,255);

  motor1PID.SetSampleTime(interval); 
  motor2PID.SetSampleTime(interval);
  
  //Start PIDs
  motor1PID.SetMode(AUTOMATIC);
  motor2PID.SetMode(AUTOMATIC);
}

String direction1;
String direction2;
void loop() {
  motor1PID.Compute();
  motor2PID.Compute();
  direction1 = (dutyCycle1 > 0) ? "Forward" : "Reverse";
  direction2 = (dutyCycle2 > 0) ? "Forward" : "Reverse";
  
  Serial.print("PID 1: (");
  Serial.print(direction1);
  Serial.print(") ");
  Serial.println(abs(dutyCycle1));
  Serial.print("PID 2: (");
  Serial.print(direction2);
  Serial.print(") ");
  Serial.println(abs(dutyCycle2));
  Serial.println("");
}
