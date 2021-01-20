using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.FuzzySets
{
    public class LifetimeFuzzySet
    {
        double initEnergy;
        double alpha;
        double gamma;
        double tx;
        double re;
        public LifetimeFuzzySet(double _initEnergy, double _alpha, double _gamma, double _tx,double _re) 
        {
            initEnergy = _initEnergy;
            alpha = _alpha;
            gamma = _gamma;
            tx = _tx;
            re = _re;
        }
        public double Ult 
        {
            get 
            {
                double ult;
                if (re > alpha * initEnergy && re <= initEnergy)
                {
                    ult = 1 - ((1 - gamma) / (1 - alpha)) * (1 - (re / initEnergy));
                }
                else if (re > tx && re < alpha * initEnergy)
                {
                    ult = ((gamma) / (initEnergy * alpha - tx)) * (re - tx);
                }
                else 
                {
                    ult = 0.0;
                }
                return ult;
            }
        }
    }
}
