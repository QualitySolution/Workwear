using System;
using QS.Permissions;
using QSProjectsLib;

namespace Workwear.Tools.Permissions {
	public class PermissionsService: ICurrentPermissionService {
		private readonly BaseParameters baseParameters;

		public PermissionsService(BaseParameters baseParameters) {
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
		}

		public IPermissionResult ValidateEntityPermission(Type entityType, DateTime? documentDate = null) {
			var canEditByDate =
				documentDate == null || baseParameters.EditLockDate == null || documentDate >= baseParameters.EditLockDate.Value.AddDays(1);
			return new SimplePermissionResult {
				CanCreate = true,
				CanRead = true,
				CanUpdate = canEditByDate,
				CanDelete = QSMain.User.Permissions["can_delete"] && canEditByDate,
			};
		}

		public bool ValidatePresetPermission(string permissionName) {
			return QSMain.User.Permissions[permissionName];
		}
	}
}
