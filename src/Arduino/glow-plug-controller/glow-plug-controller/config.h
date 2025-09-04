#ifndef CONFIG_H
#define CONFIG_H

#include <Arduino.h>

// Configuration constants
const int START_WAIT_SECONDS = 3;
const int OUTPUT_PINS[] = {3,5,6,9,10,11};    // pwm outputs
const int INPUT_PINS[] = {A0,A1,A2,A3,A4,A5}; // voltage sense inputs
const int NUM_OUTPUTS = sizeof(OUTPUT_PINS) / sizeof(OUTPUT_PINS[0]);
const int NUM_INPUTS = sizeof(INPUT_PINS) / sizeof(INPUT_PINS[0]);
const int FULL_POWER_DURATION_MS = 6000;      // 6 seconds at 100%
const int RAMP_DOWN_DURATION_MS = 3000;       // 3 seconds ramp down

// Debug macros
// Uncomment this line to enable debug output
#define DEBUG

#ifdef DEBUG
  #define DEBUG_PRINT(x) Serial.print(x)
  #define DEBUG_PRINTLN(x) Serial.println(x)
#else
  #define DEBUG_PRINT(x)
  #define DEBUG_PRINTLN(x)
#endif

// State machine
enum ControllerState {
  STATE_BOOT_DELAY,
  STATE_FULL_POWER,
  STATE_RAMP_DOWN,
  STATE_IDLE,
  STATE_LOW_POWER
};

// Global variables
extern ControllerState currentState;
extern unsigned long stateStartTime;
extern bool outputEnabled[NUM_OUTPUTS];
extern float currentDutyCycle[NUM_OUTPUTS];

#endif