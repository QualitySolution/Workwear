using System;
using RglibInterop;

namespace workwear.Tools.IdentityCards
{
	public class CardStateEventArgs : EventArgs
	{
		RG_PIN_SATETS_16 pinStates;
		RG_DEVICE_STATUS_TYPE statusType;
		RG_CARD_INFO cardInfo;
		RG_CARD_MEMORY cardMemory;

		public CardStateEventArgs(RG_PIN_SATETS_16 pinStates, RG_DEVICE_STATUS_TYPE statusType, RG_CARD_INFO cardInfo, RG_CARD_MEMORY cardMemory)
		{
			this.pinStates = pinStates;
			this.statusType = statusType;
			this.cardInfo = cardInfo;
			this.cardMemory = cardMemory;
		}

		#region Обработанные
		public bool ReadBad => pinStates.Pin00;
		public string CardCode => statusType != RG_DEVICE_STATUS_TYPE.DS_NOCARD ? ((byte)cardInfo.CardType).ToString("X2") : String.Empty;
		public RG_CARD_TYPE_CODE CardType => cardInfo.CardType;
		public string CardUid => statusType != RG_DEVICE_STATUS_TYPE.DS_NOCARD ? BitConverter.ToString(cardInfo.CardUid) : String.Empty;
		public string MemoryData => statusType == RG_DEVICE_STATUS_TYPE.DS_CARDAUTH ? BitConverter.ToString(cardMemory.MemBlock) : String.Empty;
		#endregion
	}
}
