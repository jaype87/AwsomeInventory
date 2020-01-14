using System;

namespace RPG_Inventory_Remake.RPGLoadout
{
	public enum LoadoutCountType : byte
	{
		pickupDrop, // Indicates that the goal is to keep LoadoutSlot.Count items in inventory.  Pickup to get there and drop excess.
		dropExcess  // Indicates that the goal is to ignore any items of this type until Count is reached, then drop excess.
	}
}