using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mRenamer.Model
{
    public enum StatusEnum
    {
        Ready = 1 << 0,
        Running = 1 << 1,
        Success = 1 << 2,
        Error= 1 << 3
    }
}
