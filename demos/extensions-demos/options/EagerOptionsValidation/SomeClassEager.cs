﻿using Microsoft.Extensions.Options;

namespace EagerOptionsValidation
{
    public interface ISomeClassEager
    {
        string GetMessage();
    }

    public class SomeClassEager : ISomeClassEager
    {
        private readonly IOptions<MyOptionsEager> _myOptions;

        public SomeClassEager(IOptions<MyOptionsEager> myOptions)
        {
            _myOptions = myOptions;
        }

        public string GetMessage()
        {
            return $"appsettings says: {_myOptions.Value.SomeOption}";
        }
    }
}
