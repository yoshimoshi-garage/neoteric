# Neoteric Glow Plug Controller

An intelligent Arduino-based glow plug controller for 6-cylinder diesel engines featuring individual plug monitoring, temperature-adaptive timing, and comprehensive fault indication.

## Features

### 🔥 **Intelligent Heating Control**
- **Two-Phase Heating**: 5 seconds at 100% power, then reduced to 60% 
- **Temperature-Adaptive Duration**: Cold engines (15s total), Hot engines (8s total)
- **Staggered Startup**: 1-second delay between plugs to reduce electrical load
- **Individual Control**: Each glow plug operates independently

### 📊 **Advanced Monitoring**
- **Real-Time Current Sensing**: Individual current monitoring per cylinder using BTS50010 high-side switches
- **Temperature Estimation**: Calculates glow plug temperature from current draw
- **Fault Detection**: Over/undercurrent protection with automatic plug disable
- **Voltage Divider Input**: 4.7kΩ/1.5kΩ divider for Arduino ADC compatibility

### 🚨 **Fault Indication**
- **LED Blinking Codes**: Visual indication of failed plugs (1 blink = plug 1, 2 blinks = plug 2, etc.)
- **Priority System**: Shows lowest numbered fault first when multiple faults exist
- **Non-Interfering**: Fault indication doesn't disrupt normal operation

## Hardware Requirements

### **Microcontroller**
- Arduino-compatible board (Uno, Nano, etc.)
- 6 PWM-capable output pins
- 6 analog input pins

### **Power Switching**
- 6x BTS50010 high-side switches (or equivalent)
- Rated for 15A+ continuous current per channel

### **Current Sensing**
- Voltage divider: 4.7kΩ (R1) to Arduino input, 1.5kΩ (R2) to ground
- Connected to BTS50010 current sense outputs

### **Connections**
```
Arduino Pins:
- PWM Outputs: 3, 5, 6, 9, 10, 11 (to BTS50010 control inputs)
- Analog Inputs: A0, A1, A2, A3, A4, A5 (from voltage dividers)
- Built-in LED: Fault indication
```

## Operation Sequence

### **1. Boot Sequence (1 second)**
- Initialize all outputs to OFF
- Initialize inputs for current monitoring
- Prepare for temperature measurement

### **2. Temperature Measurement (0.25 seconds)**
- Simultaneously energize all plugs at 10% duty cycle for 200ms
- Measure current and estimate initial temperature for each plug
- Classify as "hot" (≥200°C) or "cold" (<200°C)
- Set appropriate heating duration per plug

### **3. Staggered Startup**
- Plug 1 starts immediately
- Plug 2 starts after 1 second
- Plug 3 starts after 2 seconds
- Continue pattern for remaining plugs

### **4. Two-Phase Heating**
**Phase 1 - Full Power (5 seconds):**
- 100% PWM duty cycle
- Maximum current draw per plug
- Rapid initial heating

**Phase 2 - Reduced Power (3-10 seconds):**
- 60% PWM duty cycle  
- Reduced power consumption
- Sustained heating to operating temperature

### **5. Completion**
- All plugs shut off individually based on their timing
- Enter low-power mode
- Continue fault monitoring and indication

## Configuration Variables

### **Timing Constants**
```cpp
const int START_WAIT_SECONDS = 1;          // Boot delay before operation
const int FULL_POWER_DURATION_MS = 5000;   // Phase 1 duration (100% power)
const int COLD_ENGINE_TOTAL_MS = 15000;    // Total time for cold engines
const int HOT_ENGINE_TOTAL_MS = 8000;      // Total time for hot engines  
const int STAGGER_DELAY_MS = 1000;         // Delay between plug startups
const float REDUCED_DUTY_CYCLE = 0.6;      // Phase 2 duty cycle (60%)
```

### **Temperature Classification**
```cpp
const float HOT_PLUG_TEMP_THRESHOLD = 200.0;  // Hot vs cold threshold (°C)
const float AMBIENT_TEMP = 25.0;              // Assumed ambient temperature
```

### **Current Monitoring**
```cpp
const float MIN_CURRENT_THRESHOLD = 8.0;   // Minimum expected current (A)
const float MAX_CURRENT_THRESHOLD = 20.0;  // Maximum safe current (A)
const float VOLTAGE_DIVIDER_R1 = 4700.0;   // Upper resistor (Ω)
const float VOLTAGE_DIVIDER_R2 = 1500.0;   // Lower resistor (Ω)
const float BTS50010_SENSE_RATIO = 10000.0; // Current sense ratio
```

### **Hardware Configuration**
```cpp
const int OUTPUT_PINS[] = {3,5,6,9,10,11};     // PWM output pins
const int INPUT_PINS[] = {A0,A1,A2,A3,A4,A5};  // Analog input pins
```

### **Fault Indication**
```cpp
const int BLINK_ON_TIME_MS = 300;      // LED on duration per blink
const int BLINK_OFF_TIME_MS = 200;     // LED off duration between blinks
const int SEQUENCE_PAUSE_MS = 1500;    // Pause between blink sequences
```

## LED Fault Codes

| **Blinks** | **Meaning** | **Action** |
|------------|-------------|------------|
| 1 | Glow Plug #1 Fault | Check plug #1 wiring/connection |
| 2 | Glow Plug #2 Fault | Check plug #2 wiring/connection |
| 3 | Glow Plug #3 Fault | Check plug #3 wiring/connection |
| 4 | Glow Plug #4 Fault | Check plug #4 wiring/connection |
| 5 | Glow Plug #5 Fault | Check plug #5 wiring/connection |
| 6 | Glow Plug #6 Fault | Check plug #6 wiring/connection |

**Note**: If multiple faults exist, the LED shows the lowest numbered fault first.

## Power Consumption Analysis

### **Peak Current (Traditional System)**
- 6 plugs × 15A each = **90A simultaneous**
- **1080W** at 12V system voltage

### **Peak Current (This System)**
- Staggered startup: **15A per plug maximum**
- **180W** peak power draw
- **83% reduction** in peak electrical load

### **Energy Consumption**
- **Cold Engine**: ~13.5Ah total energy
- **Hot Engine**: ~6.5Ah total energy
- **Efficiency**: 40% reduction vs traditional fixed timing

## File Structure

```
glow-plug-controller/
├── README.md                    # This file
├── glow-plug-controller.ino     # Main Arduino sketch
├── config.h                     # Configuration constants and globals
├── output_control.h/.cpp        # PWM output control functions
├── state_machine.h/.cpp         # Main operation state machine
├── current_monitor.h/.cpp       # Current sensing and monitoring
└── fault_indication.h/.cpp      # LED fault indication system
```

## Customization

### **Adjusting Timing**
Modify timing constants in `config.h`:
- Increase `COLD_ENGINE_TOTAL_MS` for colder climates
- Decrease `HOT_ENGINE_TOTAL_MS` for faster warm starts
- Adjust `STAGGER_DELAY_MS` for different electrical load requirements

### **Current Thresholds**
Update current limits in `current_monitor.h`:
- `MIN_CURRENT_THRESHOLD`: Lower for ceramic plugs, higher for metal plugs
- `MAX_CURRENT_THRESHOLD`: Set based on plug specifications and safety margins

### **Temperature Classification**
Modify `HOT_PLUG_TEMP_THRESHOLD` based on:
- Engine operating temperature
- Ambient temperature range
- Plug characteristics

## Troubleshooting

### **No Glow Plug Operation**
- Check 12V power supply to BTS50010 switches  
- Verify Arduino PWM output connections
- Ensure `DEBUG` is defined for serial diagnostic output

### **Incorrect Current Readings**
- Verify voltage divider resistor values (4.7kΩ/1.5kΩ)
- Check BTS50010 current sense output connections
- Calibrate `BTS50010_SENSE_RATIO` for your specific switch variant

### **LED Not Indicating Faults**
- Confirm current thresholds are appropriate for your glow plugs
- Check serial monitor for fault detection messages
- Verify built-in LED functionality

### **Inconsistent Heating Times**
- Check individual plug resistances (should be ~0.8Ω cold)
- Verify PWM output voltages under load
- Monitor current readings during operation

## Safety Considerations

⚠️ **High Current System**: This controller switches high currents (15A+ per channel). Ensure:
- Adequate wire gauges for current capacity
- Proper fusing on power feeds
- Heat dissipation for BTS50010 switches
- Secure connections to prevent arcing

⚠️ **Electromagnetic Interference**: PWM switching at high currents can cause EMI:
- Use appropriate filtering capacitors
- Consider twisted pair wiring for sensitive signals  
- Test radio/electronic compatibility

⚠️ **Thermal Management**: Monitor operating temperatures:
- BTS50010 switches generate heat under load
- Ensure adequate airflow or heat sinking
- Consider PCB copper area for heat spreading

## License

MIT License - See main project LICENSE file for details.

© 2025 Chris Tacke