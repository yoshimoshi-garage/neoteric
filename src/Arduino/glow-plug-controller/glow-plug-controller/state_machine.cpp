#include "state_machine.h"
#include "output_control.h"
#include "current_monitor.h"

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


void startFullPowerPhase() {
  DEBUG_PRINTLN("Starting staggered output full power phases");
  digitalWrite(LED_BUILTIN, HIGH);
  
  unsigned long currentTime = millis();
  for (int i = 0; i < NUM_OUTPUTS; i++) {
    if (outputEnabled[i]) {
      outputStates[i] = OUTPUT_WAITING_TO_START;
      outputStaggerStartTime[i] = currentTime + (i * STAGGER_DELAY_MS);
      
      DEBUG_PRINT("Output ");
      DEBUG_PRINT(i);
      DEBUG_PRINT(" will start in ");
      DEBUG_PRINT(i * STAGGER_DELAY_MS / 1000.0);
      DEBUG_PRINT("s - total duration: ");
      DEBUG_PRINT(outputTotalDuration[i] / 1000);
      DEBUG_PRINTLN("s");
    }
  }
}

void updateIndividualOutputs() {
  unsigned long currentTime = millis();
  bool anyOutputActive = false;
  bool anyOutputInRampDown = false;
  
  for (int i = 0; i < NUM_OUTPUTS; i++) {
    if (!outputEnabled[i]) {
      outputStates[i] = OUTPUT_FINISHED;
      continue;
    }
    
    unsigned long outputElapsed = currentTime - outputStartTimes[i];
    
    switch (outputStates[i]) {
      case OUTPUT_WAITING_TO_START:
        if (currentTime >= outputStaggerStartTime[i]) {
          DEBUG_PRINT("Output ");
          DEBUG_PRINT(i);
          DEBUG_PRINTLN(" starting full power phase");
          outputStates[i] = OUTPUT_FULL_POWER;
          outputStartTimes[i] = currentTime;
          setOutput(i, 1.0);
        }
        anyOutputActive = true; // Still considered active while waiting
        break;
        
      case OUTPUT_FULL_POWER:
        if (outputElapsed >= FULL_POWER_DURATION_MS) {
          DEBUG_PRINT("Output ");
          DEBUG_PRINT(i);
          DEBUG_PRINTLN(" switching to reduced power (60%)");
          outputStates[i] = OUTPUT_REDUCED_POWER;
          outputStartTimes[i] = currentTime; // Reset timer for reduced power phase
          setOutput(i, REDUCED_DUTY_CYCLE);
          anyOutputActive = true; // Still active in reduced power mode
        } else {
          anyOutputActive = true;
        }
        break;
        
      case OUTPUT_REDUCED_POWER:
        {
          // Calculate time spent in reduced power phase
          unsigned long reducedPowerElapsed = currentTime - outputStartTimes[i];
          unsigned long reducedPowerDuration = outputTotalDuration[i] - FULL_POWER_DURATION_MS;
          
          if (reducedPowerElapsed >= reducedPowerDuration) {
            DEBUG_PRINT("Output ");
            DEBUG_PRINT(i);
            DEBUG_PRINTLN(" heating cycle complete");
            outputStates[i] = OUTPUT_FINISHED;
            setOutput(i, 0.0);
          } else {
            // Continue at 60% duty cycle
            setOutput(i, REDUCED_DUTY_CYCLE);
            anyOutputActive = true;
            anyOutputInRampDown = true; // Consider this as "finishing phase"
          }
        }
        break;
        
      case OUTPUT_FINISHED:
      case OUTPUT_OFF:
        break;
    }
  }
  
  // Update main state based on individual output states
  if (currentState == STATE_FULL_POWER && !anyOutputActive && !anyOutputInRampDown) {
    DEBUG_PRINTLN("All outputs finished - entering low power mode");
    digitalWrite(LED_BUILTIN, LOW);
    enterLowPowerMode();
  }
}

void updateStateMachine() {
  unsigned long currentTime = millis();
  unsigned long elapsedTime = currentTime - stateStartTime;
  
  switch (currentState) {
    case STATE_BOOT_DELAY:
      if (elapsedTime >= (START_WAIT_SECONDS * 1000)) {
        DEBUG_PRINTLN("Boot delay complete - measuring initial temperatures");
        measureInitialTemperatures();
        
        currentState = STATE_FULL_POWER;
        stateStartTime = currentTime;
        startFullPowerPhase();
      }
      break;
      
    case STATE_FULL_POWER:
      updateIndividualOutputs();
      break;
      
    case STATE_IDLE:
    case STATE_LOW_POWER:
      break;
  }
}