# NmmReader (Csharp-NmmReader)

C# library for reading files produced by the Nanopositioning and Nanomeasuring Machine (NNM)

## Overview

The Nanopositioning and Nanomeasuring Machine (NNM) by [SIOS GmbH](https://sios-de.com) is a versatile metrological instrument. It can perform various dimensional measurements like profile and surface scans with different sensors. Moreover, it can be operated as a µCMM for actual 3D point probing. All measurement modes are traceable due to three laser interferometers.

The system is usually operated by a custom software (nmmcontrol.exe) running in a MS Windows environment. The results are stored in a multitude of text files with proprietary data formats. Depending on the measurement mode (scan or 3D mode, respectively) a number of different files are produced. The nonstandard data format together with some bugs in the control software complicates the correct data input somewhat. This fact is reflected somehow in the structure of this library.

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

* `NmmScanData`
  This is the main class for reading surface scans. Class to handle topographic surface scan data (including all metadata) of the NMM.
 
* `TopographyData`
  Class to store and retreive the topographic surface scan data of the NMM.
 
* `ScanMetaData`
  A container class for metadata of a NMM scan file collection.
 
* `ScanColumnPredicate`
  A simple container class to hold the title and corresponding measurement unit for the data columns of NMM scan files.
 
* `Scan`
  A convienient container for some methods used to evaluate NMM scan files.
 
* `NmmIndFileParser`
   This Class consumes the index files (*.ind) produced during a scan on the SIOS NMM. The data is provided by properties only, there are no public methods.
 
* `NmmDatFileParser`
   Class to read the topographic surface scan data of the NMM. Data is read line per line from the (usually) two *.dat files.
 




### 3D mode specific classes

