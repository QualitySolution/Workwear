﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "штрихкоды",
		Nominative = "штрихкод",
		Genitive = "штрихкода"
		)]
	[HistoryTrace]
	public class Barcode : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; }

		private DateTime createDate = DateTime.Today;
		[Display(Name = "Дата создания")]
		public virtual DateTime CreateDate {
			get => createDate;
			set => SetField(ref createDate, value);
		}
		
		private string title;
		[Display (Name = "Значение")]
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}
		
		private Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}
		
		private Size size;
		[Display(Name = "Размер")]
		public virtual Size Size {
			get => size;
			set => SetField(ref size, value);
		}
		
		private Size height;
		[Display(Name = "Рост одежды")]
		public virtual Size Height {
			get => height;
			set => SetField(ref height, value);
		}
		
		private string label;
		[Display(Name = "Название")]
		[StringLength(50)]
		public virtual string Label 
		{
			get => label;
			set => SetField(ref label, value, () => Label);
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		#region Операции

		private Dictionary<DateTime, BarcodeOperation> operationsWithDates;
		private IList<BarcodeOperation> barcodeOperations = new List<BarcodeOperation>();
		[Display(Name = "Операции")]
		public virtual IList<BarcodeOperation> BarcodeOperations {
			get => barcodeOperations;
			set {
				if(SetField(ref barcodeOperations, value))
					operationsWithDates = BarcodeOperations.ToDictionary(o => o.OperationDate ?? DateTime.MinValue);
			}
		}
		
////1289 проверить коментарий
		//Предворительно нужно загрузиоть все BarcodeOperation и связанные с ними операции иначе будет много запросов в базу
		public virtual BarcodeOperation LastOperation => operationsWithDates[LastOperationTime];
		public virtual DateTime LastOperationTime => operationsWithDates.Keys.Max();

		#endregion
	}
}
