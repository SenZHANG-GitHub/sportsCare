using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace sportsCare
{
    [Serializable]
    class SaveUnit
    {
        public ArrayList dataList ;
        public ArrayList pulList ;

        public SaveUnit(ArrayList a, ArrayList b)
        {
            dataList = a;
            pulList = b;
        }
     
    }
}