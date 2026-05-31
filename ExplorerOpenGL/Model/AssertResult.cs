using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model
{
    public class AssertResult
    {
        public bool Sucess { get; private set; }
        public string Message{ get; private set; }
        public AssertResult(bool sucess, string message = "")
        {
            Sucess = sucess;
            Message = message;
        }
    }
}
