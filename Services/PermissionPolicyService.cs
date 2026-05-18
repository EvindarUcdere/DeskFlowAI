using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class PermissionPolicyService
{
    public IReadOnlyCollection<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            RoleNames.Admin => AllAdminPermissions,
            RoleNames.Manager => ManagerPermissions,
            RoleNames.Staff => StaffPermissions,
            _ => []
        };
    }

    private static readonly string[] AllAdminPermissions =
    [
        PermissionNames.CustomerCreate,
        PermissionNames.CustomerUpdate,
        PermissionNames.CustomerDelete,
        PermissionNames.ProjectCreate,
        PermissionNames.ProjectUpdate,
        PermissionNames.ProjectNotifyTeam,
        PermissionNames.TaskCreate,
        PermissionNames.TaskUpdate,
        PermissionNames.EmployeeManage,
        PermissionNames.UserManage,
        PermissionNames.DocumentCreate,
        PermissionNames.DocumentUpdate,
        PermissionNames.DocumentAnalyze,
        PermissionNames.DocumentApproveExternalAI,
        PermissionNames.DocumentReviewAI
    ];

    private static readonly string[] ManagerPermissions =
    [
        PermissionNames.CustomerCreate,
        PermissionNames.CustomerUpdate,
        PermissionNames.ProjectCreate,
        PermissionNames.ProjectUpdate,
        PermissionNames.ProjectNotifyTeam,
        PermissionNames.TaskCreate,
        PermissionNames.TaskUpdate,
        PermissionNames.EmployeeManage,
        PermissionNames.DocumentCreate,
        PermissionNames.DocumentUpdate,
        PermissionNames.DocumentAnalyze,
        PermissionNames.DocumentApproveExternalAI,
        PermissionNames.DocumentReviewAI
    ];

    private static readonly string[] StaffPermissions =
    [
        PermissionNames.TaskCreate,
        PermissionNames.TaskUpdate
    ];
}
