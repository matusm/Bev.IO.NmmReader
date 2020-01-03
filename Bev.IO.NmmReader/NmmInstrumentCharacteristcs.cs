namespace Bev.IO.NmmReader
{
    public class NmmInstrumentCharacteristcs
    {
        public NmmInstrumentCharacteristcs()
        {
            SetDefaultCharacteristics();
        }


        public string User { get; private set; }
        public string OrganisationLong { get; private set; }
        public string Organisation { get; private set; }
        public string InstrumentManufacturer { get; private set; }
        public string InstrumentModel { get; private set; }
        public string InstrumentSerial { get; private set; }
        public string InstrumentVersion { get; private set; }
        public string EnvironmentMode { get; private set; }
        public string InstrumentIdentifier 
        { 
            get 
            { 
                return string.Format("{0} {1} {2} {3}", InstrumentManufacturer, InstrumentModel, InstrumentVersion, InstrumentSerial);
            } 
        }
        public string Institute
        {
            get 
            {
                return string.Format("{0} ({1})", OrganisationLong, Organisation);
            }
        }

        public void LoadCharacteristicFromFile(string fileName)
        {
            // TODO
        }

        private void SetDefaultCharacteristics()
        {
            User = "Michael Matus";
            OrganisationLong = "Bundesamt fuer Eich- und Vermessungswesen";
            Organisation = "BEV";
            InstrumentManufacturer = "SIOS";
            InstrumentModel = "NMM-1";
            InstrumentSerial = "PN11100209";
            InstrumentVersion = "0016";
            EnvironmentMode = "air";
        }

    }
}
