#include "state_machine.h"
#include "output_control.h"

void initializeStateMachine() {
  stateStartTime = millis();
  currentState = STATE_BOOT_DELAY;
  
  DEBUG_PRINT("Boot delay started - waiting ");
  DEBUG_PRINT(START_WAIT_SECONDS);
  DEBUG_PRINTLN(" seconds");
}

void enterLowPowerMode() {
  DEBUG_PRINTLN("Entering low power mode");
  
  // Turn off all outputs
  setAllOutputs(0.0);
  
  // Turn off built-in LED
  digitalWrite(LED_BUILTIN, LOW);
  
  // Put processor to sleep (if supported)
  // This is Arduino-specific and may vary by board
  // For now, just enter idle state
  currentState = STATE_LOW_POWER;
}

float calculateRampDutyCycle(unsigned long elapsedMs, unsigned long rampDurationMs) {
  if (elapsedMs >= rampDurationMs) {
    return 0.0;
  }
  
  float progress = (float)elapsedMs / (float)rampDurationMs;
  return 1.0 - progress;
}

void updateStateMachine() {
  unsigned long currentTime = millis();
  unsigned long elapsedTime = currentTime - stateStartTime;
  
  switch (currentState) {
    case STATE_BOOT_DELAY:
      if (elapsedTime >= (START_WAIT_SECONDS * 1000)) {
        DEBUG_PRINTLN("Boot delay complete - starting full power phase");
        currentState = STATE_FULL_POWER;
        stateStartTime = currentTime;
        setAllOutputs(1.0);
        digitalWrite(LED_BUILTIN, HIGH);
      }
      break;
      
    case STATE_FULL_POWER:
      if (elapsedTime >= FULL_POWER_DURATION_MS) {
        DEBUG_PRINTLN("Full power phase complete - starting ramp down");
        currentState = STATE_RAMP_DOWN;
        stateStartTime = currentTime;
      }
      break;
      
    case STATE_RAMP_DOWN:
      if (elapsedTime >= RAMP_DOWN_DURATION_MS) {
        DEBUG_PRINTLN("Ramp down complete - entering low power mode");
        setAllOutputs(0.0);
        digitalWrite(LED_BUILTIN, LOW);
        enterLowPowerMode();
      } else {
        float dutyCycle = calculateRampDutyCycle(elapsedTime, RAMP_DOWN_DURATION_MS);
        setAllOutputs(dutyCycle);
      }
      break;
      
    case STATE_IDLE:
    case STATE_LOW_POWER:
      break;
  }
}