#include "fault_indication.h"

// Local state variables for LED blinking
static unsigned long lastBlinkTime = 0;
static int currentBlink = 0;
static bool ledState = false;
static bool inSequencePause = false;
static unsigned long sequencePauseStart = 0;
static int faultOutputToIndicate = -1;

void initializeFaultIndication() {
  DEBUG_PRINTLN("Fault indication system initialized");
  lastBlinkTime = 0;
  currentBlink = 0;
  ledState = false;
  inSequencePause = false;
  faultOutputToIndicate = -1;
}

void setOutputFault(int outputIndex, bool faulted) {
  if (outputIndex < 0 || outputIndex >= NUM_OUTPUTS) {
    return;
  }
  
  bool wasFaulted = outputFaulted[outputIndex];
  outputFaulted[outputIndex] = faulted;
  
  if (faulted && !wasFaulted) {
    DEBUG_PRINT("FAULT detected on output ");
    DEBUG_PRINTLN(outputIndex);
    
    // Update first faulted output if this is the first or lower index
    if (firstFaultedOutput == -1 || outputIndex < firstFaultedOutput) {
      firstFaultedOutput = outputIndex;
      DEBUG_PRINT("First faulted output updated to: ");
      DEBUG_PRINTLN(firstFaultedOutput);
    }
  }
  
  // If this fault is cleared and it was the first faulted output,
  // find the next lowest faulted output
  if (!faulted && wasFaulted && outputIndex == firstFaultedOutput) {
    firstFaultedOutput = -1;
    for (int i = 0; i < NUM_OUTPUTS; i++) {
      if (outputFaulted[i]) {
        firstFaultedOutput = i;
        break;
      }
    }
    
    if (firstFaultedOutput == -1) {
      DEBUG_PRINTLN("All faults cleared");
    } else {
      DEBUG_PRINT("First faulted output updated to: ");
      DEBUG_PRINTLN(firstFaultedOutput);
    }
  }
}

bool hasAnyFaults() {
  return (firstFaultedOutput != -1);
}

int getFirstFaultedOutput() {
  return firstFaultedOutput;
}

void updateFaultIndication() {
  unsigned long currentTime = millis();
  
  // If no faults, ensure LED is off and reset state
  if (!hasAnyFaults()) {
    if (currentState != STATE_FULL_POWER) { // Don't interfere with normal operation LED
      digitalWrite(LED_BUILTIN, LOW);
    }
    currentBlink = 0;
    inSequencePause = false;
    faultOutputToIndicate = -1;
    return;
  }
  
  // Don't interfere with LED during normal glow plug operation
  if (currentState == STATE_FULL_POWER) {
    return;
  }
  
  // Get the fault to indicate (1-based for user display)
  int faultToShow = firstFaultedOutput + 1; // Convert to 1-based
  
  // Check if we need to start a new sequence
  if (faultOutputToIndicate != firstFaultedOutput) {
    faultOutputToIndicate = firstFaultedOutput;
    currentBlink = 0;
    inSequencePause = false;
    lastBlinkTime = currentTime;
    ledState = false;
    digitalWrite(LED_BUILTIN, LOW);
  }
  
  // Handle sequence pause between repetitions
  if (inSequencePause) {
    if (currentTime - sequencePauseStart >= SEQUENCE_PAUSE_MS) {
      inSequencePause = false;
      currentBlink = 0;
      lastBlinkTime = currentTime;
      ledState = false;
      digitalWrite(LED_BUILTIN, LOW);
    }
    return;
  }
  
  // Check if it's time to change LED state
  unsigned long timeSinceLastChange = currentTime - lastBlinkTime;
  
  if (!ledState && timeSinceLastChange >= BLINK_OFF_TIME_MS) {
    // Time to turn LED on for next blink
    if (currentBlink < faultToShow) {
      digitalWrite(LED_BUILTIN, HIGH);
      ledState = true;
      lastBlinkTime = currentTime;
    } else {
      // Finished all blinks, start sequence pause
      inSequencePause = true;
      sequencePauseStart = currentTime;
    }
  } else if (ledState && timeSinceLastChange >= BLINK_ON_TIME_MS) {
    // Time to turn LED off
    digitalWrite(LED_BUILTIN, LOW);
    ledState = false;
    lastBlinkTime = currentTime;
    currentBlink++;
  }
}