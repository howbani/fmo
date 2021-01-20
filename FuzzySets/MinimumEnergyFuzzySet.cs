using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.FuzzySets
{
    public class MinimumEnergyFuzzySet
    {
        double delta;
        double tx;
        double maxTx;
        public MinimumEnergyFuzzySet(double _delta, double _tx, double _maxTx) 
        {
            delta = _delta;
            tx = _tx;
            maxTx = _maxTx;
        }
        public double Ume
        {
            get 
            {
                double ume;
                ume = 1 + ((delta - 1) * tx) / (maxTx);
                return ume;
            }
        }
    }
}
