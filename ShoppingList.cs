using System;

namespace Models
{
	public class ShoppingList
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public Users Owner { get; set; }
	}
}
