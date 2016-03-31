using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni.Command
{
    public interface ICommand
    {
        byte[] getBytes(int counter);
    }
}
