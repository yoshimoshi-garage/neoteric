#ifndef CONFIG_H
#define CONFIG_H

#include <Arduino.h>

// Configuration constants
const int START_WAIT_SECONDS = 1;  // Reduced from 3 seconds

// uncomment these for 1-channel test board
//const int OUTPUT_PINS[] = {11};    // pwm outputs
//const int INPUT_PINS[] = {A5}; // voltage sense inputs

const int OUTPUT_PINS[] = {3,5,6,9,10,11};    // pwm outputs
const int INPUT_PINS[] = {A0,A1,A2,A3,A4,A5}; // voltage sense inputs
const int NUM_OUTPUTS = sizeof(OUTPUT_PINS) / sizeof(OUTPUT_PINS[0]);
const int NUM_INPUTS = sizeof(INPUT_PINS) / sizeof(INPUT_PINS[0]);
const int FULL_POWER_DURATION_MS = 5000;      // 5 seconds at 100% for all plugs
const int COLD_ENGINE_TOTAL_MS = 15000;       // 15 seconds total for cold engine
const int HOT_ENGINE_TOTAL_MS = 10000;        // 10 seconds total for hot engine
const int STAGGER_DELAY_MS = 500;             // 0.5 seconds between starting each plug
const float HOT_PLUG_TEMP_THRESHOLD = 200.0;  // Temperature threshold for "hot" plug
const float REDUCED_DUTY_CYCLE = 0.6;         // 60% duty cycle for second phase

// Current monitoring constants
const float VOLTAGE_DIVIDER_R1 = 4700.0;    // 4.7k to Arduino input
const float VOLTAGE_DIVIDER_R2 = 1500.0;    // 1.5k to ground
const float ARDUINO_VREF = 5.0;             // Arduino reference voltage
const float ADC_RESOLUTION = 1024.0;        // 10-bit ADC
const float BTS50010_SENSE_RATIO = 10000.0; // 10000:1 current sense ratio (typical)

// Current limits (in Amperes)
const float MIN_CURRENT_THRESHOLD = 1.0;    // Minimum expected current
const float MAX_CURRENT_THRESHOLD = 20.0;   // Maximum safe current
const bool DISABLE_CURRENT_LIMITS = false;  // Set to true to disable current limit checking

// Temperature estimation constants (simplified model)
// These values are used to estimate glow plug temperature from current:
// Temperature = AMBIENT_TEMP + (R_measured - GLOW_PLUG_RESISTANCE_COLD) / (GLOW_PLUG_RESISTANCE_COLD * TEMP_COEFFICIENT)
// where R_measured = voltage / current
// 
// Numeric example with default values:
// If voltage=13.8V, current=10A: R_measured = 13.8/10 = 1.38Ω
// Temperature = 25 + (1.38 - 0.8) / (0.8 * 0.006) = 25 + 0.58/0.0048 = 25 + 121 = 146°C
//
// Adjust these values based on your specific glow plug specifications:
// Example 1: Bosch Duraterm glow plugs typically have ~0.6Ω cold resistance, TC ~0.0055/°C
// Example 2: NGK ceramic glow plugs typically have ~1.2Ω cold resistance, TC ~0.004/°C
// Example 3: ACDelco 6.6 Duramax ceramic glow plugs typically have ~0.4Ω cold resistance, TC ~0.0008/°C
const float GLOW_PLUG_RESISTANCE_COLD = 0.8;  // Cold resistance in ohms
const float TEMP_COEFFICIENT = 0.006;         // Temperature coefficient per °C
const float AMBIENT_TEMP = 25.0;              // Ambient temperature in °C

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

enum OutputState {
  OUTPUT_OFF,
  OUTPUT_WAITING_TO_START,
  OUTPUT_FULL_POWER,
  OUTPUT_REDUCED_POWER,
  OUTPUT_FINISHED
};

// Global variables
extern ControllerState currentState;
extern unsigned long stateStartTime;
extern bool outputEnabled[NUM_OUTPUTS];
extern float currentDutyCycle[NUM_OUTPUTS];
extern OutputState outputStates[NUM_OUTPUTS];
extern unsigned long outputStartTimes[NUM_OUTPUTS];
extern unsigned long outputStaggerStartTime[NUM_OUTPUTS];
extern int outputTotalDuration[NUM_OUTPUTS];
extern float initialTemperatures[NUM_OUTPUTS];
extern bool outputFaulted[NUM_OUTPUTS];
extern int firstFaultedOutput;

#endif