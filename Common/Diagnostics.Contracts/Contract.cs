using System;
using System.Diagnostics.Contracts;
using static System.Diagnostics.Contracts.Contract;

namespace Common.Diagnostics.Contracts
{
    public static class Contract
    {
        public static void Requires<TException>(bool condition) where TException : Exception, new()
        {
            if (!condition)
                throw new TException();

            EndContractBlock();
        }

        public static void Requires<TException>(bool condition, Func<TException> exceptionFactory) where TException : Exception, new()
        {
            if (!condition)
                throw exceptionFactory?.Invoke() ?? new TException();

            EndContractBlock();
        }

        [ContractInvariantMethod]
        public static void Invariant(bool condition, string message = null)
        {
            RequiresInternal(condition, GetInvalidOperationExceptionFactory(message));

            EndContractBlock();
        }

        [ContractArgumentValidator]
        public static void RequiresArgument(bool condition, string message = null, string paramName = null)
        {
            RequiresInternal(condition, GetArgumentExceptionFactory(message, paramName));

            EndContractBlock();
        }

        [ContractArgumentValidator]
        public static void RequiresArgumentRange(bool condition, string paramName, object actualValue, string message)
        {
            RequiresInternal(condition, () => new ArgumentOutOfRangeException(paramName, actualValue, message));

            EndContractBlock();
        }

        [ContractArgumentValidator]
        public static void RequiresArgumentRange(bool condition, string paramName = null, string message = null)
        {
            RequiresInternal(condition, GetArgumentOutOfRangeExceptionFactory(paramName, message));

            EndContractBlock();
        }

        [ContractArgumentValidator]
        public static void RequiresArgumentNotNull<T>(T value, string paramName = null, string message = null) where T : class
        {
            RequiresInternal(!(value is null), GetArgumentNullExceptionFactory(paramName, message));

            EndContractBlock();
        }

        [ContractArgumentValidator]
        public static void RequiresArgumentNotNull<T>(T? value, string paramName = null, string message = null) where T : struct
        {
            RequiresInternal(!(value is null), GetArgumentNullExceptionFactory(paramName, message));

            EndContractBlock();
        }

        private static void RequiresInternal(bool condition, Func<Exception> exceptionFactory)
        {
            if (!condition)
                throw exceptionFactory();
        }

        private static Func<InvalidOperationException> GetInvalidOperationExceptionFactory(string message)
        {
            if (message is null)
                return () => new InvalidOperationException();

            return () => new InvalidOperationException(message);
        }

        private static Func<ArgumentException> GetArgumentExceptionFactory(string message, string paramName)
        {
            if (message is null && paramName is null)
                return () => new ArgumentException();

            if (paramName is null)
                return () => new ArgumentException(message);

            return () => new ArgumentException(message, paramName);
        }

        private static Func<ArgumentOutOfRangeException> GetArgumentOutOfRangeExceptionFactory(string paramName, string message)
        {
            if (paramName is null && message is null)
                return () => new ArgumentOutOfRangeException();

            if (message is null)
                return () => new ArgumentOutOfRangeException(paramName);

            return () => new ArgumentOutOfRangeException(paramName, message);
        }

        private static Func<ArgumentNullException> GetArgumentNullExceptionFactory(string paramName, string message)
        {
            if (paramName is null && message is null)
                return () => new ArgumentNullException();

            if (message is null)
                return () => new ArgumentNullException(paramName);

            return () => new ArgumentNullException(paramName, message);
        }
    }
}
