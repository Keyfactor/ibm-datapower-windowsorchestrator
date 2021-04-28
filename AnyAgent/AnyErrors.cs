using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPower
{
    internal struct AnyErrors
    {
        public bool HasError{ get; set; }

        public string ErrorMessage { get;set; }
    }
}
