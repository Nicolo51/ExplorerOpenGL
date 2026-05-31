using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Interface
{
    public interface IUserControl
    {
        public string Description { get; set; } 
        public Func<IUserControl, AssertResult> Assert { get; set; }
        public object GetValueOfUC();
        public Type GetTypeOfUC();
        public string ToConfigFile(); 
        public void SetValueOfUC(object value);

    }
}
