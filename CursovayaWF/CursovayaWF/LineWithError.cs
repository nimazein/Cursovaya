using System;

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
        public LineWithError(string content, string errorMessage)
        {
            this.Content = content;
            this.ErrorMessage = errorMessage;
        }
    }
}
