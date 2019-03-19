using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.Build.Tools
{
    [Guid(PackageGuid)]
    [PackageRegistration(AllowsBackgroundLoading = true, UseManagedResourcesOnly = true)]
    internal sealed class Package : AsyncPackage
    {
        public const string PackageGuid = "1d693cb7-c6f1-46b3-b707-dbbd6b5604f8";
    }
}
