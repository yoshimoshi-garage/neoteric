#include "output_control.h"

void initializeOutputs() {
  // Initialize all outputs to OFF and enable all outputs by default
  for(int i = 0 ; i < NUM_OUTPUTS ; i++) {
    pinMode(OUTPUT_PINS[i], OUTPUT);
    digitalWrite(OUTPUT_PINS[i], LOW);
    analogWrite(OUTPUT_PINS[i], 0);
    outputEnabled[i] = true;
    currentDutyCycle[i] = 0.0;
  }
  DEBUG_PRINTLN("All outputs initialized to OFF and enabled");
}

void setOutput(int outputIndex, float dutyCycle) {
  if (outputIndex < 0 || outputIndex >= NUM_OUTPUTS) {
    return;
  }
  
  dutyCycle = constrain(dutyCycle, 0.0, 1.0);
  currentDutyCycle[outputIndex] = dutyCycle;
  
  if (outputEnabled[outputIndex]) {
    int pwmValue = (int)(dutyCycle * 255);
    analogWrite(OUTPUT_PINS[outputIndex], pwmValue);
  } else {
    analogWrite(OUTPUT_PINS[outputIndex], 0);
  }
}

void setAllOutputs(float dutyCycle) {
  for(int i = 0; i < NUM_OUTPUTS; i++) {
    setOutput(i, dutyCycle);
  }
}

void enableOutput(int outputIndex, bool enabled) {
  if (outputIndex < 0 || outputIndex >= NUM_OUTPUTS) {
    return;
  }
  
  outputEnabled[outputIndex] = enabled;
  
  if (!enabled) {
    analogWrite(OUTPUT_PINS[outputIndex], 0);
    DEBUG_PRINT("Output ");
    DEBUG_PRINT(outputIndex);
    DEBUG_PRINTLN(" disabled");
  } else {
    setOutput(outputIndex, currentDutyCycle[outputIndex]);
    DEBUG_PRINT("Output ");
    DEBUG_PRINT(outputIndex);
    DEBUG_PRINTLN(" enabled");
  }
}

bool isOutputEnabled(int outputIndex) {
  if (outputIndex < 0 || outputIndex >= NUM_OUTPUTS) {
    return false;
  }
  return outputEnabled[outputIndex];
}