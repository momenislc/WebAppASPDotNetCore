﻿using DutchTreat.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DutchTreat.Data
{
    public class DutchSeeder
    {
        private readonly DutchContext _ctx;
        private readonly IHostingEnvironment _hosting;
        private readonly UserManager<StoreUser> _userManager;

        public DutchSeeder(DutchContext ctx, IHostingEnvironment hosting, UserManager<StoreUser> userManager)
        {
            _ctx = ctx;
            _hosting = hosting;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            // ensure that the Database is created (otherwise can cause a crazy error)
            _ctx.Database.EnsureCreated();

            // Seed the Main User
            StoreUser user = await _userManager.FindByEmailAsync("anderson@gmail.com");
            if (user == null)
            {
                user = new StoreUser()
                {
                    LastName = "momeni",
                    FirstName = "ali",
                    Email = "anderson@gmail.com",
                    UserName = "anderson@gmail.com"
                };

                var result = await _userManager.CreateAsync(user, "P@ssw0rd!");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create user in Seeding");
                }
            }

            // if there aren't any products
            // -> need to create sample data
            if (!_ctx.Products.Any())
            {
                var filepath = Path.Combine(_hosting.ContentRootPath, "Data/art.json");
                var json = File.ReadAllText(filepath);
                var products = JsonConvert.DeserializeObject<IEnumerable<Product>>(json);
                _ctx.Products.AddRange(products);

                var order = _ctx.Orders.Where(o => o.Id == 1).FirstOrDefault();

                if (order == null)
                {
                    order = new Order();
                    order.User = user;
                    order.User.Id = user.Id;
                    //order.Id = 123456; // magic number
                    order.Items = new List<OrderItem>()
                    {
                        new OrderItem()
                        {
                            Product = products.First(),
                            Quantity = 5,
                            UnitPrice = products.First().Price
                        }
                    };

                    _ctx.Add(order);
                }
                _ctx.SaveChanges();
            }
        }
    }
}
