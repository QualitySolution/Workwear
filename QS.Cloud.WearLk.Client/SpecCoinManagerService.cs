using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using QS.Cloud.Client;
using QS.Cloud.WearLk.Manage;

namespace QS.Cloud.WearLk.Client
{
	public class SpecCoinManagerService : WearLkServiceBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public SpecCoinManagerService(ISessionInfoProvider sessionInfoProvider) : base(sessionInfoProvider)
		{
		}

		#region Запросы
		public IList<UserBalance> GetListBalances(CancellationToken token)
		{
			SpecCoinsManager.SpecCoinsManagerClient client = new SpecCoinsManager.SpecCoinsManagerClient(Channel);
			GetListBalancesRequest request = new GetListBalancesRequest();
			GetListBalancesResponse response = client.GetListBalances(request, Headers, cancellationToken: token);
			return response.Balances;
		}
		
		public int GetCoinsBalance(string employeePhone)
		{
			try 
			{
				if(string.IsNullOrWhiteSpace(employeePhone))
				{
					throw new ArgumentNullException(nameof(employeePhone));
				}

				SpecCoinsManager.SpecCoinsManagerClient client = new SpecCoinsManager.SpecCoinsManagerClient(Channel);
				GetCoinsBalanceRequest request = new GetCoinsBalanceRequest() 
				{
					Phone = employeePhone
				};

				GetCoinsBalanceResponse response = client.GetCoinsBalance(request, Headers);
				return response.Balance;
			}
			catch (RpcException prcEx) {
				logger.Error(prcEx, "Error while getting coins balance");
				return -1;
			}
		}

		public async Task<int> GetCoinsBalanceAsync(string employeePhone)
		{
			try 
			{
				if(string.IsNullOrWhiteSpace(employeePhone))
				{
					throw new ArgumentNullException(nameof(employeePhone));
				}

				SpecCoinsManager.SpecCoinsManagerClient client = new SpecCoinsManager.SpecCoinsManagerClient(Channel);
				GetCoinsBalanceRequest request = new GetCoinsBalanceRequest()
				{
					Phone = employeePhone
				};

				GetCoinsBalanceResponse response = await client.GetCoinsBalanceAsync(request, Headers)
					.ConfigureAwait(false);
				return response.Balance;
			}
			catch(RpcException rpcEx) {
				logger.Error(rpcEx, "Error while getting coins balance");
				return -1;
			}
		}

		public string DeductCoins(string employeePhone, int amount, string description)
		{
			try 
			{
				if(string.IsNullOrWhiteSpace(employeePhone))
				{
					throw new ArgumentNullException(nameof(employeePhone));
				}

				if(amount < 0) 
				{
					throw new ArgumentException(nameof(amount));
				}

				if(string.IsNullOrWhiteSpace(description)) 
				{
					throw new ArgumentNullException(nameof(description));
				}

				SpecCoinsManager.SpecCoinsManagerClient client = new SpecCoinsManager.SpecCoinsManagerClient(Channel);
				CreateCoinsOperationRequest request = new CreateCoinsOperationRequest() 
				{
					Amount = amount,
					Description = description,
					Phone = employeePhone
				};

				CreateCoinsOperationResponse response = client.CreateCoinsOperation(request, Headers);
				return response.Result;
			}
			catch(RpcException rpcEx)
			{
				return rpcEx.Status.Detail;
			}
		}
		
		public async Task<string> DeductCoinsAsync(string employeePhone, int amount, string description) 
		{
			try
			{
				if(string.IsNullOrWhiteSpace(employeePhone)) 
				{
					throw new ArgumentNullException(nameof(employeePhone));
				}

				if(amount < 0) 
				{
					throw new ArgumentException(nameof(amount));
				}

				if(string.IsNullOrWhiteSpace(description)) 
				{
					throw new ArgumentNullException(nameof(description));
				}

				SpecCoinsManager.SpecCoinsManagerClient client = new SpecCoinsManager.SpecCoinsManagerClient(Channel);
				CreateCoinsOperationRequest request = new CreateCoinsOperationRequest() 
				{
					Amount = amount,
					Description = description,
					Phone = employeePhone
				};

				CreateCoinsOperationResponse response = await client.CreateCoinsOperationAsync(request, Headers)
					.ConfigureAwait(false);
				return response.Result;
			}
			catch(RpcException rpcEx)
			{
				return rpcEx.Status.Detail;
			}
		}
		
		public IList<CoinsOperation> GetCoinsOperations(string employeePhone, CancellationToken token)
		{
			if(string.IsNullOrWhiteSpace(employeePhone))
				throw new ArgumentNullException(nameof(employeePhone));

			SpecCoinsManager.SpecCoinsManagerClient client = new SpecCoinsManager.SpecCoinsManagerClient(Channel);
			GetCoinsOperationsRequest request = new GetCoinsOperationsRequest {
				Phone = employeePhone
			};

			GetCoinsOperationsResponse response = client.GetCoinsOperations(request, Headers, cancellationToken: token);
			return response.Operations;
		}
		#endregion
	}
}
