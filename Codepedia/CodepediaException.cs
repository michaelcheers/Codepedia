using System;

namespace Codepedia
{
    public class CodepediaException : Exception
    {
        public CodepediaException (string message) : base(message) { }
    }
}
