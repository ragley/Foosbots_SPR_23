class PID {
  private:
    double KP;
    double KD;
    double KI;
    double period;
    double maximum;
    double minimum;
    double lastError;
    double continuousIntegral;
    
  public:
    PID(double proportionalCoefficient, double derivativeCoefficient, double integralCoefficient, double period, double maximum, double minimum) {
      this->KP = proportionalCoefficient;
      this->KD = derivativeCoefficient;
      this->KI = integralCoefficient;
      this->period = period;
      this->maximum = maximum;
      this->minimum = minimum;
      this->lastError = 0;
      this->continuousIntegral = 0;
    }
    double calculate(double error) {
      double proportionalTerm = KP * error;

      double derivative = (error - lastError) / period;
      double derivativeTerm = KD * derivative;
      
      continuousIntegral += error * period;
      double integralTerm = KI * continuousIntegral;

      double output = proportionalTerm + derivativeTerm + integralTerm;

      if(output > maximum) output = maximum;
      else if(output < minimum) output = minimum;

      lastError = error;

      return output;
    }
};

void setup() {
  // put your setup code here, to run once:

}

void loop() {
  // put your main code here, to run repeatedly:

}
