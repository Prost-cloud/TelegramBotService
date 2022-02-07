using System;

namespace Models
{
    public class Payer
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Payed { get; set; }
        public ShoppingList ShoppingList { get; set; }
        public bool IsDeleted { get; set; }

        public override string ToString()
        {
            return ID.ToString() + ". " + Name;
        }
    }
}
