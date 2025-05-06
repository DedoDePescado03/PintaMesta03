using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintaMesta.Services
{
    public interface IOrientationService
    {
        void ForceLandscape();
        void AllowOrientations();
        void ForcePortrait();
    }
}
