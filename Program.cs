using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // Fields
        private List<IMyTerminalBlock> gridBlocks = null;
        private List<IMyTerminalBlock> blocksCanHoldIce = null;
        private List<IMyTerminalBlock> iceStorage = null;
        private List<IMyTerminalBlock> collectors = null;
        private MyItemType iceItemType = MyItemType.MakeOre("Ice");

        private List<IMyTerminalBlock> GetIceHolders()
        {
            List<IMyTerminalBlock> iceHolders = new List<IMyTerminalBlock>();

            foreach (IMyTerminalBlock block in gridBlocks)
            {
                if (block.HasInventory)
                {
                    for (int i = 0; i < block.InventoryCount; i++)
                    {
                        var inventory = block.GetInventory(i);
                        // Check inventory capacity
                        MyFixedPoint remainingCapacity = inventory.MaxVolume - inventory.CurrentVolume;
                        if (remainingCapacity > 0 && inventory.CanItemsBeAdded(1, iceItemType))
                        {
                            iceHolders.Add(block);
                        }
                    }
                }
            }
            return iceHolders;
        }

        private List<IMyTerminalBlock> GetIceStorage()
        {
            List<IMyTerminalBlock> temp = new List<IMyTerminalBlock>();

            if (blocksCanHoldIce != null)
            {
                foreach (IMyTerminalBlock block in blocksCanHoldIce)
                {
                    if (block.CustomName.Contains("ICE STORAGE"))
                    {
                        temp.Add(block);
                    }
                }
            }
            return temp;
        }

        private List<IMyTerminalBlock> GetCollectors()
        {
            List<IMyTerminalBlock> collectors = new List<IMyTerminalBlock>();
            
            foreach (IMyTerminalBlock block in gridBlocks)
            {
                if (block.CustomName.ToLower().Contains("collector"))
                {
                    collectors.Add(block);
                }
            }
            return collectors;
        }

        public Program()
        {
            // Fields
            gridBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(gridBlocks);
            blocksCanHoldIce = GetIceHolders();
            iceStorage = GetIceStorage();
            collectors = GetCollectors();

            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main()
        {
            List<bool> iceStorageFull = new List<bool>();
            bool collectorsOn = true;
            // Check each non-ice-storage inventory
            foreach (IMyTerminalBlock iceHolder in blocksCanHoldIce)
            {
                for (int i = 0; i < iceHolder.InventoryCount; i++)
                {
                    // Get holder inventory and its contents
                    List<MyInventoryItem> holderContents = new List<MyInventoryItem>();
                    var holderInventory = iceHolder.GetInventory(i);
                    holderInventory.GetItems(holderContents);

                    // Check for ice
                    foreach (MyInventoryItem item in holderContents)
                    {
                        // If the item is ice, then try to move it to the next available ice storage
                        if (item.Type == iceItemType)
                        {
                            foreach (IMyTerminalBlock storage in iceStorage)
                            {
                                var storageInventory = storage.GetInventory(i);
                                MyFixedPoint remainingCapacity = storageInventory.MaxVolume - storageInventory.CurrentVolume;
                                
                                if (remainingCapacity > 0 && storageInventory.CanItemsBeAdded(1, iceItemType) && !iceHolder.CustomName.Contains("Generator")) {
                                    storageInventory.TransferItemFrom(holderInventory, item);
                                    iceStorageFull.Add(false);
                                    break;
                                } 
                                else
                                {
                                    iceStorageFull.Add(true);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            // Check if storage is full
            bool isIceStorageFull = false;
            for (int i = 0; i < iceStorageFull.Count; i++)
            {
                if (i == 0)
                {
                    isIceStorageFull = iceStorageFull[i];
                }
                else
                {
                    // AND gate: if ALL ice storage is full, then we cannot fit any more ice.
                    isIceStorageFull &= iceStorageFull[i];
                }
            }

            // If ice storage is full, disable any and all connectors
            if (isIceStorageFull)
            {
                foreach (IMyTerminalBlock collector in collectors)
                {
                    collector.ApplyAction("OnOff_Off");
                    collectorsOn = false;
                }
            } 
            else
            {
                foreach (IMyTerminalBlock collector in collectors)
                {
                    collector.ApplyAction("OnOff_On");
                    collectorsOn = true;
                }
            }
            Echo($"Is ice storage full: {isIceStorageFull}\n");
            Echo($"Detected blocks that can hold ice: {blocksCanHoldIce.Count}\n");
            Echo($"Number of blocks delegated as ice storage: {iceStorage.Count}\n");
            Echo($"Number of collectors detected: {collectors.Count}\n");
            Echo($"Collectors power on status: {collectorsOn}");
        }
    }
}
