using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Simple.Data")]
[assembly: AssemblyDescription("Open source data library.")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a3becbf2-2e23-4399-a6d0-267e0e278ca6")]

[assembly: InternalsVisibleTo("Simple.Data.Ado")]
[assembly: InternalsVisibleTo("Simple.Data.AdapterApi")]
[assembly: InternalsVisibleTo("Simple.Data.TestHelper")]
[assembly: InternalsVisibleTo("Simple.Data.UnitTest")]
[assembly: InternalsVisibleTo("Simple.Data.IntegrationTest")]
[assembly: InternalsVisibleTo("Simple.Data.Mocking")]
[assembly: InternalsVisibleTo("Simple.Data.Mocking.Test")]

[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
[assembly: AllowPartiallyTrustedCallers]
