/*
 * Neoteric Transfer Case Switch Converter
 *
 * MIT LICENSE
 * 
 * © 2025 Chris Tacke
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
 * IN THE SOFTWARE.
 */

 #include <Arduino.h>

// Uncomment this line to enable debug output
//#define DEBUG

#ifdef DEBUG
  // slower in debug to not overload the console output
  const int LOOP_WAIT = 1000;
#else
  // faster is release for responsiveness
  const int LOOP_WAIT = 100;
#endif

// const 
// Pin definitions
const int INPUT_PIN = A1;   
const int OUTPUT_PIN = 3;

const float ACTUAL_VCC = 5.0;
// due to impedance, etc of RC filter, we need to accound for a voltage drop
// Calibration = Target_Voltage / Measured_Voltage
const float PWM_CALIBRATION_FACTOR = 1.08;// 1.067;

// Voltage mapping structure
struct VoltageMap {
  const char* gearName;
  float inputVoltage;
  float outputVoltage;
};

// Single lookup table with gear->input->output mappings
const VoltageMap VOLTAGE_TABLE[] = {
  {"4L",   1.08, 1.5},  
  {"4H",   1.83, 3.0},  
  {"2H",   2.85, 2.0}   
};

// Variables
int inputValue = 0;

// Debug macros.  If #debug is not defined, they do nothing
#ifdef DEBUG
  #define DEBUG_PRINT(x) Serial.print(x)
  #define DEBUG_PRINTLN(x) Serial.println(x)
#else
  #define DEBUG_PRINT(x)
  #define DEBUG_PRINTLN(x)
#endif

const int TABLE_SIZE = sizeof(VOLTAGE_TABLE) / sizeof(VOLTAGE_TABLE[0]);

// Function to find the closest voltage mapping entry
const VoltageMap* getVoltageMapping(int adcValue) {
  // Convert ADC value to voltage (5V reference, 10-bit ADC)
  float inputVoltage = (adcValue / 1023.0) * 5.0;
  
  DEBUG_PRINT("input voltage: ");
  DEBUG_PRINTLN(inputVoltage);

  int closestIndex = 0;
  float minDistance = fabs(inputVoltage - VOLTAGE_TABLE[0].inputVoltage);
  
  // Find the closest input voltage
  for (int i = 1; i < TABLE_SIZE; i++) {
    float distance = fabs(inputVoltage - VOLTAGE_TABLE[i].inputVoltage);
    if (distance < minDistance) {
      minDistance = distance;
      closestIndex = i;
    }
  }
  
  DEBUG_PRINT("switch: ");
  DEBUG_PRINTLN(VOLTAGE_TABLE[closestIndex].gearName);

  return &VOLTAGE_TABLE[closestIndex];
}

// Convert desired voltage to PWM value (0-255)
// Assumes 5V supply for PWM output
int voltageToPWM(float voltage) {
  if (voltage < 0) voltage = 0;
  if (voltage > 5.0) voltage = 5.0;
  
  float calibratedVoltage = voltage * PWM_CALIBRATION_FACTOR;

  return (int)((calibratedVoltage / ACTUAL_VCC) * 255);
}

void processVoltageIO() {
  // Read input and get the voltage mapping
  int adcReading = analogRead(INPUT_PIN);
  const VoltageMap* mapping = getVoltageMapping(adcReading);
  
  // Convert to PWM and output to pin D3
  int pwmValue = voltageToPWM(mapping->outputVoltage);
  analogWrite(OUTPUT_PIN, pwmValue);
  
  // Debug output
#if DEBUG
  float inputVoltage = (adcReading / 1023.0) * 5.0;
  Serial.print("Gear: ");
  Serial.print(mapping->gearName);
  Serial.print(" (");
  Serial.print(inputVoltage);
  Serial.print("V) -> Output: ");
  Serial.print(mapping->outputVoltage);
  Serial.print("V (PWM: ");
  Serial.print(pwmValue);
  Serial.println(")");
#else
// Debug output only when switch position changes
  static const char* previousGear = "";  // Remember previous gear
  
  if (strcmp(mapping->gearName, previousGear) != 0) {
    Serial.print("Switch changed to: ");
    Serial.print(mapping->gearName);
    Serial.print(" -> Output: ");
    Serial.print(mapping->outputVoltage);
    Serial.println("V");
    
    previousGear = mapping->gearName;  // Update for next comparison
  }
#endif
}

void setup() {
  Serial.begin(9600);
  DEBUG_PRINTLN("Switch Converter Initialized");

  pinMode(OUTPUT_PIN, OUTPUT);  // Set output for PWM
}

void loop() {
  processVoltageIO();

  // Short delay for stability
  delay(LOOP_WAIT);
}
