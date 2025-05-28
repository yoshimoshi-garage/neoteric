/*
 * Seat Cooling Control System
 * 
 * This sketch controls 4 PWM fans based on 4 input switches.
 * Each fan is associated with a specific switch and will only run when its switch is ON.
 * The speed of all active fans is controlled by a single potentiometer.
 * 
 * Hardware connections:
 * - Potentiometer connected to analog pin A0
 * - 4 switches connected to digital pins 2, 3, 4, 5
 * - 4 PWM fans connected to PWM pins 6, 9, 10, 11
 */

 #include <Arduino.h>

// Uncomment this line to enable debug output
#define DEBUG

// Pin definitions
const int INPUT_PIN = A0;   
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
}

void setup() {
  #ifdef DEBUG
    Serial.begin(9600);
    DEBUG_PRINTLN("Switch Converter Initialized");
  #endif

  pinMode(OUTPUT_PIN, OUTPUT);  // Set output for PWM
}

void loop() {
  // inputValue = analogRead(INPUT_PIN);
  // int switchSelection = detectVoltageLevel(inputValue);

  // DEBUG_PRINT("switch: ");
  // DEBUG_PRINTLN(switchSelection);

  processVoltageIO();

  // Short delay for stability
  delay(1000);
  }
