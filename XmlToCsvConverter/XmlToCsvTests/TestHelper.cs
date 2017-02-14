using System;
using NUnit.Framework;

namespace XmlToCsvTests
{
    public static class TestHelper
    {
        public static void Throws<TException>(Action action, string message)
            where TException : Exception
        {
            try
            {
                action();
                Assert.Fail("Exception of type {0} expected, but no exception was thrown", typeof(TException).Name);
            }
            catch (TException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception of type {0} expected but instead got an exception of type {1}", typeof(TException).Name, ex.GetType().Name);
            }
        }
    }
}
