using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Simple.Data.SqlCe40")]
[assembly: AssemblyDescription("SQL Server CE 4.0 add-in for ADO adapter.")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2aa66263-94ee-46d5-bb0f-bae22bcec3e8")]

[assembly: InternalsVisibleTo("Simple.Data.SqlCe40Test")]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
[assembly: AllowPartiallyTrustedCallers]
