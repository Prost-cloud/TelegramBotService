using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelegramBotService.DBContext
{
    public class DbProvider : IDbProvider
    {

        SqlLiteDBContext _sqlLiteDBContext;

        public DbProvider()
        {
            _sqlLiteDBContext = new SqlLiteDBContext();
            _sqlLiteDBContext.Database.EnsureCreatedAsync();
        }

        public void AddFundsToUser(decimal count, Payer payer)
        {
            // var payers = _sqlLiteDBContext.Payers;
            // payers.Where(x => x == payer).AsQueryable().ForEachAsync(x => x.Payed += count);

            payer.Payed += count;
        }

        public void AddPayer(Payer payer)
        {
            var payers = _sqlLiteDBContext.Payers;
            payers.Add(payer);

        }

        public void AddProduct(Product product)
        {
            var Products = _sqlLiteDBContext.Products;
            Products.Add(product);
        }

        public void AddShoppingList(ShoppingList shoppingList)
        {
            var shoppingLists = _sqlLiteDBContext.ShoppingList;
            shoppingLists.Add(shoppingList);
        }

        public User AddUser(string name, long chatId)
        {
            var users = _sqlLiteDBContext.Users;

            var newUser = new User()
            {
                Name = name,
                ChatID = chatId
            };

            users.Add(newUser);

            return newUser;
        }

        public List<Payer> GetAllPayerNotDeletedByShoppingList(ShoppingList shoppingList)
        {
            return _sqlLiteDBContext.Payers.Where(x => x.ShoppingList == shoppingList && !x.IsDeleted).ToList();
        }

        public List<ShoppingList> GetAllShoppingListNotDeletedByUser(User user)
        {
            return _sqlLiteDBContext.ShoppingList.Where(x => x.Owner == user && !x.IsDeleted).ToList();
        }

        public Payer GetPayerById(int payerId)
        {
            return _sqlLiteDBContext.Payers.Find(payerId);
        }

        public Product GetProductById(int id)
        {
            return _sqlLiteDBContext.Products.ToList().Where(x => x.ID == id).FirstOrDefault();
        }

        public Product GetProductByMessageId(int messageId)
        {
            return _sqlLiteDBContext.Products.Where(x => x.MessageID == messageId).FirstOrDefault();
        }

        public ShoppingList GetShoppingListById(int shoppingListId)
        {
            return _sqlLiteDBContext.ShoppingList.Find(shoppingListId);
        }

        public ShoppingList GetCurrentShoppingListByUser(User user)
        {
            return _sqlLiteDBContext.ShoppingList.Where(x => x.Owner == user && x.Current).FirstOrDefault();
        }

        public User GetUserByChatId(long chatId)
        {
            return _sqlLiteDBContext.Users.Where(x => x.ChatID == chatId).FirstOrDefault();
        }

        public void MakeShoppingListAsCurrent(ShoppingList shoppingList)
        {
            User currentUser = shoppingList.Owner;

            _sqlLiteDBContext.ShoppingList.Where(x => x.Owner == currentUser).AsQueryable()
                .ForEachAsync(x=>x.Current=false);

            shoppingList.Current = true;            
        }

        public void MarkAsDeletePayer(Payer payer)
        {
                payer.IsDeleted = true;
        }

        public void MarkAsDeleteProduct(Product product)
        {
                product.IsDeleted = true;
        }

        public void MarkAsDeleteShoppingList(ShoppingList shoppingList)
        {
                shoppingList.IsDeleted = true;
        }

        public void UpdateProduct(Product product, string name, decimal newPrice)
        {
          //  _sqlLiteDBContext.Products.Where(x => x == product)
          //      .AsQueryable()
          //      .ForEachAsync(x => { x.Name = name; x.Price = newPrice; });

            product.Name = name;
            product.Price = newPrice;
          // _sqlLiteDBContext.upda
        }

        public List<Product> GetAllProductNotDeletedByShoppingList(ShoppingList shoppingList)
        {
            return _sqlLiteDBContext.Products.Where(x => x.ShoppingList == shoppingList && !x.IsDeleted).ToList();
        }

        public void SaveChanges()
        {
                _sqlLiteDBContext.SaveChanges();
        }
    }
}
