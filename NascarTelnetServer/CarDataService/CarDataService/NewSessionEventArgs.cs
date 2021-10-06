using System;

namespace CarDataService
{
    internal class NewSessionEventArgs: EventArgs
    {
        public NewSessionEventArgs(string message)
        {
            Message = message;
        }
        public string Message;
    }
}
