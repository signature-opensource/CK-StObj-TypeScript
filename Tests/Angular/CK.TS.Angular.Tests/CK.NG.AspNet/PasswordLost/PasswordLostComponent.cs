using CK.StObj.TypeScript;
using CK.TS.Angular;

namespace CK.NG.AspNet.Auth;

[TypeScriptResourceFiles]
[RoutedComponent( typeof( LoginComponent ), "password-lost", RouteRegistrationMode.Lazy, AsChildOf = "kilo/papa" )]
public class PasswordLostComponent : RoutedComponent
{
}

