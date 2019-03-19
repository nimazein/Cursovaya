using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursovayaWF
{
    [Serializable]
    class ErrorMessage
    {
       public string Message
       {
            get;
            set;
       }
        public ErrorMessage()
        {
            
        }
        public ErrorMessage(string message)
        {
            this.Message = message;
        }
    }
}
