using System;
using QS.Permissions;
using QSProjectsLib;

namespace Workwear.Tools.Permissions {
	public class PermissionsService: ICurrentPermissionService {
		public IPermissionResult ValidateEntityPermission(Type entityType) {
			return new SimplePermissionResult {
				CanCreate = true,
				CanRead = true,
				CanUpdate = true,
				CanDelete = QSMain.User.Permissions["can_delete"]
			};
		}

		public bool ValidatePresetPermission(string permissionName) {
			throw new NotImplementedException();
		}
	}
}
