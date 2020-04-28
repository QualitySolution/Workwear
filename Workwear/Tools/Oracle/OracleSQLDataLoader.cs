using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using QS.DomainModel.UoW;
using QS.Project.Journal.DataLoader;

namespace workwear.Tools.Oracle
{
	public delegate void OracleSqlMakeQuery(OracleCommand cmd, bool isCountQuery, int? pageSize, int? skip);

	public class OracleSQLDataLoader<TNode> : IDataLoader
		where TNode : class
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly OracleConnection connection;

		public OracleSQLDataLoader(OracleConnection connection)
		{
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			cts = new CancellationTokenSource();
		}

		#region Events
		public event EventHandler ItemsListUpdated;
		public event EventHandler<LoadErrorEventArgs> LoadError;
		public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;
		public event EventHandler TotalCountChanged;

		public PostLoadProcessing PostLoadProcessingFunc { set; private get; }
		#endregion

		#region Работа с запросом.

		OracleSqlMakeQuery makeQuery;
		Func<OracleDataReader, TNode> mapFunc;

		/// <summary>
		/// Добавляем SQL запрос.
		/// </summary>
		/// <param name="makeQuery">Функция формирования запроса.</param>
		/// <exception cref="T:System.NotImplementedException"></exception>
		public void AddQuery(OracleSqlMakeQuery makeQuery, Func<OracleDataReader, TNode> mapFunc)
		{
			if(this.makeQuery != null || this.mapFunc != null)
				throw new NotImplementedException("Пока реализована работа только с одним запросом.");

			this.makeQuery = makeQuery;
			this.mapFunc = mapFunc;
		}

		#endregion

		public IList Items {
			get {
				lock(publishedNodesLock) {
					return publishedNodes;
				}
			}
		}

		#region Настройка работы
		public bool DynamicLoadingEnabled { get; set; } = true;
		public int PageSize { get; set; } = 100;
		#endregion

		public bool HasUnloadedItems { get; private set; } = true;

		public bool FirstPage { get; private set; } = true;

		public bool TotalCountingInProgress { get; private set; }

		public uint? TotalCount { get; private set; }

		private CancellationTokenSource cts;
		public void CancelLoading()
		{
			cts.Cancel();
		}

		public IEnumerable<object> GetNodes(int entityId, IUnitOfWork uow)
		{
			throw new NotImplementedException();
		}

		#region Вызов событий

		protected virtual void OnLoadError(Exception exception)
		{
			logger.Error(exception);
			var args = new LoadErrorEventArgs {
				Exception = exception
			};
			LoadError?.Invoke(this, args);
		}

		protected virtual void OnLoadingStateChange(LoadingState state)
		{
			var args = new LoadingStateChangedEventArgs {
				LoadingState = state
			};
			LoadingStateChanged?.Invoke(this, args);
		}

		#endregion

		#region Загрузка данных
		//Храним отдельно список узлов доступных снаружи и внутренний список с которым работает сам модуль.
		//Так как мы работает в отдельном потоке, снаружи всегда должен быть доступен предыдущий список. Пока мы обрабатываем новый запрос.
		private IList publishedNodes;
		private readonly List<TNode> readedNodes = new List<TNode>();
		private readonly object publishedNodesLock = new object();
		private Task RunningTask;

		private int reloadRequested = 0;
		private DateTime startLoading;

		bool loadInProgress;
		public bool LoadInProgress {
			get => loadInProgress;
			private set {
				loadInProgress = value;
				OnLoadingStateChange(value ? LoadingState.InProgress : LoadingState.Idle);
			}
		}

		public void LoadData(bool nextPage)
		{
			if(cts.IsCancellationRequested)
				cts = new CancellationTokenSource();

			Console.WriteLine($"LoadData({nextPage})");
			if(LoadInProgress) {
				if(!nextPage)
					Interlocked.Exchange(ref reloadRequested, 1);
				return;
			}

			LoadInProgress = true;
			LoadDataInternal(nextPage);
		}

		private void LoadDataInternal(bool nextPage)
		{
			startLoading = DateTime.Now;

			FirstPage = !nextPage;
			if(!nextPage) {
				readedNodes.Clear();
				TotalCount = null;
				TotalCountChanged?.Invoke(this, EventArgs.Empty);
				HasUnloadedItems = true;
			}

			logger.Info("Запрос данных...");
			RunningTask = Task.Factory.StartNew(ExecuteQuery, cts.Token)
			.ContinueWith((task) => {
				if(cts.IsCancellationRequested) {
					logger.Info($"Загрузка данных отменена");
					return;
				}
				LoadInProgress = false;
				if(task.IsFaulted)
					OnLoadError(task.Exception);

				if(reloadRequested == 0) {
					CopyNodesToPublish();
					ItemsListUpdated?.Invoke(this, EventArgs.Empty);
				}
				logger.Info($"Запрос выполнен за {(DateTime.Now - startLoading).TotalSeconds} сек.");
				if(1 == Interlocked.Exchange(ref reloadRequested, 0)) {
					LoadDataInternal(false);
				}
			}, cts.Token);
		}

		void ExecuteQuery()
		{
			if(makeQuery == null && mapFunc == null)
				return;

			var beforeCount = readedNodes.Count;
			var cmd = connection.CreateCommand();
			makeQuery(cmd, false, GetPageSize, beforeCount);
			logger.Debug($"Oracle query: {cmd.CommandText}");
			using(var reader = cmd.ExecuteReader()) {
				while(reader.Read()) {
					readedNodes.Add(mapFunc(reader));
				}
				HasUnloadedItems = (readedNodes.Count - beforeCount) == GetPageSize;
			}
			if(readedNodes.Count > beforeCount)
				PostLoadProcessingFunc?.Invoke(readedNodes, (uint)beforeCount);
		}
		#endregion

		#region Внутренние хелперы
		protected int? GetPageSize => DynamicLoadingEnabled ? PageSize : (int?)null;

		private void CopyNodesToPublish()
		{
			var copied = readedNodes.ToList();
			lock(publishedNodesLock) {
				publishedNodes = copied;
			}
		}
		#endregion

		#region Получение общего количества

		private Task[] CountingTasks = new Task[] { };
		private DateTime startCounting;
		private readonly object totalCountLock = new object();

		public void GetTotalCount()
		{
			if(TotalCountingInProgress)
				return;

			if(TotalCount.HasValue)
				return; //Незачем пресчитывать.
				
			if(cts.IsCancellationRequested)
				cts = new CancellationTokenSource();

			startCounting = DateTime.Now;
			TotalCountingInProgress = true;
			logger.Info("Запрос общего количества строк...");

			Task.Factory.StartNew(() => {
				var countRows = ExecuteCounting();
				lock(totalCountLock) {
					TotalCount = countRows;
				}
				if(cts.IsCancellationRequested) {
					logger.Info($"Загрузка общего количества строк отменена");
					return;
				}
				TotalCountChanged?.Invoke(this, EventArgs.Empty);
			}, cts.Token)
			.ContinueWith((tsk) => {
					TotalCountingInProgress = false;
					logger.Info($"{(DateTime.Now - startCounting).TotalSeconds} сек.");
					if(tsk.IsFaulted)
						OnLoadError(tsk.Exception);
				}
			);
		}

		uint ExecuteCounting()
		{
			if(makeQuery == null)
				return 0;
			
			var cmd = connection.CreateCommand();
			makeQuery(cmd, true, null, null);
			//Здесь почему то приходит Decemal
			var count = cmd.ExecuteScalar();
			return Convert.ToUInt32(count);
		}

		#endregion

		#region Публичный помощники

		public static string MakeSearchConditions(string[] searchText, string[] textColumns, string[] numberColumns)
		{
			if((searchText?.Length ?? 0) == 0 || ((textColumns?.Length ?? 0) == 0 && (numberColumns?.Length ?? 0) == 0))
				return null;

			string condition = null;
			foreach(var text in searchText) {
				condition += " AND (";
				List<string> list = textColumns.Select(c => $"UPPER({c}) LIKE '{text.ToUpper()}%'").ToList();
				if(long.TryParse(text, out long number) && numberColumns != null) {
					foreach(var numberColumn in numberColumns)
						list.Add($"{numberColumn} = {number}");
				}
				condition += String.Join(" OR ", list);
				condition += ")";
			}
			return condition;
		}

		#endregion
	}
}
