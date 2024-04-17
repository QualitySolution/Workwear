using System;
using System.Threading.Tasks;
using Grpc.Core;
using NLog;
using QS.Cloud.Client;
using QS.Cloud.WearLk.Manage;

namespace QS.Cloud.WearLk.Client
{
	public class SpecCoinManagerService : WearLkServiceBase
	{
		public SpecCoinManagerService(ISessionInfoProvider sessionInfoProvider) : base(sessionInfoProvider)
		{
		}

		#region Запросы
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
			catch (RpcException prcEx) 
			{
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
			catch(RpcException rpcEx)
			{
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
		#endregion
	}
}
