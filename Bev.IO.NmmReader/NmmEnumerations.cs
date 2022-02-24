namespace Bev.IO.NmmReader
{
    /// <summary>
    /// Air parameters source.
    /// </summary>
    public enum EnvironmentDataStatus
    {
        Unknown,        // this should not happen
        NoDataProvided, // file(s) not found or invalid, implies DefaultValues
        DefaultValues,  // files(s) contained only default values (NMM malfunction)
        MeasuredValues, // all parameters (including sample temperature) recorded
        SampleEstimatedbyAir    // air parameters recorded, sample temperature estimated by air temperature
    }

    /// <summary>
    /// Measurement procedures the NMM can perform.
    /// </summary>
    public enum MeasurementProcedure
    {
        NoFile,     // no *.dsc file found
        Unknown,    // 
        Scan,       // topography or profile by LFS or AFM
        Point3D,    // freeform 3D data (µCMM)
        Object3D    // graphic primitives 3D data (µCMM)
    }

    /// <summary>
    /// Processing types for retreving scan profiles.
    /// </summary>
    public enum TopographyProcessType
    {
        None,
        ForwardOnly,    // data for forward scan only
        BackwardOnly,   // data for backward scan only
        Average,        // average of forward and backward scan
        Difference      // backward minus forward
    }

    /// <summary>
    /// Direction of topography scan.
    /// </summary>
    public enum ScanDirection
    {
        Unknown,
        Forward,
        Backward
    }

    /// <summary>
    /// Scan direction status.
    /// </summary>
    public enum ScanDirectionStatus
    {
        Unknown,
        NoData,
        ForwardOnly,    // measurement files contained forward scan files only
        ForwardAndBackward, // measurement files contained forward and backward scan files
        ForwardAndBackwardJustified // backward scan files shows peculiar storage error
    }

}
