using System;
using System.Collections.Generic;

namespace CarrierPidgin.ServiceA
{
    public static class ExceptionProcessor
    {
        public static List<string> GetAllExceptionMessages(Exception ex)
        {
            if (ex.InnerException == null)
                return new List<string> {ex.Message};
            var otherErrors = GetAllExceptionMessages(ex.InnerException);
            otherErrors.Add(ex.Message);
            return otherErrors;
        }
    }
}