using MultiCloud.FileSharing.K8S.Interfaces;
using System;

namespace MultiCloud.FileSharing.K8S.Extensions
{
    public static class IValidatableExtensions
    {
        public static void ValidateArgument(this IValidatable argument, string argumentName)
        {
            if (string.IsNullOrEmpty(argumentName))
                throw new ArgumentException($"[{nameof(argumentName)}] is required.", nameof(argumentName));

            if (argument == null)
                throw new ArgumentNullException(argumentName);

            try
            {
                argument.Validate();
            }
            catch (InvalidOperationException ioEx)
            {
                throw new ArgumentException($"[{argumentName}] is invalid. See inner exception for details.", ioEx);
            }
        }
    }
}
