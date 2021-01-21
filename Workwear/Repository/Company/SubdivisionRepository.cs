using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Stock;

namespace workwear.Repository.Company
{
	public class SubdivisionRepository
	{

		public IQueryOver<Subdivision,Subdivision> ActiveQuery (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Subdivision> ();
		}

		public Subdivision GetSubdivisionByCode(IUnitOfWork uow, string code)
		{
			return uow.Session.QueryOver<Subdivision>().Where(x => x.Code == code).SingleOrDefault();
		}


		public static IList<SubdivisionRecivedInfo> ItemsBalance(IUnitOfWork uow, Subdivision subdivision)
		{
			SubdivisionRecivedInfo resultAlias = null;

			SubdivisionIssueOperation subdivisionIssueOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType itemsTypeAlias = null;
			MeasurementUnits measurementUnitsAlias = null;
			SubdivisionPlace subdivisionPlaceAlias = null;

			IProjection projection = Projections.Sum(Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "IFNULL(?1, 0) - IFNULL(?2, 0)"),
				NHibernateUtil.Int32,
				Projections.Property<SubdivisionIssueOperation>(x => x.Issued),
				Projections.Property<SubdivisionIssueOperation>(x => x.Returned)
			));

			var incomeList = uow.Session.QueryOver<SubdivisionIssueOperation>(() => subdivisionIssueOperationAlias)
				.Where(x => x.Subdivision == subdivision)
				.JoinAlias(() => subdivisionIssueOperationAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Type, () => itemsTypeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => itemsTypeAlias.Units, () => measurementUnitsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => subdivisionIssueOperationAlias.SubdivisionPlace, () => subdivisionPlaceAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				   .SelectGroup(() => subdivisionIssueOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
				   .SelectGroup(() => subdivisionIssueOperationAlias.SubdivisionPlace.Id).WithAlias(() => resultAlias.PlaceId)
				   .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomeclatureName)
				   .Select(() => subdivisionPlaceAlias.Name).WithAlias(() => resultAlias.Place)
				   .SelectMax(() => subdivisionIssueOperationAlias.OperationTime).WithAlias(() => resultAlias.LastReceive)
				   .Select(projection).WithAlias(() => resultAlias.Amount)
				)
				.Where(Restrictions.Gt(projection, 0))
				.TransformUsing(Transformers.AliasToBean<SubdivisionRecivedInfo>())
				.List<SubdivisionRecivedInfo>();

			return incomeList;
		}
	}

	public class SubdivisionRecivedInfo
	{
		public int NomenclatureId { get; set; }

		public string NomeclatureName { get; set; }

		public string Units { get; set; }

		public decimal WearPercent { get; set; }

		public int PlaceId { get; set; }

		public string Place { get; set; }

		public DateTime LastReceive { get; set; }

		public int Amount { get; set; }
	}
}

