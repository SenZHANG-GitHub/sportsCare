using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sportsCare
{
    [Serializable]
    class DataUnitRSO
    {
        public int value11, value12, value21, value22;
        public int rSO2;

        public DataUnitRSO()
        {
            value11 = 0; value12 = 0; value21 = 0; value22 = 0;
        }
        public DataUnitRSO(int a)
        {
           rSO2=a;
        }
        public DataUnitRSO(int[] data)
        {
            value11 = data[0]; value12 = data[1]; value21 = data[2]; value22 = data[3];
        }

        public void addVal(int[] data)
        {
            value11 = data[0]; value12 = data[1]; value21 = data[2]; value22 = data[3];
        }

    }
}
