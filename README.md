# Csharp-NmmReader

C# library for reading files produced by the Nanopositioning and Nanomeasuring Machine (NNM)

## Overview

The Nanopositioning and Nanomeasuring Machine (NNM) by [SIOS GmbH](https://www.sios.de) is a versatile metrological instrument. It can perform various dimensional measurements like profile and surface scans with differnt sensors. Moreover it can be operated as a µCMM for actual 3D point probing. All measurement modes are traceable due to three laser interferometers.

The system is usually operated by a costum software (nmmcontrol.exe) running in a MS Windows environment. The results are stored in a multitude of text files with prprietary data formats. Depending on the measurement mode (scan or 3D mode, respectively) a number of different files are produced. The nonstandard data format together with some bugs in the control software complicates the correct data input somewhat. This fact is reflected somehow in the structure of this library.

The API of this library allows to handle files produced by either mode of measurement in a consistent way. 

## Library structure

The library is made of a lot of classes. Some classes are needed for both modi of operations, other are specific for the surface scan or 3D mode.

### Common classes

* `NmmFileName`
To be worked out.
 
* `NmmInstrumentCharacteristcs`
To be worked out.
 
* `NmmEnvironmentData`
To be worked out.
 
* `NmmDescriptionFileParser`
To be worked out.
 

### Scan specific classes

### 3D mode specific classes

