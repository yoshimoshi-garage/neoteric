# Transfer Case Switch Converter

This purpose Arduino app, designed for the Nano, is to 
- Read a voltage input on `A0`
- Determine which "sense voltage" it is closest to
- Output a specific voltage on `D3` associated with that sensed voltage

The theory of operation is that you can use a 3-position resistive switch with a voltage divider on the input, with a regulated 5V input to produce well-known voltages for each switch position.  The application will then convert these switch positions into the output voltages required to drive a transfer case controller that takes in an analog voltage.

This could likely be used for transfer case controllers that require a PWM input as well, but that is untested.

The reference application uses a Dorman 601-604 transfer case switch as `R2` in a voltage divider, and a 470-ohm `R1`.

The switch/divider with an input of 5V then yields inputs on `A0` of:

| position | `R2` resistance | voltage to `A0` |
|-|-|-|
| 4L | 130 | 1.083 |
| 4H | 270 | 1.824 |
| 2H | 620 | 2.844 |

For outputs, the reference application drives an aftermarket controller that needs the following input voltage for a gear selection:

| gear | input voltage |
| - | - |
| 4L | 1.5V |
| 2H | 2V |
| 4H | 3V | 

## Wiring

### Input

The input uses a simple voltage divider to determine the resistance selected on the switch:

```
5V >----[R1 470-ohm]----+----[SWITCH]----< GND
                        |
                        |
                     Arduino A0
```

### Output

Arduinos don't have a DAC, so we use a PWM and an RC filter to create a desired output voltage.  The RC filter has a voltage drop, so the theoretical PWM voltage does not directly map to the output voltage.  The code has a "calibration" value to scale for that.

```
D3 >----[10k]----+-----< Filtered Output
                 |
               [10uF]
                 |
              (Ground)
```