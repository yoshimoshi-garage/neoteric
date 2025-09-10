#include "current_monitor.h"
#include "output_control.h"
#include "fault_indication.h"

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
  
  // ALWAYS show detailed debug during current issues
  DEBUG_PRINT("[DEBUG] Pin A");
  DEBUG_PRINT(inputPin - A0);
  DEBUG_PRINT(" - ADC raw: ");
  DEBUG_PRINT(adcValue);
  DEBUG_PRINT("/1024, Arduino input: ");
  DEBUG_PRINT(arduinoVoltage);
  DEBUG_PRINT("V, Reconstructed IS voltage: ");
  DEBUG_PRINT(originalVoltage);
  DEBUG_PRINTLN("V");
  
  return originalVoltage;
}

float convertVoltageToCurrent(float senseVoltage) {
  // BTS50010 provides a current sense output
  // Empirical calibration: reported 9.67A when actual was 16A
  // Correction factor: 16A / 9.67A = 1.65
  
  const float SENSE_RESISTOR = 1.0; // Assuming 1 ohm sense resistor  
  const float CALIBRATION_FACTOR = 1.65; // Empirical correction based on actual measurements
  
  float senseCurrent = senseVoltage / SENSE_RESISTOR;
  float loadCurrent = senseCurrent * (BTS50010_SENSE_RATIO / 10000.0) * CALIBRATION_FACTOR;
  
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
  float voltage = 13.8; // Vehicle supply voltage
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
  bool hasFault = false;
  
  if (reading.isOvercurrent) {
    DEBUG_PRINT("OVERCURRENT detected on output ");
    DEBUG_PRINT(outputIndex);
    DEBUG_PRINT(": ");
    DEBUG_PRINT(reading.current);
    DEBUG_PRINTLN("A");
    shouldDisable = true;
    hasFault = true;
  }
  
  if (reading.isUndercurrent) {
    DEBUG_PRINT("UNDERCURRENT detected on output ");
    DEBUG_PRINT(outputIndex);
    DEBUG_PRINT(": ");
    DEBUG_PRINT(reading.current);
    DEBUG_PRINTLN("A");
    shouldDisable = true;
    hasFault = true;
  }
  
  // Set fault flag for LED indication
  setOutputFault(outputIndex, hasFault);
  
  if (shouldDisable && isOutputEnabled(outputIndex)) {
    enableOutput(outputIndex, false);
    DEBUG_PRINT("Output ");
    DEBUG_PRINT(outputIndex);
    DEBUG_PRINTLN(" disabled due to current fault - LED will indicate fault");
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

void measureInitialTemperatures() {
  DEBUG_PRINTLN("Measuring initial glow plug temperatures...");
  
  // Turn on all outputs simultaneously at low power for faster measurement
  for (int i = 0; i < NUM_OUTPUTS; i++) {
    if (outputEnabled[i]) {
      setOutput(i, 0.1); // Apply low duty cycle (10%) to all plugs
    }
  }
  
  delay(200); // Wait for current to stabilize - reduced from 500ms per plug
  
  // Read all temperatures
  for (int i = 0; i < NUM_OUTPUTS; i++) {
    if (!outputEnabled[i]) continue;
    
    CurrentReading reading = readGlowPlugCurrent(i);
    if (reading.isValid) {
      initialTemperatures[i] = reading.estimatedTemp;
      setOutputTimingBasedOnTemperature(i, reading.estimatedTemp);
      
      DEBUG_PRINT("Output ");
      DEBUG_PRINT(i);
      DEBUG_PRINT(" initial temp: ");
      DEBUG_PRINT(reading.estimatedTemp);
      DEBUG_PRINT("°C, total duration: ");
      DEBUG_PRINT(outputTotalDuration[i] / 1000);
      DEBUG_PRINTLN("s");
    }
  }
  
  // Turn all outputs off
  for (int i = 0; i < NUM_OUTPUTS; i++) {
    setOutput(i, 0.0);
  }
  
  delay(50); // Brief pause before starting main sequence - reduced from 100ms
}

void setOutputTimingBasedOnTemperature(int outputIndex, float temperature) {
  if (outputIndex < 0 || outputIndex >= NUM_OUTPUTS) {
    return;
  }
  
  if (temperature >= HOT_PLUG_TEMP_THRESHOLD) {
    outputTotalDuration[outputIndex] = HOT_ENGINE_TOTAL_MS;
    DEBUG_PRINT("Output ");
    DEBUG_PRINT(outputIndex);
    DEBUG_PRINTLN(" classified as HOT engine - 8 second total (5s@100% + 3s@60%)");
  } else {
    outputTotalDuration[outputIndex] = COLD_ENGINE_TOTAL_MS;
    DEBUG_PRINT("Output ");
    DEBUG_PRINT(outputIndex);
    DEBUG_PRINTLN(" classified as COLD engine - 15 second total (5s@100% + 10s@60%)");
  }
}

void monitorAllCurrents() {
  // Only monitor when outputs are actually running
  if (currentState == STATE_FULL_POWER) {
    for (int i = 0; i < NUM_OUTPUTS; i++) {
      CurrentReading reading = readGlowPlugCurrent(i);
      checkCurrentLimitsAndDisable(i, reading);
    }
  }
}