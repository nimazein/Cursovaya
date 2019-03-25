using System;

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
        public ErrorMessage(string message)
        {
            this.Message = message;
        }
    }
}
