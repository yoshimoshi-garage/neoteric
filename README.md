# neoteric: An open-source Automotive Control System for Classic Cars

This repository is a bin for all of my automotive electronics projects.  There are plans for sensor controllers and a CAN bus digital dashboard for them, but you'll also find individual controllers and projects for other problems.

## Build Tutorials

Some of these project have accompanying videos covering part or all of the design and build.  Take a look in the individual module readme, or [browse YouTube](https://www.youtube.com/playlist?list=PLhCiXHwyBtSG45CGrDBlqeXXfJQwoWFP4).

## Microcontroller and Programming Language

The choice of microcontroller for a given project depends largely on the requirements.  Typically I'll use one of the following:

Arduino/C: when boot time is important and complex logic isn't required.
[Meadow F7](https://www.wildernesslabs.co)/C#: when boot time isn't critical, or I need more features and complexity
Raspberry Pi/C#: when I need lots of compute power or need lots of logic/library support

## Modules

For lack of a better name, each project that creates hardware to solve a specific problem is called a "Module".  There is a folder for them, and that is currently divided by the general technology used for each.  Some have custom PCBs, some were wired and hand soldered on perf board.

### Existing Modules

| Name | Description |
| --- | --- |
| [6-channel solid-state switch.](modules\Arduino\glow-plug-controller\readme.md) | 6 channels, up to 20A each.  PWM controllable with current sensing.  Used as a glow plug controller, but could be used for a wide array of things |
| [Transfer case switch converter](modules\Arduino\t-case-switch-converter\readme.md) | Use a Dorman 601-604 (i.e. GM) switch to control a Ford transfer case | 
| [Transfer case controller](modules\Meadow\TransferCase\readme.md) | Control a BW4419 or a MP3023NQH in your custom build |


## Sensors

The following sensors are in development or planned:

Coolant Temperature
Fan Control Relay
System Voltage
Fuel Level
Oil Pressure
Air Temperature
Intake Manifold Air Pressure (MAP)

## Open Source

Everything here is open source.  I often am paid to design and build things, and those things still end up here.  If you pay me to design and build something, it's going to end up here.  We all benefit.

## Getting Hardware

If you see a module that you think might be useful in your project or build, contact me.  When I order PCBs, I always order extra, so I can likely assemble on for you.  Bear in mind, I did custom design, ordered parts in low quantity, and hand solder everything, so it is absolutely not going to be price competitive with Amazon or AliExpress. Anything beyond the most simple is going to be a couple hundred dollars.  They're also assembled to order, so depending on what I have going on and what you order, it coule take a week or more to order parts, assemble and test.

Anything popular enough that I've sold > 1 of, will be [available for ordering in my online store](https://www.yoshimaker.com/).