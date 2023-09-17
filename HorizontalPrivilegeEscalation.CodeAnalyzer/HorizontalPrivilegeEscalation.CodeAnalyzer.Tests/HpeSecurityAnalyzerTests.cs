using System.Threading.Tasks;
using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        HorizontalPrivilegeEscalation.CodeAnalyzer.HpeSecurityAnalyzer>;

namespace HorizontalPrivilegeEscalation.CodeAnalyzer.Tests;

public class HpeSecurityAnalyzerTests
{
    [Fact]
    public async Task HpeSecurityAnalyzer_ForVulnerableTestMethod_AlertDiagnostic()
    {
        const string text = @"

using System.Threading.Tasks;

// Mimic Microsoft's Assembly
namespace Microsoft.AspNetCore.Mvc {
    public class ControllerBase {
    }
}

public class TestController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    public async Task VulnerableTestMethod()
    {
        await Task.Delay(1);
    }
}
            ";

        // Expecting a diagnostics report for VulnerableTestMethod
        var expected = Verifier.Diagnostic()
            .WithLocation(13, 23)
            .WithArguments("VulnerableTestMethod");
        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task HpeSecurityAnalyzer_ForSafeTestMethod_ShouldNotAlertDiagnostic()
    {
        const string text = @"

using System.Threading.Tasks;

// Mimic Microsoft's Assembly
namespace Microsoft.AspNetCore.Mvc {
    public class ControllerBase {
    }
}

public class TestController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    public async Task SafeTestMethod()
    {
        this.GetLoggedInUserId();
        await Task.Delay(1);
    }

    private int GetLoggedInUserId()
    {
        return 1;
    }
}
            ";

        // Expecting no diagnostics
        await Verifier.VerifyAnalyzerAsync(text);
    }
}