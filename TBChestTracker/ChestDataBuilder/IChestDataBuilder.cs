using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TBChestTracker
{
    public interface IChestDataBuilder : IDisposable
    {
        IChestDataBuildConfiguration BuildConfiguration { get; }
        Task Build();
    }
}
