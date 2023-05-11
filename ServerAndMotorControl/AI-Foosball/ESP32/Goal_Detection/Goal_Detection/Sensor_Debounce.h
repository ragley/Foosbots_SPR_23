class Sensor_Debounce {
    public:
        Sensor_Debounce(int pin, int count, int pin_mode, int pressedValue){
            READ = false;
            Pressed = false;
            PIN = pin;
            COUNT = count;
            activeCounter = 0;
            inactiveCounter = 0;
            PressedValue = pressedValue;
            pinMode(pin ,pin_mode);
        }

        bool readSensor(){
            if (READ) {
                return false;
            } 
            if (Pressed) READ = true;
            return Pressed;
        }

        bool sensorActive(){
            return Pressed;
        }

        void sensorMonitor(){
            if (digitalRead(PIN) == PressedValue){
                inactiveCounter = 0;
                activeCounter += 1;
            } else {
                activeCounter = 0;
                inactiveCounter += 1;
            }
            if (activeCounter > COUNT) {
                Pressed = true;
            }
            if (inactiveCounter > COUNT) {
                READ = false;
                Pressed = false;
            }
        }

    private:
        bool READ;
        bool Pressed;
        int COUNT;
        int PIN;
        int activeCounter;
        int inactiveCounter;
        int PressedValue;
};