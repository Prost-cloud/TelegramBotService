using System;

namespace Models
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public long ChatID { get; set; }

        public override string ToString()
        {
            return ID.ToString() + " " + Name;
        }
    }
}
