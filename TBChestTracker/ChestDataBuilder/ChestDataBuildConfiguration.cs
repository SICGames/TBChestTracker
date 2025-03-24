using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ChestDataBuildConfiguration : IChestDataBuildConfiguration, IDisposable
    {
        private string[] _files;
        private IProgress<BuildingChestsProgress> _progress;

        public string[] Files => _files;
        public IProgress<BuildingChestsProgress> BuildingProgress => _progress;

        readonly bool _hasTempFiles;
        public bool hasTempFiles => _hasTempFiles;

        public ChestDataBuildConfiguration(string[] files, IProgress<BuildingChestsProgress> progress, bool hastempfiles = false) 
        { 
            _files = files;
            _progress = progress;
            _hasTempFiles = hastempfiles;
        }

        public IChestDataBuilder CreateBuilder()
        {
            if (_hasTempFiles)
            {
                return new TempChestDataBuilder(this);
            }
            
            return new ChestDataBuilder(this);
        }

        public void Dispose()
        {
            _files = null;
            _progress = null;
        }
    }
}
