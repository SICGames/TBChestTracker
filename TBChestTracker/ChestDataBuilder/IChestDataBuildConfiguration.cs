using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public interface IChestDataBuildConfiguration : IDisposable
    {
        String[] Files { get; }
        IProgress<BuildingChestsProgress> BuildingProgress { get; }
        bool hasTempFiles { get; }
    }
}
