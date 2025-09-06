#ifndef CURRENT_MONITOR_H
#define CURRENT_MONITOR_H

#include "config.h"

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
void measureInitialTemperatures();
void setOutputTimingBasedOnTemperature(int outputIndex, float temperature);

#endif