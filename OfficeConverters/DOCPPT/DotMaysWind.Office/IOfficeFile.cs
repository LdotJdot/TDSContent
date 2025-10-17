using System;
using System.Collections.Generic;

namespace DotMaysWind.Office
{
    public interface IOfficeFile
    {
        Dictionary<string, string> DocumentSummaryInformation { get; }

        Dictionary<string, string> SummaryInformation { get; }
    }
}