using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Models;
using TelegramBotService.DBContext;

namespace MethodProcessor
{
    public interface IMethodProcessor
    {
        public string AddProduct(string name, string cost);
        public string UpdateProduct(string name, string cost);
        public string DeleteProduct(string id);
        public string AddPayer(string name);
        public string DeletePayer(string id);
        public string AddFunds(string payerId, string count);
        public string RemoveFunds(string payerId, string count);
        public string AddShoppingList(string name);
        public string DeleteShoppingList(string id);
        public string SelectShoppingList(string id);

        public string Show(string shoppingListId);

        public string Start();

        public string GetCountingByPayers();
        public string GetShoppingLists();
        public void Dispose();
    }
}
