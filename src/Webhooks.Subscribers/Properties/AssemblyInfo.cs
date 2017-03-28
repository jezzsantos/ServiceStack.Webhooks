using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("ServiceStack.Webhooks.Subscribers")]
[assembly: AssemblyDescription("Add Webhooks to your ServiceStack services")]
[assembly: Guid("670bbdd7-0cd5-4049-ac74-83e97c20df38")]

#if !ASSEMBLYSIGNED

[assembly: InternalsVisibleTo("ServiceStack.Webhooks.Subscribers.UnitTests")]
[assembly: InternalsVisibleTo("ServiceStack.Webhooks.IntTests")]
#endif