﻿using System;
using CarrierPidgin.Lib;
using NLog;

namespace CarrierPidgin.ServiceA
{
    public class DummyHandlerException: Exception {
        public DummyHandlerException(string failureMessage)
            :base(failureMessage)
        {}
    }

    public class WidgetizeWhenSomethingHappenedEventHandler
    {
        public WidgetizeWhenSomethingHappenedEventHandler()
        {
            _random = new Random();
        }

        public static double FailureProbability { get; } = 0.95;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
        private readonly Random _random;

        public void Handle(SomethingHappenedEvent msg)
        {
            Logger.Trace($"Widgetizing - Something happened: {msg.Description}");
            var randomDouble = _random.NextDouble();
            bool succeeds = randomDouble > FailureProbability;
            if (!succeeds)
            {
                var failureMessage = $"Randomized failure caused handler to throw exception. Probability value = {randomDouble}";
                Logger.Trace($"Random handler failure : {failureMessage}");
                throw new DummyHandlerException(failureMessage);
            }
        }
    }
}