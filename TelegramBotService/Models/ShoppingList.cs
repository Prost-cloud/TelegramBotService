using System;

namespace Models
{
	public class ShoppingList
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public Users Owner { get; set; }
		public bool Current { get; set; }
		public bool isDeleted { get; set; }

        public override string ToString()
        {
            return ID.ToString()+ ". " + Name;
        }
    }
}
