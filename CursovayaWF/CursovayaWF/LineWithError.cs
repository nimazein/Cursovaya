using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursovayaWF
{ 
    [Serializable]
    class LineWithError
    {
        public string Content
        {
            get;
            set;
        }
        public string ErrorMessage
        {
            get;
            set;
        }

        public LineWithError()
        {

        }
        public LineWithError(string content)
        {
            this.Content = content;       
        }
        public LineWithError(string content, string errorMessage)
        {
            this.Content = content;
            this.ErrorMessage = errorMessage;
        }


    }
}
