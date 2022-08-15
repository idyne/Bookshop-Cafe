using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames
{
    [System.Serializable]
    public class PlayerData : Data
    {
        public int Version = 0;
        public int CurrentLevel = 1;
        public int Money = 60;
        public int MoneyInPile = 0;
        public bool IsCashierHired = false;
        public Dictionary<int, SlotData> BookShelfSlotData;
        public Dictionary<int, ItemHolderData> BookShelfData;
        public Dictionary<int, SlotData> VendingMachineSlotData;
        public Dictionary<int, ItemHolderData> VendingMachineData;
        public Dictionary<int, SlotData> TableSlotData;
        public Dictionary<int, SlotData> CarrierHiringSlotData;
        public Dictionary<int, SlotData> WaitressHiringSlotData;
        public Dictionary<int, SlotData> CashierHiringSlotData;
        public Dictionary<int, SlotData> PlayerLimitSlotData;
        public Dictionary<int, SlotData> PlayerSpeedSlotData;
        public Dictionary<int, SlotData> CarrierLimitSlotData;
        public Dictionary<int, SlotData> CarrierSpeedSlotData;
        public Dictionary<int, SlotData> WaitressLimitSlotData;
        public Dictionary<int, SlotData> WaitressSpeedSlotData;
        public Dictionary<int, SlotData> ExpandSlotData;
        public bool IsInitialized = false;
        public int BookshopLevel = 0;

        [System.Serializable]
        public class SlotData
        {
            public int ID { get; }
            public int Price;
            public SlotData(int id, int price)
            {
                ID = id;
                Price = price;
            }
        }

        [System.Serializable]
        public class ItemHolderData
        {
            public int ID { get; }
            public int NumberOfItems;

            public ItemHolderData(int id, int numberOfItems)
            {
                ID = id;
                NumberOfItems = numberOfItems;
            }
        }

        public PlayerData()
        {
            BookShelfSlotData = new Dictionary<int, SlotData>();
            BookShelfData = new Dictionary<int, ItemHolderData>();
            VendingMachineData = new Dictionary<int, ItemHolderData>();
            VendingMachineSlotData = new Dictionary<int, SlotData>();
            TableSlotData = new Dictionary<int, SlotData>();
            CarrierHiringSlotData = new Dictionary<int, SlotData>();
            WaitressHiringSlotData = new Dictionary<int, SlotData>();
            CashierHiringSlotData = new Dictionary<int, SlotData>();
            PlayerLimitSlotData = new Dictionary<int, SlotData>();
            PlayerSpeedSlotData = new Dictionary<int, SlotData>();
            CarrierLimitSlotData = new Dictionary<int, SlotData>();
            CarrierSpeedSlotData = new Dictionary<int, SlotData>();
            WaitressLimitSlotData = new Dictionary<int, SlotData>();
            WaitressSpeedSlotData = new Dictionary<int, SlotData>();
            ExpandSlotData = new Dictionary<int, SlotData>();
        }
    }

}
