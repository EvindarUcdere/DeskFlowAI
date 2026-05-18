using DeskFlowAI.Models;
using DeskFlowAI.Services;

namespace DeskFlowAI.Tests;

internal static class Program
{
    private static int Main()
    {
        List<(string Name, Action Test)> tests =
        [
            ("Admin permissions include user management and AI review", PermissionTests.AdminPermissionsIncludeSensitiveActions),
            ("Manager permissions exclude user management but include AI approval", PermissionTests.ManagerPermissionsMatchOperationsRole),
            ("Staff permissions are limited to task workflow", PermissionTests.StaffPermissionsAreTaskScoped),
            ("Mock AI detects high risk keywords", MockAITests.MockAIDetectsHighRisk),
            ("Mock AI detects medium risk keywords", MockAITests.MockAIDetectsMediumRisk),
            ("Mock AI keeps controlled demo mode out of fallback", MockAITests.MockAIIsNotFallback)
        ];

        int passed = 0;

        foreach ((string name, Action test) in tests)
        {
            try
            {
                test();
                Console.WriteLine($"PASS {name}");
                passed++;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"FAIL {name}");
                Console.WriteLine(exception.Message);
                return 1;
            }
        }

        Console.WriteLine($"{passed}/{tests.Count} tests passed.");
        return 0;
    }
}

internal static class PermissionTests
{
    public static void AdminPermissionsIncludeSensitiveActions()
    {
        IReadOnlyCollection<string> permissions = new PermissionPolicyService().GetPermissionsForRole(RoleNames.Admin);

        TestAssert.Contains(permissions, PermissionNames.UserManage);
        TestAssert.Contains(permissions, PermissionNames.CustomerDelete);
        TestAssert.Contains(permissions, PermissionNames.DocumentReviewAI);
    }

    public static void ManagerPermissionsMatchOperationsRole()
    {
        IReadOnlyCollection<string> permissions = new PermissionPolicyService().GetPermissionsForRole(RoleNames.Manager);

        TestAssert.Contains(permissions, PermissionNames.ProjectNotifyTeam);
        TestAssert.Contains(permissions, PermissionNames.DocumentApproveExternalAI);
        TestAssert.DoesNotContain(permissions, PermissionNames.UserManage);
        TestAssert.DoesNotContain(permissions, PermissionNames.CustomerDelete);
    }

    public static void StaffPermissionsAreTaskScoped()
    {
        IReadOnlyCollection<string> permissions = new PermissionPolicyService().GetPermissionsForRole(RoleNames.Staff);

        TestAssert.Contains(permissions, PermissionNames.TaskCreate);
        TestAssert.Contains(permissions, PermissionNames.TaskUpdate);
        TestAssert.DoesNotContain(permissions, PermissionNames.DocumentAnalyze);
        TestAssert.DoesNotContain(permissions, PermissionNames.ProjectUpdate);
    }
}

internal static class MockAITests
{
    public static void MockAIDetectsHighRisk()
    {
        DocumentAIAnalysisResult result = Analyze("delivery is overdue and missing approval with penalty risk");

        TestAssert.Equal("High", result.RiskLevel);
        TestAssert.Equal(DocumentAIProviderNames.MockAI, result.ProviderName);
    }

    public static void MockAIDetectsMediumRisk()
    {
        DocumentAIAnalysisResult result = Analyze("refund approval is pending because of a dependency");

        TestAssert.Equal("Medium", result.RiskLevel);
        TestAssert.Equal(AIComplianceStatusNames.ReviewRequired, result.ComplianceStatus);
    }

    public static void MockAIIsNotFallback()
    {
        DocumentAIAnalysisResult result = Analyze("normal handoff document with no configured risk keyword");

        TestAssert.Equal(DocumentAIProviderNames.MockAI, result.ProviderName);
        TestAssert.False(result.UsedFallback, "MockAI must not be marked as fallback.");
    }

    private static DocumentAIAnalysisResult Analyze(string extractedText)
    {
        ProjectDocument document = new(
            projectId: 1,
            fileName: "demo.txt",
            filePath: @"C:\demo\demo.txt",
            status: DocumentStatusNames.Uploaded,
            uploadedByEmail: "admin@deskflow.ai",
            notes: string.Empty);

        document.UpdateAIProcessingPolicy(DocumentAIProcessingPolicyNames.ExternalAIAllowed);
        document.UpdateTextExtraction(DocumentTextExtractionStatusNames.Extracted, extractedText, DateTime.Now);

        return new MockDocumentAIAnalysisProvider().Analyze(document);
    }
}

internal static class TestAssert
{
    public static void Contains(IReadOnlyCollection<string> values, string expected)
    {
        if (!values.Contains(expected))
        {
            throw new InvalidOperationException($"Expected permission '{expected}' was not found.");
        }
    }

    public static void DoesNotContain(IReadOnlyCollection<string> values, string unexpected)
    {
        if (values.Contains(unexpected))
        {
            throw new InvalidOperationException($"Unexpected permission '{unexpected}' was found.");
        }
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
        }
    }

    public static void False(bool value, string message)
    {
        if (value)
        {
            throw new InvalidOperationException(message);
        }
    }
}
