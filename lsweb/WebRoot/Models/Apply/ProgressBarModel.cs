using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Apply
{
    public class ProgressBarModel
    {
        public ProgressBarModel()
        {
        }

        public ProgressBarModel(int step)
        {
            this.Step = step;
        }

        public int Step { get; set; }
        public int CompletedPercent
        {
            get
            {
                switch (Step)
                {
                    case 1:
                        return 25;
                    case 2:
                        return 50;
                    case 3:
                        return 75;
                    case 4:
                        return 100;
                    default:
                        return 0;
                }
            }
        }
    }
}