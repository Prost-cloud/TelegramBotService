using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Models;
using TelegramBotService.DBContext;

namespace Interfaces
{
    public interface IDbProvider
    {
        public string AddProduct(string name, string cost, string messageId, string chatId);
        public string AddUpdate(string name, string cost, string messageId, string chatId);
        public string UpdateProduct(string name, string cost, string messageId, string chatId);
        public string DeleteProduct(string id, string messageId, string chatId);
        public string AddPayer(string name, string messageId, string userId);
        public string DeletePayer(string id, string messageId, string chatId);
        public string AddFunds(string payerId, string count, string messageId, string chatId);
        public string RemoveFunds(string payerId, string count, string messageId, string chatId);
        public string AddShoppingList(string name, string messageId, string chatId);
        public string DeleteShoppingList(string id, string messageId, string chatId);
        public string SelectShoppingList(string id, string messageId, string chatId);

        public string Show(string shoppingListId, string messageId, string chatId);

        public string AddUser(string chatId, string name);

        public string GetCountingByPayers(string chatIdAsString, string notUsed);
        public string GetShoppingLists(string chatIdAsString, string notUsed);

    }
    
}
