#ifndef CURRENT_MONITOR_H
#define CURRENT_MONITOR_H

#include "config.h"

// Current monitoring constants
const float VOLTAGE_DIVIDER_R1 = 4700.0;    // 4.7k to Arduino input
const float VOLTAGE_DIVIDER_R2 = 1500.0;    // 1.5k to ground
const float ARDUINO_VREF = 5.0;             // Arduino reference voltage
const float ADC_RESOLUTION = 1024.0;        // 10-bit ADC
const float BTS50010_SENSE_RATIO = 10000.0; // 10000:1 current sense ratio (typical)

// Current limits (in Amperes)
const float MIN_CURRENT_THRESHOLD = 8.0;    // Minimum expected current
const float MAX_CURRENT_THRESHOLD = 20.0;   // Maximum safe current

// Temperature estimation constants (simplified model)
const float GLOW_PLUG_RESISTANCE_COLD = 0.8;  // Cold resistance in ohms
const float TEMP_COEFFICIENT = 0.006;         // Temperature coefficient per °C
const float AMBIENT_TEMP = 25.0;              // Ambient temperature in °C

struct CurrentReading {
  float current;
  float estimatedTemp;
  bool isValid;
  bool isOvercurrent;
  bool isUndercurrent;
};

// Function declarations
void initializeCurrentMonitoring();
float readVoltageFromADC(int inputPin);
float convertVoltageToCurrent(float senseVoltage);
float estimateGlowPlugTemperature(float current);
CurrentReading readGlowPlugCurrent(int outputIndex);
void checkCurrentLimitsAndDisable(int outputIndex, CurrentReading reading);
void monitorAllCurrents();

#endif