using System;

namespace Models
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int MessageID { get; set; }
        public ShoppingList ShoppingList { get; set; }
        public bool IsDeleted { get; set; }

        public override string ToString()
        {
            return ID.ToString() + " " + Name;
        }
    }
}