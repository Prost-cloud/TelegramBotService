using System;
using System.Collections.Generic;
using System.Text;
using Models;

namespace Interfaces
{
    public interface IDbProvider 
    {

        public User AddUser(string name, long chatId);
        public void AddPayer(Payer payer);
        public void AddFundsToUser(decimal count, Payer payer);
        public void AddProduct(Product product);
        public void AddShoppingList(ShoppingList shoppingList);
        public Payer GetPayerById(int payerId);
        public User GetUserByChatId(long chatId);
        public ShoppingList GetShoppingListById(int shoppingListId);
        public ShoppingList GetCurrentShoppingListByUser(User user);
        public List<ShoppingList> GetAllShoppingListNotDeletedByUser(User user);
        public List<Payer> GetAllPayerNotDeletedByShoppingList(ShoppingList shoppingList);
        public List<Product> GetAllProductNotDeletedByShoppingList(ShoppingList shoppingList);
        public Product GetProductByMessageId(int messageId);
        public Product GetProductById(int id);
        public void MakeShoppingListAsCurrent(ShoppingList shoppingList);
        public void MarkAsDeletePayer(Payer payer);
        public void MarkAsDeleteProduct(Product product);
        public void MarkAsDeleteShoppingList(ShoppingList shoppingList);
        public void UpdateProduct(Product product, string name, decimal newPrice);
        public void SaveChanges();
    }
}
