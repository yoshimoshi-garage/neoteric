#include "current_monitor.h"
#include "output_control.h"

void initializeCurrentMonitoring() {
  DEBUG_PRINTLN("Current monitoring initialized");
  DEBUG_PRINT("Voltage divider ratio: ");
  DEBUG_PRINTLN(VOLTAGE_DIVIDER_R2 / (VOLTAGE_DIVIDER_R1 + VOLTAGE_DIVIDER_R2));
  DEBUG_PRINT("Current limits: ");
  DEBUG_PRINT(MIN_CURRENT_THRESHOLD);
  DEBUG_PRINT("A to ");
  DEBUG_PRINT(MAX_CURRENT_THRESHOLD);
  DEBUG_PRINTLN("A");
}

float readVoltageFromADC(int inputPin) {
  int adcValue = analogRead(inputPin);
  float arduinoVoltage = (adcValue / ADC_RESOLUTION) * ARDUINO_VREF;
  
  // Convert back to original voltage before voltage divider
  float originalVoltage = arduinoVoltage * (VOLTAGE_DIVIDER_R1 + VOLTAGE_DIVIDER_R2) / VOLTAGE_DIVIDER_R2;
  
  return originalVoltage;
}

float convertVoltageToCurrent(float senseVoltage) {
  // BTS50010 provides a current sense output
  // Typical sense ratio is 10000:1 (10000A load current = 1A sense current)
  // The sense voltage represents the current through the sense resistor
  // Assuming a 1-ohm sense resistor: V_sense = I_sense * R_sense
  // I_load = I_sense * sense_ratio = (V_sense / R_sense) * sense_ratio
  
  const float SENSE_RESISTOR = 1.0; // Assuming 1 ohm sense resistor
  float senseCurrent = senseVoltage / SENSE_RESISTOR;
  float loadCurrent = senseCurrent * (BTS50010_SENSE_RATIO / 10000.0); // Adjust ratio as needed
  
  return loadCurrent;
}

float estimateGlowPlugTemperature(float current) {
  if (current <= 0.1) {
    return AMBIENT_TEMP; // No current, assume ambient temperature
  }
  
  // Simplified temperature estimation based on current
  // Real glow plugs have complex temperature/current relationships
  // This is a basic linear approximation
  
  // Assume 12V supply for resistance calculation
  float voltage = 12.0; // Vehicle supply voltage
  float resistance = voltage / current;
  
  // Temperature calculation based on resistance change
  // R(T) = R0 * (1 + α * ΔT)
  // T = T0 + (R - R0) / (R0 * α)
  
  float deltaR = resistance - GLOW_PLUG_RESISTANCE_COLD;
  float deltaT = deltaR / (GLOW_PLUG_RESISTANCE_COLD * TEMP_COEFFICIENT);
  float temperature = AMBIENT_TEMP + deltaT;
  
  // Clamp to reasonable range
  temperature = constrain(temperature, AMBIENT_TEMP, 1000.0);
  
  return temperature;
}

CurrentReading readGlowPlugCurrent(int outputIndex) {
  CurrentReading reading;
  reading.isValid = false;
  reading.current = 0.0;
  reading.estimatedTemp = AMBIENT_TEMP;
  reading.isOvercurrent = false;
  reading.isUndercurrent = false;
  
  // Check if output index is valid
  if (outputIndex < 0 || outputIndex >= NUM_INPUTS) {
    return reading;
  }
  
  // Check if output is enabled - no current if disabled
  if (!isOutputEnabled(outputIndex)) {
    reading.isValid = true;
    return reading;
  }
  
  // Read voltage from corresponding input pin
  float senseVoltage = readVoltageFromADC(INPUT_PINS[outputIndex]);
  
  // Convert voltage to current
  reading.current = convertVoltageToCurrent(senseVoltage);
  
  // Estimate temperature
  reading.estimatedTemp = estimateGlowPlugTemperature(reading.current);
  
  // Check current limits
  reading.isOvercurrent = (reading.current > MAX_CURRENT_THRESHOLD);
  reading.isUndercurrent = (reading.current < MIN_CURRENT_THRESHOLD && reading.current > 0.5); // Only flag if some current
  
  reading.isValid = true;
  
  return reading;
}

void checkCurrentLimitsAndDisable(int outputIndex, CurrentReading reading) {
  if (!reading.isValid) {
    return;
  }
  
  bool shouldDisable = false;
  
  if (reading.isOvercurrent) {
    DEBUG_PRINT("OVERCURRENT detected on output ");
    DEBUG_PRINT(outputIndex);
    DEBUG_PRINT(": ");
    DEBUG_PRINT(reading.current);
    DEBUG_PRINTLN("A");
    shouldDisable = true;
  }
  
  if (reading.isUndercurrent) {
    DEBUG_PRINT("UNDERCURRENT detected on output ");
    DEBUG_PRINT(outputIndex);
    DEBUG_PRINT(": ");
    DEBUG_PRINT(reading.current);
    DEBUG_PRINTLN("A");
    shouldDisable = true;
  }
  
  if (shouldDisable && isOutputEnabled(outputIndex)) {
    enableOutput(outputIndex, false);
    DEBUG_PRINT("Output ");
    DEBUG_PRINT(outputIndex);
    DEBUG_PRINTLN(" disabled due to current fault");
  }
  
  // Optional: Log temperature for monitoring
  if (reading.current > 1.0) { // Only log when significant current
    static unsigned long lastTempLog = 0;
    if (millis() - lastTempLog > 1000) { // Log every second
      DEBUG_PRINT("Output ");
      DEBUG_PRINT(outputIndex);
      DEBUG_PRINT(" - Current: ");
      DEBUG_PRINT(reading.current);
      DEBUG_PRINT("A, Est. Temp: ");
      DEBUG_PRINT(reading.estimatedTemp);
      DEBUG_PRINTLN("°C");
      lastTempLog = millis();
    }
  }
}

void monitorAllCurrents() {
  // Only monitor when outputs are actually running
  if (currentState == STATE_FULL_POWER || currentState == STATE_RAMP_DOWN) {
    for (int i = 0; i < NUM_OUTPUTS; i++) {
      CurrentReading reading = readGlowPlugCurrent(i);
      checkCurrentLimitsAndDisable(i, reading);
    }
  }
}