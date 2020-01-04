using System;

namespace Bev.IO.NmmReader.scan_mode
{
    /// <summary>
    /// A simple container class to hold the title and corresponding measurement unit for the data columns of NNM scan files.
    /// </summary>
    public class ScanColumnPredicate
    {
        #region Ctor
        /// <summary>
        /// The constructor is the only way to set the three properties.
        /// </summary>
        /// <param name="colSymbol">The symbol of the data column.</param>
        /// <param name="title">The fully qualified title of the data column.</param>
        /// <param name="unit">The measurement unit symbol of the quantities in the data column.</param>
        public ScanColumnPredicate(string colSymbol, string title, string unit)
        {
            ColumnSymbol = colSymbol.Trim();
            ColumnTitle = title.Trim();
            UnitSymbol = unit.Trim();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the symbol for the data column.
        /// </summary>
        public string ColumnSymbol { get; }

        /// <summary>
        /// Gets the fully qualified title of the data column.
        /// </summary>
        public string ColumnTitle { get; }

        /// <summary>
        /// Gets the measurement unit symbol of the quantities in the data column.
        /// </summary>
        public string UnitSymbol { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Check if the ScanColumnPredicate object is of the given symbol type.
        /// </summary>
        /// <returns><c>true</c>, if ScanColumnPredicate is of this symbol, <c>false</c> otherwise.</returns>
        /// <param name="columnSymbol">Column symbol.</param>
        public bool IsOf(string columnSymbol)
        {
            if (String.Equals(columnSymbol, "height", StringComparison.OrdinalIgnoreCase))
                columnSymbol = "-LZ+AZ";
            if (String.Equals(columnSymbol, "AZ-LZ", StringComparison.OrdinalIgnoreCase))
                columnSymbol = "-LZ+AZ";
            if (String.Equals(columnSymbol, "XY", StringComparison.OrdinalIgnoreCase))
                columnSymbol = "XYvec";
            return string.Equals(ColumnSymbol, columnSymbol.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("[ScanColumnPredicate: ColumnSymbol={0}, ColumnTitle={1}, UnitSymbol={2}]", ColumnSymbol, ColumnTitle, UnitSymbol);
        }

    }
}
