using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome.BaseVM
{
    public interface ICloseable
    {
        event EventHandler<bool> CloseRequest;
    }
}
