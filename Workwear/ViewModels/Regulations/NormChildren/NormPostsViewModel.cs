using System;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Company;
using Workwear.ViewModels.Company;

namespace Workwear.ViewModels.Regulations.NormChildren {
	public class NormPostsViewModel : ViewModelBase {
		private readonly NormViewModel parent;
		private readonly INavigationManager navigation;

		public NormPostsViewModel(NormViewModel parent, INavigationManager navigation) {
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}

		#region Helpers

		Norm Entity => parent.Entity;
		IUnitOfWork UoW => parent.UoW;

		#endregion

		#region Свойства View

		public GenericObservableList<Post> Posts => Entity.ObservablePosts;

		#endregion
		
		bool showed = false;
		public void OnShow() {
			if(!showed) {
				showed = true;
				//Запрашиваем вложенные сущности для ускорения загрузка.
				Post postAlias = null;
				UoW.Session.QueryOver<Norm>()
					.Where(x => x.Id == Entity.Id)
					.Fetch(SelectMode.ChildFetch, x => x)
					.Fetch(SelectMode.JoinOnly, x => x.Posts)
					.JoinAlias(x => x.Posts, () => postAlias)
					.Fetch(SelectMode.Fetch, () => postAlias.Subdivision)
					.Fetch(SelectMode.Fetch, () => postAlias.Department)
					.List();
				OnPropertyChanged(nameof(Posts));
			}
		}
		
		#region Действия View

		public void NewProfession()
		{
			var page = navigation.OpenViewModel<PostViewModel, IEntityUoWBuilder>(parent, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);
			page.PageClosed += NewPost_PageClosed;
		}

		void NewPost_PageClosed(object sender, PageClosedEventArgs e)
		{
			if(e.CloseSource == CloseSource.Save) {
				var page = sender as IPage<PostViewModel>;
				var post = UoW.GetById<Post>(page.ViewModel.Entity.Id);
				Entity.AddPost(post);
			}
		}

		public void AddProfession()
		{
			var page = navigation.OpenViewModel<PostJournalViewModel>(parent, OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += Post_OnSelectResult;
		}

		void Post_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var postNode in e.SelectedObjects) {
				var post = UoW.GetById<Post>(postNode.GetId());
				Entity.AddPost(post);
			}
		}

		public void RemoveProfession(Post[] professions)
		{
			foreach(var prof in professions) {
				Entity.RemovePost(prof);
			}
		}
		#endregion
	}
}
