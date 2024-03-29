﻿using System;
using QS.Utilities.Text;

namespace Workwear.Models.Import
{
	public class FIO : IEquatable<FIO> {
		public object UserData;
		public string LastName;
		public string FirstName;
		public string Patronymic;

		public string FullName => PersonHelper.PersonFullName(LastName, FirstName, Patronymic);
		public string ShortName => PersonHelper.PersonNameWithInitials(LastName, FirstName, Patronymic);
		
		public string GetHash() => LastName + FirstName + Patronymic;

		public bool IsEmpty => String.IsNullOrWhiteSpace(LastName) && String.IsNullOrWhiteSpace(FirstName) &&
		                       String.IsNullOrWhiteSpace(Patronymic);

		public bool Equals(FIO other) {
			return LastName == other.LastName && FirstName == other.FirstName && Patronymic == other.Patronymic;
		}

		public override int GetHashCode() {
			return GetHash().GetHashCode();
		}
	}
}
