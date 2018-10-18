using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Regulations;

namespace workwear.ViewModel
{
	public class NormVM : RepresentationModelEntityBase<Norm, NormVMNode>
	{
		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			NormVMNode resultAlias = null;

			Post professionAlias = null;
			Norm normAlias = null;
			RegulationDoc regulationDocAlias = null;
			RegulationDocAnnex docAnnexAlias = null;

			var norms = UoW.Session.QueryOver<Norm> (() => normAlias);

			var normsList = norms
				.JoinAlias(n => n.Document, () => regulationDocAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(n => n.Annex, () => docAnnexAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => normAlias.Professions, () => professionAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList (list => list
					.SelectGroup (() => normAlias.Id).WithAlias (() => resultAlias.Id)
	             	.Select (() => regulationDocAlias.Number).WithAlias (() => resultAlias.TonNumber)
				    .Select (() => docAnnexAlias.Number).WithAlias (() => resultAlias.AnnexNumber)
					.Select (() => normAlias.TONParagraph).WithAlias (() => resultAlias.TonParagraph)
					.Select (Projections.SqlFunction (
						new SQLFunctionTemplate (NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.Property (() => professionAlias.Name),
						Projections.Constant ("; "))
					).WithAlias (() => resultAlias.Professions)
				)
				.TransformUsing (Transformers.AliasToBean<NormVMNode> ())
				.List<NormVMNode> ();

			SetItemsSource (normsList.ToList ());
		}

		IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<NormVMNode>()
			.AddColumn ("Код").SetDataProperty (node => node.Id.ToString())
			.AddColumn ("№ ТОН").SetDataProperty (node => node.TonNumber)
		    .AddColumn ("№ Приложения").SetDataProperty (node => node.TonAttachment)
			.AddColumn ("№ Пункта").SetDataProperty (node => node.TonParagraph)
			.AddColumn ("Профессии").AddTextRenderer (node => node.Professions)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelEntityBase

		protected override bool NeedUpdateFunc (Norm updatedSubject)
		{
			return true;
		}

		#endregion

		public NormVM () : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			
		}

		public NormVM (IUnitOfWork uow) : base ()
		{
			this.UoW = uow;
		}
	}

	public class NormVMNode
	{
		public int Id { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string TonNumber { get; set; }

		public int? AnnexNumber { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string TonAttachment => AnnexNumber?.ToString();

		[UseForSearch]
		[SearchHighlight]
		public string TonParagraph { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Professions { get; set; }
	}
}

