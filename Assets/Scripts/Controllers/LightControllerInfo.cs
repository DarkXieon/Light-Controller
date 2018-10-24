using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightControls.ControlOptions;

namespace LightControls.Controllers
{
    [Serializable]
    public class LightControlInfo
    {
        public ControlOptionInfo ControlOptionInfo;
        public LightControlOption[] LightControlOptions;

        public LightControlInfo()
        {
            ControlOptionInfo = new ControlOptionInfo();

            LightControlOptions = new LightControlOption[0];
        }
    }
}
